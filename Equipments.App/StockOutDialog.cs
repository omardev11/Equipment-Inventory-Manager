using Equipments.App.Models;

namespace Equipments.App;

public sealed class StockOutDialog : Form
{
    private readonly NumericUpDown _quantityNumeric = new();
    private readonly DateTimePicker _datePicker = new();
    private readonly ComboBox _toLocationComboBox = new();
    private readonly ComboBox _receivedByComboBox = new();
    private readonly ComboBox _deliveredByComboBox = new();
    private readonly TextBox _notesTextBox = new();

    public decimal Quantity => _quantityNumeric.Value;
    public DateTime MovementDate => _datePicker.Value.Date;
    public string ToLocation => _toLocationComboBox.Text.Trim();
    public string ReceivedBy => _receivedByComboBox.Text.Trim();
    public string DeliveredBy => _deliveredByComboBox.Text.Trim();
    public string Notes => _notesTextBox.Text.Trim();

    public StockOutDialog(StoreStockItem stockItem, IEnumerable<string> locationOptions, IEnumerable<string> peopleOptions)
    {
        InitializeComponent(stockItem);
        BindValues(_toLocationComboBox, locationOptions);
        BindValues(_receivedByComboBox, peopleOptions);
        BindValues(_deliveredByComboBox, peopleOptions);
    }

    private void InitializeComponent(StoreStockItem stockItem)
    {
        Text = "Stok Çıkışı";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(580, 390);
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 7
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var infoLabel = new Label
        {
            Text = $"Ürün: {stockItem.ProductName}\nLokasyon: {stockItem.Location}\nMevcut: {stockItem.Quantity:0.##} {stockItem.Unit}",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(28, 45, 76)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        layout.Controls.Add(infoLabel, 0, 0);
        layout.SetColumnSpan(infoLabel, 2);

        _quantityNumeric.DecimalPlaces = 2;
        _quantityNumeric.Minimum = 0.01M;
        _quantityNumeric.Maximum = Math.Max(0.01M, stockItem.Quantity);
        _quantityNumeric.Value = Math.Min(Math.Max(0.01M, stockItem.Quantity), 1);
        _quantityNumeric.Width = 160;

        _datePicker.Format = DateTimePickerFormat.Short;
        _datePicker.Width = 140;

        ConfigureCombo(_toLocationComboBox);
        ConfigureCombo(_receivedByComboBox);
        ConfigureCombo(_deliveredByComboBox);
        _notesTextBox.Dock = DockStyle.Fill;
        _notesTextBox.Multiline = true;
        _notesTextBox.Height = 84;

        AddField(layout, 1, "Çıkış Miktarı", _quantityNumeric);
        AddField(layout, 2, "Tarih", _datePicker);
        AddField(layout, 3, "Nereye", _toLocationComboBox);
        AddField(layout, 4, "Teslim Alan", _receivedByComboBox);
        AddField(layout, 5, "Teslim Eden", _deliveredByComboBox);
        AddField(layout, 6, "Açıklama", _notesTextBox, 96);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 48
        };
        var saveButton = new Button { Text = "Kaydet", Width = 110, Height = 34, DialogResult = DialogResult.OK };
        saveButton.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(ToLocation))
            {
                MessageBox.Show("Nereye alanı zorunludur.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        };
        var cancelButton = new Button { Text = "İptal", Width = 110, Height = 34, DialogResult = DialogResult.Cancel };
        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);

        Controls.Add(layout);
        Controls.Add(buttonPanel);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static void AddField(TableLayoutPanel layout, int rowIndex, string label, Control control, int rowHeight = 40)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
        layout.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, rowIndex);
        layout.Controls.Add(control, 1, rowIndex);
    }

    private static void ConfigureCombo(ComboBox comboBox)
    {
        comboBox.Dock = DockStyle.Fill;
        comboBox.DropDownStyle = ComboBoxStyle.DropDown;
        comboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
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
}
