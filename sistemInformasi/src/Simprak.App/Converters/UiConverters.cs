using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Simprak.App.Services;

namespace Simprak.App.Converters;

public class StringEqualsConverter : IValueConverter
{
    public static readonly StringEqualsConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StatusToBadgeBrushConverter : IValueConverter
{
    public static readonly StatusToBadgeBrushConverter BgInstance = new() { IsForeground = false };
    public static readonly StatusToBadgeBrushConverter FgInstance = new() { IsForeground = true };

    public bool IsForeground { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string status = value?.ToString() ?? string.Empty;
        bool dark = ThemePaletteService.IsDark;

        if (status is "Tepat Waktu" or "Aman" or "Selesai" or "Aktif" or "1")
        {
            return new SolidColorBrush(Color.Parse(IsForeground
                ? (dark ? "#4ADE80" : "#15803D")
                : (dark ? "#133020" : "#DCFCE7")));
        }

        if (status is "Terlambat" or "Berjalan")
        {
            return new SolidColorBrush(Color.Parse(IsForeground
                ? (dark ? "#FBBF24" : "#B45309")
                : (dark ? "#36280B" : "#FEF3C7")));
        }

        if (status is "Belum Mengumpulkan" or "Berisiko" or "Nonaktif" or "0")
        {
            return new SolidColorBrush(Color.Parse(IsForeground
                ? (dark ? "#F87171" : "#B91C1C")
                : (dark ? "#3B1515" : "#FEE2E2")));
        }

        return new SolidColorBrush(Color.Parse(IsForeground
            ? (dark ? "#38BDF8" : "#0369A1")
            : (dark ? "#102A3B" : "#E0F2FE")));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
