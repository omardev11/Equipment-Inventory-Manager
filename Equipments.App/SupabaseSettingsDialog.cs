using Equipments.App.Services;

namespace Equipments.App;

public sealed class SupabaseSettingsDialog : Form
{
    private readonly TextBox _baseUrlTextBox = new();
    private readonly TextBox _publishableKeyTextBox = new();
    private readonly TextBox _tableNameTextBox = new();
    private readonly CheckBox _autoSyncCheckBox = new();

    public SupabaseSettings Settings { get; }

    public SupabaseSettingsDialog(SupabaseSettings settings)
    {
        Settings = new SupabaseSettings
        {
            BaseUrl = settings.BaseUrl,
            PublishableKey = settings.PublishableKey,
            EquipmentTable = settings.EquipmentTable,
            AutoSyncEnabled = settings.AutoSyncEnabled
        };

        InitializeComponent();
        LoadSettings();
    }

    private void InitializeComponent()
    {
        Text = "Supabase Ayarları";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(640, 320);
        Font = new Font("Segoe UI", 9F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 6
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        ConfigureTextBox(_baseUrlTextBox, "https://YOUR-PROJECT.supabase.co");
        ConfigureTextBox(_publishableKeyTextBox, "sb_publishable_...");
        ConfigureTextBox(_tableNameTextBox, "equipments");

        AddField(layout, 0, "Base URL", _baseUrlTextBox);
        AddField(layout, 1, "Publishable Key", _publishableKeyTextBox);
        AddField(layout, 2, "Tablo Adı", _tableNameTextBox);

        _autoSyncCheckBox.Text = "Yerel kayıttan sonra otomatik bulut senkronu yap";
        _autoSyncCheckBox.Dock = DockStyle.Fill;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.Controls.Add(_autoSyncCheckBox, 0, 3);
        layout.SetColumnSpan(_autoSyncCheckBox, 2);

        var helpLabel = new Label
        {
            Dock = DockStyle.Fill,
            ForeColor = Color.DimGray,
            Text = "Bu hızlı prototip yapısı publishable key ile çalışır. Güçlü güvenlik için ayrı bir sunucu veya imzalı erişim gerekir.",
            Padding = new Padding(0, 6, 0, 0)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        layout.Controls.Add(helpLabel, 0, 4);
        layout.SetColumnSpan(helpLabel, 2);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 12, 0, 0)
        };

        var saveButton = new Button
        {
            Text = "Kaydet",
            Width = 110,
            Height = 34,
            DialogResult = DialogResult.OK
        };
        saveButton.Click += (_, _) => SaveSettings();

        var cancelButton = new Button
        {
            Text = "İptal",
            Width = 110,
            Height = 34,
            DialogResult = DialogResult.Cancel
        };

        buttonPanel.Controls.Add(saveButton);
        buttonPanel.Controls.Add(cancelButton);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.Controls.Add(buttonPanel, 0, 5);
        layout.SetColumnSpan(buttonPanel, 2);

        Controls.Add(layout);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private void LoadSettings()
    {
        _baseUrlTextBox.Text = Settings.BaseUrl;
        _publishableKeyTextBox.Text = Settings.PublishableKey;
        _tableNameTextBox.Text = string.IsNullOrWhiteSpace(Settings.EquipmentTable) ? "equipments" : Settings.EquipmentTable;
        _autoSyncCheckBox.Checked = Settings.AutoSyncEnabled;
    }

    private void SaveSettings()
    {
        Settings.BaseUrl = _baseUrlTextBox.Text.Trim();
        Settings.PublishableKey = _publishableKeyTextBox.Text.Trim();
        Settings.EquipmentTable = string.IsNullOrWhiteSpace(_tableNameTextBox.Text) ? "equipments" : _tableNameTextBox.Text.Trim();
        Settings.AutoSyncEnabled = _autoSyncCheckBox.Checked;

        if (string.IsNullOrWhiteSpace(Settings.BaseUrl) || string.IsNullOrWhiteSpace(Settings.PublishableKey))
        {
            MessageBox.Show("Base URL ve Publishable Key zorunludur.", "Eksik Ayar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }
    }

    private static void AddField(TableLayoutPanel layout, int rowIndex, string label, Control control)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.Controls.Add(new Label
        {
            Text = label,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, rowIndex);
        layout.Controls.Add(control, 1, rowIndex);
    }

    private static void ConfigureTextBox(TextBox textBox, string placeholder)
    {
        textBox.Dock = DockStyle.Fill;
        textBox.PlaceholderText = placeholder;
    }
}
