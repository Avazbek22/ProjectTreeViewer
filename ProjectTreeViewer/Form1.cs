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

		private const string ClipboardBlankLine = "\u00A0"; // NBSP: looks like an empty line, but does NOT collapse on paste
		private static void AppendClipboardBlankLine(StringBuilder sb) => sb.AppendLine(ClipboardBlankLine);

		// ───────────────────────────────────────── Tree icons
		// NOTE:
		// - TreeView does NOT scale icons with font zoom automatically.
		// - Icons are strictly controlled by ImageList.ImageSize (here 24x24).
		// - If later you want icon scaling with zoom, we will rebuild ImageList on zoom.
		private const int TreeIconSize = 24;

		private ImageList? _treeImages;

		// Keys are internal IDs inside ImageList.
		// Keep them stable — other code relies on these string keys.
		private const string IconKeyFolder = "folder";
		private const string IconKeyGrayFolder = "grayFolder";
		private const string IconKeyUnknownFile = "unknownFile";
		private const string IconKeyFile = "file";          // generic file icon (we map it to text icon for readability)
		private const string IconKeyText = "text";
		private const string IconKeyCSharp = "csharp";
		private const string IconKeyPython = "python";
		private const string IconKeyPyc = "pyc";
		private const string IconKeyC = "c";
		private const string IconKeyCpp = "cpp";
		private const string IconKeyGo = "go";
		private const string IconKeyRust = "rust";
		private const string IconKeyJava = "java";
		private const string IconKeyKotlin = "kotlin";
		private const string IconKeySwift = "swift";
		private const string IconKeyPhp = "php";
		private const string IconKeyJs = "js";
		private const string IconKeyTypeScript = "typescript";
		private const string IconKeyHtml = "html";
		private const string IconKeyCss = "css";
		private const string IconKeyJson = "json";
		private const string IconKeyXml = "xml";
		private const string IconKeyXaml = "xaml";
		private const string IconKeySql = "sql";
		private const string IconKeyMd = "md";
		private const string IconKeySln = "sln";
		private const string IconKeyDll = "dll";
		private const string IconKeyExe = "exe";
		private const string IconKeyPdf = "pdf";
		private const string IconKeyPicture = "picture";
		private const string IconKeyVideo = "video";
		private const string IconKeyAudio = "audio";
		private const string IconKeyGit = "git";
		private const string IconKeyConf = "conf";

		// "Unwanted" folders we want to show as gray even if user decided to include them.
		// This is purely visual, not filtering logic.
		private static readonly HashSet<string> _grayFolderNames = new(StringComparer.OrdinalIgnoreCase)
		{
			"bin", "obj",
			".git",
			"node_modules",
			".idea",
			".vs",
			".vscode"
		};

		// Main association: extension -> icon key.
		// The goal is MAX coverage with the icons you have.
		private static readonly Dictionary<string, string> _extensionToIconKey = new(StringComparer.OrdinalIgnoreCase)
		{
			// Solutions / .NET
			[".sln"] = IconKeySln,
			[".cs"] = IconKeyCSharp,
			[".csx"] = IconKeyCSharp,
			[".csproj"] = IconKeyCSharp,
			[".props"] = IconKeyConf,
			[".targets"] = IconKeyConf,
			[".editorconfig"] = IconKeyConf,
			[".resx"] = IconKeyXml,
			[".config"] = IconKeyConf,

			// C / C++
			[".c"] = IconKeyC,
			[".h"] = IconKeyCpp,       // header file: could be C or C++ — using cpp icon as "generic native"
			[".hpp"] = IconKeyCpp,
			[".hh"] = IconKeyCpp,
			[".hxx"] = IconKeyCpp,
			[".cpp"] = IconKeyCpp,
			[".cc"] = IconKeyCpp,
			[".cxx"] = IconKeyCpp,

			// Python
			[".py"] = IconKeyPython,
			[".pyw"] = IconKeyPython,
			[".pyi"] = IconKeyPython,
			[".pyc"] = IconKeyPyc,
			[".pyo"] = IconKeyPyc,

			// JS / TS
			[".js"] = IconKeyJs,
			[".mjs"] = IconKeyJs,
			[".cjs"] = IconKeyJs,
			[".jsx"] = IconKeyJs,
			[".ts"] = IconKeyTypeScript,
			[".tsx"] = IconKeyTypeScript,

			// Web
			[".html"] = IconKeyHtml,
			[".htm"] = IconKeyHtml,
			[".css"] = IconKeyCss,
			[".scss"] = IconKeyCss,
			[".sass"] = IconKeyCss,
			[".less"] = IconKeyCss,

			// Data / Markup
			[".json"] = IconKeyJson,
			[".jsonc"] = IconKeyJson,
			[".xml"] = IconKeyXml,
			[".xsd"] = IconKeyXml,
			[".xslt"] = IconKeyXml,
			[".xsl"] = IconKeyXml,
			[".xaml"] = IconKeyXaml,

			// DB / SQL
			[".sql"] = IconKeySql,

			// Docs / text
			[".md"] = IconKeyMd,
			[".markdown"] = IconKeyMd,
			[".txt"] = IconKeyText,
			[".log"] = IconKeyText,
			[".rtf"] = IconKeyText,
			[".csv"] = IconKeyText,
			[".tsv"] = IconKeyText,
			[".yml"] = IconKeyConf,
			[".yaml"] = IconKeyConf,
			[".toml"] = IconKeyConf,
			[".ini"] = IconKeyConf,
			[".cfg"] = IconKeyConf,
			[".conf"] = IconKeyConf,

			// Languages
			[".go"] = IconKeyGo,
			[".rs"] = IconKeyRust,
			[".java"] = IconKeyJava,
			[".kt"] = IconKeyKotlin,
			[".kts"] = IconKeyKotlin,
			[".swift"] = IconKeySwift,
			[".php"] = IconKeyPhp,

			// Binaries
			[".dll"] = IconKeyDll,
			[".exe"] = IconKeyExe,
			[".msi"] = IconKeyExe,
			[".bat"] = IconKeyConf,
			[".cmd"] = IconKeyConf,
			[".ps1"] = IconKeyConf,
			[".sh"] = IconKeyConf,

			// PDF
			[".pdf"] = IconKeyPdf,

			// Pictures
			[".png"] = IconKeyPicture,
			[".jpg"] = IconKeyPicture,
			[".jpeg"] = IconKeyPicture,
			[".gif"] = IconKeyPicture,
			[".bmp"] = IconKeyPicture,
			[".tif"] = IconKeyPicture,
			[".tiff"] = IconKeyPicture,
			[".webp"] = IconKeyPicture,
			[".svg"] = IconKeyPicture,
			[".ico"] = IconKeyPicture,

			// Video
			[".mp4"] = IconKeyVideo,
			[".mkv"] = IconKeyVideo,
			[".mov"] = IconKeyVideo,
			[".avi"] = IconKeyVideo,
			[".webm"] = IconKeyVideo,
			[".wmv"] = IconKeyVideo,
			[".m4v"] = IconKeyVideo,
			[".flv"] = IconKeyVideo,

			// Audio
			[".mp3"] = IconKeyAudio,
			[".wav"] = IconKeyAudio,
			[".flac"] = IconKeyAudio,
			[".aac"] = IconKeyAudio,
			[".ogg"] = IconKeyAudio,
			[".m4a"] = IconKeyAudio,
			[".wma"] = IconKeyAudio,
			[".opus"] = IconKeyAudio,
			[".aiff"] = IconKeyAudio,
		};

		// File name based associations (important for dotfiles and extension-less files).
		// NOTE:
		// - If IgnoreDot is enabled in your TreeBuilder, dotfiles might not appear at all.
		// - Still keeping mappings here for maximum coverage when they do appear.
		private static readonly Dictionary<string, string> _fileNameToIconKey = new(StringComparer.OrdinalIgnoreCase)
		{
			["readme"] = IconKeyMd,
			["readme.md"] = IconKeyMd,
			["license"] = IconKeyText,
			["license.txt"] = IconKeyText,
			["dockerfile"] = IconKeyConf,
			["makefile"] = IconKeyConf,

			// Git specific
			[".gitignore"] = IconKeyGit,
			[".gitattributes"] = IconKeyGit,
			[".gitmodules"] = IconKeyGit,
			[".gitkeep"] = IconKeyGit,

			// Common configs
			[".env"] = IconKeyConf,
			[".env.example"] = IconKeyConf,
			["nuget.config"] = IconKeyConf,
			["global.json"] = IconKeyJson,
			["appsettings.json"] = IconKeyJson,
			["appsettings.development.json"] = IconKeyJson,
		};

		public Form1() : this(CommandLineOptions.Empty)
		{
		}

		public Form1(CommandLineOptions startupOptions)
		{
			_startupOptions = startupOptions;

			InitializeComponent();

			InitTreeIcons();

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

		// ───────────────────────────────────────── Tree icons init
		private void InitTreeIcons()
		{
			_treeImages = new ImageList
			{
				ColorDepth = ColorDepth.Depth32Bit,
				ImageSize = new Size(TreeIconSize, TreeIconSize)
			};

			// We load icons by file name (24x24) with two strategies:
			// 1) EmbeddedResource (best for single-file publish)
			// 2) File from disk relative to executable folder (best for Debug / portable folder)
			//
			// IMPORTANT:
			// If you publish and icons disappear, it means they were not embedded and not copied to output.
			// We keep both approaches to be resilient.
			AddIcon(IconKeyFolder, "folder24.png");
			AddIcon(IconKeyGrayFolder, "grayFolder24.png");
			AddIcon(IconKeyUnknownFile, "uknownFile24.png"); // file name has your exact spelling
			AddIcon(IconKeyText, "text24.png");
			AddIcon(IconKeyFile, "text24.png"); // generic file -> text icon

			AddIcon(IconKeyCSharp, "csharp24.png");
			AddIcon(IconKeyPython, "python24.png");
			AddIcon(IconKeyPyc, "pyc24.png");
			AddIcon(IconKeyC, "c24.png");
			AddIcon(IconKeyCpp, "cpp24.png");
			AddIcon(IconKeyGo, "go24.png");
			AddIcon(IconKeyRust, "rust24.png");
			AddIcon(IconKeyJava, "java24.png");
			AddIcon(IconKeyKotlin, "kotlin24.png");
			AddIcon(IconKeySwift, "swift24.png");
			AddIcon(IconKeyPhp, "php24.png");
			AddIcon(IconKeyJs, "js24.png");
			AddIcon(IconKeyTypeScript, "typescript24.png");
			AddIcon(IconKeyHtml, "html24.png");
			AddIcon(IconKeyCss, "css24.png");
			AddIcon(IconKeyJson, "json24.png");
			AddIcon(IconKeyXml, "xml24.png");
			AddIcon(IconKeyXaml, "xaml24.png");
			AddIcon(IconKeySql, "sql24.png");
			AddIcon(IconKeyMd, "md24.png");
			AddIcon(IconKeySln, "sln24.png");
			AddIcon(IconKeyDll, "dll24.png");
			AddIcon(IconKeyExe, "exe24.png");
			AddIcon(IconKeyPdf, "pdf24.png");
			AddIcon(IconKeyPicture, "picture24.png");
			AddIcon(IconKeyVideo, "video24.png");
			AddIcon(IconKeyAudio, "audio24.png");
			AddIcon(IconKeyGit, "git24.png");
			AddIcon(IconKeyConf, "conf24.png");

			treeProject.ImageList = _treeImages;

			UpdateTreeItemHeight();

			void AddIcon(string key, string fileName)
			{
				var img = LoadImageEmbeddedOrFile(
					embeddedResourceName: $"ProjectTreeViewer.Resources.Icons.{fileName}",
					relativeFilePath: Path.Combine("Resources", "Icons", fileName));

				_treeImages!.Images.Add(key, img ?? CreateTransparentFallbackIcon());
			}
		}

		private static Image? LoadImageEmbeddedOrFile(string embeddedResourceName, string relativeFilePath)
		{
			// Embedded
			var embedded = TryLoadEmbedded(embeddedResourceName);
			if (embedded is not null) return embedded;

			// File near exe
			var fromDisk = TryLoadFromDisk(relativeFilePath);
			return fromDisk;

			static Image? TryLoadEmbedded(string resourceName)
			{
				var asm = typeof(Form1).Assembly;
				using var stream = asm.GetManifestResourceStream(resourceName);
				if (stream is null) return null;

				using var img = Image.FromStream(stream);
				return (Image)img.Clone();
			}

			static Image? TryLoadFromDisk(string rel)
			{
				var fullPath = Path.Combine(AppContext.BaseDirectory, rel);
				if (!File.Exists(fullPath)) return null;

				using var fs = File.OpenRead(fullPath);
				using var img = Image.FromStream(fs);
				return (Image)img.Clone();
			}
		}

		private static Image CreateTransparentFallbackIcon()
		{
			// Transparent placeholder prevents exceptions and keeps layout stable.
			var bmp = new Bitmap(TreeIconSize, TreeIconSize);
			using var g = Graphics.FromImage(bmp);
			g.Clear(Color.Transparent);
			return bmp;
		}

		private void ApplyIconsToTree()
		{
			if (_treeImages is null) return;
			if (treeProject.ImageList != _treeImages)
				treeProject.ImageList = _treeImages;

			foreach (TreeNode node in treeProject.Nodes)
				ApplyIconRecursive(node);
		}

		private void ApplyIconRecursive(TreeNode node)
		{
			ApplyIconToNode(node);

			foreach (TreeNode child in node.Nodes)
				ApplyIconRecursive(child);
		}

		private void ApplyIconToNode(TreeNode node)
		{
			if (node.Tag is not string path) return;

			// Directory icon decision
			if (Directory.Exists(path))
			{
				var dirName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

				// Gray folder for "unwanted" directories:
				// - dot-prefixed directories (".something")
				// - common build/system folders (bin/obj/node_modules/.git/etc)
				//
				// NOTE:
				// Hidden folders may not appear at all due to TreeBuilder filtering hidden attributes.
				// But we still keep dot-name rule for when IgnoreDot is disabled.
				bool isGray =
					(!string.IsNullOrEmpty(dirName) && dirName.StartsWith(".", StringComparison.Ordinal)) ||
					_grayFolderNames.Contains(dirName);

				var key = isGray ? IconKeyGrayFolder : IconKeyFolder;

				node.ImageKey = key;
				node.SelectedImageKey = key;
				return;
			}

			// File icon decision
			if (File.Exists(path))
			{
				var fileName = Path.GetFileName(path);

				// 1) File name rules first (dotfiles, extension-less, special names)
				if (!string.IsNullOrWhiteSpace(fileName) && _fileNameToIconKey.TryGetValue(fileName, out var nameKey))
				{
					node.ImageKey = nameKey;
					node.SelectedImageKey = nameKey;
					return;
				}

				// 2) Extension based rules
				var ext = Path.GetExtension(fileName);

				if (!string.IsNullOrWhiteSpace(ext) && _extensionToIconKey.TryGetValue(ext, out var extKey))
				{
					node.ImageKey = extKey;
					node.SelectedImageKey = extKey;
					return;
				}

				// 3) If we don't know the format -> unknownFile
				// This matches your requirement: unknown formats must not be treated as generic file.
				node.ImageKey = IconKeyUnknownFile;
				node.SelectedImageKey = IconKeyUnknownFile;
			}
		}

		private void UpdateTreeItemHeight()
		{
			// Ensure icons are not clipped and work with font zoom.
			// +6 gives some breathing space for checkbox + icon + text.
			var height = Math.Max(treeProject.Font.Height + 6, TreeIconSize + 2);
			treeProject.ItemHeight = height;
		}

		// ───────────────────────────────────────── Expand “layer-by-layer”
		private void treeProject_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
		{
			treeProject.BeginUpdate();
			try
			{
				// Show only one level down:
				// collapse all descendants so user doesn't lose the structure.
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

			// Settings panel
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
			try
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

					// After path: 1 "blank" line that won't collapse on paste
					AppendClipboardBlankLine(sb);

					try
					{
						sb.AppendLine(ReadFileTextForClipboard(file));
					}
					catch (Exception ex)
					{
						sb.AppendLine($"[Не удалось прочитать файл: {ex.Message}]");
					}

					// Before next file: 2 "blank" lines that won't collapse
					if (i < files.Count - 1)
					{
						AppendClipboardBlankLine(sb);
						AppendClipboardBlankLine(sb);
					}
				}

				Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText);
			}
			catch (Exception ex)
			{
				_messages.ShowException(ex);
			}
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
			var text = File.ReadAllText(path, Encoding.UTF8);

			// Fast binary guard: if text contains NUL -> treat as binary
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

				UpdateTreeItemHeight();
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

				if (result.RootAccessDenied && TryElevateAndRestart(_currentPath))
					return;

				// Render without ExpandAll: expansion is "layer-by-layer"
				_renderer.Render(treeProject, result.Root, expandAll: false);

				if (treeProject.Nodes.Count > 0)
					treeProject.Nodes[0].Expand();

				// Apply icons after render: TreeViewRenderer creates nodes with Text+Tag,
				// then we assign ImageKey/SelectedImageKey based on file/folder rules.
				ApplyIconsToTree();
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

			UpdateTreeItemHeight();
		}
	}
}
