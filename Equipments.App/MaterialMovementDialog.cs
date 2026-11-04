using Equipments.App.Models;

namespace Equipments.App;

public sealed class MaterialMovementDialog : Form
{
    private readonly ComboBox _requestComboBox = new();
    private readonly ComboBox _movementTypeComboBox = new();
    private readonly TextBox _productNameTextBox = new();
    private readonly ComboBox _unitComboBox = new();
    private readonly NumericUpDown _quantityNumeric = new();
    private readonly DateTimePicker _datePicker = new();
    private readonly ComboBox _fromComboBox = new();
    private readonly ComboBox _toComboBox = new();
    private readonly ComboBox _receivedByComboBox = new();
    private readonly ComboBox _deliveredByComboBox = new();
    private readonly TextBox _notesTextBox = new();
    private readonly List<StfRequestItem> _requests;

    public MaterialMovementItem Item { get; }

    public MaterialMovementDialog(
        MaterialMovementItem item,
        IEnumerable<StfRequestItem> requests,
        IEnumerable<string> locationOptions,
        IEnumerable<string> peopleOptions,
        IEnumerable<string> unitOptions)
    {
        Item = item.Clone();
        _requests = requests.OrderBy(x => x.RowNo).ToList();
        InitializeComponent();
        BindRequests();
        BindValues(_fromComboBox, locationOptions);
        BindValues(_toComboBox, locationOptions);
        BindValues(_receivedByComboBox, peopleOptions);
        BindValues(_deliveredByComboBox, peopleOptions);
        BindValues(_unitComboBox, unitOptions);
        LoadItem();
    }

    private void InitializeComponent()
    {
        Text = "Malzeme Hareketi";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(620, 580);
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

        ConfigureCombo(_requestComboBox, dropDownList: true);
        ConfigureCombo(_movementTypeComboBox, dropDownList: true);
        _movementTypeComboBox.Items.AddRange(["Teslim", "Depoya Giriş", "Transfer", "İade"]);
        _movementTypeComboBox.SelectedIndex = 0;
        _requestComboBox.SelectedIndexChanged += (_, _) => FillFromRequest();

        _productNameTextBox.Dock = DockStyle.Fill;
        _unitComboBox.Dock = DockStyle.Fill;
        _datePicker.Format = DateTimePickerFormat.Short;
        _datePicker.Width = 140;

        _quantityNumeric.DecimalPlaces = 2;
        _quantityNumeric.Minimum = 0.01M;
        _quantityNumeric.Maximum = 1000000;
        _quantityNumeric.Width = 160;

        ConfigureCombo(_fromComboBox);
        ConfigureCombo(_toComboBox);
        ConfigureCombo(_receivedByComboBox);
        ConfigureCombo(_deliveredByComboBox);
        _notesTextBox.Dock = DockStyle.Fill;
        _notesTextBox.Multiline = true;
        _notesTextBox.Height = 90;

        AddField(layout, 0, "Bağlı STF", _requestComboBox);
        AddField(layout, 1, "Hareket Tipi", _movementTypeComboBox);
        AddField(layout, 2, "Ürün Adı", _productNameTextBox);
        AddField(layout, 3, "Birim", _unitComboBox);
        AddField(layout, 4, "Miktar", _quantityNumeric);
        AddField(layout, 5, "Tarih", _datePicker);
        AddField(layout, 6, "Nereden", _fromComboBox);
        AddField(layout, 7, "Nereye", _toComboBox);
        AddField(layout, 8, "Teslim Alan", _receivedByComboBox);
        AddField(layout, 9, "Teslim Eden", _deliveredByComboBox);
        AddField(layout, 10, "Açıklama", _notesTextBox, 96);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 48
        };
        var saveButton = new Button { Text = "Kaydet", Width = 110, Height = 34, DialogResult = DialogResult.OK };
        saveButton.Click += (_, _) => SaveItem();
        var cancelButton = new Button { Text = "İptal", Width = 110, Height = 34, DialogResult = DialogResult.Cancel };
        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);

        Controls.Add(layout);
        Controls.Add(buttonPanel);
    }

    private void BindRequests()
    {
        _requestComboBox.Items.Clear();
        _requestComboBox.Items.Add(new RequestOption(Guid.Empty, "(Bağlı değil)"));
        foreach (var request in _requests)
        {
            _requestComboBox.Items.Add(new RequestOption(request.Id, $"{request.StfNo} | {request.ProductName}"));
        }
        _requestComboBox.SelectedIndex = 0;
    }

    private void LoadItem()
    {
        _movementTypeComboBox.Text = string.IsNullOrWhiteSpace(Item.MovementType) ? "Teslim" : Item.MovementType;
        _productNameTextBox.Text = Item.ProductName;
        _unitComboBox.Text = Item.Unit;
        _quantityNumeric.Value = Convert.ToDecimal(Math.Max(0.01M, Item.Quantity));
        _datePicker.Value = Item.MovementDate == default ? DateTime.Today : Item.MovementDate;
        _fromComboBox.Text = Item.FromLocation;
        _toComboBox.Text = Item.ToLocation;
        _receivedByComboBox.Text = Item.ReceivedBy;
        _deliveredByComboBox.Text = Item.DeliveredBy;
        _notesTextBox.Text = Item.Notes;

        if (Item.RelatedRequestId.HasValue)
        {
            for (var i = 0; i < _requestComboBox.Items.Count; i++)
            {
                if (_requestComboBox.Items[i] is RequestOption option && option.RequestId == Item.RelatedRequestId.Value)
                {
                    _requestComboBox.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    private void FillFromRequest()
    {
        if (_requestComboBox.SelectedItem is not RequestOption option || option.RequestId == Guid.Empty)
        {
            return;
        }

        var request = _requests.FirstOrDefault(x => x.Id == option.RequestId);
        if (request is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_productNameTextBox.Text))
        {
            _productNameTextBox.Text = request.ProductName;
        }

        if (string.IsNullOrWhiteSpace(_unitComboBox.Text))
        {
            _unitComboBox.Text = request.Unit;
        }

        if (string.IsNullOrWhiteSpace(_receivedByComboBox.Text))
        {
            _receivedByComboBox.Text = request.RequestedBy;
        }

        if (string.IsNullOrWhiteSpace(_toComboBox.Text))
        {
            _toComboBox.Text = request.Department;
        }
    }

    private void SaveItem()
    {
        Item.RelatedRequestId = _requestComboBox.SelectedItem is RequestOption option && option.RequestId != Guid.Empty
            ? option.RequestId
            : null;
        Item.MovementType = _movementTypeComboBox.Text.Trim();
        Item.ProductName = _productNameTextBox.Text.Trim();
        Item.Unit = string.IsNullOrWhiteSpace(_unitComboBox.Text) ? "Adet" : _unitComboBox.Text.Trim();
        Item.Quantity = _quantityNumeric.Value;
        Item.MovementDate = _datePicker.Value.Date;
        Item.FromLocation = _fromComboBox.Text.Trim();
        Item.ToLocation = _toComboBox.Text.Trim();
        Item.ReceivedBy = _receivedByComboBox.Text.Trim();
        Item.DeliveredBy = _deliveredByComboBox.Text.Trim();
        Item.Notes = _notesTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(Item.ProductName))
        {
            MessageBox.Show("Ürün adı zorunludur.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }
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

    private static void AddField(TableLayoutPanel layout, int rowIndex, string label, Control control, int rowHeight = 40)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
        layout.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, rowIndex);
        layout.Controls.Add(control, 1, rowIndex);
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

    private sealed class RequestOption
    {
        public Guid RequestId { get; }
        public string Label { get; }

        public RequestOption(Guid requestId, string label)
        {
            RequestId = requestId;
            Label = label;
        }

        public override string ToString()
        {
            return Label;
        }
    }
}
