using Equipments.App.Models;

namespace Equipments.App;

public sealed class EquipmentDialog : Form
{
    private readonly ComboBox _groupComboBox = new();
    private readonly TextBox _nameTextBox = new();
    private readonly ComboBox _unitComboBox = new();
    private readonly NumericUpDown _quantityNumeric = new();
    private readonly TextBox _modelTextBox = new();
    private readonly TextBox _serialNoTextBox = new();
    private readonly ComboBox _locationComboBox = new();
    private readonly ComboBox _ownerComboBox = new();

    public EquipmentItem Item { get; }

    public EquipmentDialog(EquipmentItem item, IEnumerable<string> groups, IEnumerable<string> units, IEnumerable<string> locations, IEnumerable<string> owners)
    {
        Item = item.Clone();
        InitializeComponent();
        SetWindowIcon();
        BindLookupValues(_groupComboBox, groups);
        BindLookupValues(_unitComboBox, units);
        BindLookupValues(_locationComboBox, locations);
        BindLookupValues(_ownerComboBox, owners);
        LoadItem();
    }

    private void InitializeComponent()
    {
        Text = "Ekipman Düzenle";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(580, 540);
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 10
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        ConfigureComboBox(_groupComboBox);
        ConfigureTextBox(_nameTextBox, multiline: true, height: 70);
        ConfigureComboBox(_unitComboBox);
        ConfigureTextBox(_modelTextBox);
        ConfigureTextBox(_serialNoTextBox);
        ConfigureComboBox(_locationComboBox);
        ConfigureComboBox(_ownerComboBox);

        _quantityNumeric.Minimum = 1;
        _quantityNumeric.Maximum = 100000;
        _quantityNumeric.Width = 120;

        AddField(layout, 0, "Malzeme Grubu", _groupComboBox);
        AddField(layout, 1, "Malzeme Adı", _nameTextBox);
        AddField(layout, 2, "Birim", _unitComboBox);
        AddField(layout, 3, "Miktar", _quantityNumeric);
        AddField(layout, 4, "Model", _modelTextBox);
        AddField(layout, 5, "Seri No", _serialNoTextBox);
        AddField(layout, 6, "Lokasyon", _locationComboBox);
        AddField(layout, 7, "Sorumlu", _ownerComboBox);

        var helperLabel = new Label
        {
            Text = "İpuçları: Grup ve Malzeme Adı zorunludur. Seri No girersen uygulama tekrar kayıtlarını kontrol eder.",
            Dock = DockStyle.Fill,
            ForeColor = Color.DimGray,
            TextAlign = ContentAlignment.TopLeft,
            MaximumSize = new Size(0, 90),
            Padding = new Padding(0, 6, 0, 8)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        layout.Controls.Add(helperLabel, 0, 8);
        layout.SetColumnSpan(helperLabel, 2);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0),
            Padding = new Padding(0, 12, 0, 12)
        };

        var saveButton = new Button
        {
            Text = "Kaydet",
            Width = 110,
            Height = 36,
            DialogResult = DialogResult.OK
        };
        saveButton.Click += (_, _) => SaveItem();

        var cancelButton = new Button
        {
            Text = "İptal",
            Width = 110,
            Height = 36,
            DialogResult = DialogResult.Cancel
        };

        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
        layout.Controls.Add(buttonPanel, 0, 9);
        layout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(layout);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private void LoadItem()
    {
        _groupComboBox.Text = Item.Group;
        _nameTextBox.Text = Item.Name;
        _unitComboBox.Text = Item.Unit;
        _quantityNumeric.Value = Math.Max(1, Item.Quantity);
        _modelTextBox.Text = Item.Model;
        _serialNoTextBox.Text = Item.SerialNo;
        _locationComboBox.Text = Item.Location;
        _ownerComboBox.Text = Item.Owner;
    }

    private void SaveItem()
    {
        Item.Group = _groupComboBox.Text.Trim();
        Item.Name = _nameTextBox.Text.Trim();
        Item.Unit = string.IsNullOrWhiteSpace(_unitComboBox.Text) ? "Adet" : _unitComboBox.Text.Trim();
        Item.Quantity = decimal.ToInt32(_quantityNumeric.Value);
        Item.Model = _modelTextBox.Text.Trim();
        Item.SerialNo = _serialNoTextBox.Text.Trim();
        Item.Location = _locationComboBox.Text.Trim();
        Item.Owner = _ownerComboBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(Item.Group) || string.IsNullOrWhiteSpace(Item.Name))
        {
            MessageBox.Show("Malzeme grubu ve malzeme adı zorunludur.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }
    }

    private static void ConfigureTextBox(TextBox textBox, bool multiline = false, int height = 28)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.Multiline = multiline;
        textBox.Height = height;
    }

    private static void ConfigureComboBox(ComboBox comboBox)
    {
        comboBox.Dock = DockStyle.Fill;
        comboBox.DropDownStyle = ComboBoxStyle.DropDown;
        comboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
        comboBox.IntegralHeight = false;
        comboBox.MaxDropDownItems = 12;
    }

    private static void AddField(TableLayoutPanel layout, int rowIndex, string label, Control control)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, rowIndex == 1 ? 84 : 36));
        layout.Controls.Add(new Label
        {
            Text = label,
            TextAlign = ContentAlignment.MiddleLeft,
            Dock = DockStyle.Fill
        }, 0, rowIndex);
        layout.Controls.Add(control, 1, rowIndex);
    }

    private static void BindLookupValues(ComboBox comboBox, IEnumerable<string> values)
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
