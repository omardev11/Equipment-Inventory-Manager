using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;

namespace Equipments.App.Services;

public sealed class TableWorkbookExportService
{
    public void Export(string filePath, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<object?>> rows, string sheetName)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
        AddEntry(archive, "[Content_Types].xml", BuildContentTypesXml());
        AddEntry(archive, "_rels/.rels", BuildRootRelationshipsXml());
        AddEntry(archive, "docProps/app.xml", BuildAppXml(sheetName));
        AddEntry(archive, "docProps/core.xml", BuildCoreXml(sheetName));
        AddEntry(archive, "xl/workbook.xml", BuildWorkbookXml(sheetName));
        AddEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRelationshipsXml());
        AddEntry(archive, "xl/styles.xml", BuildStylesXml());
        AddEntry(archive, "xl/worksheets/sheet1.xml", BuildWorksheetXml(headers, rows));
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
                new XElement(ns + "Default", new XAttribute("Extension", "rels"), new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                new XElement(ns + "Default", new XAttribute("Extension", "xml"), new XAttribute("ContentType", "application/xml")),
                new XElement(ns + "Override", new XAttribute("PartName", "/xl/workbook.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
                new XElement(ns + "Override", new XAttribute("PartName", "/xl/worksheets/sheet1.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
                new XElement(ns + "Override", new XAttribute("PartName", "/xl/styles.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml")),
                new XElement(ns + "Override", new XAttribute("PartName", "/docProps/core.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-package.core-properties+xml")),
                new XElement(ns + "Override", new XAttribute("PartName", "/docProps/app.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.extended-properties+xml"))));
    }

    private static XDocument BuildRootRelationshipsXml()
    {
        XNamespace ns = "http://schemas.openxmlformats.org/package/2006/relationships";
        return new XDocument(
            new XElement(ns + "Relationships",
                new XElement(ns + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"), new XAttribute("Target", "xl/workbook.xml")),
                new XElement(ns + "Relationship", new XAttribute("Id", "rId2"), new XAttribute("Type", "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"), new XAttribute("Target", "docProps/core.xml")),
                new XElement(ns + "Relationship", new XAttribute("Id", "rId3"), new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"), new XAttribute("Target", "docProps/app.xml"))));
    }

    private static XDocument BuildAppXml(string sheetName)
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
                        new XElement(vt + "lpstr", sheetName)))));
    }

    private static XDocument BuildCoreXml(string title)
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
                new XElement(dcterms + "created", new XAttribute(xsi + "type", "dcterms:W3CDTF"), now),
                new XElement(dcterms + "modified", new XAttribute(xsi + "type", "dcterms:W3CDTF"), now),
                new XElement(dc + "title", title)));
    }

    private static XDocument BuildWorkbookXml(string sheetName)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        return new XDocument(
            new XElement(ns + "workbook",
                new XAttribute(XNamespace.Xmlns + "r", rel),
                new XElement(ns + "sheets",
                    new XElement(ns + "sheet", new XAttribute("name", sheetName), new XAttribute("sheetId", "1"), new XAttribute(rel + "id", "rId1")))));
    }

    private static XDocument BuildWorkbookRelationshipsXml()
    {
        XNamespace ns = "http://schemas.openxmlformats.org/package/2006/relationships";
        return new XDocument(
            new XElement(ns + "Relationships",
                new XElement(ns + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"), new XAttribute("Target", "worksheets/sheet1.xml")),
                new XElement(ns + "Relationship", new XAttribute("Id", "rId2"), new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"), new XAttribute("Target", "styles.xml"))));
    }

    private static XDocument BuildStylesXml()
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        return new XDocument(
            new XElement(ns + "styleSheet",
                new XElement(ns + "fonts",
                    new XAttribute("count", "2"),
                    new XElement(ns + "font", new XElement(ns + "sz", new XAttribute("val", "11")), new XElement(ns + "name", new XAttribute("val", "Calibri"))),
                    new XElement(ns + "font", new XElement(ns + "b"), new XElement(ns + "sz", new XAttribute("val", "11")), new XElement(ns + "color", new XAttribute("rgb", "FFFFFFFF")), new XElement(ns + "name", new XAttribute("val", "Calibri")))),
                new XElement(ns + "fills",
                    new XAttribute("count", "3"),
                    new XElement(ns + "fill", new XElement(ns + "patternFill", new XAttribute("patternType", "none"))),
                    new XElement(ns + "fill", new XElement(ns + "patternFill", new XAttribute("patternType", "gray125"))),
                    new XElement(ns + "fill", new XElement(ns + "patternFill", new XAttribute("patternType", "solid"), new XElement(ns + "fgColor", new XAttribute("rgb", "FF1F4E78")), new XElement(ns + "bgColor", new XAttribute("indexed", "64"))))),
                new XElement(ns + "borders",
                    new XAttribute("count", "2"),
                    new XElement(ns + "border", new XElement(ns + "left"), new XElement(ns + "right"), new XElement(ns + "top"), new XElement(ns + "bottom"), new XElement(ns + "diagonal")),
                    new XElement(ns + "border", new XElement(ns + "left", new XAttribute("style", "thin")), new XElement(ns + "right", new XAttribute("style", "thin")), new XElement(ns + "top", new XAttribute("style", "thin")), new XElement(ns + "bottom", new XAttribute("style", "thin")), new XElement(ns + "diagonal"))),
                new XElement(ns + "cellStyleXfs", new XAttribute("count", "1"), new XElement(ns + "xf", new XAttribute("numFmtId", "0"), new XAttribute("fontId", "0"), new XAttribute("fillId", "0"), new XAttribute("borderId", "0"))),
                new XElement(ns + "cellXfs",
                    new XAttribute("count", "2"),
                    new XElement(ns + "xf", new XAttribute("numFmtId", "0"), new XAttribute("fontId", "0"), new XAttribute("fillId", "0"), new XAttribute("borderId", "1"), new XAttribute("xfId", "0"), new XAttribute("applyBorder", "1")),
                    new XElement(ns + "xf", new XAttribute("numFmtId", "0"), new XAttribute("fontId", "1"), new XAttribute("fillId", "2"), new XAttribute("borderId", "1"), new XAttribute("xfId", "0"), new XAttribute("applyFont", "1"), new XAttribute("applyFill", "1"), new XAttribute("applyBorder", "1")))));
    }

    private static XDocument BuildWorksheetXml(IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<object?>> rows)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var xmlRows = new List<XElement> { BuildRow(1, headers.Cast<object?>().ToList(), true) };
        for (var i = 0; i < rows.Count; i++)
        {
            xmlRows.Add(BuildRow(i + 2, rows[i], false));
        }

        var columns = Enumerable.Range(1, headers.Count)
            .Select(index => new XElement(ns + "col",
                new XAttribute("min", index),
                new XAttribute("max", index),
                new XAttribute("width", "18"),
                new XAttribute("customWidth", "1")));

        return new XDocument(
            new XElement(ns + "worksheet",
                new XElement(ns + "sheetViews", new XElement(ns + "sheetView", new XAttribute("workbookViewId", "0"))),
                new XElement(ns + "sheetFormatPr", new XAttribute("defaultRowHeight", "15")),
                new XElement(ns + "cols", columns),
                new XElement(ns + "sheetData", xmlRows),
                new XElement(ns + "autoFilter", new XAttribute("ref", $"A1:{GetExcelColumnName(headers.Count)}{rows.Count + 1}")),
                new XElement(ns + "pageMargins",
                    new XAttribute("left", "0.7"),
                    new XAttribute("right", "0.7"),
                    new XAttribute("top", "0.75"),
                    new XAttribute("bottom", "0.75"),
                    new XAttribute("header", "0.3"),
                    new XAttribute("footer", "0.3"))));
    }

    private static XElement BuildRow(int rowNumber, IReadOnlyList<object?> values, bool isHeader)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var cells = new List<XElement>();
        for (var i = 0; i < values.Count; i++)
        {
            var reference = $"{GetExcelColumnName(i + 1)}{rowNumber}";
            cells.Add(BuildCell(reference, values[i], isHeader ? 1u : 0u));
        }

        return new XElement(ns + "row", new XAttribute("r", rowNumber), cells);
    }

    private static XElement BuildCell(string reference, object? value, uint styleIndex)
    {
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        if (value is null)
        {
            return new XElement(ns + "c", new XAttribute("r", reference), new XAttribute("s", styleIndex));
        }

        if (value is int or long or double or float or decimal)
        {
            return new XElement(ns + "c", new XAttribute("r", reference), new XAttribute("s", styleIndex), new XElement(ns + "v", Convert.ToString(value, CultureInfo.InvariantCulture)));
        }

        if (value is DateTime dateTime)
        {
            value = dateTime.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
        }

        return new XElement(ns + "c",
            new XAttribute("r", reference),
            new XAttribute("t", "inlineStr"),
            new XAttribute("s", styleIndex),
            new XElement(ns + "is", new XElement(ns + "t", value.ToString() ?? string.Empty)));
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
}
