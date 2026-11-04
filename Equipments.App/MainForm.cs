using System.ComponentModel;
using Equipments.App.Models;
using Equipments.App.Services;

namespace Equipments.App;

public partial class MainForm : Form
{
    private const string AllFilterText = "Tümü";

    private readonly EquipmentRepository _repository = new(AppPaths.DatabasePath);
    private readonly StfRequestRepository _stfRepository = new(AppPaths.StfRequestsPath);
    private readonly MaterialMovementRepository _movementRepository = new(AppPaths.MaterialMovementsPath);
    private readonly SupabaseSettingsRepository _supabaseSettingsRepository = new(AppPaths.SupabaseSettingsPath);
    private readonly SupabaseSyncService _supabaseSyncService = new();
    private readonly ExcelWorkbookService _excelService = new();
    private readonly TableWorkbookExportService _tableExportService = new();

    private readonly BindingSource _equipmentBindingSource = new();
    private readonly BindingSource _stfBindingSource = new();
    private readonly BindingSource _movementBindingSource = new();
    private readonly BindingSource _stockBindingSource = new();

    private List<EquipmentItem> _allItems = [];
    private List<StfRequestItem> _allStfRequests = [];
    private List<MaterialMovementItem> _allMovements = [];
    private List<StoreStockItem> _allStockItems = [];
    private SupabaseSettings _supabaseSettings = new();

    public MainForm()
    {
        InitializeComponent();
        SetWindowIcon();
        ConfigureGrids();
        _supabaseSettings = _supabaseSettingsRepository.Load();
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        AdjustSplitLayout();
        LoadInitialData();
        ApplyEquipmentFilters();
        ApplyStfFilters();
        ApplyMovementFilters();
        ApplyStockFilters();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        AdjustSplitLayout();
    }

    private void LoadInitialData()
    {
        _allItems = _repository.Load();
        _allStfRequests = _stfRepository.Load();
        _allMovements = _movementRepository.Load();

        RefreshEquipmentFilterValues();
        RefreshStfFilterValues();
        RefreshMovementFilterValues();
        RefreshStockData();
        RefreshStockFilterValues();
    }

    private void ConfigureGrids()
    {
        _equipmentBindingSource.DataSource = typeof(EquipmentItem);
        equipmentGrid.AutoGenerateColumns = false;
        equipmentGrid.DataSource = _equipmentBindingSource;
        equipmentGrid.DoubleClick += async (_, _) => await EditSelectedItemAsync();
        equipmentGrid.SelectionChanged += (_, _) => UpdateSelectionSummary();
        equipmentGrid.CellFormatting += EquipmentGrid_CellFormatting;

        _stfBindingSource.DataSource = typeof(StfRequestItem);
        stfGrid.AutoGenerateColumns = false;
        stfGrid.DataSource = _stfBindingSource;
        stfGrid.DoubleClick += (_, _) => EditSelectedStfRequest();
        stfGrid.CellFormatting += StfGrid_CellFormatting;

        _movementBindingSource.DataSource = typeof(MaterialMovementItem);
        movementGrid.AutoGenerateColumns = false;
        movementGrid.DataSource = _movementBindingSource;
        movementGrid.DoubleClick += (_, _) => EditSelectedMovement();
        movementGrid.CellFormatting += MovementGrid_CellFormatting;

        _stockBindingSource.DataSource = typeof(StoreStockItem);
        stockGrid.AutoGenerateColumns = false;
        stockGrid.DataSource = _stockBindingSource;
        stockGrid.DoubleClick += (_, _) => StartStockOut();
    }

    private void ApplyEquipmentFilters()
    {
        var search = searchTextBox.Text.Trim();
        var ownerFilter = ownerFilterTextBox.Text.Trim();
        var selectedGroup = groupFilterComboBox.SelectedItem?.ToString();
        var selectedLocation = locationFilterComboBox.SelectedItem?.ToString();

        var filtered = _allItems
            .Where(item => string.IsNullOrWhiteSpace(search)
                || Contains(item.Group, search)
                || Contains(item.Name, search)
                || Contains(item.Model, search)
                || Contains(item.SerialNo, search)
                || Contains(item.Location, search)
                || Contains(item.Owner, search))
            .Where(item => string.IsNullOrWhiteSpace(ownerFilter) || Contains(item.Owner, ownerFilter))
            .Where(item => selectedGroup is null || selectedGroup == AllFilterText || string.Equals(item.Group, selectedGroup, StringComparison.OrdinalIgnoreCase))
            .Where(item => selectedLocation is null || selectedLocation == AllFilterText || string.Equals(item.Location, selectedLocation, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.SequenceNo)
            .ToList();

        _equipmentBindingSource.DataSource = new BindingList<EquipmentItem>(filtered);
        UpdateEquipmentSummary(filtered);
        UpdateSelectionSummary();
    }

    private void RefreshEquipmentFilterValues()
    {
        RebindFilter(groupFilterComboBox, _allItems.Select(x => x.Group));
        RebindFilter(locationFilterComboBox, _allItems.Select(x => x.Location));
    }

    private void UpdateEquipmentSummary(IReadOnlyCollection<EquipmentItem> items)
    {
        totalCountValueLabel.Text = items.Count.ToString();
        missingModelValueLabel.Text = items.Count(x => string.IsNullOrWhiteSpace(x.Model)).ToString();
        missingSerialValueLabel.Text = items.Count(x => string.IsNullOrWhiteSpace(x.SerialNo)).ToString();
        duplicateSerialValueLabel.Text = GetDuplicateSerialCount(items).ToString();

        statusLabel.Text = "Hazır";

        FillTopList(topGroupsListBox, items.Select(x => x.Group));
        FillTopList(topLocationsListBox, items.Select(x => x.Location));
    }

    private static void FillTopList(ListBox listBox, IEnumerable<string> values)
    {
        listBox.BeginUpdate();
        listBox.Items.Clear();
        foreach (var entry in values
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .GroupBy(x => x.Trim())
                     .OrderByDescending(g => g.Count())
                     .ThenBy(g => g.Key)
                     .Take(8))
        {
            listBox.Items.Add($"{entry.Key} ({entry.Count()})");
        }
        listBox.EndUpdate();
    }

    private void ApplyStfFilters()
    {
        var search = stfSearchTextBox.Text.Trim();
        var department = stfDepartmentFilterComboBox.SelectedItem?.ToString();
        var status = stfStatusFilterComboBox.SelectedItem?.ToString();

        var filtered = _allStfRequests
            .Where(item => string.IsNullOrWhiteSpace(search)
                || Contains(item.StfNo, search)
                || Contains(item.RequestedBy, search)
                || Contains(item.Department, search)
                || Contains(item.ProductName, search)
                || Contains(item.Description, search))
            .Where(item => department is null || department == AllFilterText || string.Equals(item.Department, department, StringComparison.OrdinalIgnoreCase))
            .Where(item => status is null || status == AllFilterText || string.Equals(item.Status, status, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.RequestDate)
            .ThenBy(item => item.RowNo)
            .ToList();

        _stfBindingSource.DataSource = new BindingList<StfRequestItem>(filtered);
        stfTotalValueLabel.Text = filtered.Count.ToString();
        stfWaitingValueLabel.Text = filtered.Count(x => x.Status == "Bekliyor").ToString();
        stfPartialValueLabel.Text = filtered.Count(x => x.Status == "Kısmi Geldi").ToString();
        stfCompletedValueLabel.Text = filtered.Count(x => x.Status == "Alındı").ToString();
    }

    private void RefreshStfFilterValues()
    {
        RebindFilter(stfDepartmentFilterComboBox, _allStfRequests.Select(x => x.Department));
    }

    private void ApplyMovementFilters()
    {
        var search = movementSearchTextBox.Text.Trim();
        var movementType = movementTypeFilterComboBox.SelectedItem?.ToString();

        var filtered = _allMovements
            .Where(item => string.IsNullOrWhiteSpace(search)
                || Contains(item.ProductName, search)
                || Contains(item.ReceivedBy, search)
                || Contains(item.DeliveredBy, search)
                || Contains(item.FromLocation, search)
                || Contains(item.ToLocation, search)
                || Contains(item.Notes, search))
            .Where(item => movementType is null || movementType == AllFilterText || string.Equals(item.MovementType, movementType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.MovementDate)
            .ThenBy(item => item.ProductName)
            .ToList();

        _movementBindingSource.DataSource = new BindingList<MaterialMovementItem>(filtered);
        movementTotalValueLabel.Text = filtered.Count.ToString();
        movementInboundValueLabel.Text = filtered.Count(x => x.MovementType == "Depoya Giriş").ToString();
        movementOutboundValueLabel.Text = filtered.Count(x => x.MovementType is "Teslim" or "Transfer" or "Stok Çıkışı").ToString();
    }

    private void RefreshMovementFilterValues()
    {
        if (movementTypeFilterComboBox.SelectedIndex < 0)
        {
            movementTypeFilterComboBox.SelectedIndex = 0;
        }
    }

    private void RefreshStockData()
    {
        var entries = new List<(string ProductName, string Unit, string Location, decimal Delta, DateTime MovementDate, string ReceivedBy)>();

        foreach (var movement in _allMovements)
        {
            var type = movement.MovementType?.Trim() ?? string.Empty;

            if (type is "Depoya Giriş" or "Transfer" or "İade")
            {
                if (!string.IsNullOrWhiteSpace(movement.ToLocation))
                {
                    entries.Add((movement.ProductName, movement.Unit, movement.ToLocation, movement.Quantity, movement.MovementDate, movement.ReceivedBy));
                }
            }

            if (type is "Teslim" or "Transfer" or "Stok Çıkışı")
            {
                if (!string.IsNullOrWhiteSpace(movement.FromLocation))
                {
                    entries.Add((movement.ProductName, movement.Unit, movement.FromLocation, -movement.Quantity, movement.MovementDate, movement.ReceivedBy));
                }
            }
        }

        _allStockItems = entries
            .Where(x => !string.IsNullOrWhiteSpace(x.ProductName) && !string.IsNullOrWhiteSpace(x.Location))
            .GroupBy(x => new { x.ProductName, x.Unit, x.Location })
            .Select(group =>
            {
                var latest = group.OrderByDescending(x => x.MovementDate).First();
                var latestMovement = FindLatestMovement(group.Key.ProductName, group.Key.Unit, group.Key.Location);
                return new StoreStockItem
                {
                    StfNo = ResolveStfNo(latestMovement?.RelatedRequestId),
                    ProductName = group.Key.ProductName,
                    Unit = group.Key.Unit,
                    Location = group.Key.Location,
                    Quantity = group.Sum(x => x.Delta),
                    LastMovementDate = latest.MovementDate,
                    LastReceivedBy = latest.ReceivedBy
                };
            })
            .Where(x => x.Quantity > 0)
            .OrderBy(x => x.ProductName)
            .ThenBy(x => x.Location)
            .ToList();
    }

    private void ApplyStockFilters()
    {
        var search = stockSearchTextBox.Text.Trim();
        var location = stockLocationFilterComboBox.SelectedItem?.ToString();

        var filtered = _allStockItems
            .Where(item => string.IsNullOrWhiteSpace(search)
                || Contains(item.ProductName, search)
                || Contains(item.Location, search))
            .Where(item => location is null || location == AllFilterText || string.Equals(item.Location, location, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.ProductName)
            .ThenBy(item => item.Location)
            .ToList();

        _stockBindingSource.DataSource = new BindingList<StoreStockItem>(filtered);
        stockLineCountValueLabel.Text = filtered.Count.ToString();
        stockTotalQuantityValueLabel.Text = filtered.Sum(x => x.Quantity).ToString("0.##");
        stockLowCountValueLabel.Text = filtered.Count(x => x.Quantity <= 5).ToString();
    }

    private void RefreshStockFilterValues()
    {
        RebindFilter(stockLocationFilterComboBox, _allStockItems.Select(x => x.Location));
    }

    private async void AddButton_Click(object? sender, EventArgs e)
    {
        var item = new EquipmentItem { SequenceNo = _allItems.Count + 1 };
        using var dialog = CreateEquipmentDialog(item);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (!WarnAboutDuplicateSerial(dialog.Item, null))
        {
            return;
        }

        _allItems.Add(dialog.Item);
        await SaveEquipmentAndRefreshAsync("Yeni ekipman eklendi.");
    }

    private async void EditButton_Click(object? sender, EventArgs e)
    {
        await EditSelectedItemAsync();
    }

    private async Task EditSelectedItemAsync()
    {
        var item = GetSelectedEquipmentItem();
        if (item is null)
        {
            return;
        }

        using var dialog = CreateEquipmentDialog(item);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var index = _allItems.FindIndex(x => x.Id == item.Id);
        if (index < 0)
        {
            return;
        }

        if (!WarnAboutDuplicateSerial(dialog.Item, item.Id))
        {
            return;
        }

        _allItems[index] = dialog.Item;
        await SaveEquipmentAndRefreshAsync("Kayıt güncellendi.");
    }

    private async void DeleteButton_Click(object? sender, EventArgs e)
    {
        var selectedItems = GetSelectedEquipmentItems();
        if (selectedItems.Count == 0)
        {
            return;
        }

        var message = selectedItems.Count == 1
            ? $"'{selectedItems[0].Name}' kaydı silinsin mi?"
            : $"{selectedItems.Count} kayıt silinsin mi?";

        if (MessageBox.Show(message, "Kaydı Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        var ids = selectedItems.Select(x => x.Id).ToHashSet();
        _allItems.RemoveAll(x => ids.Contains(x.Id));
        await SaveEquipmentAndRefreshAsync("Kayıt silindi.");
    }

    private void ExportButton_Click(object? sender, EventArgs e)
    {
        var selectedItems = GetSelectedEquipmentItems();
        if (selectedItems.Count == 0)
        {
            MessageBox.Show("Lütfen dışa aktarmak için en az bir satır seçin.", "Seçim Gerekli", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = CreateExcelSaveDialog("equipments");
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            _excelService.Export(dialog.FileName, selectedItems);
            statusLabel.Text = $"Excel dışa aktarıldı: {Path.GetFileName(dialog.FileName)}";
            MessageBox.Show("Excel dosyası başarıyla oluşturuldu.", "Dışa Aktarma", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Excel dışa aktarma sırasında hata oluştu:\n{ex.Message}", "Dışa Aktarma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        await SaveEquipmentAndRefreshAsync("Veriler kaydedildi.");
    }

    private void ClearFiltersButton_Click(object? sender, EventArgs e)
    {
        searchTextBox.Clear();
        ownerFilterTextBox.Clear();
        groupFilterComboBox.SelectedItem = AllFilterText;
        locationFilterComboBox.SelectedItem = AllFilterText;
        ApplyEquipmentFilters();
    }

    private void SearchTextBox_TextChanged(object? sender, EventArgs e) => ApplyEquipmentFilters();
    private void FilterComboBox_SelectedIndexChanged(object? sender, EventArgs e) => ApplyEquipmentFilters();
    private void OwnerFilterTextBox_TextChanged(object? sender, EventArgs e) => ApplyEquipmentFilters();

    private async Task SaveEquipmentAndRefreshAsync(string statusMessage)
    {
        EquipmentRepository.Resequence(_allItems);
        _allItems = EquipmentRepository.Normalize(_allItems);
        _repository.Save(_allItems);
        RefreshEquipmentFilterValues();
        ApplyEquipmentFilters();
        statusLabel.Text = statusMessage;

        if (_supabaseSettings.IsConfigured && _supabaseSettings.AutoSyncEnabled)
        {
            await SyncToSupabaseAsync();
        }
    }

    private void AddStfButton_Click(object? sender, EventArgs e)
    {
        var item = new StfRequestItem { RowNo = _allStfRequests.Count + 1 };
        using var dialog = CreateStfDialog(item);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _allStfRequests.Add(dialog.Item);
        SaveStfAndRefresh("Yeni STF talebi eklendi.");
    }

    private void EditStfButton_Click(object? sender, EventArgs e)
    {
        EditSelectedStfRequest();
    }

    private void EditSelectedStfRequest()
    {
        var item = GetSelectedStfItem();
        if (item is null)
        {
            return;
        }

        using var dialog = CreateStfDialog(item);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var index = _allStfRequests.FindIndex(x => x.Id == item.Id);
        if (index < 0)
        {
            return;
        }

        _allStfRequests[index] = dialog.Item;
        SaveStfAndRefresh("STF talebi güncellendi.");
    }

    private void DeleteStfButton_Click(object? sender, EventArgs e)
    {
        var selectedItems = GetSelectedStfItems();
        if (selectedItems.Count == 0)
        {
            return;
        }

        if (MessageBox.Show($"{selectedItems.Count} STF kaydı silinsin mi?", "STF Kaydını Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        var ids = selectedItems.Select(x => x.Id).ToHashSet();
        _allStfRequests.RemoveAll(x => ids.Contains(x.Id));
        SaveStfAndRefresh("STF kayıtları silindi.");
    }

    private void ReceiveStfButton_Click(object? sender, EventArgs e)
    {
        var item = GetSelectedStfItem();
        if (item is null)
        {
            MessageBox.Show("Önce bir STF talebi seçin.", "Seçim Gerekli", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new ReceiveStockDialog(item, GetKnownLocations(), GetKnownPeople());
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var index = _allStfRequests.FindIndex(x => x.Id == item.Id);
        if (index < 0)
        {
            return;
        }

        _allStfRequests[index].ReceivedQuantity += dialog.Quantity;
        _allStfRequests[index].Status = StfRequestRepository.ComputeStatus(_allStfRequests[index]);
        if (!string.IsNullOrWhiteSpace(dialog.Notes))
        {
            _allStfRequests[index].Description = string.Join(" | ", new[] { _allStfRequests[index].Description, dialog.Notes }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        _allMovements.Add(new MaterialMovementItem
        {
            RelatedRequestId = item.Id,
            MovementDate = dialog.ReceiveDate,
            MovementType = "Depoya Giriş",
            ProductName = item.ProductName,
            Unit = item.Unit,
            Quantity = dialog.Quantity,
            FromLocation = dialog.FromLocation,
            ToLocation = dialog.ToLocation,
            ReceivedBy = dialog.ReceivedBy,
            DeliveredBy = string.Empty,
            Notes = dialog.Notes
        });

        SaveAllTrackingData("Gelen malzeme kaydı eklendi.");
    }

    private void ExportStfButton_Click(object? sender, EventArgs e)
    {
        var items = GetSelectedStfItems();
        if (items.Count == 0)
        {
            items = _stfBindingSource.List.Cast<StfRequestItem>().ToList();
        }

        if (items.Count == 0)
        {
            MessageBox.Show("Dışa aktarılacak STF kaydı yok.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = CreateExcelSaveDialog("stf-takip");
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var headers = new[]
        {
            "No", "STF No", "Talep Tarihi", "Talep Eden", "Bölüm", "Ürün Adı", "Birim", "Talep Miktarı", "Gelen Miktar", "Kalan Talep", "Durum", "Açıklama"
        };
        var rows = items.Select(item => (IReadOnlyList<object?>)
        [
            item.RowNo, item.StfNo, item.RequestDate, item.RequestedBy, item.Department, item.ProductName, item.Unit,
            item.RequestedQuantity, item.ReceivedQuantity, item.RemainingQuantity, item.Status, item.Description
        ]).ToList();

        try
        {
            _tableExportService.Export(dialog.FileName, headers, rows, "STF Takip");
            statusLabel.Text = $"STF listesi dışa aktarıldı: {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"STF listesi dışa aktarılamadı:\n{ex.Message}", "Dışa Aktarma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearStfFiltersButton_Click(object? sender, EventArgs e)
    {
        stfSearchTextBox.Clear();
        stfDepartmentFilterComboBox.SelectedItem = AllFilterText;
        stfStatusFilterComboBox.SelectedItem = AllFilterText;
        ApplyStfFilters();
    }

    private void StfFilterChanged(object? sender, EventArgs e) => ApplyStfFilters();

    private void AddMovementButton_Click(object? sender, EventArgs e)
    {
        var item = new MaterialMovementItem();
        using var dialog = CreateMovementDialog(item);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _allMovements.Add(dialog.Item);
        SaveMovementsAndRefresh("Malzeme hareketi eklendi.");
    }

    private void EditMovementButton_Click(object? sender, EventArgs e)
    {
        EditSelectedMovement();
    }

    private void EditSelectedMovement()
    {
        var item = GetSelectedMovementItem();
        if (item is null)
        {
            return;
        }

        using var dialog = CreateMovementDialog(item);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var index = _allMovements.FindIndex(x => x.Id == item.Id);
        if (index < 0)
        {
            return;
        }

        _allMovements[index] = dialog.Item;
        SaveMovementsAndRefresh("Malzeme hareketi güncellendi.");
    }

    private void DeleteMovementButton_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedMovementItems();
        if (selected.Count == 0)
        {
            return;
        }

        if (MessageBox.Show($"{selected.Count} hareket kaydı silinsin mi?", "Hareket Sil", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        var ids = selected.Select(x => x.Id).ToHashSet();
        _allMovements.RemoveAll(x => ids.Contains(x.Id));
        SaveMovementsAndRefresh("Malzeme hareketleri silindi.");
    }

    private void ExportMovementsButton_Click(object? sender, EventArgs e)
    {
        var items = GetSelectedMovementItems();
        if (items.Count == 0)
        {
            items = _movementBindingSource.List.Cast<MaterialMovementItem>().ToList();
        }

        if (items.Count == 0)
        {
            MessageBox.Show("Dışa aktarılacak hareket kaydı yok.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = CreateExcelSaveDialog("malzeme-hareketleri");
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var headers = new[]
        {
            "STF No", "Tarih", "Hareket Tipi", "Ürün Adı", "Miktar", "Birim", "Nereden", "Nereye", "Teslim Alan", "Teslim Eden", "Açıklama"
        };
        var rows = items.Select(item => (IReadOnlyList<object?>)
        [
            ResolveStfNo(item.RelatedRequestId), item.MovementDate, item.MovementType, item.ProductName, item.Quantity, item.Unit, item.FromLocation, item.ToLocation, item.ReceivedBy, item.DeliveredBy, item.Notes
        ]).ToList();

        try
        {
            _tableExportService.Export(dialog.FileName, headers, rows, "Malzeme Hareketleri");
            statusLabel.Text = $"Hareket listesi dışa aktarıldı: {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hareket listesi dışa aktarılamadı:\n{ex.Message}", "Dışa Aktarma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearMovementFiltersButton_Click(object? sender, EventArgs e)
    {
        movementSearchTextBox.Clear();
        movementTypeFilterComboBox.SelectedItem = AllFilterText;
        ApplyMovementFilters();
    }

    private void MovementFilterChanged(object? sender, EventArgs e) => ApplyMovementFilters();

    private void StockOutButton_Click(object? sender, EventArgs e)
    {
        StartStockOut();
    }

    private void StartStockOut()
    {
        var stockItem = GetSelectedStockItem();
        if (stockItem is null)
        {
            MessageBox.Show("Önce bir stok kalemi seçin.", "Seçim Gerekli", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new StockOutDialog(stockItem, GetKnownLocations(), GetKnownPeople());
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _allMovements.Add(new MaterialMovementItem
        {
            MovementDate = dialog.MovementDate,
            MovementType = "Stok Çıkışı",
            ProductName = stockItem.ProductName,
            Unit = stockItem.Unit,
            Quantity = dialog.Quantity,
            FromLocation = stockItem.Location,
            ToLocation = dialog.ToLocation,
            ReceivedBy = dialog.ReceivedBy,
            DeliveredBy = dialog.DeliveredBy,
            Notes = dialog.Notes
        });

        SaveMovementsAndRefresh("Stok çıkışı kaydedildi.");
    }

    private void ExportStockButton_Click(object? sender, EventArgs e)
    {
        var items = GetSelectedStockItems();
        if (items.Count == 0)
        {
            items = _stockBindingSource.List.Cast<StoreStockItem>().ToList();
        }

        if (items.Count == 0)
        {
            MessageBox.Show("Dışa aktarılacak stok kaydı yok.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = CreateExcelSaveDialog("depo-stoklari");
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var headers = new[] { "STF No", "Ürün Adı", "Lokasyon", "Mevcut Stok", "Birim", "Son Hareket", "Son Teslim Alan" };
        var rows = items.Select(item => (IReadOnlyList<object?>)
        [
            item.StfNo, item.ProductName, item.Location, item.Quantity, item.Unit, item.LastMovementDate, item.LastReceivedBy
        ]).ToList();

        try
        {
            _tableExportService.Export(dialog.FileName, headers, rows, "Depo Stokları");
            statusLabel.Text = $"Depo stokları dışa aktarıldı: {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Depo stokları dışa aktarılamadı:\n{ex.Message}", "Dışa Aktarma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearStockFiltersButton_Click(object? sender, EventArgs e)
    {
        stockSearchTextBox.Clear();
        stockLocationFilterComboBox.SelectedItem = AllFilterText;
        ApplyStockFilters();
    }

    private void StockFilterChanged(object? sender, EventArgs e) => ApplyStockFilters();

    private void SaveStfAndRefresh(string statusMessage)
    {
        _allStfRequests = StfRequestRepository.Normalize(_allStfRequests);
        _stfRepository.Save(_allStfRequests);
        RefreshStfFilterValues();
        ApplyStfFilters();
        statusLabel.Text = statusMessage;
    }

    private void SaveMovementsAndRefresh(string statusMessage)
    {
        _allMovements = MaterialMovementRepository.Normalize(_allMovements);
        _movementRepository.Save(_allMovements);
        RefreshStockData();
        RefreshStockFilterValues();
        ApplyMovementFilters();
        ApplyStockFilters();
        statusLabel.Text = statusMessage;
    }

    private void SaveAllTrackingData(string statusMessage)
    {
        _allStfRequests = StfRequestRepository.Normalize(_allStfRequests);
        _allMovements = MaterialMovementRepository.Normalize(_allMovements);
        _stfRepository.Save(_allStfRequests);
        _movementRepository.Save(_allMovements);
        RefreshStfFilterValues();
        RefreshStockData();
        RefreshStockFilterValues();
        ApplyStfFilters();
        ApplyMovementFilters();
        ApplyStockFilters();
        statusLabel.Text = statusMessage;
    }

    private EquipmentItem? GetSelectedEquipmentItem() => equipmentGrid.CurrentRow?.DataBoundItem as EquipmentItem;
    private StfRequestItem? GetSelectedStfItem() => stfGrid.CurrentRow?.DataBoundItem as StfRequestItem;
    private MaterialMovementItem? GetSelectedMovementItem() => movementGrid.CurrentRow?.DataBoundItem as MaterialMovementItem;
    private StoreStockItem? GetSelectedStockItem() => stockGrid.CurrentRow?.DataBoundItem as StoreStockItem;

    private List<EquipmentItem> GetSelectedEquipmentItems()
    {
        return equipmentGrid.SelectedRows.Cast<DataGridViewRow>()
            .Select(row => row.DataBoundItem as EquipmentItem)
            .Where(item => item is not null)
            .Cast<EquipmentItem>()
            .DistinctBy(item => item.Id)
            .OrderBy(item => item.SequenceNo)
            .ToList();
    }

    private List<StfRequestItem> GetSelectedStfItems()
    {
        return stfGrid.SelectedRows.Cast<DataGridViewRow>()
            .Select(row => row.DataBoundItem as StfRequestItem)
            .Where(item => item is not null)
            .Cast<StfRequestItem>()
            .DistinctBy(item => item.Id)
            .OrderBy(item => item.RowNo)
            .ToList();
    }

    private List<MaterialMovementItem> GetSelectedMovementItems()
    {
        return movementGrid.SelectedRows.Cast<DataGridViewRow>()
            .Select(row => row.DataBoundItem as MaterialMovementItem)
            .Where(item => item is not null)
            .Cast<MaterialMovementItem>()
            .DistinctBy(item => item.Id)
            .OrderByDescending(item => item.MovementDate)
            .ToList();
    }

    private List<StoreStockItem> GetSelectedStockItems()
    {
        return stockGrid.SelectedRows.Cast<DataGridViewRow>()
            .Select(row => row.DataBoundItem as StoreStockItem)
            .Where(item => item is not null)
            .Cast<StoreStockItem>()
            .DistinctBy(item => $"{item.ProductName}|{item.Location}|{item.Unit}", StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item.ProductName)
            .ThenBy(item => item.Location)
            .ToList();
    }

    private EquipmentDialog CreateEquipmentDialog(EquipmentItem item)
    {
        return new EquipmentDialog(item, _allItems.Select(x => x.Group), _allItems.Select(x => x.Unit), _allItems.Select(x => x.Location), _allItems.Select(x => x.Owner));
    }

    private StfRequestDialog CreateStfDialog(StfRequestItem item)
    {
        return new StfRequestDialog(item, GetKnownPeople(), GetKnownDepartments(), GetKnownUnits());
    }

    private MaterialMovementDialog CreateMovementDialog(MaterialMovementItem item)
    {
        return new MaterialMovementDialog(item, _allStfRequests, GetKnownLocations(), GetKnownPeople(), GetKnownUnits());
    }

    private IEnumerable<string> GetKnownPeople()
    {
        return _allItems.Select(x => x.Owner)
            .Concat(_allStfRequests.Select(x => x.RequestedBy))
            .Concat(_allMovements.Select(x => x.ReceivedBy))
            .Concat(_allMovements.Select(x => x.DeliveredBy));
    }

    private IEnumerable<string> GetKnownDepartments()
    {
        return _allStfRequests.Select(x => x.Department)
            .Concat(_allItems.Select(x => x.Location));
    }

    private IEnumerable<string> GetKnownLocations()
    {
        return _allItems.Select(x => x.Location)
            .Concat(_allStfRequests.Select(x => x.Department))
            .Concat(_allMovements.Select(x => x.FromLocation))
            .Concat(_allMovements.Select(x => x.ToLocation));
    }

    private IEnumerable<string> GetKnownUnits()
    {
        return _allItems.Select(x => x.Unit)
            .Concat(_allStfRequests.Select(x => x.Unit))
            .Concat(_allMovements.Select(x => x.Unit));
    }

    private void UpdateSelectionSummary()
    {
        var item = GetSelectedEquipmentItem();
        if (item is null)
        {
            selectedNameValueLabel.Text = "-";
            selectedGroupValueLabel.Text = "-";
            selectedLocationValueLabel.Text = "-";
            selectedOwnerValueLabel.Text = "-";
            selectedSerialValueLabel.Text = "-";
            selectedModelValueLabel.Text = "-";
            return;
        }

        selectedNameValueLabel.Text = item.Name;
        selectedGroupValueLabel.Text = string.IsNullOrWhiteSpace(item.Group) ? "-" : item.Group;
        selectedLocationValueLabel.Text = string.IsNullOrWhiteSpace(item.Location) ? "-" : item.Location;
        selectedOwnerValueLabel.Text = string.IsNullOrWhiteSpace(item.Owner) ? "-" : item.Owner;
        selectedSerialValueLabel.Text = string.IsNullOrWhiteSpace(item.SerialNo) ? "-" : item.SerialNo;
        selectedModelValueLabel.Text = string.IsNullOrWhiteSpace(item.Model) ? "-" : item.Model;
    }

    private bool WarnAboutDuplicateSerial(EquipmentItem candidate, Guid? currentItemId)
    {
        var serial = candidate.SerialNo.Trim();
        if (string.IsNullOrWhiteSpace(serial))
        {
            return true;
        }

        var matches = _allItems
            .Where(x => x.Id != currentItemId && string.Equals(x.SerialNo.Trim(), serial, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
        {
            return true;
        }

        var message = $"'{serial}' seri numarası zaten {matches.Count} kayıtta var.\n\nYine de kaydetmek istiyor musun?";
        return MessageBox.Show(message, "Seri No Uyarısı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
    }

    private int GetDuplicateSerialCount(IEnumerable<EquipmentItem> items)
    {
        return items.Where(x => !string.IsNullOrWhiteSpace(x.SerialNo))
            .GroupBy(x => x.SerialNo.Trim(), StringComparer.OrdinalIgnoreCase)
            .Count(g => g.Count() > 1);
    }

    private void EquipmentGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || equipmentGrid.Rows[e.RowIndex].DataBoundItem is not EquipmentItem item)
        {
            return;
        }

        var duplicateSerial = !string.IsNullOrWhiteSpace(item.SerialNo)
            && _allItems.Count(x => string.Equals(x.SerialNo.Trim(), item.SerialNo.Trim(), StringComparison.OrdinalIgnoreCase)) > 1;

        var row = equipmentGrid.Rows[e.RowIndex];
        row.DefaultCellStyle.BackColor = duplicateSerial ? Color.FromArgb(255, 244, 230) : Color.White;
        row.DefaultCellStyle.ForeColor = Color.Black;
        row.DefaultCellStyle.SelectionBackColor = duplicateSerial ? Color.FromArgb(245, 196, 142) : Color.FromArgb(39, 110, 241);
        row.DefaultCellStyle.SelectionForeColor = Color.White;
    }

    private void StfGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || stfGrid.Rows[e.RowIndex].DataBoundItem is not StfRequestItem item)
        {
            return;
        }

        var row = stfGrid.Rows[e.RowIndex];
        row.DefaultCellStyle.BackColor = item.Status switch
        {
            "Bekliyor" => Color.FromArgb(255, 247, 230),
            "Kısmi Geldi" => Color.FromArgb(235, 244, 255),
            "Alındı" => Color.FromArgb(235, 250, 238),
            _ => Color.White
        };
    }

    private void MovementGrid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != 0 || movementGrid.Rows[e.RowIndex].DataBoundItem is not MaterialMovementItem item)
        {
            return;
        }

        e.Value = ResolveStfNo(item.RelatedRequestId);
        e.FormattingApplied = true;
    }

    private async Task SyncToSupabaseAsync()
    {
        if (!_supabaseSettings.IsConfigured)
        {
            return;
        }

        try
        {
            UseWaitCursor = true;
            await _supabaseSyncService.SyncAsync(_supabaseSettings, _allItems);
        }
        catch
        {
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private static void RebindFilter(ComboBox comboBox, IEnumerable<string> values)
    {
        var current = comboBox.SelectedItem?.ToString();
        var items = values.Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        comboBox.BeginUpdate();
        comboBox.Items.Clear();
        comboBox.Items.Add(AllFilterText);
        comboBox.Items.AddRange(items.Cast<object>().ToArray());
        comboBox.SelectedItem = comboBox.Items.Contains(current) ? current : AllFilterText;
        comboBox.EndUpdate();
    }

    private static bool Contains(string? value, string search)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Contains(search, StringComparison.OrdinalIgnoreCase);
    }

    private string ResolveStfNo(Guid? relatedRequestId)
    {
        if (!relatedRequestId.HasValue)
        {
            return "-";
        }

        return _allStfRequests.FirstOrDefault(x => x.Id == relatedRequestId.Value)?.StfNo ?? "-";
    }

    private MaterialMovementItem? FindLatestMovement(string productName, string unit, string location)
    {
        return _allMovements
            .Where(x =>
                string.Equals(x.ProductName, productName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Unit, unit, StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(x.ToLocation, location, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.FromLocation, location, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(x => x.MovementDate)
            .FirstOrDefault();
    }

    private SaveFileDialog CreateExcelSaveDialog(string filePrefix)
    {
        return new SaveFileDialog
        {
            Filter = "Excel Workbook|*.xlsx",
            InitialDirectory = AppPaths.ExportsDirectory,
            FileName = $"{filePrefix}-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx",
            Title = "Excel Olarak Dışa Aktar"
        };
    }

    private void AdjustSplitLayout()
    {
        if (mainSplitContainer.Width <= 0)
        {
            return;
        }

        mainSplitContainer.Panel1MinSize = 520;
        mainSplitContainer.Panel2MinSize = 280;

        var desiredSidebarWidth = 320;
        var maxSidebarWidth = Math.Max(mainSplitContainer.Panel2MinSize, mainSplitContainer.Width / 3);
        var sidebarWidth = Math.Min(desiredSidebarWidth, maxSidebarWidth);
        var newDistance = Math.Max(mainSplitContainer.Panel1MinSize, mainSplitContainer.Width - sidebarWidth - mainSplitContainer.SplitterWidth);
        var maxDistance = Math.Max(mainSplitContainer.Panel1MinSize, mainSplitContainer.Width - mainSplitContainer.Panel2MinSize - mainSplitContainer.SplitterWidth);

        mainSplitContainer.SplitterDistance = Math.Min(newDistance, maxDistance);
    }

    private void SetWindowIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "app.ico");
        if (File.Exists(iconPath))
        {
            Icon = new Icon(iconPath);
        }
    }
}
