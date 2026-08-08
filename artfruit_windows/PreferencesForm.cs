using System.Drawing;
using System.Windows.Forms;

namespace ArtFruit;

/// <summary>
/// The Preferences window — the WinForms port of the SwiftUI
/// <c>PreferencesView</c>, with General / Sources / Style tabs.
/// </summary>
public sealed class PreferencesForm : Form
{
    private readonly ArtFruitViewModel _vm;

    // Interval options (minutes) — matches the Swift picker.
    private static readonly double[] Intervals = { 15, 30, 60, 120, 240, 480 };

    private ComboBox _intervalCombo = null!;
    private CheckBox _pauseCheck = null!;
    private CheckBox _multiMonitorCheck = null!;
    private CheckBox _showTitleCheck = null!;
    private CheckBox _showArtistCheck = null!;
    private Label _currentArtworkLabel = null!;

    private readonly Dictionary<string, CheckBox> _styleChecks = new();
    private readonly Dictionary<string, CheckBox> _artistChecks = new();

    // Pending selections (applied via the "Apply" buttons, mirroring the Swift UI).
    private HashSet<string> _pendingStyles = new();
    private HashSet<string> _pendingArtists = new();

    private Button _applyStylesButton = null!;
    private Button _applyArtistsButton = null!;

    public PreferencesForm(ArtFruitViewModel vm)
    {
        _vm = vm;
        _pendingStyles = new HashSet<string>(_vm.SelectedStyles);
        _pendingArtists = new HashSet<string>(_vm.SelectedArtists);

        Text = "ArtFruit Preferences";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(380, 440);
        Font = new Font("Segoe UI", 9f);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildGeneralTab());
        tabs.TabPages.Add(BuildStyleTab());
        tabs.TabPages.Add(BuildArtistsTab());
        Controls.Add(tabs);

        _vm.CurrentArtworkChanged += OnCurrentArtworkChanged;
        FormClosed += (_, _) => _vm.CurrentArtworkChanged -= OnCurrentArtworkChanged;
    }

    // ------------------------------------------------------------------
    // General tab
    // ------------------------------------------------------------------

    private TabPage BuildGeneralTab()
    {
        var page = new TabPage("General") { Padding = new Padding(16) };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            AutoSize = true,
        };

        var intervalPanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 4, 0, 8) };
        intervalPanel.Controls.Add(new Label { Text = "Change artwork every:", AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
        _intervalCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        foreach (var m in Intervals)
            _intervalCombo.Items.Add(LabelForMinutes(m));
        _intervalCombo.SelectedIndex = Math.Max(0, Array.IndexOf(Intervals, ClosestInterval(_vm.ChangeIntervalMinutes)));
        _intervalCombo.SelectedIndexChanged += (_, _) =>
        {
            if (_intervalCombo.SelectedIndex >= 0)
                _vm.ChangeIntervalMinutes = Intervals[_intervalCombo.SelectedIndex];
        };
        intervalPanel.Controls.Add(_intervalCombo);
        layout.Controls.Add(intervalPanel);

        _pauseCheck = new CheckBox { Text = "Pause artwork rotation", AutoSize = true, Checked = _vm.IsPaused, Margin = new Padding(0, 4, 0, 4) };
        _pauseCheck.CheckedChanged += (_, _) => _vm.SetPaused(_pauseCheck.Checked);
        layout.Controls.Add(_pauseCheck);

        _multiMonitorCheck = new CheckBox { Text = "Display artwork on multiple monitors", AutoSize = true, Checked = _vm.MultiMonitor, Margin = new Padding(0, 4, 0, 4) };
        _multiMonitorCheck.CheckedChanged += (_, _) => _vm.MultiMonitor = _multiMonitorCheck.Checked;
        layout.Controls.Add(_multiMonitorCheck);

        _showTitleCheck = new CheckBox { Text = "Display artwork title", AutoSize = true, Checked = _vm.ShowTitle, Margin = new Padding(0, 4, 0, 4) };
        _showTitleCheck.CheckedChanged += (_, _) => _vm.ShowTitle = _showTitleCheck.Checked;
        layout.Controls.Add(_showTitleCheck);

        _showArtistCheck = new CheckBox { Text = "Display artist name", AutoSize = true, Checked = _vm.ShowArtist, Margin = new Padding(0, 4, 0, 12) };
        _showArtistCheck.CheckedChanged += (_, _) => _vm.ShowArtist = _showArtistCheck.Checked;
        layout.Controls.Add(_showArtistCheck);

        _currentArtworkLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(330, 0),
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 8, 0, 0),
        };
        UpdateCurrentArtworkLabel();
        layout.Controls.Add(_currentArtworkLabel);

        page.Controls.Add(layout);
        return page;
    }

    // ------------------------------------------------------------------
    // Style tab
    // ------------------------------------------------------------------

    private TabPage BuildStyleTab()
    {
        var page = new TabPage("Style") { Padding = new Padding(16) };

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(new Label
        {
            Text = "Filter artwork by style. Leave all unchecked for any style.",
            AutoSize = true,
            MaximumSize = new Size(340, 0),
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 0, 0, 8),
        }, 0, 0);

        var list = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        foreach (var style in ArtStyles.All)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 1, 0, 1) };
            var cb = new CheckBox
            {
                Text = style,
                AutoSize = true,
                Checked = _pendingStyles.Contains(style),
                Margin = new Padding(0, 2, 8, 2),
            };
            cb.CheckedChanged += (_, _) =>
            {
                if (cb.Checked) _pendingStyles.Add(style);
                else _pendingStyles.Remove(style);
                UpdateApplyButtons();
            };
            _styleChecks[style] = cb;

            var badge = new Label
            {
                Text = StyleSourceLabel(style),
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Margin = new Padding(0, 5, 0, 0),
                Font = new Font(Font.FontFamily, 7.5f),
            };

            row.Controls.Add(cb);
            row.Controls.Add(badge);
            list.Controls.Add(row);
        }
        root.Controls.Add(list, 0, 1);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        var clear = new Button { Text = "Clear All", AutoSize = true };
        clear.Click += (_, _) =>
        {
            _pendingStyles.Clear();
            foreach (var cb in _styleChecks.Values) cb.Checked = false;
            UpdateApplyButtons();
        };
        _applyStylesButton = new Button { Text = "Apply", AutoSize = true };
        _applyStylesButton.Click += (_, _) =>
        {
            _vm.SelectedStyles = new HashSet<string>(_pendingStyles);
            _vm.FetchAndApplyArtwork();
            FlashApplied(_applyStylesButton);
        };
        buttons.Controls.Add(clear);
        buttons.Controls.Add(_applyStylesButton);
        root.Controls.Add(buttons, 0, 2);

        page.Controls.Add(root);
        UpdateApplyButtons();
        return page;
    }

    // ------------------------------------------------------------------
    // Artists tab
    // ------------------------------------------------------------------

    private TabPage BuildArtistsTab()
    {
        var page = new TabPage("Artists") { Padding = new Padding(16) };

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(new Label
        {
            Text = "Filter artwork by artist. Leave all unchecked for any artist.",
            AutoSize = true,
            MaximumSize = new Size(340, 0),
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 0, 0, 8),
        }, 0, 0);

        var list = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        foreach (var artist in ArtArtists.All)
        {
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 1, 0, 1) };
            var cb = new CheckBox
            {
                Text = artist,
                AutoSize = true,
                Checked = _pendingArtists.Contains(artist),
                Margin = new Padding(0, 2, 8, 2),
            };
            cb.CheckedChanged += (_, _) =>
            {
                if (cb.Checked) _pendingArtists.Add(artist);
                else _pendingArtists.Remove(artist);
                UpdateApplyButtons();
            };
            _artistChecks[artist] = cb;

            var badge = new Label
            {
                Text = ArtistSourceLabel(artist),
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Margin = new Padding(0, 5, 0, 0),
                Font = new Font(Font.FontFamily, 7.5f),
            };

            row.Controls.Add(cb);
            row.Controls.Add(badge);
            list.Controls.Add(row);
        }
        root.Controls.Add(list, 0, 1);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
        var clear = new Button { Text = "Clear All", AutoSize = true };
        clear.Click += (_, _) =>
        {
            _pendingArtists.Clear();
            foreach (var cb in _artistChecks.Values) cb.Checked = false;
            UpdateApplyButtons();
        };
        _applyArtistsButton = new Button { Text = "Apply", AutoSize = true };
        _applyArtistsButton.Click += (_, _) =>
        {
            _vm.SelectedArtists = new HashSet<string>(_pendingArtists);
            _vm.FetchAndApplyArtwork();
            FlashApplied(_applyArtistsButton);
        };
        buttons.Controls.Add(clear);
        buttons.Controls.Add(_applyArtistsButton);
        root.Controls.Add(buttons, 0, 2);

        page.Controls.Add(root);
        UpdateApplyButtons();
        return page;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private void OnCurrentArtworkChanged()
    {
        if (InvokeRequired)
        {
            BeginInvoke(UpdateCurrentArtworkLabel);
            return;
        }
        UpdateCurrentArtworkLabel();
    }

    private void UpdateCurrentArtworkLabel()
    {
        if (_currentArtworkLabel is null) return;
        if (string.IsNullOrEmpty(_vm.CurrentTitle))
        {
            _currentArtworkLabel.Text = string.Empty;
            return;
        }

        var text = $"Current artwork:\n{_vm.CurrentTitle}";
        if (!string.IsNullOrEmpty(_vm.CurrentArtist))
            text += $"\n{_vm.CurrentArtist}";
        _currentArtworkLabel.Text = text;
    }

    private void UpdateApplyButtons()
    {
        if (_applyStylesButton is not null)
            _applyStylesButton.Enabled = !_pendingStyles.SetEquals(_vm.SelectedStyles);
        if (_applyArtistsButton is not null)
            _applyArtistsButton.Enabled = !_pendingArtists.SetEquals(_vm.SelectedArtists);
    }

    private void FlashApplied(Button button)
    {
        var original = button.Text;
        button.Text = "Applied \u2713";
        button.Enabled = false;
        var t = new System.Windows.Forms.Timer { Interval = 2000 };
        t.Tick += (_, _) =>
        {
            t.Stop();
            t.Dispose();
            button.Text = original;
            UpdateApplyButtons();
        };
        t.Start();
    }

    private static string StyleSourceLabel(string style) =>
        WikiArtApiClient.StyleSlugMap.ContainsKey(style) ? "WikiArt" : "";

    private static string ArtistSourceLabel(string artist) =>
        WikiArtApiClient.ArtistSlugMap.ContainsKey(artist) ? "WikiArt" : "";

    private static string LabelForMinutes(double minutes)
    {
        if (minutes < 60) return $"{(int)minutes} minutes";
        var hours = (int)(minutes / 60);
        return hours == 1 ? "1 hour" : $"{hours} hours";
    }

    private static double ClosestInterval(double minutes)
    {
        var best = Intervals[0];
        var bestDiff = double.MaxValue;
        foreach (var m in Intervals)
        {
            var diff = Math.Abs(m - minutes);
            if (diff < bestDiff) { bestDiff = diff; best = m; }
        }
        return best;
    }
}
