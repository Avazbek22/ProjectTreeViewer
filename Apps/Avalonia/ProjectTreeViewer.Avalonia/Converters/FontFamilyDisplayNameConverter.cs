using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ProjectTreeViewer.Avalonia.Converters;

public sealed class FontFamilyDisplayNameConverter : IValueConverter
{
    public static readonly FontFamilyDisplayNameConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not FontFamily family)
            return "Default";

        var name = family.Name ?? string.Empty;
        if (name.StartsWith("$", StringComparison.Ordinal))
            return "Default";

        return name;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value ?? "Default";
}
