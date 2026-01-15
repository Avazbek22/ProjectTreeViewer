using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ProjectTreeViewer.Application.Services;
using ProjectTreeViewer.Application.UseCases;
using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Contracts;
using ProjectTreeViewer.Kernel.Models;
using ProjectTreeViewer.WinForms.Services;

namespace ProjectTreeViewer.WinForms
{
    public partial class Form1 : Form
    {
		private string? _currentPath;
		private string _pendingFontName = "Consolas";
		private bool _suppressAfterCheck;
		private bool _suppressIgnoreItemCheck;

        private float _treeFontSize;

        private readonly CommandLineOptions _startupOptions;

        private readonly LocalizationService _localization;
        private readonly MessageService _messages;
        private readonly IElevationService _elevation;
        private readonly ScanOptionsUseCase _scanOptions;
        private readonly BuildTreeUseCase _buildTree;
        private readonly TreeViewRenderer _renderer;
        private readonly TreeExportService _treeExport;
        private readonly SelectedContentExportService _contentExport;
        private readonly TreeSelectionService _selection;
        private readonly IIconStore _iconStore;

        private BuildTreeResult? _currentTree;

        private bool _elevationAttempted;

		private const string
			ClipboardBlankLine = "\u00A0"; // NBSP: looks like an empty line, but does NOT collapse on paste

		private const int IgnoreIndexBin = 0;
		private const int IgnoreIndexObj = 1;
		private const int IgnoreIndexDot = 2;

        private static void AppendClipboardBlankLine(StringBuilder sb) => sb.AppendLine(ClipboardBlankLine);

        private const int TreeIconSize = 24;

        private ImageList? _treeImages;

        public Form1() : this(CommandLineOptions.Empty, WinFormsCompositionRoot.CreateDefault(CommandLineOptions.Empty))
        {
        }
        public Form1(CommandLineOptions startupOptions, WinFormsAppServices services)
        {
            _startupOptions = startupOptions;
            _localization = services.Localization;
            _messages = new MessageService(_localization);
            _elevation = services.Elevation;
            _scanOptions = services.ScanOptionsUseCase;
            _buildTree = services.BuildTreeUseCase;
            _treeExport = services.TreeExportService;
            _contentExport = services.ContentExportService;
            _renderer = services.TreeViewRenderer;
            _selection = services.TreeSelectionService;
            _iconStore = services.IconStore;

            InitializeComponent();

            SetupStableLayout();

            InitTreeIcons();

            treeProject.BeforeExpand -= treeProject_BeforeExpand;
            treeProject.BeforeExpand += treeProject_BeforeExpand;

            RemoveUnneededMenuMargins();

            _treeFontSize = treeProject.Font.Size;

            _elevationAttempted = startupOptions.ElevationAttempted;

            _localization.LanguageChanged += (_, _) => ApplyLocalization();

            LoadFonts();
            _pendingFontName = (string?)cboFont.SelectedItem ?? "Consolas";

            InitIgnoreList();

            SetMenuEnabled(false);
            ApplyLocalization();

            Shown += Form1_Shown;
        }

        // ───────────────────────────────────────── Layout: MenuStrip must stay on top
        private void EnsureDockLayout()
        {
            // Stable manual layout:
            // Menu stays on top, settings panel is on the right, tree uses the remaining space.
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

            // Tree always starts from the left and never goes under the panel.
            treeProject.Location = new Point(0, top);
            treeProject.Size = new Size(
                Math.Max(0, ClientSize.Width - panelWidth),
                Math.Max(0, ClientSize.Height - top));

            // Panel is always on the right.
            panelSettings.Location = new Point(ClientSize.Width - panelSettings.Width, top);
            panelSettings.Height = Math.Max(0, ClientSize.Height - top);

            // Keep menu always above everything.
            menuStripMain.BringToFront();
			if (panelSettings.Visible)
				panelSettings.BringToFront();
		}

		private void SetupStableLayout()
		{
			EnsureDockLayout();
			Resize += (_, _) => ApplyStableLayout();
			panelSettings.VisibleChanged += (_, _) => ApplyStableLayout();
		}

		private void UpdateTreeItemHeight()
		{
			var textHeight = TextRenderer.MeasureText("Wg", treeProject.Font).Height + 4;
			treeProject.ItemHeight = Math.Max(textHeight, TreeIconSize + 2);
		}

		private void EnsureRootNodeVisible(string rootPath)
		{
			if (treeProject.Nodes.Count == 0) return;

			var rootName = new DirectoryInfo(rootPath).Name;
			var node = treeProject.Nodes[0];
			node.Text = rootName;
			node.Tag = rootPath;
		}



        // ───────────────────────────────────────── Ignore list init (replaces 3 left checkboxes)
        private void InitIgnoreList()
        {
            // These controls are added in the updated Form1.Designer.cs (you asked me to send it after).
            // Defaults match your previous checkboxes: all three are checked.
            _suppressIgnoreItemCheck = true;
            try
            {
                lstIgnore.CheckOnClick = true;
                lstIgnore.Items.Clear();

                lstIgnore.Items.Add(_localization["Settings.IgnoreBin"], true);
                lstIgnore.Items.Add(_localization["Settings.IgnoreObj"], true);
                lstIgnore.Items.Add(_localization["Settings.IgnoreDot"], true);

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
                    lstIgnore.Items.Add(_localization["Settings.IgnoreBin"], bin);
                    lstIgnore.Items.Add(_localization["Settings.IgnoreObj"], obj);
                    lstIgnore.Items.Add(_localization["Settings.IgnoreDot"], dot);
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

            // ItemCheck fires BEFORE the state changes. Update lists after WinForms applies the new check state.
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

        // ───────────────────────────────────────── Tree icons init
        // ───────────────────────────────────────── Tree icons init
        private void InitTreeIcons()
        {
            _treeImages = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(TreeIconSize, TreeIconSize)
            };

            foreach (var key in _iconStore.Keys)
            {
                using var ms = new MemoryStream(_iconStore.GetIconBytes(key));
                using var img = Image.FromStream(ms);
                _treeImages.Images.Add(key, (Image)img.Clone());
            }

            treeProject.ImageList = _treeImages;
            UpdateTreeItemHeight();
        }

        private void RemoveUnneededMenuMargins()
        {
            // Remove image margin everywhere to avoid the gray column.
            ConfigureDropDownMenu(miFile, showCheckMargin: false);
            ConfigureDropDownMenu(miCopy, showCheckMargin: false);
            ConfigureDropDownMenu(miView, showCheckMargin: false);
            ConfigureDropDownMenu(miOptions, showCheckMargin: false);
            ConfigureDropDownMenu(miHelp, showCheckMargin: false);

            // Language menu uses checked items, so we keep check margin there.
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
                {
                    // Default for submenus: no check margin.
                    // If you add checkable items later, enable explicitly for that submenu.
                    ConfigureDropDownMenu(childMi, showCheckMargin: false);
                }
            }
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_startupOptions.Path))
                TryOpenFolder(_startupOptions.Path!, fromDialog: false);
        }

        // ───────────────────────────────────────── Localization
        private void ApplyLocalization()
        {
            miFile.Text = _localization["Menu.File"];
            miFileOpen.Text = _localization["Menu.File.Open"];
            miFileRefresh.Text = _localization["Menu.File.Refresh"];
            miFileExit.Text = _localization["Menu.File.Exit"];

            miCopy.Text = _localization["Menu.Copy"];
            miCopyFullTree.Text = _localization["Menu.Copy.FullTree"];
            miCopySelectedTree.Text = _localization["Menu.Copy.SelectedTree"];
            miCopySelectedContent.Text = _localization["Menu.Copy.SelectedContent"];

            // Fix #4: new copy option (localized here without touching LocalizationCatalog)
            miCopyFullTreeAndContent.Text = _localization["Menu.Copy.FullTreeAndContent"];

            miView.Text = _localization["Menu.View"];
            miViewExpandAll.Text = _localization["Menu.View.ExpandAll"];
            miViewCollapseAll.Text = _localization["Menu.View.CollapseAll"];
            miViewZoomIn.Text = _localization["Menu.View.ZoomIn"];
            miViewZoomOut.Text = _localization["Menu.View.ZoomOut"];
            miViewZoomReset.Text = _localization["Menu.View.ZoomReset"];

            miOptions.Text = _localization["Menu.Options"];

            miLanguage.Text = _localization["Menu.Language"];

            miLangRu.Text = _localization["Language.Ru"];
            miLangEn.Text = _localization["Language.En"];
            miLangUz.Text = _localization["Language.Uz"];
            miLangTg.Text = _localization["Language.Tg"];
            miLangKk.Text = _localization["Language.Kk"];
            miLangFr.Text = _localization["Language.Fr"];
            miLangDe.Text = _localization["Language.De"];
            miLangIt.Text = _localization["Language.It"];

            miHelp.Text = _localization["Menu.Help"];
            miHelpAbout.Text = _localization["Menu.Help.About"];

            // Settings panel
            labelIgnore.Text = _localization["Settings.IgnoreTitle"];
            checkBoxAll.Text = _localization["Settings.All"];
            labelExtensions.Text = _localization["Settings.Extensions"];
            labelRootFolders.Text = _localization["Settings.RootFolders"];
            labelFont.Text = _localization["Settings.Font"];
            btnApply.Text = _localization["Settings.Apply"];

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
            miCopyFullTreeAndContent.Enabled = enabled; // Fix #4

            miViewExpandAll.Enabled = enabled;
            miViewCollapseAll.Enabled = enabled;

            miOptions.Enabled = enabled;
        }

        private void UpdateTitle()
        {
            Text = _currentPath is null
                ? _localization["Title.Default"]
                : _localization.Format("Title.WithPath", _currentPath);
        }

        // ───────────────────────────────────────── Menu actions
        private void miFileOpen_Click(object? sender, EventArgs e)
        {
            try
            {
                using var dlg = new FolderBrowserDialog { Description = _localization["Dialog.SelectRoot"] };
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

		private void miOptions_Click(object? sender, EventArgs e)
		{
			panelSettings.Visible = !panelSettings.Visible;
			ApplyStableLayout();
		}

		private void miCopyFullTree_Click(object? sender, EventArgs e)
		{
			try
			{
				if (!EnsureTreeReady()) return;

				var text = _treeExport.BuildFullTree(_currentPath!, _currentTree!.Root);
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

				var selectedPaths = new HashSet<string>(_selection.GetCheckedPaths(treeProject.Nodes), StringComparer.OrdinalIgnoreCase);
				var text = _treeExport.BuildSelectedTree(_currentPath!, _currentTree!.Root, selectedPaths);

				if (string.IsNullOrWhiteSpace(text))
				{
					_messages.ShowInfo(_localization["Msg.NoCheckedTree"]);
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
				if (!EnsureTreeReady()) return;

				var files = _selection.GetCheckedPaths(treeProject.Nodes)
					.Where(File.Exists)
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
					.ToList();

				if (files.Count == 0)
				{
					_messages.ShowInfo(_localization["Msg.NoCheckedFiles"]);
					return;
				}

				var content = BuildContentForClipboard(files);
				if (string.IsNullOrWhiteSpace(content))
				{
					_messages.ShowInfo(_localization["Msg.NoTextContent"]);
					return;
				}

				Clipboard.SetText(content, TextDataFormat.UnicodeText);
			}
			catch (Exception ex)
			{
				_messages.ShowException(ex);
			}
		}

		private void miCopyFullTreeAndContent_Click(object? sender, EventArgs e)
		{
			try
			{
				if (!EnsureTreeReady()) return;

				var files = _selection.GetCheckedPaths(treeProject.Nodes)
					.Where(File.Exists)
					.Distinct(StringComparer.OrdinalIgnoreCase)
					.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
					.ToList();

				if (files.Count == 0)
				{
					_messages.ShowInfo(_localization["Msg.NoCheckedFiles"]);
					return;
				}

				var content = BuildContentForClipboard(files);
				if (string.IsNullOrWhiteSpace(content))
				{
					_messages.ShowInfo(_localization["Msg.NoTextContent"]);
					return;
				}

				var treeText = _treeExport.BuildFullTree(_currentPath!, _currentTree!.Root);

				var sb = new StringBuilder(treeText);
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

		private bool EnsureTreeReady()
		{
			if (string.IsNullOrWhiteSpace(_currentPath)) return false;
			return _currentTree is not null;
		}

		private string BuildContentForClipboard(IEnumerable<string> files) => _contentExport.Build(files);

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
            _messages.ShowInfo(_localization["Msg.AboutStub"]);
        }

        // ───────────────────────────────────────── Open folder / elevation
        private void TryOpenFolder(string path, bool fromDialog)
        {
            if (!Directory.Exists(path))
            {
                _messages.ShowErrorFormat("Msg.PathNotFound", path);
                return;
            }

            if (!_scanOptions.CanReadRoot(path))
            {
                if (TryElevateAndRestart(path)) return;

                _messages.ShowError(_localization["Msg.AccessDeniedRoot"]);
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

            _messages.ShowInfo(_localization["Msg.ElevationCanceled"]);
            return false;
        }

        // ───────────────────────────────────────── Settings panel
        private void btnApply_Click(object? sender, EventArgs e)
        {
            try
            {
                if (treeProject.Font.FontFamily.Name != _pendingFontName)
                    treeProject.Font = new Font(_pendingFontName, _treeFontSize);

                UpdateTreeItemHeight();
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
            cboFont.Items.AddRange(new[]
                { "Consolas", "Courier New", "Lucida Console", "Fira Code", "Times New Roman", "Tahoma" });
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

            var allowedExt = new HashSet<string>(lstExtensions.CheckedItems.Cast<string>(),
                StringComparer.OrdinalIgnoreCase);
            var allowedRoot = new HashSet<string>(lstRootFolders.CheckedItems.Cast<string>(),
                StringComparer.OrdinalIgnoreCase);

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
				var result = _buildTree.Execute(new BuildTreeRequest(_currentPath, options));
				_currentTree = result;

                if (result.RootAccessDenied && TryElevateAndRestart(_currentPath))
                    return;

                // Render without ExpandAll: expansion is "layer-by-layer"
                _renderer.Render(treeProject, result.Root, expandAll: false);

                // Fix: always show the real root folder as the first line in the TreeView
                EnsureRootNodeVisible(_currentPath!);

                if (treeProject.Nodes.Count > 0)
                    treeProject.Nodes[0].Expand();

				UpdateTreeItemHeight();
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
            var scan = _scanOptions.Execute(new ScanOptionsRequest(path, new IgnoreRules(ignore.IgnoreBin, ignore.IgnoreObj, ignore.IgnoreDot)));
            if (scan.RootAccessDenied && TryElevateAndRestart(path))
                return;

            lstExtensions.Items.Clear();

            foreach (var ext in scan.Extensions.OrderBy(e => e, StringComparer.OrdinalIgnoreCase))
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

            var prev = new HashSet<string>(lstRootFolders.CheckedItems.Cast<string>(),
                StringComparer.OrdinalIgnoreCase);

            var ignore = GetIgnoreOptions();
            var scan = _scanOptions.Execute(new ScanOptionsRequest(path, new IgnoreRules(ignore.IgnoreBin, ignore.IgnoreObj, ignore.IgnoreDot)));
            if (scan.RootAccessDenied && TryElevateAndRestart(path))
                return;

            lstRootFolders.Items.Clear();

            foreach (var name in scan.RootFolders)
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
		private void treeProject_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
		{
			UpdateTreeItemHeight();
		}

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

            UpdateTreeItemHeight();
        }
    }
}
