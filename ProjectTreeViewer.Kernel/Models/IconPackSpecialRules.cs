using System.Collections.Generic;

namespace ProjectTreeViewer;

public sealed record IconPackSpecialRules(
    IReadOnlyList<string> BlazorCodeBehindSuffixes)
{
    public static IconPackSpecialRules Empty { get; } =
        new IconPackSpecialRules(new List<string>());
}