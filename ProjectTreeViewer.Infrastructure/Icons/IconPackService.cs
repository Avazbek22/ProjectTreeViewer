using System;
using System.IO;
using System.Linq;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.Icons;

public sealed class IconPackService : IIconPackService
{
	private readonly IconPackManifest _manifest;

	public string PackId => _manifest.PackId;

	public IconPackService(IResourceStore store, string packId = "Default")
	{
		if (store is null) throw new ArgumentNullException(nameof(store));

		var loader = new IconPackManifestLoader(store);
		_manifest = loader.Load(packId);
	}

	public string ResolveIconKey(string itemName, bool isDirectory)
	{
		var nameOnly = NormalizeFileName(itemName);

		if (isDirectory)
		{
			if (IsGrayFolder(nameOnly))
				return "grayFolder";

			return "folder";
		}

		// 1) fileNameToIconKey (readme, dockerfile, .gitignore и т.п.)
		if (_manifest.FileNameToIconKey.TryGetValue(nameOnly, out var byName) && !string.IsNullOrWhiteSpace(byName))
			return byName;

		// 2) special rules (blazor .razor.cs etc)
		var special = ResolveSpecialRules(nameOnly);
		if (!string.IsNullOrWhiteSpace(special))
			return special;

		// 3) extension mapping
		var ext = Path.GetExtension(nameOnly);
		if (!string.IsNullOrWhiteSpace(ext) &&
		    _manifest.ExtensionToIconKey.TryGetValue(ext, out var byExt) &&
		    !string.IsNullOrWhiteSpace(byExt))
			return byExt;

		// fallback
		if (_manifest.Icons.ContainsKey("unknownFile"))
			return "unknownFile";

		if (_manifest.Icons.ContainsKey("file"))
			return "file";

		return "text";
	}

	public string ResolveIconResourceId(string itemName, bool isDirectory)
	{
		var key = ResolveIconKey(itemName, isDirectory);
		return ResolveIconResourceIdByKey(key);
	}

	public string ResolveIconResourceIdByKey(string iconKey)
	{
		if (string.IsNullOrWhiteSpace(iconKey))
			iconKey = "unknownFile";

		if (!_manifest.Icons.TryGetValue(iconKey, out var fileName) || string.IsNullOrWhiteSpace(fileName))
		{
			if (_manifest.Icons.TryGetValue("unknownFile", out var unk) && !string.IsNullOrWhiteSpace(unk))
				fileName = unk;
			else if (_manifest.Icons.TryGetValue("file", out var f) && !string.IsNullOrWhiteSpace(f))
				fileName = f;
			else
				fileName = string.Empty;
		}

		if (string.IsNullOrWhiteSpace(fileName))
			return $"IconPacks/{_manifest.PackId}/icons/uknownFile24.png";

		return $"IconPacks/{_manifest.PackId}/icons/{fileName}";
	}

	public bool IsContentExcludedByName(string fileName)
	{
		var nameOnly = NormalizeFileName(fileName);
		var ext = Path.GetExtension(nameOnly);
		if (string.IsNullOrWhiteSpace(ext))
			return false;

		return _manifest.ContentExcludedExtensions.Contains(ext);
	}

	private bool IsGrayFolder(string folderName)
	{
		if (_manifest.GrayFolders.Count == 0) return false;

		return _manifest.GrayFolders.Any(x =>
			string.Equals(x, folderName, StringComparison.OrdinalIgnoreCase));
	}

	private string ResolveSpecialRules(string fileName)
	{
		var rules = _manifest.SpecialRules;
		if (rules.BlazorCodeBehindSuffixes.Count == 0)
			return string.Empty;

		foreach (var suffix in rules.BlazorCodeBehindSuffixes)
		{
			if (string.IsNullOrWhiteSpace(suffix))
				continue;

			if (fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
				return "blazor";
		}

		return string.Empty;
	}

	private static string NormalizeFileName(string value)
	{
		var name = string.IsNullOrWhiteSpace(value) ? string.Empty : value;

		try
		{
			name = Path.GetFileName(name);
		}
		catch
		{
			// ignore
		}

		return name.Trim().ToLowerInvariant();
	}
}
