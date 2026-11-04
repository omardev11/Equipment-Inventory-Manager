using Equipments.App.Models;

namespace Equipments.App;

public sealed class ReceiveStockDialog : Form
{
    private readonly NumericUpDown _quantityNumeric = new();
    private readonly DateTimePicker _datePicker = new();
    private readonly TextBox _fromTextBox = new();
    private readonly ComboBox _toComboBox = new();
    private readonly ComboBox _receivedByComboBox = new();
    private readonly TextBox _notesTextBox = new();

    public decimal Quantity => _quantityNumeric.Value;
    public DateTime ReceiveDate => _datePicker.Value.Date;
    public string FromLocation => _fromTextBox.Text.Trim();
    public string ToLocation => _toComboBox.Text.Trim();
    public string ReceivedBy => _receivedByComboBox.Text.Trim();
    public string Notes => _notesTextBox.Text.Trim();

    public ReceiveStockDialog(StfRequestItem request, IEnumerable<string> locationOptions, IEnumerable<string> peopleOptions)
    {
        InitializeComponent(request);
        BindValues(_toComboBox, locationOptions);
        BindValues(_receivedByComboBox, peopleOptions);
    }

    private void InitializeComponent(StfRequestItem request)
    {
        Text = "Gelen Malzeme Kaydı";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(580, 430);
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 8
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var infoLabel = new Label
        {
            Text = $"Ürün: {request.ProductName}\nTalep: {request.RequestedQuantity:0.##} {request.Unit}  |  Kalan: {request.RemainingQuantity:0.##} {request.Unit}",
            Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(28, 45, 76)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.Controls.Add(infoLabel, 0, 0);
        layout.SetColumnSpan(infoLabel, 2);

        _quantityNumeric.DecimalPlaces = 2;
        _quantityNumeric.Minimum = 0.01M;
        _quantityNumeric.Maximum = 1000000;
        _quantityNumeric.Value = Math.Max(0.01M, request.RemainingQuantity > 0 ? request.RemainingQuantity : 1);
        _quantityNumeric.Width = 160;

        _datePicker.Format = DateTimePickerFormat.Short;
        _datePicker.Width = 140;

        _fromTextBox.Dock = DockStyle.Fill;
        _fromTextBox.Text = "Tedarikçi";

        ConfigureCombo(_toComboBox);
        _toComboBox.Text = "Depo";

        ConfigureCombo(_receivedByComboBox);

        _notesTextBox.Dock = DockStyle.Fill;
        _notesTextBox.Multiline = true;
        _notesTextBox.ScrollBars = ScrollBars.Vertical;
        _notesTextBox.Height = 92;

        AddField(layout, 1, "Gelen Miktar", _quantityNumeric);
        AddField(layout, 2, "Geliş Tarihi", _datePicker);
        AddField(layout, 3, "Nereden", _fromTextBox);
        AddField(layout, 4, "Nereye", _toComboBox);
        AddField(layout, 5, "Teslim Alan", _receivedByComboBox);
        AddField(layout, 6, "Açıklama", _notesTextBox, 110);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        var saveButton = new Button
        {
            Text = "Kaydet",
            Width = 110,
            Height = 34,
            DialogResult = DialogResult.OK
        };
        saveButton.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(ToLocation))
            {
                MessageBox.Show("Nereye alanı zorunludur.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
            }
        };

        var cancelButton = new Button
        {
            Text = "İptal",
            Width = 110,
            Height = 34,
            DialogResult = DialogResult.Cancel
        };

        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);

        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.Controls.Add(buttonPanel, 0, 7);
        layout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(layout);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
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
