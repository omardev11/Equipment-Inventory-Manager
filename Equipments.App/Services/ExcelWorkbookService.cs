using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using Equipments.App.Models;

namespace Equipments.App.Services;

public sealed class ExcelWorkbookService
{
    private static readonly string[] Headers =
    [
        "Sıra No",
        "Malz.Grubu",
        "Malzeme Adı",
        "Birim",
        "Miktar",
        "Model",
        "Seri No",
        "Lokasyon",
        "Sorumlu"
    ];

    public List<EquipmentItem> Import(string filePath)
    {
        using var archive = ZipFile.OpenRead(filePath);
        var workbookEntry = archive.GetEntry("xl/workbook.xml") ?? throw new InvalidOperationException("Workbook.xml bulunamadı.");
        var workbookRelsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels") ?? throw new InvalidOperationException("Workbook ilişkileri bulunamadı.");

        var workbookDoc = XDocument.Load(workbookEntry.Open());
        var workbookRelsDoc = XDocument.Load(workbookRelsEntry.Open());
        var sharedStrings = LoadSharedStrings(archive);

        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace relNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        XNamespace packageRelNs = "http://schemas.openxmlformats.org/package/2006/relationships";

        var firstSheet = workbookDoc.Root?
            .Element(ns + "sheets")?
            .Elements(ns + "sheet")
            .FirstOrDefault() ?? throw new InvalidOperationException("Çalışma sayfası bulunamadı.");

        var relationshipId = firstSheet.Attribute(relNs + "id")?.Value
            ?? throw new InvalidOperationException("Sayfa ilişki bilgisi bulunamadı.");

        var target = workbookRelsDoc.Root?
            .Elements(packageRelNs + "Relationship")
            .FirstOrDefault(x => string.Equals(x.Attribute("Id")?.Value, relationshipId, StringComparison.Ordinal))?
            .Attribute("Target")?.Value
            ?? throw new InvalidOperationException("Sayfa dosya yolu bulunamadı.");

        var normalizedTarget = target.Replace('\\', '/').TrimStart('/');
        var worksheetEntry = archive.GetEntry($"xl/{normalizedTarget}") ?? archive.GetEntry(normalizedTarget);
        if (worksheetEntry is null)
        {
            throw new InvalidOperationException("Çalışma sayfası içeriği açılamadı.");
        }

        var worksheetDoc = XDocument.Load(worksheetEntry.Open());
        var rows = ParseRows(worksheetDoc, sharedStrings);
        if (rows.Count == 0)
        {
            return [];
        }

        var dataRows = rows.Skip(1)
            .Where(row => row.Any(cell => !string.IsNullOrWhiteSpace(cell)))
            .ToList();

        var items = new List<EquipmentItem>();
        foreach (var row in dataRows)
        {
            items.Add(new EquipmentItem
            {
                SequenceNo = ParseInt(GetCell(row, 0), items.Count + 1),
                Group = GetCell(row, 1),
                Name = GetCell(row, 2),
                Unit = string.IsNullOrWhiteSpace(GetCell(row, 3)) ? "Adet" : GetCell(row, 3),
                Quantity = ParseInt(GetCell(row, 4), 1),
                Model = GetCell(row, 5),
                SerialNo = GetCell(row, 6),
                Location = GetCell(row, 7),
                Owner = GetCell(row, 8)
            });
        }

        EquipmentRepository.Resequence(items);
        return EquipmentRepository.Normalize(items);
    }

    public void Export(string filePath, IEnumerable<EquipmentItem> items)
    {
        var ordered = EquipmentRepository.Normalize(items).OrderBy(x => x.SequenceNo).ToList();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);

        AddEntry(archive, "[Content_Types].xml", BuildContentTypesXml());
        AddEntry(archive, "_rels/.rels", BuildRootRelationshipsXml());
        AddEntry(archive, "docProps/app.xml", BuildAppXml());
        AddEntry(archive, "docProps/core.xml", BuildCoreXml());
        AddEntry(archive, "xl/workbook.xml", BuildWorkbookXml());
        AddEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelationshipsXml());
        AddEntry(archive, "xl/styles.xml", BuildStylesXml());
        AddEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(ordered));
    }

    private static List<string> LoadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry is null)
        {
            return [];
        }

        var doc = XDocument.Load(entry.Open());
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        return doc.Root?
            .Elements(ns + "si")
            .Select(si => string.Concat(si.Descendants(ns + "t").Select(t => t.Value)))
            .ToList() ?? [];
    }

    private static List<List<string>> ParseRows(XDocument worksheetDoc, IReadOnlyList<string> sharedStrings)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var result = new List<List<string>>();

        var rows = worksheetDoc.Root?
            .Element(ns + "sheetData")?
            .Elements(ns + "row") ?? [];

        foreach (var row in rows)
        {
            var values = new List<string>();
            foreach (var cell in row.Elements(ns + "c"))
            {
                var reference = cell.Attribute("r")?.Value ?? string.Empty;
                var columnIndex = GetColumnIndex(reference);
                while (values.Count <= columnIndex)
                {
                    values.Add(string.Empty);
                }

                values[columnIndex] = GetCellValue(cell, ns, sharedStrings);
            }

            result.Add(values);
        }

        return result;
    }

    private static string GetCellValue(XElement cell, XNamespace ns, IReadOnlyList<string> sharedStrings)
    {
        var type = cell.Attribute("t")?.Value;

        if (type == "inlineStr")
        {
            return string.Concat(cell.Descendants(ns + "t").Select(x => x.Value));
        }

        var raw = cell.Element(ns + "v")?.Value ?? string.Empty;
        return type switch
        {
            "s" when int.TryParse(raw, out var index) && index >= 0 && index < sharedStrings.Count => sharedStrings[index],
            "str" => raw,
            "b" => raw == "1" ? "TRUE" : "FALSE",
            _ => raw
        };
    }

    private static string GetCell(IReadOnlyList<string> row, int index)
    {
        return index < row.Count ? row[index].Trim() : string.Empty;
    }

    private static int ParseInt(string? value, int fallback)
    {
        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue))
        {
            return (int)Math.Round(doubleValue, MidpointRounding.AwayFromZero);
        }

        return fallback;
    }

    private static int GetColumnIndex(string cellReference)
    {
        var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        var value = 0;
        foreach (var letter in letters)
        {
            value = (value * 26) + (char.ToUpperInvariant(letter) - 'A' + 1);
        }

        return Math.Max(0, value - 1);
    }

    private static string GetExcelColumnName(int columnNumber)
    {
        var dividend = columnNumber;
        var columnName = string.Empty;

        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            columnName = Convert.ToChar(65 + modulo) + columnName;
            dividend = (dividend - modulo) / 26;
        }

        return columnName;
    }

    private static void AddEntry(ZipArchive archive, string path, XDocument document)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var stream = entry.Open();
        document.Save(stream);
    }

    private static XDocument BuildContentTypesXml()
    {
        XNamespace ns = "http://schemas.openxmlformats.org/package/2006/content-types";

        return new XDocument(
            new XElement(ns + "Types",
                new XElement(ns + "Default",
                    new XAttribute("Extension", "rels"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                new XElement(ns + "Default",
                    new XAttribute("Extension", "xml"),
                    new XAttribute("ContentType", "application/xml")),
                new XElement(ns + "Override",
                    new XAttribute("PartName", "/xl/workbook.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
                new XElement(ns + "Override",
                    new XAttribute("PartName", "/xl/worksheets/sheet1.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
                new XElement(ns + "Override",
                    new XAttribute("PartName", "/xl/styles.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml")),
                new XElement(ns + "Override",
                    new XAttribute("PartName", "/docProps/core.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-package.core-properties+xml")),
                new XElement(ns + "Override",
                    new XAttribute("PartName", "/docProps/app.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.extended-properties+xml"))));
    }

    private static XDocument BuildRootRelationshipsXml()
    {
        XNamespace ns = "http://schemas.openxmlformats.org/package/2006/relationships";

        return new XDocument(
            new XElement(ns + "Relationships",
                new XElement(ns + "Relationship",
                    new XAttribute("Id", "rId1"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                    new XAttribute("Target", "xl/workbook.xml")),
                new XElement(ns + "Relationship",
                    new XAttribute("Id", "rId2"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"),
                    new XAttribute("Target", "docProps/core.xml")),
                new XElement(ns + "Relationship",
                    new XAttribute("Id", "rId3"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"),
                    new XAttribute("Target", "docProps/app.xml"))));
    }

    private static XDocument BuildAppXml()
    {
        XNamespace ns = "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";
        XNamespace vt = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";

        return new XDocument(
            new XElement(ns + "Properties",
                new XAttribute(XNamespace.Xmlns + "vt", vt),
                new XElement(ns + "Application", "Equipments App"),
                new XElement(ns + "HeadingPairs",
                    new XElement(vt + "vector",
                        new XAttribute("size", "2"),
                        new XAttribute("baseType", "variant"),
                        new XElement(vt + "variant", new XElement(vt + "lpstr", "Worksheets")),
                        new XElement(vt + "variant", new XElement(vt + "i4", "1")))),
                new XElement(ns + "TitlesOfParts",
                    new XElement(vt + "vector",
                        new XAttribute("size", "1"),
                        new XAttribute("baseType", "lpstr"),
                        new XElement(vt + "lpstr", "Ekipmanlar")))));
    }

    private static XDocument BuildCoreXml()
    {
        XNamespace cp = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";
        XNamespace dc = "http://purl.org/dc/elements/1.1/";
        XNamespace dcterms = "http://purl.org/dc/terms/";
        XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
        var now = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture) + "Z";

        return new XDocument(
            new XElement(cp + "coreProperties",
                new XAttribute(XNamespace.Xmlns + "dc", dc),
                new XAttribute(XNamespace.Xmlns + "dcterms", dcterms),
                new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                new XElement(dc + "creator", "Codex"),
                new XElement(cp + "lastModifiedBy", "Codex"),
                new XElement(dcterms + "created",
                    new XAttribute(xsi + "type", "dcterms:W3CDTF"),
                    now),
                new XElement(dcterms + "modified",
                    new XAttribute(xsi + "type", "dcterms:W3CDTF"),
                    now),
                new XElement(dc + "title", "Office Equipment Inventory")));
    }

    private static XDocument BuildWorkbookXml()
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

        return new XDocument(
            new XElement(ns + "workbook",
                new XAttribute(XNamespace.Xmlns + "r", rel),
                new XElement(ns + "sheets",
                    new XElement(ns + "sheet",
                        new XAttribute("name", "Ekipmanlar"),
                        new XAttribute("sheetId", "1"),
                        new XAttribute(rel + "id", "rId1")))));
    }

    private static XDocument BuildWorkbookRelationshipsXml()
    {
        XNamespace ns = "http://schemas.openxmlformats.org/package/2006/relationships";

        return new XDocument(
            new XElement(ns + "Relationships",
                new XElement(ns + "Relationship",
                    new XAttribute("Id", "rId1"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                    new XAttribute("Target", "worksheets/sheet1.xml")),
                new XElement(ns + "Relationship",
                    new XAttribute("Id", "rId2"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                    new XAttribute("Target", "styles.xml"))));
    }

    private static XDocument BuildStylesXml()
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        return new XDocument(
            new XElement(ns + "styleSheet",
                new XElement(ns + "fonts",
                    new XAttribute("count", "2"),
                    new XElement(ns + "font",
                        new XElement(ns + "sz", new XAttribute("val", "11")),
                        new XElement(ns + "name", new XAttribute("val", "Calibri"))),
                    new XElement(ns + "font",
                        new XElement(ns + "b"),
                        new XElement(ns + "sz", new XAttribute("val", "11")),
                        new XElement(ns + "color", new XAttribute("rgb", "FFFFFFFF")),
                        new XElement(ns + "name", new XAttribute("val", "Calibri")))),
                new XElement(ns + "fills",
                    new XAttribute("count", "3"),
                    new XElement(ns + "fill", new XElement(ns + "patternFill", new XAttribute("patternType", "none"))),
                    new XElement(ns + "fill", new XElement(ns + "patternFill", new XAttribute("patternType", "gray125"))),
                    new XElement(ns + "fill",
                        new XElement(ns + "patternFill",
                            new XAttribute("patternType", "solid"),
                            new XElement(ns + "fgColor", new XAttribute("rgb", "FF1F4E78")),
                            new XElement(ns + "bgColor", new XAttribute("indexed", "64"))))),
                new XElement(ns + "borders",
                    new XAttribute("count", "2"),
                    new XElement(ns + "border",
                        new XElement(ns + "left"),
                        new XElement(ns + "right"),
                        new XElement(ns + "top"),
                        new XElement(ns + "bottom"),
                        new XElement(ns + "diagonal")),
                    new XElement(ns + "border",
                        new XElement(ns + "left", new XAttribute("style", "thin")),
                        new XElement(ns + "right", new XAttribute("style", "thin")),
                        new XElement(ns + "top", new XAttribute("style", "thin")),
                        new XElement(ns + "bottom", new XAttribute("style", "thin")),
                        new XElement(ns + "diagonal"))),
                new XElement(ns + "cellStyleXfs",
                    new XAttribute("count", "1"),
                    new XElement(ns + "xf",
                        new XAttribute("numFmtId", "0"),
                        new XAttribute("fontId", "0"),
                        new XAttribute("fillId", "0"),
                        new XAttribute("borderId", "0"))),
                new XElement(ns + "cellXfs",
                    new XAttribute("count", "2"),
                    new XElement(ns + "xf",
                        new XAttribute("numFmtId", "0"),
                        new XAttribute("fontId", "0"),
                        new XAttribute("fillId", "0"),
                        new XAttribute("borderId", "1"),
                        new XAttribute("xfId", "0"),
                        new XAttribute("applyBorder", "1")),
                    new XElement(ns + "xf",
                        new XAttribute("numFmtId", "0"),
                        new XAttribute("fontId", "1"),
                        new XAttribute("fillId", "2"),
                        new XAttribute("borderId", "1"),
                        new XAttribute("xfId", "0"),
                        new XAttribute("applyFont", "1"),
                        new XAttribute("applyFill", "1"),
                        new XAttribute("applyBorder", "1")))));
    }

    private static XDocument BuildWorksheetXml(IReadOnlyList<EquipmentItem> items)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var rows = new List<XElement>();

        rows.Add(BuildRow(1, Headers, isHeader: true));

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var rowNumber = index + 2;
            var values = new object?[]
            {
                item.SequenceNo,
                item.Group,
                item.Name,
                item.Unit,
                item.Quantity,
                item.Model,
                item.SerialNo,
                item.Location,
                item.Owner
            };

            rows.Add(BuildRow(rowNumber, values, isHeader: false));
        }

        return new XDocument(
            new XElement(ns + "worksheet",
                new XElement(ns + "sheetViews",
                    new XElement(ns + "sheetView",
                        new XAttribute("workbookViewId", "0"))),
                new XElement(ns + "sheetFormatPr",
                    new XAttribute("defaultRowHeight", "15")),
                new XElement(ns + "cols",
                    BuildColumn(1, 1, 12),
                    BuildColumn(2, 2, 18),
                    BuildColumn(3, 3, 48),
                    BuildColumn(4, 4, 10),
                    BuildColumn(5, 5, 10),
                    BuildColumn(6, 6, 24),
                    BuildColumn(7, 7, 22),
                    BuildColumn(8, 8, 20),
                    BuildColumn(9, 9, 24)),
                new XElement(ns + "sheetData", rows),
                new XElement(ns + "autoFilter", new XAttribute("ref", $"A1:I{items.Count + 1}")),
                new XElement(ns + "pageMargins",
                    new XAttribute("left", "0.7"),
                    new XAttribute("right", "0.7"),
                    new XAttribute("top", "0.75"),
                    new XAttribute("bottom", "0.75"),
                    new XAttribute("header", "0.3"),
                    new XAttribute("footer", "0.3"))));
    }

    private static XElement BuildColumn(int min, int max, double width)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        return new XElement(ns + "col",
            new XAttribute("min", min),
            new XAttribute("max", max),
            new XAttribute("width", width.ToString(CultureInfo.InvariantCulture)),
            new XAttribute("customWidth", "1"));
    }

    private static XElement BuildRow(int rowNumber, IEnumerable<object?> values, bool isHeader)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var cells = new List<XElement>();
        var column = 1;

        foreach (var value in values)
        {
            var cellRef = $"{GetExcelColumnName(column)}{rowNumber}";
            cells.Add(BuildCell(cellRef, value, isHeader ? 1u : 0u));
            column++;
        }

        return new XElement(ns + "row",
            new XAttribute("r", rowNumber),
            cells);
    }

    private static XElement BuildCell(string reference, object? value, uint styleIndex)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        if (value is null)
        {
            return new XElement(ns + "c",
                new XAttribute("r", reference),
                new XAttribute("s", styleIndex));
        }

        if (value is int or long or double or float or decimal)
        {
            return new XElement(ns + "c",
                new XAttribute("r", reference),
                new XAttribute("s", styleIndex),
                new XElement(ns + "v", Convert.ToString(value, CultureInfo.InvariantCulture)));
        }

        return new XElement(ns + "c",
            new XAttribute("r", reference),
            new XAttribute("t", "inlineStr"),
            new XAttribute("s", styleIndex),
            new XElement(ns + "is", new XElement(ns + "t", value.ToString() ?? string.Empty)));
    }
}
