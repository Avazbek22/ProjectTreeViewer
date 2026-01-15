using System;
using System.Collections.Generic;

namespace ProjectTreeViewer;

public sealed record IconPackManifest(
    string PackId,
    IReadOnlyDictionary<string, string> Icons,
    IReadOnlyList<string> GrayFolders,
    IconPackSpecialRules SpecialRules,
    IReadOnlyDictionary<string, string> ExtensionToIconKey,
    IReadOnlyDictionary<string, string> FileNameToIconKey,
    IReadOnlySet<string> ContentExcludedExtensions)
{
    public static IconPackManifest Empty(string packId)
    {
        packId = string.IsNullOrWhiteSpace(packId) ? "Default" : packId;

        return new IconPackManifest(
            PackId: packId,
            Icons: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            GrayFolders: new List<string>(),
            SpecialRules: IconPackSpecialRules.Empty,
            ExtensionToIconKey: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            FileNameToIconKey: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            ContentExcludedExtensions: new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        );
    }

    public IconPackManifest Normalize(string fallbackPackId)
    {
        var packId = string.IsNullOrWhiteSpace(PackId) ? fallbackPackId : PackId;
        if (string.IsNullOrWhiteSpace(packId))
            packId = "Default";

        var icons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (Icons is not null)
        {
            foreach (var kv in Icons)
            {
                if (!string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
                    icons[kv.Key.Trim()] = kv.Value.Trim();
            }
        }

        var gray = new List<string>();
        if (GrayFolders is not null)
        {
            foreach (var g in GrayFolders)
            {
                if (!string.IsNullOrWhiteSpace(g))
                    gray.Add(g.Trim());
            }
        }

        var rules = SpecialRules ?? IconPackSpecialRules.Empty;

        var extMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (ExtensionToIconKey is not null)
        {
            foreach (var kv in ExtensionToIconKey)
            {
                var k = (kv.Key ?? string.Empty).Trim();
                var v = (kv.Value ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(k) || string.IsNullOrWhiteSpace(v))
                    continue;

                if (!k.StartsWith(".", StringComparison.Ordinal))
                    k = "." + k;

                extMap[k] = v;
            }
        }

        var nameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (FileNameToIconKey is not null)
        {
            foreach (var kv in FileNameToIconKey)
            {
                var k = (kv.Key ?? string.Empty).Trim();
                var v = (kv.Value ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(k) || string.IsNullOrWhiteSpace(v))
                    continue;

                nameMap[k] = v;
            }
        }

        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (ContentExcludedExtensions is not null)
        {
            foreach (var e in ContentExcludedExtensions)
            {
                var k = (e ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(k))
                    continue;

                if (!k.StartsWith(".", StringComparison.Ordinal))
                    k = "." + k;

                excluded.Add(k);
            }
        }

        return this with
        {
            PackId = packId,
            Icons = icons,
            GrayFolders = gray,
            SpecialRules = rules,
            ExtensionToIconKey = extMap,
            FileNameToIconKey = nameMap,
            ContentExcludedExtensions = excluded
        };
    }
}
