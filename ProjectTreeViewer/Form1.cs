using System.Text;

namespace ProjectTreeViewer
{
	public partial class Form1 : Form
	{
		private string? _currentPath;
		private string _pendingFontName = "Consolas";

		public Form1()
		{
			InitializeComponent();
			LoadFonts();
			_pendingFontName = (string?)cboFont.SelectedItem ?? "Consolas";
			SetButtonsEnabled(false);
		}

		// ───────────────────────────────────────── UI helpers
		private void SetButtonsEnabled(bool enabled)
		{
			btnRefresh.Enabled = enabled;
			btnCopy.Enabled = enabled;
		}

		private void UpdateTitle() =>
			Text = _currentPath is null
				? "Project Tree Viewer by Avazbek"
				: $"Project Tree Viewer - {_currentPath}";

		// ───────────────────────────────────────── Toolbar
		private void btnOpen_Click(object? sender, EventArgs e)
		{
			using var dlg = new FolderBrowserDialog { Description = "Выберите корневую папку проекта" };
			if (dlg.ShowDialog() != DialogResult.OK) return;

			_currentPath = dlg.SelectedPath;
			UpdateTitle();
			ReloadProject();
			SetButtonsEnabled(true);
		}

		private void btnRefresh_Click(object? sender, EventArgs e) => ReloadProject();

		// ───────────────────────────────────────── Settings
		private void btnSettings_Click(object? sender, EventArgs e) =>
			panelSettings.Visible = !panelSettings.Visible;

		private void btnApply_Click(object? sender, EventArgs e)
		{
			if (txtTree.Font.FontFamily.Name != _pendingFontName)
				txtTree.Font = new Font(_pendingFontName, txtTree.Font.Size);

			RefreshTree();
		}

		// чек-боксы — меняют только список
		private void cbIgnoreBin_CheckedChanged(object? s, EventArgs e) => PopulateRootFolders(_currentPath ?? "");
		private void cbIgnoreObj_CheckedChanged(object? s, EventArgs e) => PopulateRootFolders(_currentPath ?? "");
		private void cbIgnoreDot_CheckedChanged(object? s, EventArgs e) => PopulateRootFolders(_currentPath ?? "");

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
			txtTree.Text = BuildTree(_currentPath);
		}

		// ───────────────────────────────────────── Build-Tree
		private string BuildTree(string path)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"├── {new DirectoryInfo(path).Name}");

			var allowedExt = new HashSet<string>(lstExtensions.CheckedItems.Cast<string>(), StringComparer.OrdinalIgnoreCase);
			var allowedRoot = new HashSet<string>(lstRootFolders.CheckedItems.Cast<string>(), StringComparer.OrdinalIgnoreCase);

			PrintTree(path, "│   ", sb, allowedExt, allowedRoot,
					  cbIgnoreBin.Checked, cbIgnoreObj.Checked, cbIgnoreDot.Checked);

			return sb.ToString();
		}

		// ───────────────────────────────────────── Misc-UI
		private void btnCopy_Click(object? sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(txtTree.Text))
				Clipboard.SetText(txtTree.Text);
		}

		private void cboFont_SelectedIndexChanged(object? sender, EventArgs e) =>
			_pendingFontName = (string?)cboFont.SelectedItem ?? _pendingFontName;

		private void LoadFonts()
		{
			cboFont.Items.AddRange(new[] { "Consolas", "Courier New", "Lucida Console", "Fira Code", "Times New Roman", "Tahoma" });
			cboFont.SelectedItem = "Consolas";
		}

		// ───────────────────────────────────────── Populate
		private static readonly string[] _defaultExts = { ".cs", ".sln", ".csproj", ".designer" };

		private void PopulateExtensions(string path)
		{
			var prev = new HashSet<string>(lstExtensions.CheckedItems.Cast<string>(), StringComparer.OrdinalIgnoreCase);

			var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
			{
				if (ShouldSkipPath(file, path, cbIgnoreBin.Checked, cbIgnoreObj.Checked, cbIgnoreDot.Checked)) continue;
				var ext = Path.GetExtension(file);
				if (!string.IsNullOrEmpty(ext)) exts.Add(ext);
			}

			lstExtensions.Items.Clear();
			foreach (var ext in exts.OrderBy(e => e))
			{
				bool chk = prev.Contains(ext) ||
						   (prev.Count == 0 && _defaultExts.Contains(ext, StringComparer.OrdinalIgnoreCase));
				lstExtensions.Items.Add(ext, chk);
			}
		}

		private void PopulateRootFolders(string path)
		{
			var prev = new HashSet<string>(lstRootFolders.CheckedItems.Cast<string>(), StringComparer.OrdinalIgnoreCase);

			bool showBin = !cbIgnoreBin.Checked;
			bool showObj = !cbIgnoreObj.Checked;
			bool showDot = !cbIgnoreDot.Checked;

			lstRootFolders.Items.Clear();
			if (string.IsNullOrEmpty(path)) return;

			foreach (var dir in Directory.GetDirectories(path))
			{
				var name = Path.GetFileName(dir);
				bool hidden = name.StartsWith(".");
				bool isBin = name.Equals("bin", StringComparison.OrdinalIgnoreCase);
				bool isObj = name.Equals("obj", StringComparison.OrdinalIgnoreCase);

				if ((isBin && !showBin) || (isObj && !showObj) || (hidden && !showDot))
					continue;

				bool chk = prev.Contains(name) ||
						   (prev.Count == 0 && !hidden && !isBin && !isObj);

				lstRootFolders.Items.Add(name, chk);
			}
		}

		// ───────────────────────────────────────── Helpers
		private static bool ShouldSkipPath(string file, string root, bool ignoreBin, bool ignoreObj, bool ignoreDot)
		{
			var di = new DirectoryInfo(Path.GetDirectoryName(file)!);
			while (di.FullName.Length >= root.Length)
			{
				if ((ignoreBin && di.Name.Equals("bin", StringComparison.OrdinalIgnoreCase)) ||
					(ignoreObj && di.Name.Equals("obj", StringComparison.OrdinalIgnoreCase)) ||
					(ignoreDot && di.Name.StartsWith(".")))
					return true;

				if (di.FullName.Length == root.Length) break;
				di = di.Parent!;
			}

			return ignoreDot && Path.GetFileName(file).StartsWith(".");
		}

		private void PrintTree(
			string path,
			string indent,
			StringBuilder sb,
			HashSet<string> allowedExt,
			HashSet<string> allowedRoot,
			bool ignoreBin,
			bool ignoreObj,
			bool ignoreDot)
		{
			FileSystemInfo[] entries;
			try
			{
				entries = new DirectoryInfo(path)
					.GetFileSystemInfos()
					.Where(fi => (fi.Attributes & FileAttributes.Hidden) == 0)
					.OrderBy(fi => !fi.Attributes.HasFlag(FileAttributes.Directory))
					.ThenBy(fi => fi.Name)
					.ToArray();
			}
			catch { return; }

			for (int i = 0; i < entries.Length; i++)
			{
				var e = entries[i];
				var name = e.Name;
				bool isDir = e.Attributes.HasFlag(FileAttributes.Directory);

				if (isDir && path == _currentPath && !allowedRoot.Contains(name)) continue;

				if (isDir)
				{
					if ((ignoreBin && name.Equals("bin", StringComparison.OrdinalIgnoreCase)) ||
						(ignoreObj && name.Equals("obj", StringComparison.OrdinalIgnoreCase)) ||
						(ignoreDot && name.StartsWith("."))) continue;
				}
				else
				{
					if (ignoreDot && name.StartsWith(".")) continue;
					var ext = Path.GetExtension(name);
					if (allowedExt.Count > 0 && !allowedExt.Contains(ext)) continue;
				}

				bool last = i == entries.Length - 1;
				sb.Append(indent).Append(last ? "└── " : "├── ").AppendLine(name);

				if (isDir)
				{
					var nxt = indent + (last ? "    " : "│   ");
					PrintTree(e.FullName, nxt, sb, allowedExt, allowedRoot, ignoreBin, ignoreObj, ignoreDot);
				}
			}
		}
	}
}
