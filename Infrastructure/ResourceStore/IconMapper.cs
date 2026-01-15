using ProjectTreeViewer.Kernel.Abstractions;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Infrastructure.ResourceStore;

public sealed class IconMapper : IIconMapper
{
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
		[".sln"] = "sln",
		[".cs"] = "csharp",
		[".csx"] = "csharp",
		[".csproj"] = "csharp",
		[".props"] = "conf",
		[".targets"] = "conf",
		[".editorconfig"] = "conf",
		[".resx"] = "xml",
		[".config"] = "conf",

		[".razor"] = "blazor",

		[".c"] = "c",
		[".h"] = "cpp",
		[".hpp"] = "cpp",
		[".hh"] = "cpp",
		[".hxx"] = "cpp",
		[".cpp"] = "cpp",
		[".cc"] = "cpp",
		[".cxx"] = "cpp",

		[".py"] = "python",
		[".pyw"] = "python",
		[".pyi"] = "python",
		[".pyc"] = "pyc",
		[".pyo"] = "pyc",

		[".js"] = "js",
		[".mjs"] = "js",
		[".cjs"] = "js",
		[".jsx"] = "js",
		[".ts"] = "typescript",
		[".tsx"] = "typescript",

		[".html"] = "html",
		[".htm"] = "html",
		[".css"] = "css",
		[".scss"] = "css",
		[".sass"] = "css",
		[".less"] = "css",

		[".json"] = "json",
		[".jsonc"] = "json",
		[".xml"] = "xml",
		[".xsd"] = "xml",
		[".xslt"] = "xml",
		[".xsl"] = "xml",
		[".xaml"] = "xaml",

		[".sql"] = "sql",

		[".md"] = "md",
		[".markdown"] = "md",
		[".txt"] = "text",
		[".log"] = "text",
		[".rtf"] = "word",
		[".csv"] = "text",
		[".tsv"] = "text",
		[".yml"] = "conf",
		[".yaml"] = "conf",
		[".toml"] = "conf",
		[".ini"] = "conf",
		[".cfg"] = "conf",
		[".conf"] = "conf",

		[".xls"] = "excel",
		[".xlsx"] = "excel",
		[".xlsm"] = "excel",
		[".xlt"] = "excel",
		[".xltx"] = "excel",
		[".doc"] = "word",
		[".docx"] = "word",
		[".ppt"] = "powerpoint",
		[".pptx"] = "powerpoint",
		[".pps"] = "powerpoint",
		[".ppsx"] = "powerpoint",
		[".mdb"] = "access",
		[".accdb"] = "access",

		[".go"] = "go",
		[".rs"] = "rust",
		[".java"] = "java",
		[".kt"] = "kotlin",
		[".kts"] = "kotlin",
		[".swift"] = "swift",
		[".php"] = "php",

		[".dll"] = "dll",
		[".exe"] = "exe",
		[".msi"] = "exe",
		[".bat"] = "conf",
		[".cmd"] = "conf",
		[".ps1"] = "conf",
		[".sh"] = "conf",

		[".pdf"] = "pdf",

		[".png"] = "picture",
		[".jpg"] = "picture",
		[".jpeg"] = "picture",
		[".gif"] = "picture",
		[".bmp"] = "picture",
		[".tif"] = "picture",
		[".tiff"] = "picture",
		[".webp"] = "picture",
		[".svg"] = "picture",
		[".ico"] = "picture",

		[".mp4"] = "video",
		[".mkv"] = "video",
		[".mov"] = "video",
		[".avi"] = "video",
		[".webm"] = "video",
		[".wmv"] = "video",
		[".m4v"] = "video",
		[".flv"] = "video",

		[".mp3"] = "audio",
		[".wav"] = "audio",
		[".flac"] = "audio",
		[".aac"] = "audio",
		[".ogg"] = "audio",
		[".m4a"] = "audio",
		[".wma"] = "audio",
		[".opus"] = "audio",
		[".aiff"] = "audio"
	};

	private static readonly Dictionary<string, string> FileNameToIconKey = new(StringComparer.OrdinalIgnoreCase)
	{
		["readme"] = "md",
		["readme.md"] = "md",
		["license"] = "text",
		["license.txt"] = "text",
		["license.md"] = "md",
		["copying"] = "text",
		["copying.txt"] = "text",
		["copying.md"] = "md",
		["changelog"] = "text",
		["changelog.md"] = "md",
		[".gitignore"] = "git",
		[".gitattributes"] = "git",
		[".gitmodules"] = "git",
		[".editorconfig"] = "conf",
		["makefile"] = "conf",
		["dockerfile"] = "conf",
		[".dockerignore"] = "conf",
		[".npmignore"] = "conf",
		[".npmrc"] = "conf",
		[".yarnrc"] = "conf",
		[".yarnrc.yml"] = "conf",
		["package.json"] = "json",
		["package-lock.json"] = "json",
		["pnpm-lock.yaml"] = "conf",
		["yarn.lock"] = "conf",
		["nuget.config"] = "conf",
		["global.json"] = "json",
		[".env"] = "conf",
		[".env.local"] = "conf",
		[".env.dev"] = "conf",
		[".env.production"] = "conf"
	};

	public string GetIconKey(FileSystemNode node)
	{
		if (node.IsDirectory)
		{
			if (node.IsAccessDenied || GrayFolderNames.Contains(node.Name))
				return "grayFolder";

			return "folder";
		}

		var fileName = node.Name;
		if (FileNameToIconKey.TryGetValue(fileName, out var fileIcon))
			return fileIcon;

		var ext = Path.GetExtension(fileName);
		if (!string.IsNullOrWhiteSpace(ext) && ExtensionToIconKey.TryGetValue(ext, out var icon))
			return icon;

		return "unknownFile";
	}
}
