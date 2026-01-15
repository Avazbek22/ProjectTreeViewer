using System;
using System.Collections.Generic;
using ProjectTreeViewer.Infrastructure.Resources;
using ProjectTreeViewer.Kernel.Abstractions;

namespace ProjectTreeViewer.Infrastructure.Icons;

public sealed class IconPackManifestLoader
{
	private readonly IResourceStore _store;

	public IconPackManifestLoader(IResourceStore store)
	{
		_store = store ?? throw new ArgumentNullException(nameof(store));
	}

	public IconPackManifest Load(string packId)
	{
		if (string.IsNullOrWhiteSpace(packId))
			packId = "Default";

		var resourceId = $"IconPacks/{packId}/manifest.json";
		var reader = new ResourceJsonReader(_store);

		if (!reader.TryReadJson<IconPackManifestDto>(resourceId, out var dto) || dto is null)
			return CreateEmpty(packId);

		return Map(dto, packId);
	}

	private static IconPackManifest CreateEmpty(string packId)
	{
		return new IconPackManifest(
			PackId: packId,
			Icons: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
			GrayFolders: new List<string>(),
			SpecialRules: IconPackSpecialRules.Empty,
			ExtensionToIconKey: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
			FileNameToIconKey: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
			ContentExcludedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase));
	}

	private static IconPackManifest Map(IconPackManifestDto dto, string fallbackPackId)
	{
		var packId = string.IsNullOrWhiteSpace(dto.PackId) ? fallbackPackId : dto.PackId!;

		var icons = dto.Icons is null
			? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			: new Dictionary<string, string>(dto.Icons, StringComparer.OrdinalIgnoreCase);

		var extMap = dto.ExtensionToIconKey is null
			? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			: new Dictionary<string, string>(dto.ExtensionToIconKey, StringComparer.OrdinalIgnoreCase);

		var nameMap = dto.FileNameToIconKey is null
			? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			: new Dictionary<string, string>(dto.FileNameToIconKey, StringComparer.OrdinalIgnoreCase);

		var grayFolders = dto.GrayFolders ?? new List<string>();

		var rules = dto.SpecialRules?.ToDomain() ?? IconPackSpecialRules.Empty;

		var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (dto.ContentExcludedExtensions is not null)
		{
			foreach (var e in dto.ContentExcludedExtensions)
			{
				if (!string.IsNullOrWhiteSpace(e))
					excluded.Add(e.Trim());
			}
		}

		return new IconPackManifest(
			PackId: packId,
			Icons: icons,
			GrayFolders: grayFolders,
			SpecialRules: rules,
			ExtensionToIconKey: extMap,
			FileNameToIconKey: nameMap,
			ContentExcludedExtensions: excluded);
	}

	private sealed class IconPackManifestDto
	{
		public string? PackId { get; set; }
		public Dictionary<string, string>? Icons { get; set; }
		public List<string>? GrayFolders { get; set; }
		public IconPackSpecialRulesDto? SpecialRules { get; set; }
		public Dictionary<string, string>? ExtensionToIconKey { get; set; }
		public Dictionary<string, string>? FileNameToIconKey { get; set; }
		public List<string>? ContentExcludedExtensions { get; set; }
	}

	private sealed class IconPackSpecialRulesDto
	{
		public List<string>? BlazorCodeBehindSuffixes { get; set; }

		public IconPackSpecialRules ToDomain()
		{
			return new IconPackSpecialRules(BlazorCodeBehindSuffixes ?? new List<string>());
		}
	}
}
