using System.Reflection;
using System.Text;
using ProjectTreeViewer.Kernel.Models;

namespace ProjectTreeViewer.Infrastructure.ResourceStore;

public sealed class HelpContentProvider
{
    private readonly Lazy<IReadOnlyDictionary<AppLanguage, string>> _cache;

    public HelpContentProvider()
    {
        _cache = new Lazy<IReadOnlyDictionary<AppLanguage, string>>(LoadAll);
    }

    public string GetHelpBody(AppLanguage language)
    {
        var cache = _cache.Value;
        return cache.TryGetValue(language, out var body) ? body : cache[AppLanguage.En];
    }

    private static IReadOnlyDictionary<AppLanguage, string> LoadAll()
    {
        var assembly = typeof(ProjectTreeViewer.Assets.Marker).Assembly;
        return new Dictionary<AppLanguage, string>
        {
            [AppLanguage.Ru] = Load(assembly, "ru"),
            [AppLanguage.En] = Load(assembly, "en"),
            [AppLanguage.Uz] = Load(assembly, "uz"),
            [AppLanguage.Tg] = Load(assembly, "tg"),
            [AppLanguage.Kk] = Load(assembly, "kk"),
            [AppLanguage.Fr] = Load(assembly, "fr"),
            [AppLanguage.De] = Load(assembly, "de"),
            [AppLanguage.It] = Load(assembly, "it")
        };
    }

    private static string Load(Assembly assembly, string code)
    {
        var resourceName = $"ProjectTreeViewer.Assets.HelpContent.help.{code}.txt";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Help content resource not found: {resourceName}");

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }
}
