using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ProjectTreeViewer
{
    public sealed class TreeIconService
    {
        private const int TreeIconSize = 24;

        private ImageList? _treeImages;

        private const string IconKeyFolder = "folder";
        private const string IconKeyGrayFolder = "grayFolder";
        private const string IconKeyUnknownFile = "unknownFile";
        private const string IconKeyFile = "file";
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

        private const string IconKeyBlazor = "blazor";
        private const string IconKeyExcel = "excel";
        private const string IconKeyWord = "word";
        private const string IconKeyAccess = "access";
        private const string IconKeyPowerPoint = "powerpoint";

        private static readonly HashSet<string> GrayFolderNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "bin", "obj",
            ".git",
            "node_modules",
            ".idea",
            ".vs",
            ".vscode"
        };

        private static readonly Dictionary<string, string> ExtensionToIconKey = new(StringComparer.OrdinalIgnoreCase)
        {
            [".sln"] = IconKeySln,
            [".cs"] = IconKeyCSharp,
            [".csx"] = IconKeyCSharp,
            [".csproj"] = IconKeyCSharp,
            [".props"] = IconKeyConf,
            [".targets"] = IconKeyConf,
            [".editorconfig"] = IconKeyConf,
            [".resx"] = IconKeyXml,
            [".config"] = IconKeyConf,

            [".razor"] = IconKeyBlazor,

            [".c"] = IconKeyC,
            [".h"] = IconKeyCpp,
            [".hpp"] = IconKeyCpp,
            [".hh"] = IconKeyCpp,
            [".hxx"] = IconKeyCpp,
            [".cpp"] = IconKeyCpp,
            [".cc"] = IconKeyCpp,
            [".cxx"] = IconKeyCpp,

            [".py"] = IconKeyPython,
            [".pyw"] = IconKeyPython,
            [".pyi"] = IconKeyPython,
            [".pyc"] = IconKeyPyc,
            [".pyo"] = IconKeyPyc,

            [".js"] = IconKeyJs,
            [".mjs"] = IconKeyJs,
            [".cjs"] = IconKeyJs,
            [".jsx"] = IconKeyJs,
            [".ts"] = IconKeyTypeScript,
            [".tsx"] = IconKeyTypeScript,

            [".html"] = IconKeyHtml,
            [".htm"] = IconKeyHtml,
            [".css"] = IconKeyCss,
            [".scss"] = IconKeyCss,
            [".sass"] = IconKeyCss,
            [".less"] = IconKeyCss,

            [".json"] = IconKeyJson,
            [".jsonc"] = IconKeyJson,
            [".xml"] = IconKeyXml,
            [".xsd"] = IconKeyXml,
            [".xslt"] = IconKeyXml,
            [".xsl"] = IconKeyXml,
            [".xaml"] = IconKeyXaml,

            [".sql"] = IconKeySql,

            [".md"] = IconKeyMd,
            [".markdown"] = IconKeyMd,
            [".txt"] = IconKeyText,
            [".log"] = IconKeyText,
            [".rtf"] = IconKeyWord,
            [".csv"] = IconKeyText,
            [".tsv"] = IconKeyText,
            [".yml"] = IconKeyConf,
            [".yaml"] = IconKeyConf,
            [".toml"] = IconKeyConf,
            [".ini"] = IconKeyConf,
            [".cfg"] = IconKeyConf,
            [".conf"] = IconKeyConf,

            [".xls"] = IconKeyExcel,
            [".xlsx"] = IconKeyExcel,
            [".xlsm"] = IconKeyExcel,
            [".xlt"] = IconKeyExcel,
            [".xltx"] = IconKeyExcel,
            [".doc"] = IconKeyWord,
            [".docx"] = IconKeyWord,
            [".ppt"] = IconKeyPowerPoint,
            [".pptx"] = IconKeyPowerPoint,
            [".pps"] = IconKeyPowerPoint,
            [".ppsx"] = IconKeyPowerPoint,
            [".mdb"] = IconKeyAccess,
            [".accdb"] = IconKeyAccess,

            [".go"] = IconKeyGo,
            [".rs"] = IconKeyRust,
            [".java"] = IconKeyJava,
            [".kt"] = IconKeyKotlin,
            [".kts"] = IconKeyKotlin,
            [".swift"] = IconKeySwift,
            [".php"] = IconKeyPhp,

            [".dll"] = IconKeyDll,
            [".exe"] = IconKeyExe,
            [".msi"] = IconKeyExe,
            [".bat"] = IconKeyConf,
            [".cmd"] = IconKeyConf,
            [".ps1"] = IconKeyConf,
            [".sh"] = IconKeyConf,

            [".pdf"] = IconKeyPdf,

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

            [".mp4"] = IconKeyVideo,
            [".mkv"] = IconKeyVideo,
            [".mov"] = IconKeyVideo,
            [".avi"] = IconKeyVideo,
            [".webm"] = IconKeyVideo,
            [".wmv"] = IconKeyVideo,
            [".m4v"] = IconKeyVideo,
            [".flv"] = IconKeyVideo,

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

        private static readonly Dictionary<string, string> FileNameToIconKey = new(StringComparer.OrdinalIgnoreCase)
        {
            ["readme"] = IconKeyMd,
            ["readme.md"] = IconKeyMd,
            ["license"] = IconKeyText,
            ["license.txt"] = IconKeyText,
            ["dockerfile"] = IconKeyConf,
            ["makefile"] = IconKeyConf,

            [".gitignore"] = IconKeyGit,
            [".gitattributes"] = IconKeyGit,
            [".gitmodules"] = IconKeyGit,
            [".gitkeep"] = IconKeyGit,

            [".env"] = IconKeyConf,
            [".env.example"] = IconKeyConf,
            ["nuget.config"] = IconKeyConf,
            ["global.json"] = IconKeyJson,
            ["appsettings.json"] = IconKeyJson,
            ["appsettings.development.json"] = IconKeyJson,
        };

        public void Initialize(TreeView treeView)
        {
            _treeImages = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(TreeIconSize, TreeIconSize)
            };

            AddIcon(IconKeyFolder, "folder24.png");
            AddIcon(IconKeyGrayFolder, "grayFolder24.png");
            AddIcon(IconKeyUnknownFile, "uknownFile24.png"); // (ваше название)
            AddIcon(IconKeyText, "text24.png");
            AddIcon(IconKeyFile, "text24.png");

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

            AddIcon(IconKeyBlazor, "blazor48.png");
            AddIcon(IconKeyExcel, "excel48.png");
            AddIcon(IconKeyWord, "word48.png");
            AddIcon(IconKeyAccess, "access48.png");
            AddIcon(IconKeyPowerPoint, "powerpoint48.png");

            treeView.ImageList = _treeImages;
            UpdateTreeItemHeight(treeView);

            void AddIcon(string key, string fileName)
            {
                var img = LoadImageEmbeddedOrFile(
                    embeddedResourceName: $"ProjectTreeViewer.Resources.Icons.{fileName}",
                    relativeFilePath: Path.Combine("Resources", "Icons", fileName));

                _treeImages.Images.Add(key, img ?? CreateTransparentFallbackIcon());
            }
        }

        public void ApplyIconsToTree(TreeView treeView)
        {
            if (_treeImages is null) return;
            if (treeView.ImageList != _treeImages)
                treeView.ImageList = _treeImages;

            foreach (TreeNode node in treeView.Nodes)
                ApplyIconRecursive(node);
        }

        public void UpdateTreeItemHeight(TreeView treeView)
        {
            var height = Math.Max(treeView.Font.Height + 6, TreeIconSize + 2);
            treeView.ItemHeight = height;
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

            if (Directory.Exists(path))
            {
                var dirName = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                bool isGray =
                    (!string.IsNullOrEmpty(dirName) && dirName.StartsWith(".", StringComparison.Ordinal)) ||
                    GrayFolderNames.Contains(dirName);

                var key = isGray ? IconKeyGrayFolder : IconKeyFolder;

                node.ImageKey = key;
                node.SelectedImageKey = key;
                return;
            }

            if (File.Exists(path))
            {
                var fileName = Path.GetFileName(path);

                // Blazor code-behind special
                if (fileName.EndsWith(".razor.cs", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".razor.css", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".razor.js", StringComparison.OrdinalIgnoreCase) ||
                    fileName.EndsWith(".razor.ts", StringComparison.OrdinalIgnoreCase))
                {
                    node.ImageKey = IconKeyBlazor;
                    node.SelectedImageKey = IconKeyBlazor;
                    return;
                }

                if (!string.IsNullOrWhiteSpace(fileName) && FileNameToIconKey.TryGetValue(fileName, out var nameKey))
                {
                    node.ImageKey = nameKey;
                    node.SelectedImageKey = nameKey;
                    return;
                }

                var ext = Path.GetExtension(fileName);
                if (!string.IsNullOrWhiteSpace(ext) && ExtensionToIconKey.TryGetValue(ext, out var extKey))
                {
                    node.ImageKey = extKey;
                    node.SelectedImageKey = extKey;
                    return;
                }

                node.ImageKey = IconKeyUnknownFile;
                node.SelectedImageKey = IconKeyUnknownFile;
            }
        }

        private static Image? LoadImageEmbeddedOrFile(string embeddedResourceName, string relativeFilePath)
        {
            var embedded = TryLoadEmbedded(embeddedResourceName);
            if (embedded is not null) return embedded;

            var fromDisk = TryLoadFromDisk(relativeFilePath);
            return fromDisk;

            static Image? TryLoadEmbedded(string resourceName)
            {
                var asm = typeof(TreeIconService).Assembly;
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
            var bmp = new Bitmap(TreeIconSize, TreeIconSize);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            return bmp;
        }
    }
}
