using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.Icons;

public sealed class IconPackService : IIconPackService
{
	private readonly IResourceStore _resourceStore;

	private readonly object _sync = new();
	private string _packId;
	private IconPackManifest _manifest;

	public IconPackService(IResourceStore resourceStore, string defaultPackId = "Default")
	{
		_resourceStore = resourceStore ?? throw new ArgumentNullException(nameof(resourceStore));
		_packId = string.IsNullOrWhiteSpace(defaultPackId) ? "Default" : defaultPackId;
		_manifest = LoadManifest(_packId);
	}

	public string PackId
	{
		get { lock (_sync) return _packId; }
	}

	public IconPackManifest Manifest
	{
		get { lock (_sync) return _manifest; }
	}

	public void SetPack(string packId)
	{
		if (string.IsNullOrWhiteSpace(packId))
			throw new ArgumentException("Pack id is empty.", nameof(packId));

		lock (_sync)
		{
			if (string.Equals(_packId, packId, StringComparison.OrdinalIgnoreCase))
				return;

			_packId = packId;
			_manifest = LoadManifest(_packId);
		}
	}

	public bool IsGrayFolderName(string folderName)
	{
		if (string.IsNullOrWhiteSpace(folderName))
			return false;

		var m = Manifest;
		foreach (var n in m.GrayFolders)
		{
			if (folderName.Equals(n, StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	public bool IsContentExcludedFile(string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
			return false;

		var ext = Path.GetExtension(fileName);
		if (string.IsNullOrWhiteSpace(ext))
			return false;

		var m = Manifest;
		foreach (var e in m.ContentExcludedExtensions)
		{
			if (ext.Equals(e, StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	public string GetIconResourceIdForPath(string fullPath, bool isDirectory)
	{
		var name = isDirectory ? Path.GetFileName(fullPath) : Path.GetFileName(fullPath);
		if (string.IsNullOrWhiteSpace(name))
			name = fullPath;

		var iconKey = isDirectory ? GetDirectoryIconKey(name) : GetFileIconKey(name);
		return ToResourceId(iconKey);
	}

	private string GetDirectoryIconKey(string directoryName)
	{
		if (IsGrayFolderName(directoryName))
			return "grayFolder";

		return "folder";
	}

	private string GetFileIconKey(string fileName)
	{
		var m = Manifest;

		// 1) filename rules
		if (m.FileNameToIconKey.TryGetValue(fileName, out var byName))
			return byName;

		var lower = fileName.ToLowerInvariant();
		if (m.FileNameToIconKey.TryGetValue(lower, out var byLowerName))
			return byLowerName;

		// 2) special rules: blazor code-behind
		foreach (var suf in m.SpecialRules.BlazorCodeBehindSuffixes)
		{
			if (!string.IsNullOrWhiteSpace(suf) && lower.EndsWith(suf, StringComparison.OrdinalIgnoreCase))
				return "blazor";
		}

		// 3) extension rules
		var ext = Path.GetExtension(fileName);
		if (!string.IsNullOrWhiteSpace(ext) && m.ExtensionToIconKey.TryGetValue(ext, out var byExt))
			return byExt;

		// fallback
		if (m.Icons.ContainsKey("file"))
			return "file";

		return "unknownFile";
	}

	private string ToResourceId(string iconKey)
	{
		var m = Manifest;

		if (!m.Icons.TryGetValue(iconKey, out var file))
		{
			if (!m.Icons.TryGetValue("unknownFile", out file))
				file = "uknownFile24.png";
		}

		return $"IconPacks/{PackId}/icons/{file}";
	}

	private IconPackManifest LoadManifest(string packId)
	{
		var resourceId = $"IconPacks/{packId}/manifest.json";

		if (!_resourceStore.TryOpenRead(resourceId, out var stream))
			return IconPackManifest.Empty(packId);

		try
		{
			using (stream)
			{
				var parsed = JsonSerializer.Deserialize<IconPackManifest>(stream, new JsonSerializerOptions
				{
					ReadCommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true,
					PropertyNameCaseInsensitive = true
				});

				return parsed?.Normalize(packId) ?? IconPackManifest.Empty(packId);
			}
		}
		catch
		{
			return IconPackManifest.Empty(packId);
		}
	}
}
