using System.Drawing;
using System.Windows.Forms;

namespace ArtFruit;

/// <summary>
/// The Preferences window — WinForms port of the SwiftUI <c>PreferencesView</c>.
/// Tabs: General / Style / Artists.
/// Style and Artists tabs show grouped collapsible sections, collapsed by default.
/// </summary>
public sealed class PreferencesForm : Form
{
    private readonly ArtFruitViewModel _vm;

    private static readonly double[] Intervals = { 15, 30, 60, 120, 240, 480 };

    private ComboBox _intervalCombo = null!;
    private CheckBox _pauseCheck = null!;
    private CheckBox _multiMonitorCheck = null!;
    private CheckBox _showTitleCheck = null!;
    private CheckBox _showArtistCheck = null!;
    private Label _currentArtworkLabel = null!;

    // style name → checkbox
    private readonly Dictionary<string, CheckBox> _styleChecks = new();
    // artist name → checkbox
    private readonly Dictionary<string, CheckBox> _artistChecks = new();

    private HashSet<string> _pendingStyles = new();
    private HashSet<string> _pendingArtists = new();

    private Button _applyStylesButton = null!;
    private Button _applyArtistsButton = null!;

    public PreferencesForm(ArtFruitViewModel vm)
    {
        _vm = vm;
        _pendingStyles  = new HashSet<string>(_vm.SelectedStyles);
        _pendingArtists = new HashSet<string>(_vm.SelectedArtists);

        Text = "ArtFruit Preferences";
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(400, 500);
        MinimumSize = new Size(416, 539);
        Font = new Font("Segoe UI", 9f);

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildGeneralTab());
        tabs.TabPages.Add(BuildGroupedTab("Style",   isStyle: true));
        tabs.TabPages.Add(BuildGroupedTab("Artists", isStyle: false));
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
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, AutoSize = true };

        var intervalPanel = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 4, 0, 8) };
        intervalPanel.Controls.Add(new Label { Text = "Change artwork every:", AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
        _intervalCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 130 };
        foreach (var m in Intervals) _intervalCombo.Items.Add(LabelForMinutes(m));
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

        _currentArtworkLabel = new Label { Dock = DockStyle.Fill, AutoSize = false, ForeColor = SystemColors.GrayText, Margin = new Padding(0, 8, 0, 0) };
        UpdateCurrentArtworkLabel();
        layout.Controls.Add(_currentArtworkLabel);

        page.Controls.Add(layout);
        return page;
    }

    // ------------------------------------------------------------------
    // Grouped collapsible tab (shared by Style and Artists)
    // ------------------------------------------------------------------

    private TabPage BuildGroupedTab(string tabName, bool isStyle)
    {
        var page = new TabPage(tabName) { Padding = new Padding(8) };

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var hint = isStyle
            ? "Filter artwork by style. Leave all unchecked for any style."
            : "Filter artwork by artist. Leave all unchecked for any artist.";

        root.Controls.Add(new Label
        {
            Text = hint,
            Dock = DockStyle.Fill,
            AutoSize = false,
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(0, 0, 0, 6),
        }, 0, 0);

        // Scrollable area containing group panels
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var groupStack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Dock = DockStyle.Top,
        };
        scroll.Controls.Add(groupStack);

        if (isStyle)
        {
            foreach (var group in WikiArtData.StyleGroups)
                groupStack.Controls.Add(BuildStyleGroupPanel(group));
        }
        else
        {
            foreach (var group in WikiArtData.ArtistGroups)
                groupStack.Controls.Add(BuildArtistGroupPanel(group));
        }

        root.Controls.Add(scroll, 0, 1);

        // Bottom buttons
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0, 4, 0, 0) };
        var clearBtn = new Button { Text = "Clear All", AutoSize = true };

        if (isStyle)
        {
            clearBtn.Click += (_, _) =>
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
            buttons.Controls.Add(clearBtn);
            buttons.Controls.Add(_applyStylesButton);
        }
        else
        {
            clearBtn.Click += (_, _) =>
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
            buttons.Controls.Add(clearBtn);
            buttons.Controls.Add(_applyArtistsButton);
        }

        root.Controls.Add(buttons, 0, 2);
        page.Controls.Add(root);
        UpdateApplyButtons();
        return page;
    }

    // ------------------------------------------------------------------
    // Per-group collapsible panel builders
    // ------------------------------------------------------------------

    private Panel BuildStyleGroupPanel(WikiArtStyleGroup group)
    {
        // FlowLayoutPanel (not a plain Panel) so hidden/collapsed children are excluded from layout height.
        var outer = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 2),
        };

        // --- Header row ---
        var header = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0),
            Padding = new Padding(0),
        };

        var arrow = new Label { Text = "▶", AutoSize = true, Margin = new Padding(0, 3, 4, 0), Font = new Font(Font.FontFamily, 7f), Cursor = Cursors.Hand };
        var groupCb = new CheckBox { Text = group.Name, AutoSize = true, Font = new Font(Font.FontFamily, 9f, FontStyle.Bold), Margin = new Padding(0, 1, 0, 1) };

        // --- Items panel (hidden by default) ---
        var items = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Visible = false,
            Margin = new Padding(20, 0, 0, 0),
        };

        foreach (var style in group.Styles)
        {
            var cb = new CheckBox { Text = style.Name, AutoSize = true, Checked = _pendingStyles.Contains(style.Name), Margin = new Padding(0, 1, 0, 1) };
            cb.CheckedChanged += (_, _) =>
            {
                if (cb.Checked) _pendingStyles.Add(style.Name);
                else _pendingStyles.Remove(style.Name);
                UpdateGroupCheckbox(groupCb, group.Styles.Select(s => s.Name).ToList(), _pendingStyles);
                UpdateApplyButtons();
            };
            _styleChecks[style.Name] = cb;
            items.Controls.Add(cb);
        }

        // Toggle expand/collapse
        void Toggle()
        {
            items.Visible = !items.Visible;
            arrow.Text = items.Visible ? "▼" : "▶";
        }
        arrow.Click += (_, _) => Toggle();
        header.Click += (_, _) => Toggle();

        // Group checkbox tri-state behaviour
        groupCb.CheckedChanged += (_, _) =>
        {
            if (groupCb.CheckState == CheckState.Indeterminate) return;
            foreach (var s in group.Styles)
            {
                if (_styleChecks.TryGetValue(s.Name, out var cb))
                    cb.Checked = groupCb.Checked;
            }
            UpdateApplyButtons();
        };

        header.Controls.Add(arrow);
        header.Controls.Add(groupCb);

        outer.Controls.Add(header);
        outer.Controls.Add(items);

        UpdateGroupCheckbox(groupCb, group.Styles.Select(s => s.Name).ToList(), _pendingStyles);
        return outer;
    }

    private Panel BuildArtistGroupPanel(WikiArtArtistGroup group)
    {
        // FlowLayoutPanel (not a plain Panel) so hidden/collapsed children are excluded from layout height.
        var outer = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 2),
        };

        var header = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0),
            Padding = new Padding(0),
        };

        var arrow = new Label { Text = "▶", AutoSize = true, Margin = new Padding(0, 3, 4, 0), Font = new Font(Font.FontFamily, 7f), Cursor = Cursors.Hand };
        var groupCb = new CheckBox { Text = group.Name, AutoSize = true, Font = new Font(Font.FontFamily, 9f, FontStyle.Bold), Margin = new Padding(0, 1, 0, 1) };

        var items = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Visible = false,
            Margin = new Padding(20, 0, 0, 0),
        };

        foreach (var artist in group.Artists)
        {
            var cb = new CheckBox { Text = artist.Name, AutoSize = true, Checked = _pendingArtists.Contains(artist.Name), Margin = new Padding(0, 1, 0, 1) };
            cb.CheckedChanged += (_, _) =>
            {
                if (cb.Checked) _pendingArtists.Add(artist.Name);
                else _pendingArtists.Remove(artist.Name);
                UpdateGroupCheckbox(groupCb, group.Artists.Select(a => a.Name).ToList(), _pendingArtists);
                UpdateApplyButtons();
            };
            _artistChecks[artist.Name] = cb;
            items.Controls.Add(cb);
        }

        void Toggle()
        {
            items.Visible = !items.Visible;
            arrow.Text = items.Visible ? "▼" : "▶";
        }
        arrow.Click += (_, _) => Toggle();
        header.Click += (_, _) => Toggle();

        groupCb.CheckedChanged += (_, _) =>
        {
            if (groupCb.CheckState == CheckState.Indeterminate) return;
            foreach (var a in group.Artists)
            {
                if (_artistChecks.TryGetValue(a.Name, out var cb))
                    cb.Checked = groupCb.Checked;
            }
            UpdateApplyButtons();
        };

        header.Controls.Add(arrow);
        header.Controls.Add(groupCb);

        outer.Controls.Add(header);
        outer.Controls.Add(items);

        UpdateGroupCheckbox(groupCb, group.Artists.Select(a => a.Name).ToList(), _pendingArtists);
        return outer;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    private static void UpdateGroupCheckbox(CheckBox cb, List<string> names, HashSet<string> pending)
    {
        var all  = names.All(n => pending.Contains(n));
        var some = names.Any(n => pending.Contains(n));
        cb.CheckState = all ? CheckState.Checked : some ? CheckState.Indeterminate : CheckState.Unchecked;
    }

    private void OnCurrentArtworkChanged()
    {
        if (InvokeRequired) { BeginInvoke(UpdateCurrentArtworkLabel); return; }
        UpdateCurrentArtworkLabel();
    }

    private void UpdateCurrentArtworkLabel()
    {
        if (_currentArtworkLabel is null) return;
        if (string.IsNullOrEmpty(_vm.CurrentTitle)) { _currentArtworkLabel.Text = string.Empty; return; }
        var text = $"Current artwork:\n{_vm.CurrentTitle}";
        if (!string.IsNullOrEmpty(_vm.CurrentArtist)) text += $"\n{_vm.CurrentArtist}";
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
        t.Tick += (_, _) => { t.Stop(); t.Dispose(); button.Text = original; UpdateApplyButtons(); };
        t.Start();
    }

    private static string LabelForMinutes(double minutes)
    {
        if (minutes < 60) return $"{(int)minutes} minutes";
        var hours = (int)(minutes / 60);
        return hours == 1 ? "1 hour" : $"{hours} hours";
    }

    private static double ClosestInterval(double minutes)
    {
        var best = Intervals[0]; var bestDiff = double.MaxValue;
        foreach (var m in Intervals) { var d = Math.Abs(m - minutes); if (d < bestDiff) { bestDiff = d; best = m; } }
        return best;
    }
}
