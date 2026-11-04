namespace Equipments.App;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private TabControl mainTabControl = null!;

    private DataGridView equipmentGrid = null!;
    private TextBox searchTextBox = null!;
    private TextBox ownerFilterTextBox = null!;
    private ComboBox groupFilterComboBox = null!;
    private ComboBox locationFilterComboBox = null!;
    private Label totalCountValueLabel = null!;
    private Label missingModelValueLabel = null!;
    private Label missingSerialValueLabel = null!;
    private Label duplicateSerialValueLabel = null!;
    private ListBox topGroupsListBox = null!;
    private ListBox topLocationsListBox = null!;
    private Label selectedNameValueLabel = null!;
    private Label selectedGroupValueLabel = null!;
    private Label selectedLocationValueLabel = null!;
    private Label selectedOwnerValueLabel = null!;
    private Label selectedSerialValueLabel = null!;
    private Label selectedModelValueLabel = null!;
    private SplitContainer mainSplitContainer = null!;

    private DataGridView stfGrid = null!;
    private TextBox stfSearchTextBox = null!;
    private ComboBox stfDepartmentFilterComboBox = null!;
    private ComboBox stfStatusFilterComboBox = null!;
    private Label stfTotalValueLabel = null!;
    private Label stfWaitingValueLabel = null!;
    private Label stfPartialValueLabel = null!;
    private Label stfCompletedValueLabel = null!;

    private DataGridView movementGrid = null!;
    private TextBox movementSearchTextBox = null!;
    private ComboBox movementTypeFilterComboBox = null!;
    private Label movementTotalValueLabel = null!;
    private Label movementInboundValueLabel = null!;
    private Label movementOutboundValueLabel = null!;

    private DataGridView stockGrid = null!;
    private TextBox stockSearchTextBox = null!;
    private ComboBox stockLocationFilterComboBox = null!;
    private Label stockLineCountValueLabel = null!;
    private Label stockTotalQuantityValueLabel = null!;
    private Label stockLowCountValueLabel = null!;

    private ToolStripStatusLabel statusLabel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        Text = "Ofis Bilgi İşlem Ekipmanları";
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1280, 760);
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 9F);

        var headerPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 96,
            Padding = new Padding(18, 16, 18, 12),
            BackColor = Color.White
        };

        var headerTitle = new Label
        {
            Dock = DockStyle.Top,
            Height = 38,
            Text = "Ekipman ve STF Yönetim Paneli",
            Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(28, 45, 76)
        };

        var headerSubtitle = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Ekipmanları, STF taleplerini ve malzeme hareketlerini tek program içinde takip et.",
            ForeColor = Color.FromArgb(92, 107, 128)
        };

        headerPanel.Controls.Add(headerSubtitle);
        headerPanel.Controls.Add(headerTitle);

        mainTabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Padding = new Point(18, 6)
        };

        var equipmentTab = new TabPage("Ekipman Listesi") { BackColor = Color.FromArgb(245, 247, 250) };
        var stfTab = new TabPage("STF Takip") { BackColor = Color.FromArgb(245, 247, 250) };
        var movementTab = new TabPage("Malzeme Hareketleri") { BackColor = Color.FromArgb(245, 247, 250) };
        var stockTab = new TabPage("Depo Stokları") { BackColor = Color.FromArgb(245, 247, 250) };

        BuildEquipmentTab(equipmentTab);
        BuildStfTab(stfTab);
        BuildMovementTab(movementTab);
        BuildStockTab(stockTab);

        mainTabControl.TabPages.Add(equipmentTab);
        mainTabControl.TabPages.Add(stfTab);
        mainTabControl.TabPages.Add(movementTab);
        mainTabControl.TabPages.Add(stockTab);

        var statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel("Hazır");
        statusStrip.Items.Add(statusLabel);

        Controls.Add(mainTabControl);
        Controls.Add(headerPanel);
        Controls.Add(statusStrip);

        Load += MainForm_Load;
        Shown += MainForm_Shown;
    }

    private void BuildEquipmentTab(TabPage tab)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var filtersCard = CreateCardPanel();
        filtersCard.Padding = new Padding(14);
        var filtersLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 8,
            RowCount = 2
        };
        filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
        filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95));
        filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 85));
        filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
        filtersLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        filtersLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        searchTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Ad, model, seri no, lokasyon veya sorumlu ara..." };
        ownerFilterTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Sorumlu ara..." };
        groupFilterComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        locationFilterComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        groupFilterComboBox.Items.Add("Tümü");
        locationFilterComboBox.Items.Add("Tümü");
        groupFilterComboBox.SelectedIndex = 0;
        locationFilterComboBox.SelectedIndex = 0;

        searchTextBox.TextChanged += SearchTextBox_TextChanged;
        ownerFilterTextBox.TextChanged += OwnerFilterTextBox_TextChanged;
        groupFilterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;
        locationFilterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        var addButton = CreateButton("Yeni", AddButton_Click);
        var editButton = CreateButton("Düzenle", EditButton_Click);
        var deleteButton = CreateButton("Sil", DeleteButton_Click);
        var exportButton = CreateButton("Excel'e Aktar", ExportButton_Click);
        var saveButton = CreateButton("Kaydet", SaveButton_Click);
        var clearButton = CreateButton("Filtre Temizle", ClearFiltersButton_Click);
        buttonPanel.Controls.Add(addButton);
        buttonPanel.Controls.Add(editButton);
        buttonPanel.Controls.Add(deleteButton);
        buttonPanel.Controls.Add(exportButton);
        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(clearButton);

        filtersLayout.Controls.Add(buttonPanel, 0, 0);
        filtersLayout.SetColumnSpan(buttonPanel, 8);
        filtersLayout.Controls.Add(new Label { Text = "Arama", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        filtersLayout.Controls.Add(searchTextBox, 1, 1);
        filtersLayout.Controls.Add(new Label { Text = "Malz. Grubu", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 1);
        filtersLayout.Controls.Add(groupFilterComboBox, 3, 1);
        filtersLayout.Controls.Add(new Label { Text = "Lokasyon", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 4, 1);
        filtersLayout.Controls.Add(locationFilterComboBox, 5, 1);
        filtersLayout.Controls.Add(new Label { Text = "Sorumlu", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 6, 1);
        filtersLayout.Controls.Add(ownerFilterTextBox, 7, 1);
        filtersCard.Controls.Add(filtersLayout);

        var summaryCard = CreateCardPanel();
        summaryCard.Padding = new Padding(14, 10, 14, 10);
        var summaryLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        totalCountValueLabel = CreateKpi(summaryLayout, "Toplam Kayıt");
        missingModelValueLabel = CreateKpi(summaryLayout, "Model Eksik");
        missingSerialValueLabel = CreateKpi(summaryLayout, "Seri No Eksik");
        duplicateSerialValueLabel = CreateKpi(summaryLayout, "Çakışan Seri No");
        summaryCard.Controls.Add(summaryLayout);

        mainSplitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel2,
            Padding = new Padding(0)
        };
        mainSplitContainer.SplitterWidth = 8;

        equipmentGrid = CreateStandardGrid();
        equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SequenceNo", HeaderText = "Sıra No", FillWeight = 50, MinimumWidth = 60 });
        equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Group", HeaderText = "Malz. Grubu", FillWeight = 95, MinimumWidth = 110 });
        equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "Malzeme Adı", FillWeight = 240, MinimumWidth = 260, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } });
        equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Unit", HeaderText = "Birim", FillWeight = 65, MinimumWidth = 70 });
        equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Miktar", FillWeight = 65, MinimumWidth = 70 });
        equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Model", HeaderText = "Model", FillWeight = 120, MinimumWidth = 140, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } });
        equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SerialNo", HeaderText = "Seri No", FillWeight = 120, MinimumWidth = 130 });
        equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Location", HeaderText = "Lokasyon", FillWeight = 115, MinimumWidth = 120, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } });
        equipmentGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Owner", HeaderText = "Sorumlu", FillWeight = 115, MinimumWidth = 130, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } });
        mainSplitContainer.Panel1.Controls.Add(equipmentGrid);

        var sidebar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var detailsGroup = CreateGroupBox("Seçili Kayıt");
        var detailsTable = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(10)
        };
        for (var i = 0; i < 6; i++)
        {
            detailsTable.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66F));
        }
        selectedNameValueLabel = AddDetailRow(detailsTable, "Malzeme Adı");
        selectedGroupValueLabel = AddDetailRow(detailsTable, "Grup");
        selectedLocationValueLabel = AddDetailRow(detailsTable, "Lokasyon");
        selectedOwnerValueLabel = AddDetailRow(detailsTable, "Sorumlu");
        selectedSerialValueLabel = AddDetailRow(detailsTable, "Seri No");
        selectedModelValueLabel = AddDetailRow(detailsTable, "Model");
        detailsGroup.Controls.Add(detailsTable);

        var groupsGroup = CreateGroupBox("En Yoğun Gruplar");
        topGroupsListBox = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, IntegralHeight = false };
        groupsGroup.Controls.Add(topGroupsListBox);

        var locationsGroup = CreateGroupBox("En Yoğun Lokasyonlar");
        topLocationsListBox = new ListBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, IntegralHeight = false };
        locationsGroup.Controls.Add(topLocationsListBox);

        sidebar.Controls.Add(detailsGroup, 0, 0);
        sidebar.Controls.Add(groupsGroup, 0, 1);
        sidebar.Controls.Add(locationsGroup, 0, 2);
        mainSplitContainer.Panel2.Controls.Add(sidebar);

        root.Controls.Add(filtersCard, 0, 0);
        root.Controls.Add(summaryCard, 0, 1);
        root.Controls.Add(mainSplitContainer, 0, 2);
        tab.Controls.Add(root);
    }

    private void BuildStfTab(TabPage tab)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var filterCard = CreateCardPanel();
        filterCard.Padding = new Padding(14);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        buttonPanel.Controls.Add(CreateButton("Yeni Talep", AddStfButton_Click));
        buttonPanel.Controls.Add(CreateButton("Düzenle", EditStfButton_Click));
        buttonPanel.Controls.Add(CreateButton("Sil", DeleteStfButton_Click));
        buttonPanel.Controls.Add(CreateButton("Geldi Ekle", ReceiveStfButton_Click));
        buttonPanel.Controls.Add(CreateButton("Excel'e Aktar", ExportStfButton_Click));
        buttonPanel.Controls.Add(CreateButton("Filtre Temizle", ClearStfFiltersButton_Click));
        layout.Controls.Add(buttonPanel, 0, 0);
        layout.SetColumnSpan(buttonPanel, 6);

        stfSearchTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "STF no, ürün adı, talep eden veya bölüm ara..." };
        stfDepartmentFilterComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        stfStatusFilterComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        stfDepartmentFilterComboBox.Items.Add("Tümü");
        stfStatusFilterComboBox.Items.AddRange(["Tümü", "Bekliyor", "Kısmi Geldi", "Alındı"]);
        stfDepartmentFilterComboBox.SelectedIndex = 0;
        stfStatusFilterComboBox.SelectedIndex = 0;
        stfSearchTextBox.TextChanged += StfFilterChanged;
        stfDepartmentFilterComboBox.SelectedIndexChanged += StfFilterChanged;
        stfStatusFilterComboBox.SelectedIndexChanged += StfFilterChanged;

        layout.Controls.Add(new Label { Text = "Arama", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        layout.Controls.Add(stfSearchTextBox, 1, 1);
        layout.Controls.Add(new Label { Text = "Bölüm", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 1);
        layout.Controls.Add(stfDepartmentFilterComboBox, 3, 1);
        layout.Controls.Add(new Label { Text = "Durum", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 4, 1);
        layout.Controls.Add(stfStatusFilterComboBox, 5, 1);
        filterCard.Controls.Add(layout);

        var summaryCard = CreateCardPanel();
        summaryCard.Padding = new Padding(14, 10, 14, 10);
        var summaryLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        stfTotalValueLabel = CreateKpi(summaryLayout, "Toplam Talep");
        stfWaitingValueLabel = CreateKpi(summaryLayout, "Bekleyen");
        stfPartialValueLabel = CreateKpi(summaryLayout, "Kısmi Geldi");
        stfCompletedValueLabel = CreateKpi(summaryLayout, "Alındı");
        summaryCard.Controls.Add(summaryLayout);

        stfGrid = CreateStandardGrid();
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RowNo", HeaderText = "No", FillWeight = 45, MinimumWidth = 55 });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StfNo", HeaderText = "STF No", FillWeight = 80, MinimumWidth = 90 });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RequestDate", HeaderText = "Talep Tarihi", FillWeight = 80, MinimumWidth = 95, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" } });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RequestedBy", HeaderText = "Talep Eden", FillWeight = 100, MinimumWidth = 110 });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Department", HeaderText = "Bölüm", FillWeight = 95, MinimumWidth = 110 });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Ürün Adı", FillWeight = 220, MinimumWidth = 240, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Unit", HeaderText = "Birim", FillWeight = 55, MinimumWidth = 70 });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RequestedQuantity", HeaderText = "Talep", FillWeight = 65, MinimumWidth = 75, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##" } });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReceivedQuantity", HeaderText = "Gelen", FillWeight = 65, MinimumWidth = 75, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##" } });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "RemainingQuantity", HeaderText = "Kalan", FillWeight = 65, MinimumWidth = 75, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##" } });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Durum", FillWeight = 75, MinimumWidth = 85 });
        stfGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Açıklama", FillWeight = 180, MinimumWidth = 180, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } });

        root.Controls.Add(filterCard, 0, 0);
        root.Controls.Add(summaryCard, 0, 1);
        root.Controls.Add(stfGrid, 0, 2);
        tab.Controls.Add(root);
    }

    private void BuildMovementTab(TabPage tab)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var filterCard = CreateCardPanel();
        filterCard.Padding = new Padding(14);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        buttonPanel.Controls.Add(CreateButton("Yeni Hareket", AddMovementButton_Click));
        buttonPanel.Controls.Add(CreateButton("Düzenle", EditMovementButton_Click));
        buttonPanel.Controls.Add(CreateButton("Sil", DeleteMovementButton_Click));
        buttonPanel.Controls.Add(CreateButton("Excel'e Aktar", ExportMovementsButton_Click));
        buttonPanel.Controls.Add(CreateButton("Filtre Temizle", ClearMovementFiltersButton_Click));
        layout.Controls.Add(buttonPanel, 0, 0);
        layout.SetColumnSpan(buttonPanel, 4);

        movementSearchTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Ürün, alan, veren, nereden veya nereye ara..." };
        movementTypeFilterComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        movementTypeFilterComboBox.Items.AddRange(["Tümü", "Teslim", "Depoya Giriş", "Transfer", "İade"]);
        movementTypeFilterComboBox.SelectedIndex = 0;
        movementSearchTextBox.TextChanged += MovementFilterChanged;
        movementTypeFilterComboBox.SelectedIndexChanged += MovementFilterChanged;

        layout.Controls.Add(new Label { Text = "Arama", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        layout.Controls.Add(movementSearchTextBox, 1, 1);
        layout.Controls.Add(new Label { Text = "Tip", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 1);
        layout.Controls.Add(movementTypeFilterComboBox, 3, 1);
        filterCard.Controls.Add(layout);

        var summaryCard = CreateCardPanel();
        summaryCard.Padding = new Padding(14, 10, 14, 10);
        var summaryLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        movementTotalValueLabel = CreateKpi(summaryLayout, "Toplam Hareket");
        movementInboundValueLabel = CreateKpi(summaryLayout, "Depoya Giriş");
        movementOutboundValueLabel = CreateKpi(summaryLayout, "Teslim / Transfer");
        summaryCard.Controls.Add(summaryLayout);

        movementGrid = CreateStandardGrid();
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Tag", HeaderText = "STF No", FillWeight = 75, MinimumWidth = 90 });
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MovementDate", HeaderText = "Tarih", FillWeight = 70, MinimumWidth = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" } });
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "MovementType", HeaderText = "Hareket Tipi", FillWeight = 90, MinimumWidth = 95 });
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Ürün Adı", FillWeight = 190, MinimumWidth = 220, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } });
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Miktar", FillWeight = 60, MinimumWidth = 70, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##" } });
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Unit", HeaderText = "Birim", FillWeight = 55, MinimumWidth = 70 });
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FromLocation", HeaderText = "Nereden", FillWeight = 105, MinimumWidth = 110 });
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ToLocation", HeaderText = "Nereye", FillWeight = 105, MinimumWidth = 110 });
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReceivedBy", HeaderText = "Teslim Alan", FillWeight = 110, MinimumWidth = 120 });
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DeliveredBy", HeaderText = "Teslim Eden", FillWeight = 110, MinimumWidth = 120 });
        movementGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Notes", HeaderText = "Açıklama", FillWeight = 160, MinimumWidth = 170, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } });

        root.Controls.Add(filterCard, 0, 0);
        root.Controls.Add(summaryCard, 0, 1);
        root.Controls.Add(movementGrid, 0, 2);
        tab.Controls.Add(root);
    }

    private void BuildStockTab(TabPage tab)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var filterCard = CreateCardPanel();
        filterCard.Padding = new Padding(14);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 2
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        buttonPanel.Controls.Add(CreateButton("Stok Çıkışı", StockOutButton_Click));
        buttonPanel.Controls.Add(CreateButton("Excel'e Aktar", ExportStockButton_Click));
        buttonPanel.Controls.Add(CreateButton("Filtre Temizle", ClearStockFiltersButton_Click));
        layout.Controls.Add(buttonPanel, 0, 0);
        layout.SetColumnSpan(buttonPanel, 4);

        stockSearchTextBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "Ürün adı veya lokasyon ara..." };
        stockLocationFilterComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        stockLocationFilterComboBox.Items.Add("Tümü");
        stockLocationFilterComboBox.SelectedIndex = 0;
        stockSearchTextBox.TextChanged += StockFilterChanged;
        stockLocationFilterComboBox.SelectedIndexChanged += StockFilterChanged;

        layout.Controls.Add(new Label { Text = "Arama", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        layout.Controls.Add(stockSearchTextBox, 1, 1);
        layout.Controls.Add(new Label { Text = "Lokasyon", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 2, 1);
        layout.Controls.Add(stockLocationFilterComboBox, 3, 1);
        filterCard.Controls.Add(layout);

        var summaryCard = CreateCardPanel();
        summaryCard.Padding = new Padding(14, 10, 14, 10);
        var summaryLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        stockLineCountValueLabel = CreateKpi(summaryLayout, "Stok Kalemi");
        stockTotalQuantityValueLabel = CreateKpi(summaryLayout, "Toplam Miktar");
        stockLowCountValueLabel = CreateKpi(summaryLayout, "Azalan Stok");
        summaryCard.Controls.Add(summaryLayout);

        stockGrid = CreateStandardGrid();
        stockGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StfNo", HeaderText = "STF No", FillWeight = 75, MinimumWidth = 90 });
        stockGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Ürün Adı", FillWeight = 220, MinimumWidth = 240, DefaultCellStyle = new DataGridViewCellStyle { WrapMode = DataGridViewTriState.True } });
        stockGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Location", HeaderText = "Lokasyon", FillWeight = 110, MinimumWidth = 120 });
        stockGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Mevcut Stok", FillWeight = 80, MinimumWidth = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "0.##" } });
        stockGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Unit", HeaderText = "Birim", FillWeight = 60, MinimumWidth = 70 });
        stockGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LastMovementDate", HeaderText = "Son Hareket", FillWeight = 80, MinimumWidth = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" } });
        stockGrid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "LastReceivedBy", HeaderText = "Son Teslim Alan", FillWeight = 110, MinimumWidth = 120 });

        root.Controls.Add(filterCard, 0, 0);
        root.Controls.Add(summaryCard, 0, 1);
        root.Controls.Add(stockGrid, 0, 2);
        tab.Controls.Add(root);
    }

    private static DataGridView CreateStandardGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            MultiSelect = true,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            BorderStyle = BorderStyle.None
        };
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 45, 76);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        grid.ColumnHeadersHeight = 36;
        grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        grid.RowTemplate.Height = 32;
        return grid;
    }

    private static Panel CreateCardPanel()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
    }

    private static Button CreateButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            Width = 138,
            Height = 34,
            Margin = new Padding(0, 0, 8, 0)
        };
        button.Click += onClick;
        return button;
    }

    private static Label CreateKpi(FlowLayoutPanel parent, string title)
    {
        var panel = new Panel
        {
            Width = 190,
            Height = 50,
            Margin = new Padding(0, 0, 12, 0),
            BackColor = Color.White
        };

        var titleLabel = new Label
        {
            Text = title,
            Location = new Point(12, 7),
            AutoSize = true
        };

        var valueLabel = new Label
        {
            Text = "0",
            Location = new Point(12, 24),
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(28, 45, 76)
        };

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(valueLabel);
        parent.Controls.Add(panel);
        return valueLabel;
    }

    private static GroupBox CreateGroupBox(string text)
    {
        return new GroupBox
        {
            Text = text,
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            BackColor = Color.White,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold)
        };
    }

    private static Label AddDetailRow(TableLayoutPanel parent, string title)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 46
        };

        var titleLabel = new Label
        {
            Text = $"{title}:",
            Width = 90,
            Dock = DockStyle.Left,
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.TopLeft
        };

        var valueLabel = new Label
        {
            Text = "-",
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            ForeColor = Color.FromArgb(28, 45, 76),
            MaximumSize = new Size(0, 40)
        };

        panel.Controls.Add(valueLabel);
        panel.Controls.Add(titleLabel);
        parent.Controls.Add(panel);
        return valueLabel;
    }
}
