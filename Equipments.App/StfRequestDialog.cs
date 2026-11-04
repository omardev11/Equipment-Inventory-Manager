using Equipments.App.Models;
using Equipments.App.Services;

namespace Equipments.App;

public sealed class StfRequestDialog : Form
{
    private readonly TextBox _stfNoTextBox = new();
    private readonly DateTimePicker _requestDatePicker = new();
    private readonly ComboBox _requestedByComboBox = new();
    private readonly ComboBox _departmentComboBox = new();
    private readonly TextBox _productNameTextBox = new();
    private readonly ComboBox _unitComboBox = new();
    private readonly NumericUpDown _requestedQuantityNumeric = new();
    private readonly NumericUpDown _receivedQuantityNumeric = new();
    private readonly ComboBox _statusComboBox = new();
    private readonly TextBox _descriptionTextBox = new();

    public StfRequestItem Item { get; }

    public StfRequestDialog(
        StfRequestItem item,
        IEnumerable<string> requestedByOptions,
        IEnumerable<string> departmentOptions,
        IEnumerable<string> unitOptions)
    {
        Item = item.Clone();
        InitializeComponent();
        SetWindowIcon();
        BindValues(_requestedByComboBox, requestedByOptions);
        BindValues(_departmentComboBox, departmentOptions);
        BindValues(_unitComboBox, unitOptions);
        LoadItem();
    }

    private void InitializeComponent()
    {
        Text = "STF Talep Kaydı";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(620, 560);
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 11
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        ConfigureCombo(_requestedByComboBox);
        ConfigureCombo(_departmentComboBox);
        ConfigureCombo(_unitComboBox);
        ConfigureCombo(_statusComboBox, dropDownList: true);
        _statusComboBox.Items.AddRange(["Bekliyor", "Kısmi Geldi", "Alındı"]);
        _statusComboBox.SelectedIndex = 0;

        ConfigureText(_productNameTextBox, multiline: true, height: 70);
        ConfigureText(_descriptionTextBox, multiline: true, height: 90);
        ConfigureText(_stfNoTextBox);

        _requestDatePicker.Format = DateTimePickerFormat.Short;
        _requestDatePicker.Dock = DockStyle.Left;
        _requestDatePicker.Width = 140;

        ConfigureNumeric(_requestedQuantityNumeric);
        ConfigureNumeric(_receivedQuantityNumeric);

        AddField(layout, 0, "STF No", _stfNoTextBox);
        AddField(layout, 1, "Talep Tarihi", _requestDatePicker);
        AddField(layout, 2, "Talep Eden", _requestedByComboBox);
        AddField(layout, 3, "Bölüm", _departmentComboBox);
        AddField(layout, 4, "Ürün Adı", _productNameTextBox, 84);
        AddField(layout, 5, "Birim", _unitComboBox);
        AddField(layout, 6, "Talep Miktarı", _requestedQuantityNumeric);
        AddField(layout, 7, "Gelen Miktar", _receivedQuantityNumeric);
        AddField(layout, 8, "Durum", _statusComboBox);
        AddField(layout, 9, "Açıklama", _descriptionTextBox, 104);

        var buttonPanel = CreateButtons(() => SaveItem());
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        layout.Controls.Add(buttonPanel, 0, 10);
        layout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(layout);
    }

    private void LoadItem()
    {
        _stfNoTextBox.Text = Item.StfNo;
        _requestDatePicker.Value = Item.RequestDate == default ? DateTime.Today : Item.RequestDate;
        _requestedByComboBox.Text = Item.RequestedBy;
        _departmentComboBox.Text = Item.Department;
        _productNameTextBox.Text = Item.ProductName;
        _unitComboBox.Text = Item.Unit;
        _requestedQuantityNumeric.Value = Convert.ToDecimal(Math.Max(1, Item.RequestedQuantity));
        _receivedQuantityNumeric.Value = Convert.ToDecimal(Math.Max(0, Item.ReceivedQuantity));
        _statusComboBox.Text = string.IsNullOrWhiteSpace(Item.Status) ? "Bekliyor" : Item.Status;
        _descriptionTextBox.Text = Item.Description;
    }

    private void SaveItem()
    {
        Item.StfNo = _stfNoTextBox.Text.Trim();
        Item.RequestDate = _requestDatePicker.Value.Date;
        Item.RequestedBy = _requestedByComboBox.Text.Trim();
        Item.Department = _departmentComboBox.Text.Trim();
        Item.ProductName = _productNameTextBox.Text.Trim();
        Item.Unit = string.IsNullOrWhiteSpace(_unitComboBox.Text) ? "Adet" : _unitComboBox.Text.Trim();
        Item.RequestedQuantity = _requestedQuantityNumeric.Value;
        Item.ReceivedQuantity = _receivedQuantityNumeric.Value;
        Item.Status = StfRequestRepository.ComputeStatus(Item);
        Item.Description = _descriptionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(Item.ProductName) || string.IsNullOrWhiteSpace(Item.RequestedBy))
        {
            MessageBox.Show("Ürün adı ve talep eden zorunludur.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }
    }

    private static void ConfigureText(TextBox textBox, bool multiline = false, int height = 28)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.Multiline = multiline;
        textBox.Height = height;
    }

    private static void ConfigureCombo(ComboBox comboBox, bool dropDownList = false)
    {
        comboBox.Dock = DockStyle.Fill;
        comboBox.DropDownStyle = dropDownList ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown;
        if (dropDownList)
        {
            comboBox.AutoCompleteMode = AutoCompleteMode.None;
        }
        else
        {
            comboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
        }
    }

    private static void ConfigureNumeric(NumericUpDown numeric)
    {
        numeric.DecimalPlaces = 2;
        numeric.Minimum = 0;
        numeric.Maximum = 1000000;
        numeric.Dock = DockStyle.Left;
        numeric.Width = 160;
    }

    private static void AddField(TableLayoutPanel layout, int rowIndex, string label, Control control, int rowHeight = 40)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
        layout.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, rowIndex);
        layout.Controls.Add(control, 1, rowIndex);
    }

    private static FlowLayoutPanel CreateButtons(Action saveAction)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };

        var saveButton = new Button { Text = "Kaydet", Width = 110, Height = 34, DialogResult = DialogResult.OK };
        saveButton.Click += (_, _) => saveAction();
        var cancelButton = new Button { Text = "İptal", Width = 110, Height = 34, DialogResult = DialogResult.Cancel };

        panel.Controls.Add(saveButton);
        panel.Controls.Add(cancelButton);
        return panel;
    }

    private static void BindValues(ComboBox comboBox, IEnumerable<string> values)
    {
        comboBox.Items.Clear();
        comboBox.Items.AddRange(values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .Cast<object>()
            .ToArray());
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
