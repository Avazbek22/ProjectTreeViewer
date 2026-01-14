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

		private bool _elevationAttempted;
		private const string ClipboardBlankLine = "\u00A0"; // NBSP: выглядит как пустая строка, но не “схлопывается” при вставке

		private static void AppendClipboardBlankLine(StringBuilder sb) =>
			sb.AppendLine(ClipboardBlankLine);


		public Form1() : this(CommandLineOptions.Empty)
		{
		}

		public Form1(CommandLineOptions startupOptions)
		{
			_startupOptions = startupOptions;

			InitializeComponent();

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

			SetMenuEnabled(false);
			ApplyLocalization();

			Shown += Form1_Shown;
		}

		private void treeProject_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
		{
			treeProject.BeginUpdate();
			try
			{
				// раскрываем строго 1 уровень: все потомки делаем свернутыми
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


		private void RemoveUnneededMenuMargins()
		{
			// Везде убираем серую колонку
			ConfigureDropDownMenu(miFile, showCheckMargin: false);
			ConfigureDropDownMenu(miCopy, showCheckMargin: false);
			ConfigureDropDownMenu(miView, showCheckMargin: false);
			ConfigureDropDownMenu(miOptions, showCheckMargin: false);
			ConfigureDropDownMenu(miHelp, showCheckMargin: false);

			// В “Язык” оставляем чекмарки
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
					// Для вложенных меню по умолчанию тоже убираем колонку.
					// Если в будущем добавите “галочные” пункты — включите там showCheckMargin вручную.
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

			miView.Text = _localization["Menu.View"];
			miViewExpandAll.Text = _localization["Menu.View.ExpandAll"];
			miViewCollapseAll.Text = _localization["Menu.View.CollapseAll"];
			miViewZoomIn.Text = _localization["Menu.View.ZoomIn"];
			miViewZoomOut.Text = _localization["Menu.View.ZoomOut"];
			miViewZoomReset.Text = _localization["Menu.View.ZoomReset"];

			miOptions.Text = _localization["Menu.Options"];
			miOptionsTreeSettings.Text = _localization["Menu.Options.TreeSettings"];

			miLanguage.Text = _localization["Menu.Language"];

			miHelp.Text = _localization["Menu.Help"];
			miHelpAbout.Text = _localization["Menu.Help.About"];

			// В панели настроек
			cbIgnoreBin.Text = _localization["Settings.IgnoreBin"];
			cbIgnoreObj.Text = _localization["Settings.IgnoreObj"];
			cbIgnoreDot.Text = _localization["Settings.IgnoreDot"];
			checkBoxAll.Text = _localization["Settings.All"];
			labelExtensions.Text = _localization["Settings.Extensions"];
			labelRootFolders.Text = _localization["Settings.RootFolders"];
			labelFont.Text = _localization["Settings.Font"];
			btnApply.Text = _localization["Settings.Apply"];

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

			miViewExpandAll.Enabled = enabled;
			miViewCollapseAll.Enabled = enabled;

			miOptionsTreeSettings.Enabled = enabled;
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

		private void miOptionsTreeSettings_Click(object? sender, EventArgs e) =>
			panelSettings.Visible = !panelSettings.Visible;

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
				Clipboard.SetText(text);
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
					_messages.ShowInfo(_localization["Msg.NoCheckedTree"]);
					return;
				}

				Clipboard.SetText(text);
			}
			catch (Exception ex)
			{
				_messages.ShowException(ex);
			}
		}

		private void miCopySelectedContent_Click(object? sender, EventArgs e)
		{
			if (treeProject.Nodes.Count == 0) return;

			var files = GetCheckedFilePaths(treeProject.Nodes)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
				.ToList();

			if (files.Count == 0)
			{
				MessageBox.Show(
					"Не выбрано ни одного файла.",
					"Копирование",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);

				return;
			}

			var sb = new StringBuilder();

			for (int i = 0; i < files.Count; i++)
			{
				var file = files[i];

				sb.AppendLine($"{file}:");

				// после пути — 1 “пустая” строка (не схлопнется)
				AppendClipboardBlankLine(sb);

				try
				{
					sb.AppendLine(ReadFileTextForClipboard(file));
				}
				catch (Exception ex)
				{
					sb.AppendLine($"[Не удалось прочитать файл: {ex.Message}]");
				}

				// перед следующим путём — 2 “пустые” строки (не схлопнутся)
				if (i < files.Count - 1)
				{
					AppendClipboardBlankLine(sb);
					AppendClipboardBlankLine(sb);
				}
			}

			Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText);
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

		private static string ReadFileTextForClipboard(string path)
		{
			// Для стабильного форматирования под ИИ:
			// 1) читаем как текст (UTF8)
			// 2) убираем только хвостовые переносы строк, чтобы не ломать “пустые строки между файлами”
			var text = File.ReadAllText(path, Encoding.UTF8);

			// Быстрая защита от бинарщины: если в тексте есть '\0' — считаем бинарным
			if (text.IndexOf('\0') >= 0)
				return "[Файл выглядит как бинарный, содержимое не вставлено]";

			return text.TrimEnd('\r', '\n');
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

			// Если корень не читается — повышаем права и перезапускаем (один раз)
			if (!_scanner.CanReadRoot(path))
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

				RefreshTree();
			}
			catch (Exception ex)
			{
				_messages.ShowException(ex);
			}
		}

		private void cbIgnoreBin_CheckedChanged(object? s, EventArgs e)
		{
			PopulateRootFolders(_currentPath ?? "");
			PopulateExtensions(_currentPath ?? "");
		}

		private void cbIgnoreObj_CheckedChanged(object? s, EventArgs e)
		{
			PopulateRootFolders(_currentPath ?? "");
			PopulateExtensions(_currentPath ?? "");
		}

		private void cbIgnoreDot_CheckedChanged(object? s, EventArgs e)
		{
			PopulateRootFolders(_currentPath ?? "");
			PopulateExtensions(_currentPath ?? "");
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

			var options = new TreeFilterOptions(
				AllowedExtensions: allowedExt,
				AllowedRootFolders: allowedRoot,
				IgnoreBin: cbIgnoreBin.Checked,
				IgnoreObj: cbIgnoreObj.Checked,
				IgnoreDot: cbIgnoreDot.Checked);

			UseWaitCursor = true;
			try
			{
				var result = _treeBuilder.Build(_currentPath, options);

				// Если корень недоступен — повышаем права
				if (result.RootAccessDenied && TryElevateAndRestart(_currentPath))
					return;

				// Требование: раскрыть на 100%
				// Строим дерево без ExpandAll — раскрытие будет “слой за слоем”
				_renderer.Render(treeProject, result.Root, expandAll: false);

				// По умолчанию раскрываем только корень (первый уровень виден сразу)
				if (treeProject.Nodes.Count > 0)
					treeProject.Nodes[0].Expand();

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

			var scan = _scanner.GetExtensions(path, cbIgnoreBin.Checked, cbIgnoreObj.Checked, cbIgnoreDot.Checked);
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

			var scan = _scanner.GetRootFolderNames(path, cbIgnoreBin.Checked, cbIgnoreObj.Checked, cbIgnoreDot.Checked);
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
		}
	}
}
