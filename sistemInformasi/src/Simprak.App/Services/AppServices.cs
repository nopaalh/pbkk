using System.Diagnostics;
using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using Simprak.Data;

namespace Simprak.App.Services;

public class PreferensiModel
{
    public string? RememberedUsername { get; set; }
    public bool RememberMe { get; set; }
    public string LastTheme { get; set; } = "Terang";
}

public static class PreferensiService
{
    private static string GetFilePath()
    {
        string dir = Path.GetDirectoryName(DatabaseInitializer.GetDefaultDatabasePath())!;
        return Path.Combine(dir, "preferensi.json");
    }

    public static PreferensiModel Load()
    {
        try
        {
            string path = GetFilePath();
            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize<PreferensiModel>(File.ReadAllText(path)) ?? new PreferensiModel();
            }
        }
        catch
        {
            // Abaikan jika gagal baca preferensi
        }
        return new PreferensiModel();
    }

    public static void Save(PreferensiModel model)
    {
        try
        {
            string path = GetFilePath();
            File.WriteAllText(path, JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
            // Abaikan
        }
    }
}

public static class ThemePaletteService
{
    public static bool IsDark { get; private set; }

    public static event Action? ThemeChanged;

    public static void ApplyTheme(string tema)
    {
        IsDark = string.Equals(tema, "Gelap", StringComparison.OrdinalIgnoreCase);
        var app = Application.Current;
        if (app == null) return;

        app.RequestedThemeVariant = IsDark ? ThemeVariant.Dark : ThemeVariant.Light;

        // 180 Degrees Consulting (180DC) Green Palette + Neutral Surfaces
        SetBrush(app, "AccentBrush", IsDark ? "#A3D769" : "#456D1B");
        SetBrush(app, "AccentHoverBrush", IsDark ? "#B8E487" : "#365714");
        SetBrush(app, "AccentForegroundBrush", IsDark ? "#14230B" : "#FFFFFF");
        SetBrush(app, "AccentSoftBrush", IsDark ? "#1E3318" : "#EBF4E1");
        SetBrush(app, "SidebarBgBrush", IsDark ? "#09100B" : "#111C14");
        SetBrush(app, "PageBgBrush", IsDark ? "#0D1510" : "#F4F7F2");
        SetBrush(app, "SurfaceBrush", IsDark ? "#152219" : "#FFFFFF");
        SetBrush(app, "SurfaceAltBrush", IsDark ? "#1B2B20" : "#F8FAF6");
        SetBrush(app, "BorderSubtleBrush", IsDark ? "#425647" : "#CFDACC");
        SetBrush(app, "InputBorderBrush", IsDark ? "#708776" : "#879A8C");
        SetBrush(app, "TextPrimaryBrush", IsDark ? "#F1F5F0" : "#111C14");
        SetBrush(app, "TextSecondaryBrush", IsDark ? "#C0CEC2" : "#435148");

        // Status Semantic Brushes
        SetBrush(app, "SuccessBrush", IsDark ? "#4ADE80" : "#15803D");
        SetBrush(app, "SuccessBgBrush", IsDark ? "#133020" : "#DCFCE7");
        SetBrush(app, "WarningBrush", IsDark ? "#FBBF24" : "#B45309");
        SetBrush(app, "WarningBgBrush", IsDark ? "#36280B" : "#FEF3C7");
        SetBrush(app, "DangerBrush", IsDark ? "#F87171" : "#B91C1C");
        SetBrush(app, "DangerBgBrush", IsDark ? "#3B1515" : "#FEE2E2");
        SetBrush(app, "InfoBrush", IsDark ? "#38BDF8" : "#0369A1");
        SetBrush(app, "InfoBgBrush", IsDark ? "#102A3B" : "#E0F2FE");

        ThemeChanged?.Invoke();
    }

    private static void SetBrush(Application app, string key, string hex)
    {
        app.Resources[key] = new SolidColorBrush(Color.Parse(hex));
    }
}

public static class UrlLauncher
{
    public static bool BukaUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url.Trim(),
                UseShellExecute = true
            });
            return true;
        }
        catch
        {
            return false;
        }
    }
}
