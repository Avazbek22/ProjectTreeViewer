using System.Text;

namespace ProjectTreeViewer
{
	public partial class Form1 : Form
	{
		private string _currentPath;

		public Form1()
		{
			InitializeComponent();
			LoadFonts();
		}

		private void LoadFonts()
		{
			cboFont.Items.AddRange(new[]
			{
				"Consolas",
				"Courier New",
				"Lucida Console",
				"Fira Code",
				"Times New Roman",
				"Tahoma"
			});
			cboFont.SelectedItem = "Consolas";
		}

		private void btnSettings_Click(object sender, EventArgs e)
		{
			panelSettings.Visible = !panelSettings.Visible;
		}

		private void btnOpen_Click(object sender, EventArgs e)
		{
			using var dlg = new FolderBrowserDialog { Description = "Выберите корневую папку проекта" };
			if (dlg.ShowDialog() != DialogResult.OK) return;

			_currentPath = dlg.SelectedPath;
			PopulateExtensions(_currentPath);
			PopulateRootFolders(_currentPath);
			RefreshTree();
		}

		private void PopulateExtensions(string path)
		{
			var ignoreBin = cbIgnoreBin.Checked;
			var ignoreObj = cbIgnoreObj.Checked;
			var ignoreDot = cbIgnoreDot.Checked;
			var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
			{
				var di = new DirectoryInfo(Path.GetDirectoryName(file) ?? "");
				bool skip = false;
				while (di.FullName.Length >= path.Length)
				{
					if ((ignoreBin && di.Name.Equals("bin", StringComparison.OrdinalIgnoreCase)) ||
						(ignoreObj && di.Name.Equals("obj", StringComparison.OrdinalIgnoreCase)) ||
						(ignoreDot && di.Name.StartsWith(".")))
					{
						skip = true; 
						break;
					}
					if (di.FullName.Length == path.Length) 
						break;
					
					di = di.Parent;
				}
				if (skip) 
					continue;
				
				var name = Path.GetFileName(file);
				if (ignoreDot && name.StartsWith(".")) 
					continue;
				var ext = Path.GetExtension(name);
				if (!string.IsNullOrEmpty(ext)) 
					exts.Add(ext);
			}

			lstExtensions.Items.Clear();
			foreach (var ext in exts.OrderBy(e => e))
			{
				bool isDef = new[] { ".cs", ".sln", ".csproj", ".designer" }
							 .Contains(ext, StringComparer.OrdinalIgnoreCase);
				lstExtensions.Items.Add(ext, isDef);
			}
		}

		private void PopulateRootFolders(string path)
		{
			lstRootFolders.Items.Clear();
			try
			{
				foreach (var dir in Directory.GetDirectories(path))
				{
					var name = Path.GetFileName(dir);
					bool isHidden = name.StartsWith(".");
					bool isDef = !(isHidden
								   || name.Equals("bin", StringComparison.OrdinalIgnoreCase)
								   || name.Equals("obj", StringComparison.OrdinalIgnoreCase));
					lstRootFolders.Items.Add(name, isDef);
				}
			}
			catch
			{
				// if something unavailable, skip
			}
		}

		private void btnApply_Click(object sender, EventArgs e)
		{
			RefreshTree();
		}

		private void RefreshTree()
		{
			if (string.IsNullOrEmpty(_currentPath)) return;
			txtTree.Text = BuildTree(_currentPath);
		}

		private void btnCopy_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(txtTree.Text))
				Clipboard.SetText(txtTree.Text);
		}

		private void cboFont_SelectedIndexChanged(object sender, EventArgs e)
		{
			txtTree.Font = new Font(cboFont.SelectedItem as string ?? "Consolas", txtTree.Font.Size);
		}

		private string BuildTree(string path)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"├── {new DirectoryInfo(path).Name}");

			var ignoreBin = cbIgnoreBin.Checked;
			var ignoreObj = cbIgnoreObj.Checked;
			var ignoreDot = cbIgnoreDot.Checked;
			var allowedExt = new HashSet<string>(lstExtensions.CheckedItems.Cast<string>(),
												   StringComparer.OrdinalIgnoreCase);
			var allowedRoot = new HashSet<string>(lstRootFolders.CheckedItems.Cast<string>(),
												   StringComparer.OrdinalIgnoreCase);

			PrintTree(path, "│   ", sb, allowedExt, allowedRoot, ignoreBin, ignoreObj, ignoreDot);
			return sb.ToString();
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

				// filer folders
				if (isDir && path == _currentPath && !allowedRoot.Contains(name))
					continue;

				if (isDir)
				{
					if ((ignoreBin && name.Equals("bin", StringComparison.OrdinalIgnoreCase)) ||
						(ignoreObj && name.Equals("obj", StringComparison.OrdinalIgnoreCase)) ||
						(ignoreDot && name.StartsWith(".")))
						continue;
				}
				else
				{
					if (ignoreDot && name.StartsWith(".")) 
						continue;
					
					var ext = Path.GetExtension(name);
					
					if (allowedExt.Count > 0 && !allowedExt.Contains(ext))
						continue;
				}

				bool last = i == entries.Length - 1;
				sb.Append(indent).Append(last ? "└── " : "├── ").AppendLine(name);

				if (isDir)
				{
					var nextIndent = indent + (last ? "    " : "│   ");
					PrintTree(e.FullName, nextIndent, sb, allowedExt, allowedRoot, ignoreBin, ignoreObj, ignoreDot);
				}
			}
		}
	}
}
