using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProjectTreeViewer
{
    public partial class Form1 : Form
    {
        private string? _currentPath;
        private string _pendingFontName = "Consolas";
        private bool _suppressAfterCheck;

        private float _treeFontSize;

        private readonly CommandLineOptions _startupOptions;

        private readonly LocalizationService _localization;
        private readonly MessageService _messages;

        private readonly ElevationService _elevation = new();
        private readonly FileSystemScanner _scanner = new();
        private readonly TreeBuilder _treeBuilder = new();
        private readonly TreeViewRenderer _renderer = new();
        private readonly TreeExportService _treeExport = new();
        private readonly SelectedContentExportService _contentExport = new();

        private readonly TreeIconService _iconService = new();
        private readonly ContentReadService _contentReader = new();

        private bool _elevationAttempted;

        private const string ClipboardBlankLine = "\u00A0"; // NBSP: визуально пустая строка, но не схлопывается при вставке
        private static void AppendClipboardBlankLine(StringBuilder sb) => sb.AppendLine(ClipboardBlankLine);

        // Ignore list indices (CheckedListBox near "Типы файлов")
        private const int IgnoreIndexBin = 0;
        private const int IgnoreIndexObj = 1;
        private const int IgnoreIndexDot = 2;

        private bool _suppressIgnoreItemCheck;

        public Form1() : this(CommandLineOptions.Empty)
        {
        }

        public Form1(CommandLineOptions startupOptions)
        {
            _startupOptions = startupOptions;

            InitializeComponent();

            SetupStableLayout(); // меню всегда сверху, панель не "толкает" его вниз

            _iconService.Initialize(treeProject);

            treeProject.BeforeExpand -= treeProject_BeforeExpand;
            treeProject.BeforeExpand += treeProject_BeforeExpand;

            RemoveUnneededMenuMargins();

            _treeFontSize = treeProject.Font.Size;

            _localization = new LocalizationService(startupOptions.Language ?? LocalizationService.DetectSystemLanguage());
            _messages = new MessageService(_localization);

            _elevationAttempted = startupOptions.ElevationAttempted;

            _localization.LanguageChanged += (_, _) => ApplyLocalization();

            LoadFonts();
            _pendingFontName = (string?)cboFont.SelectedItem ?? "Consolas";

            InitIgnoreList();

            SetMenuEnabled(false);
            ApplyLocalization();

            Shown += Form1_Shown;
        }

        // ───────────────────────────────────────── Localization-safe helper (NO AppLanguage switch in UI)
        private string T(string key, string fallback)
        {
            try
            {
                var value = _localization[key];
                if (string.IsNullOrWhiteSpace(value) || string.Equals(value, key, StringComparison.Ordinal))
                    return fallback;
                return value;
            }
            catch
            {
                return fallback;
            }
        }

        // ───────────────────────────────────────── Layout: MenuStrip must stay on top
        private void EnsureDockLayout()
        {
            menuStripMain.Dock = DockStyle.Top;

            panelSettings.Dock = DockStyle.None;
            treeProject.Dock = DockStyle.None;

            panelSettings.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            treeProject.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            ApplyStableLayout();
        }

        private void ApplyStableLayout()
        {
            int top = menuStripMain.Height;

            int panelWidth = panelSettings.Visible ? panelSettings.Width : 0;

            treeProject.Location = new Point(0, top);
            treeProject.Size = new Size(
                Math.Max(0, ClientSize.Width - panelWidth),
                Math.Max(0, ClientSize.Height - top));

            panelSettings.Location = new Point(ClientSize.Width - panelSettings.Width, top);
            panelSettings.Height = Math.Max(0, ClientSize.Height - top);

            menuStripMain.BringToFront();
            if (panelSettings.Visible)
                panelSettings.BringToFront();
        }

        // ───────────────────────────────────────── Ignore list init
        private void InitIgnoreList()
        {
            _suppressIgnoreItemCheck = true;
            try
            {
                lstIgnore.CheckOnClick = true;
                lstIgnore.Items.Clear();

                // ключи берём из локализации. fallback — чтобы не падало, если ключ не заведён.
                lstIgnore.Items.Add(T("Settings.IgnoreBin", "Ignore bin"), true);
                lstIgnore.Items.Add(T("Settings.IgnoreObj", "Ignore obj"), true);
                lstIgnore.Items.Add(T("Settings.IgnoreDot", "Ignore dotfiles"), true);

                lstIgnore.ItemCheck -= lstIgnore_ItemCheck;
                lstIgnore.ItemCheck += lstIgnore_ItemCheck;
            }
            finally
            {
                _suppressIgnoreItemCheck = false;
            }
        }

        private void UpdateIgnoreListLocalization()
        {
            _suppressIgnoreItemCheck = true;
            try
            {
                bool bin = IsIgnoreChecked(IgnoreIndexBin);
                bool obj = IsIgnoreChecked(IgnoreIndexObj);
                bool dot = IsIgnoreChecked(IgnoreIndexDot);

                lstIgnore.BeginUpdate();
                try
                {
                    lstIgnore.Items.Clear();
                    lstIgnore.Items.Add(T("Settings.IgnoreBin", "Ignore bin"), bin);
                    lstIgnore.Items.Add(T("Settings.IgnoreObj", "Ignore obj"), obj);
                    lstIgnore.Items.Add(T("Settings.IgnoreDot", "Ignore dotfiles"), dot);
                }
                finally
                {
                    lstIgnore.EndUpdate();
                }
            }
            finally
            {
                _suppressIgnoreItemCheck = false;
            }
        }

        private void lstIgnore_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (_suppressIgnoreItemCheck) return;

            BeginInvoke(new Action(() =>
            {
                PopulateRootFolders(_currentPath ?? "");
                PopulateExtensions(_currentPath ?? "");
            }));
        }

        private bool IsIgnoreChecked(int index)
        {
            if (index < 0 || index >= lstIgnore.Items.Count) return false;
            return lstIgnore.GetItemChecked(index);
        }

        private (bool IgnoreBin, bool IgnoreObj, bool IgnoreDot) GetIgnoreOptions() =>
        (
            IgnoreBin: IsIgnoreChecked(IgnoreIndexBin),
            IgnoreObj: IsIgnoreChecked(IgnoreIndexObj),
            IgnoreDot: IsIgnoreChecked(IgnoreIndexDot)
        );

        // ───────────────────────────────────────── Expand “layer-by-layer”
        private void treeProject_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
        {
            treeProject.BeginUpdate();
            try
            {
                CollapseAllDescendants(e.Node);
            }
            finally
            {
                treeProject.EndUpdate();
            }
        }

        private static void CollapseAllDescendants(TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
                CollapseDeep(child);
        }

        private static void CollapseDeep(TreeNode node)
        {
            foreach (TreeNode child in node.Nodes)
                CollapseDeep(child);

            node.Collapse();
        }

        // ───────────────────────────────────────── Menu margins
        private void RemoveUnneededMenuMargins()
        {
            ConfigureDropDownMenu(miFile, showCheckMargin: false);
            ConfigureDropDownMenu(miCopy, showCheckMargin: false);
            ConfigureDropDownMenu(miView, showCheckMargin: false);
            ConfigureDropDownMenu(miOptions, showCheckMargin: false);
            ConfigureDropDownMenu(miHelp, showCheckMargin: false);

            // язык — чекбоксы, margin нужен
            ConfigureDropDownMenu(miLanguage, showCheckMargin: true);
        }

        private static void ConfigureDropDownMenu(ToolStripMenuItem menuItem, bool showCheckMargin)
        {
            if (menuItem.DropDown is ToolStripDropDownMenu dd)
            {
                dd.ShowImageMargin = false;
                dd.ShowCheckMargin = showCheckMargin;
            }

            foreach (ToolStripItem child in menuItem.DropDownItems)
            {
                if (child is ToolStripMenuItem childMi)
                    ConfigureDropDownMenu(childMi, showCheckMargin: false);
            }
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_startupOptions.Path))
                TryOpenFolder(_startupOptions.Path!, fromDialog: false);
        }

        // ───────────────────────────────────────── Localization (NO hardcoded language switches)
        private void ApplyLocalization()
        {
            miFile.Text = T("Menu.File", "File");
            miFileOpen.Text = T("Menu.File.Open", "Open folder...");
            miFileRefresh.Text = T("Menu.File.Refresh", "Refresh");
            miFileExit.Text = T("Menu.File.Exit", "Exit");

            miCopy.Text = T("Menu.Copy", "Copy");
            miCopyFullTree.Text = T("Menu.Copy.FullTree", "Copy full tree");
            miCopySelectedTree.Text = T("Menu.Copy.SelectedTree", "Copy selected tree");
            miCopySelectedContent.Text = T("Menu.Copy.SelectedContent", "Copy selected content");
            miCopyFullTreeAndContent.Text = T("Menu.Copy.FullTreeAndContent", "Copy full tree and content");

            miView.Text = T("Menu.View", "View");
            miViewExpandAll.Text = T("Menu.View.ExpandAll", "Expand all");
            miViewCollapseAll.Text = T("Menu.View.CollapseAll", "Collapse all");
            miViewZoomIn.Text = T("Menu.View.ZoomIn", "Zoom in");
            miViewZoomOut.Text = T("Menu.View.ZoomOut", "Zoom out");
            miViewZoomReset.Text = T("Menu.View.ZoomReset", "Reset zoom");

            miOptions.Text = T("Menu.Options", "Options");

            miLanguage.Text = T("Menu.Language", "Language");

            miHelp.Text = T("Menu.Help", "Help");
            miHelpAbout.Text = T("Menu.Help.About", "About");

            // Settings panel
            labelIgnore.Text = T("Settings.IgnoreLabel", "Ignore:");
            checkBoxAll.Text = T("Settings.All", "All");
            labelExtensions.Text = T("Settings.Extensions", "Extensions:");
            labelRootFolders.Text = T("Settings.RootFolders", "Root folders:");
            labelFont.Text = T("Settings.Font", "Tree font:");
            btnApply.Text = T("Settings.Apply", "Apply");

            UpdateIgnoreListLocalization();
            UpdateLanguageChecks();
            UpdateTitle();
        }

        private void UpdateLanguageChecks()
        {
            miLangRu.Checked = _localization.CurrentLanguage == AppLanguage.Ru;
            miLangEn.Checked = _localization.CurrentLanguage == AppLanguage.En;
            miLangUz.Checked = _localization.CurrentLanguage == AppLanguage.Uz;
            miLangTg.Checked = _localization.CurrentLanguage == AppLanguage.Tg;
            miLangKk.Checked = _localization.CurrentLanguage == AppLanguage.Kk;
            miLangFr.Checked = _localization.CurrentLanguage == AppLanguage.Fr;
            miLangDe.Checked = _localization.CurrentLanguage == AppLanguage.De;
            miLangIt.Checked = _localization.CurrentLanguage == AppLanguage.It;
        }

        // ───────────────────────────────────────── UI helpers
        private void SetMenuEnabled(bool enabled)
        {
            miFileRefresh.Enabled = enabled;

            miCopyFullTree.Enabled = enabled;
            miCopySelectedTree.Enabled = enabled;
            miCopySelectedContent.Enabled = enabled;
            miCopyFullTreeAndContent.Enabled = enabled;

            miViewExpandAll.Enabled = enabled;
            miViewCollapseAll.Enabled = enabled;

            miOptions.Enabled = enabled;
        }

        private void UpdateTitle()
        {
            Text = _currentPath is null
                ? T("Title.Default", "Project Tree Viewer")
                : _localization.Format("Title.WithPath", _currentPath);
        }

        // ───────────────────────────────────────── Menu actions
        private void miFileOpen_Click(object? sender, EventArgs e)
        {
            try
            {
                using var dlg = new FolderBrowserDialog { Description = T("Dialog.SelectRoot", "Select root folder") };
                if (dlg.ShowDialog() != DialogResult.OK) return;

                TryOpenFolder(dlg.SelectedPath, fromDialog: true);
            }
            catch (Exception ex)
            {
                _messages.ShowException(ex);
            }
        }

        private void miFileRefresh_Click(object? sender, EventArgs e)
        {
            try
            {
                ReloadProject();
            }
            catch (Exception ex)
            {
                _messages.ShowException(ex);
            }
        }

        private void miFileExit_Click(object? sender, EventArgs e) => Close();

        private void miViewExpandAll_Click(object? sender, EventArgs e)
        {
            if (treeProject.Nodes.Count == 0) return;
            treeProject.Nodes[0].ExpandAll();
        }

        private void miViewCollapseAll_Click(object? sender, EventArgs e)
        {
            if (treeProject.Nodes.Count == 0) return;
            treeProject.CollapseAll();
            treeProject.Nodes[0].Expand();
        }

        private void miViewZoomIn_Click(object? sender, EventArgs e) => ChangeTreeFontSize(+1f);
        private void miViewZoomOut_Click(object? sender, EventArgs e) => ChangeTreeFontSize(-1f);
        private void miViewZoomReset_Click(object? sender, EventArgs e) => SetTreeFontSize(9f);

        private void miCopyFullTree_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!EnsureTreeReady()) return;

                var root = treeProject.Nodes[0];
                var text = _treeExport.BuildFullTree(_currentPath!, root);
                Clipboard.SetText(text, TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                _messages.ShowException(ex);
            }
        }

        private void miCopySelectedTree_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!EnsureTreeReady()) return;

                var root = treeProject.Nodes[0];
                var text = _treeExport.BuildSelectedTree(_currentPath!, root);

                if (string.IsNullOrWhiteSpace(text))
                {
                    _messages.ShowInfo(T("Msg.NoCheckedTree", "No items checked in tree."));
                    return;
                }

                Clipboard.SetText(text, TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                _messages.ShowException(ex);
            }
        }

        private void miCopySelectedContent_Click(object? sender, EventArgs e)
        {
            try
            {
                if (treeProject.Nodes.Count == 0) return;

                var files = GetCheckedFilePaths(treeProject.Nodes)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (files.Count == 0)
                {
                    _messages.ShowInfo(T("Msg.NoCheckedFiles", "No files selected."));
                    return;
                }

                var content = BuildContentForClipboard(files);
                if (string.IsNullOrWhiteSpace(content))
                {
                    _messages.ShowInfo(T("Msg.NoTextContent", "No text content to copy."));
                    return;
                }

                Clipboard.SetText(content, TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                _messages.ShowException(ex);
            }
        }

        // Copy full tree + content (auto: selected if any checked, else full)
        private void miCopyFullTreeAndContent_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!EnsureTreeReady()) return;

                var root = treeProject.Nodes[0];

                bool hasSelection = TreeExportService.HasCheckedDescendantOrSelf(root);

                string tree = hasSelection
                    ? _treeExport.BuildSelectedTree(_currentPath!, root)
                    : _treeExport.BuildFullTree(_currentPath!, root);

                if (hasSelection && string.IsNullOrWhiteSpace(tree))
                    tree = _treeExport.BuildFullTree(_currentPath!, root);

                var files = hasSelection
                    ? GetCheckedFilePaths(treeProject.Nodes).ToList()
                    : GetAllFilePathsFromTree(root).ToList();

                var content = BuildContentForClipboard(files);

                if (string.IsNullOrWhiteSpace(content))
                {
                    Clipboard.SetText(tree, TextDataFormat.UnicodeText);
                    return;
                }

                var sb = new StringBuilder();
                sb.Append(tree.TrimEnd('\r', '\n'));

                AppendClipboardBlankLine(sb);
                AppendClipboardBlankLine(sb);

                sb.Append(content);

                Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText);
            }
            catch (Exception ex)
            {
                _messages.ShowException(ex);
            }
        }

        // ───────────────────────────────────────── Stable layout
        private void SetupStableLayout()
        {
            panelSettings.AutoScroll = true;

            EnsureDockLayout();

            Resize -= Form1_ResizeRelayout;
            Resize += Form1_ResizeRelayout;

            panelSettings.VisibleChanged -= PanelSettings_VisibleChanged;
            panelSettings.VisibleChanged += PanelSettings_VisibleChanged;
        }

        private void Form1_ResizeRelayout(object? sender, EventArgs e) => ApplyStableLayout();
        private void PanelSettings_VisibleChanged(object? sender, EventArgs e) => ApplyStableLayout();

        private void miOptions_Click(object? sender, EventArgs e)
        {
            panelSettings.Visible = !panelSettings.Visible;
            ApplyStableLayout();
        }

        // ───────────────────────────────────────── Root node visibility fix
        private void EnsureRootNodeVisible(string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                return;

            if (treeProject.Nodes.Count == 0)
                return;

            var first = treeProject.Nodes[0];

            if (first.Tag is string firstPath && PathEquals(firstPath, rootPath))
            {
                if (string.IsNullOrWhiteSpace(first.Text))
                    first.Text = GetRootCaption(rootPath);

                return;
            }

            string rootText = GetRootCaption(rootPath);

            treeProject.BeginUpdate();
            try
            {
                _suppressAfterCheck = true;

                var oldTop = new List<TreeNode>();
                foreach (TreeNode n in treeProject.Nodes)
                    oldTop.Add(n);

                treeProject.Nodes.Clear();

                var rootNode = new TreeNode(rootText)
                {
                    Tag = rootPath
                };

                bool allChecked = true;
                foreach (var n in oldTop)
                {
                    if (!n.Checked)
                    {
                        allChecked = false;
                        break;
                    }
                }

                rootNode.Checked = allChecked;

                foreach (var n in oldTop)
                    rootNode.Nodes.Add(n);

                treeProject.Nodes.Add(rootNode);
            }
            finally
            {
                _suppressAfterCheck = false;
                treeProject.EndUpdate();
            }
        }

        private static string GetRootCaption(string path)
        {
            var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var name = Path.GetFileName(trimmed);
            return string.IsNullOrWhiteSpace(name) ? trimmed : name;
        }

        private static bool PathEquals(string a, string b)
        {
            string ta = a.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string tb = b.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.Equals(ta, tb, StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> GetCheckedFilePaths(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked && node.Tag is string path && File.Exists(path))
                    yield return path;

                foreach (var child in GetCheckedFilePaths(node.Nodes))
                    yield return child;
            }
        }

        private static IEnumerable<string> GetAllFilePathsFromTree(TreeNode node)
        {
            if (node.Tag is string path && File.Exists(path))
                yield return path;

            foreach (TreeNode child in node.Nodes)
            {
                foreach (var p in GetAllFilePathsFromTree(child))
                    yield return p;
            }
        }

        private string BuildContentForClipboard(IEnumerable<string> filePaths)
        {
            var files = filePaths
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (files.Count == 0)
                return string.Empty;

            var sb = new StringBuilder();
            bool anyWritten = false;

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];

                if (!_contentReader.TryReadTextForClipboard(file, out var text))
                    continue;

                if (anyWritten)
                {
                    AppendClipboardBlankLine(sb);
                    AppendClipboardBlankLine(sb);
                }

                anyWritten = true;

                sb.AppendLine($"{file}:");
                AppendClipboardBlankLine(sb);

                sb.AppendLine(text);
            }

            return anyWritten ? sb.ToString().TrimEnd('\r', '\n') : string.Empty;
        }

        private bool EnsureTreeReady()
        {
            if (treeProject.Nodes.Count == 0) return false;
            if (string.IsNullOrWhiteSpace(_currentPath)) return false;
            return true;
        }

        // ───────────────────────────────────────── Language menu
        private void miLangRu_Click(object? sender, EventArgs e) => _localization.SetLanguage(AppLanguage.Ru);
        private void miLangEn_Click(object? sender, EventArgs e) => _localization.SetLanguage(AppLanguage.En);
        private void miLangUz_Click(object? sender, EventArgs e) => _localization.SetLanguage(AppLanguage.Uz);
        private void miLangTg_Click(object? sender, EventArgs e) => _localization.SetLanguage(AppLanguage.Tg);
        private void miLangKk_Click(object? sender, EventArgs e) => _localization.SetLanguage(AppLanguage.Kk);
        private void miLangFr_Click(object? sender, EventArgs e) => _localization.SetLanguage(AppLanguage.Fr);
        private void miLangDe_Click(object? sender, EventArgs e) => _localization.SetLanguage(AppLanguage.De);
        private void miLangIt_Click(object? sender, EventArgs e) => _localization.SetLanguage(AppLanguage.It);

        // ───────────────────────────────────────── Help
        private void miHelpAbout_Click(object? sender, EventArgs e)
        {
            _messages.ShowInfo(T("Msg.AboutStub", "About"));
        }

        // ───────────────────────────────────────── Open folder / elevation
        private void TryOpenFolder(string path, bool fromDialog)
        {
            if (!Directory.Exists(path))
            {
                _messages.ShowErrorFormat("Msg.PathNotFound", path);
                return;
            }

            if (!_scanner.CanReadRoot(path))
            {
                if (TryElevateAndRestart(path)) return;

                _messages.ShowError(T("Msg.AccessDeniedRoot", "Access denied to root folder."));
                return;
            }

            _currentPath = path;
            UpdateTitle();

            SetMenuEnabled(true);

            ReloadProject();
        }

        private bool TryElevateAndRestart(string path)
        {
            if (_elevation.IsAdministrator) return false;
            if (_elevationAttempted) return false;

            _elevationAttempted = true;

            var opts = new CommandLineOptions(
                Path: path,
                Language: _localization.CurrentLanguage,
                ElevationAttempted: true);

            bool started = _elevation.TryRelaunchAsAdministrator(opts);
            if (started)
            {
                Environment.Exit(0);
                return true;
            }

            _messages.ShowInfo(T("Msg.ElevationCanceled", "Elevation canceled."));
            return false;
        }

        // ───────────────────────────────────────── Settings panel
        private void btnApply_Click(object? sender, EventArgs e)
        {
            try
            {
                if (treeProject.Font.FontFamily.Name != _pendingFontName)
                    treeProject.Font = new Font(_pendingFontName, _treeFontSize);

                _iconService.UpdateTreeItemHeight(treeProject);
                RefreshTree();
            }
            catch (Exception ex)
            {
                _messages.ShowException(ex);
            }
        }

        private void checkBoxAll_CheckedChanged(object? sender, EventArgs e)
        {
            bool selectAll = checkBoxAll.Checked;
            for (int i = 0; i < lstExtensions.Items.Count; i++)
                lstExtensions.SetItemChecked(i, selectAll);
        }

        private void cboFont_SelectedIndexChanged(object? sender, EventArgs e) =>
            _pendingFontName = (string?)cboFont.SelectedItem ?? _pendingFontName;

        private void LoadFonts()
        {
            cboFont.Items.AddRange(new[] { "Consolas", "Courier New", "Lucida Console", "Fira Code", "Times New Roman", "Tahoma" });
            cboFont.SelectedItem = "Consolas";
        }

        // ───────────────────────────────────────── Build / Refresh
        private void ReloadProject()
        {
            if (string.IsNullOrEmpty(_currentPath)) return;

            PopulateExtensions(_currentPath);
            PopulateRootFolders(_currentPath);

            RefreshTree();
        }

        private void RefreshTree()
        {
            if (string.IsNullOrEmpty(_currentPath)) return;

            var allowedExt = new HashSet<string>(lstExtensions.CheckedItems.Cast<string>(), StringComparer.OrdinalIgnoreCase);
            var allowedRoot = new HashSet<string>(lstRootFolders.CheckedItems.Cast<string>(), StringComparer.OrdinalIgnoreCase);

            var ignore = GetIgnoreOptions();

            var options = new TreeFilterOptions(
                AllowedExtensions: allowedExt,
                AllowedRootFolders: allowedRoot,
                IgnoreBin: ignore.IgnoreBin,
                IgnoreObj: ignore.IgnoreObj,
                IgnoreDot: ignore.IgnoreDot);

            UseWaitCursor = true;
            try
            {
                var result = _treeBuilder.Build(_currentPath, options);

                if (result.RootAccessDenied && TryElevateAndRestart(_currentPath))
                    return;

                _renderer.Render(treeProject, result.Root, expandAll: false);

                EnsureRootNodeVisible(_currentPath!);

                if (treeProject.Nodes.Count > 0)
                    treeProject.Nodes[0].Expand();

                _iconService.ApplyIconsToTree(treeProject);
                _iconService.UpdateTreeItemHeight(treeProject);
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        // ───────────────────────────────────────── Populate (extensions/root folders)
        private static readonly string[] _defaultExts = { ".cs", ".sln", ".csproj", ".designer" };

        private void PopulateExtensions(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            var prev = new HashSet<string>(lstExtensions.CheckedItems.Cast<string>(), StringComparer.OrdinalIgnoreCase);

            var ignore = GetIgnoreOptions();
            var scan = _scanner.GetExtensions(path, ignore.IgnoreBin, ignore.IgnoreObj, ignore.IgnoreDot);
            if (scan.RootAccessDenied && TryElevateAndRestart(path))
                return;

            lstExtensions.Items.Clear();

            foreach (var ext in scan.Value.OrderBy(e => e, StringComparer.OrdinalIgnoreCase))
            {
                bool chk = prev.Contains(ext) ||
                           (prev.Count == 0 && _defaultExts.Contains(ext, StringComparer.OrdinalIgnoreCase));

                lstExtensions.Items.Add(ext, chk);
            }

            if (checkBoxAll.Checked)
            {
                for (int i = 0; i < lstExtensions.Items.Count; i++)
                    lstExtensions.SetItemChecked(i, true);
            }
        }

        private void PopulateRootFolders(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            var prev = new HashSet<string>(lstRootFolders.CheckedItems.Cast<string>(), StringComparer.OrdinalIgnoreCase);

            var ignore = GetIgnoreOptions();
            var scan = _scanner.GetRootFolderNames(path, ignore.IgnoreBin, ignore.IgnoreObj, ignore.IgnoreDot);
            if (scan.RootAccessDenied && TryElevateAndRestart(path))
                return;

            lstRootFolders.Items.Clear();

            foreach (var name in scan.Value)
            {
                bool hiddenDot = name.StartsWith(".", StringComparison.Ordinal);
                bool isBin = name.Equals("bin", StringComparison.OrdinalIgnoreCase);
                bool isObj = name.Equals("obj", StringComparison.OrdinalIgnoreCase);

                bool chk = prev.Contains(name) ||
                           (prev.Count == 0 && !hiddenDot && !isBin && !isObj);

                lstRootFolders.Items.Add(name, chk);
            }
        }

        // ───────────────────────────────────────── Tree check behavior
        private void treeProject_AfterCheck(object? sender, TreeViewEventArgs e)
        {
            if (_suppressAfterCheck) return;

            try
            {
                _suppressAfterCheck = true;

                SetChildrenChecked(e.Node, e.Node.Checked);
                UpdateParentsChecked(e.Node.Parent);
            }
            finally
            {
                _suppressAfterCheck = false;
            }
        }

        private static void SetChildrenChecked(TreeNode node, bool isChecked)
        {
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = isChecked;
                SetChildrenChecked(child, isChecked);
            }
        }

        private static void UpdateParentsChecked(TreeNode? node)
        {
            while (node is not null)
            {
                bool all = true;

                foreach (TreeNode child in node.Nodes)
                {
                    if (!child.Checked)
                    {
                        all = false;
                        break;
                    }
                }

                node.Checked = all;
                node = node.Parent;
            }
        }

        // ───────────────────────────────────────── Zoom (Ctrl + Wheel)
        private void treeProject_MouseEnter(object? sender, EventArgs e) => treeProject.Focus();

        private void treeProject_MouseWheel(object? sender, MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Control) != Keys.Control)
                return;

            if (e.Delta > 0) ChangeTreeFontSize(+1f);
            else if (e.Delta < 0) ChangeTreeFontSize(-1f);
        }

        private void ChangeTreeFontSize(float delta) => SetTreeFontSize(_treeFontSize + delta);

        private void SetTreeFontSize(float size)
        {
            const float min = 6f;
            const float max = 28f;

            var clamped = Math.Max(min, Math.Min(max, size));
            _treeFontSize = clamped;

            var family = treeProject.Font.FontFamily.Name;
            treeProject.Font = new Font(family, _treeFontSize);

            _iconService.UpdateTreeItemHeight(treeProject);
        }
    }
}
