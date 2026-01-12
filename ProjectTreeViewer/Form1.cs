using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ProjectTreeViewer
{
	public partial class Form1 : Form
	{
		private string? _currentPath;
		private string _pendingFontName = "Consolas";

		private readonly CommandLineOptions _startupOptions;
		private readonly LocalizationService _localization;
		private readonly ElevationService _elevationService = new();
		private readonly FileSystemScanner _fileSystemScanner = new();
		private readonly ProjectTreeService _projectTreeService = new();

		private bool _elevationAttempted;

		public Form1() : this(CommandLineOptions.Empty)
		{
		}

		public Form1(CommandLineOptions startupOptions)
		{
			_startupOptions = startupOptions;
			_elevationAttempted = startupOptions.ElevationAttempted;

			InitializeComponent();

			_localization = new LocalizationService(startupOptions.Language ?? LocalizationService.DetectSystemLanguage());
			_localization.LanguageChanged += (_, _) => ApplyLocalization();

			LoadFonts();
			_pendingFontName = (string?)cboFont.SelectedItem ?? "Consolas";
			SetButtonsEnabled(false);

			ApplyLocalization();

			Shown += Form1_Shown;
		}

		private void Form1_Shown(object? sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(_startupOptions.Path))
				return;

			TryOpenFolder(_startupOptions.Path!, openDialog: false);
		}

		// ───────────────────────────────────────── UI helpers
		private void SetButtonsEnabled(bool enabled)
		{
			btnRefresh.Enabled = enabled;
			btnCopy.Enabled = enabled;
		}

		private void UpdateTitle()
		{
			Text = _currentPath is null
				? _localization["Title.Default"]
				: _localization.Format("Title.WithPath", _currentPath);
		}

		private void ApplyLocalization()
		{
			btnOpen.Text = _localization["Button.Open"];
			btnCopy.Text = _localization["Button.Copy"];
			btnRefresh.Text = _localization["Button.Refresh"];
			btnSettings.Text = _localization["Button.Settings"];
			btnApply.Text = _localization["Button.Apply"];

			labelExtensions.Text = _localization["Label.Extensions"];
			labelRootFolders.Text = _localization["Label.RootFolders"];
			labelFont.Text = _localization["Label.Font"];

			checkBoxAll.Text = _localization["Check.All"];
			cbIgnoreBin.Text = _localization["Check.IgnoreBin"];
			cbIgnoreObj.Text = _localization["Check.IgnoreObj"];
			cbIgnoreDot.Text = _localization["Check.IgnoreDot"];

			// Кнопка показывает язык, НА КОТОРЫЙ переключится
			btnLanguage.Text = _localization.CurrentLanguage == AppLanguage.Ru ? "EN" : "RU";

			UpdateTitle();
		}

		// ───────────────────────────────────────── Toolbar
		private void btnOpen_Click(object? sender, EventArgs e)
		{
			using var dlg = new FolderBrowserDialog { Description = _localization["Dialog.SelectRoot"] };
			if (dlg.ShowDialog() != DialogResult.OK) return;

			TryOpenFolder(dlg.SelectedPath, openDialog: true);
		}

		private void TryOpenFolder(string path, bool openDialog)
		{
			if (!Directory.Exists(path))
			{
				MessageBox.Show(
					_localization.Format("Msg.PathNotFound", path),
					_localization["Msg.ErrorTitle"],
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);

				return;
			}

			_currentPath = path;
			UpdateTitle();
			SetButtonsEnabled(true);

			ReloadProject();
		}

		private void btnRefresh_Click(object? sender, EventArgs e) => ReloadProject();

		private void btnCopy_Click(object? sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(txtTree.Text))
				Clipboard.SetText(txtTree.Text);
		}

		private void btnSettings_Click(object? sender, EventArgs e) =>
			panelSettings.Visible = !panelSettings.Visible;

		private void btnLanguage_Click(object? sender, EventArgs e) =>
			_localization.ToggleLanguage();

		// ───────────────────────────────────────── Settings
		private void btnApply_Click(object? sender, EventArgs e)
		{
			if (txtTree.Font.FontFamily.Name != _pendingFontName)
				txtTree.Font = new Font(_pendingFontName, txtTree.Font.Size);

			RefreshTree();
		}

		// чек-боксы — меняют только список (дерево трогаем только Apply)
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

		// "Все" — просто отмечает/снимает все расширения (без обновления дерева)
		private void checkBox1_CheckedChanged(object? sender, EventArgs e)
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

		// ───────────────────────────────────────── Core
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

			var options = new ProjectTreeOptions(
				AllowedExtensions: allowedExt,
				AllowedRootFolders: allowedRoot,
				IgnoreBin: cbIgnoreBin.Checked,
				IgnoreObj: cbIgnoreObj.Checked,
				IgnoreDot: cbIgnoreDot.Checked,
				IncludePathHeader: true);

			var result = _projectTreeService.BuildTree(_currentPath, options);
			txtTree.Text = result.Text;

			// Автозапуск от админа только если корень реально недоступен
			if (result.RootAccessDenied)
				TryElevateAndRestart(_currentPath);
		}

		// ───────────────────────────────────────── Populate
		private static readonly string[] _defaultExts = { ".cs", ".sln", ".csproj", ".designer" };

		private void PopulateExtensions(string path)
		{
			if (string.IsNullOrEmpty(path)) return;

			var prev = new HashSet<string>(lstExtensions.CheckedItems.Cast<string>(), StringComparer.OrdinalIgnoreCase);

			var scan = _fileSystemScanner.GetExtensions(path, cbIgnoreBin.Checked, cbIgnoreObj.Checked, cbIgnoreDot.Checked);
			if (scan.RootAccessDenied)
				TryElevateAndRestart(path);

			lstExtensions.Items.Clear();

			foreach (var ext in scan.Value.OrderBy(e => e, StringComparer.OrdinalIgnoreCase))
			{
				bool chk = prev.Contains(ext) ||
						   (prev.Count == 0 && _defaultExts.Contains(ext, StringComparer.OrdinalIgnoreCase));

				lstExtensions.Items.Add(ext, chk);
			}

			// Если "Все" включено — просто отметить все (дерево не трогаем до Apply)
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

			var scan = _fileSystemScanner.GetRootFolderNames(path, cbIgnoreBin.Checked, cbIgnoreObj.Checked, cbIgnoreDot.Checked);
			if (scan.RootAccessDenied)
				TryElevateAndRestart(path);

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

		// ───────────────────────────────────────── Admin
		private void TryElevateAndRestart(string path)
		{
			if (_elevationService.IsAdministrator) return;
			if (_elevationAttempted) return;

			_elevationAttempted = true;

			var options = new CommandLineOptions(
				Path: path,
				Language: _localization.CurrentLanguage,
				ElevationAttempted: true);

			bool started = _elevationService.TryRelaunchAsAdministrator(options);
			if (started)
			{
				Environment.Exit(0);
				return;
			}

			MessageBox.Show(
				_localization["Msg.ElevationCanceled"],
				_localization["Msg.ErrorTitle"],
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}
	}
}
