using System.Collections.Generic;

namespace ProjectTreeViewer;

public sealed record IconPackManifest(
    string PackId,
    IReadOnlyDictionary<string, string> Icons,
    IReadOnlyList<string> GrayFolders,
    IconPackSpecialRules SpecialRules,
    IReadOnlyDictionary<string, string> ExtensionToIconKey,
    IReadOnlyDictionary<string, string> FileNameToIconKey,
    IReadOnlySet<string> ContentExcludedExtensions);