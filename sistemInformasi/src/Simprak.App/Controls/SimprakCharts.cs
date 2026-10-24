using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Simprak.App.Services;
using Simprak.Core.Models;

namespace Simprak.App.Controls;

/// <summary>
/// Komponen Progress Ring (Donut Gauge) kustom berbasis OnRender
/// untuk menampilkan persentase ketuntasan / nilai secara visual pada kartu Dashboard, Kelompok, dan Detail.
/// </summary>
public class SimprakProgressRing : Control
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<SimprakProgressRing, double>(nameof(Value), 0.0);

    public static readonly StyledProperty<double> MaxProperty =
        AvaloniaProperty.Register<SimprakProgressRing, double>(nameof(Max), 100.0);

    public static readonly StyledProperty<double> RingThicknessProperty =
        AvaloniaProperty.Register<SimprakProgressRing, double>(nameof(RingThickness), 7.5);

    public static readonly StyledProperty<string> SuffixProperty =
        AvaloniaProperty.Register<SimprakProgressRing, string>(nameof(Suffix), "%");

    public static readonly StyledProperty<bool> UseWarningThresholdProperty =
        AvaloniaProperty.Register<SimprakProgressRing, bool>(nameof(UseWarningThreshold), true);

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Max
    {
        get => GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public double RingThickness
    {
        get => GetValue(RingThicknessProperty);
        set => SetValue(RingThicknessProperty, value);
    }

    public string Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public bool UseWarningThreshold
    {
        get => GetValue(UseWarningThresholdProperty);
        set => SetValue(UseWarningThresholdProperty, value);
    }

    static SimprakProgressRing()
    {
        AffectsRender<SimprakProgressRing>(ValueProperty, MaxProperty, RingThicknessProperty, SuffixProperty, UseWarningThresholdProperty);
    }

    public SimprakProgressRing()
    {
        ThemePaletteService.ThemeChanged += InvalidateVisual;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        double size = Math.Min(Bounds.Width, Bounds.Height);
        if (size <= 16) return;

        bool dark = ThemePaletteService.IsDark;
        double stroke = RingThickness;
        double radius = (size - stroke) / 2.0 - 2.0;
        var center = new Point(Bounds.Width / 2.0, Bounds.Height / 2.0);

        var trackBrush = new SolidColorBrush(Color.Parse(dark ? "#223528" : "#E3EDE0"));
        var trackPen = new Pen(trackBrush, stroke);
        context.DrawEllipse(null, trackPen, center, radius, radius);

        double maxVal = Max <= 0 ? 100.0 : Max;
        double pct = Math.Clamp(Value / maxVal, 0.0, 1.0);

        IBrush activeBrush;
        if (UseWarningThreshold && pct < 0.60)
            activeBrush = new SolidColorBrush(Color.Parse(dark ? "#F87171" : "#B91C1C"));
        else if (UseWarningThreshold && pct < 0.75)
            activeBrush = new SolidColorBrush(Color.Parse(dark ? "#FBBF24" : "#B45309"));
        else
            activeBrush = new SolidColorBrush(Color.Parse(dark ? "#8BC34A" : "#6D9E31"));

        if (pct >= 0.999)
        {
            context.DrawEllipse(null, new Pen(activeBrush, stroke), center, radius, radius);
        }
        else if (pct > 0.005)
        {
            double startAngle = -Math.PI / 2.0;
            double endAngle = startAngle + pct * 2.0 * Math.PI;

            var startPt = new Point(center.X + radius * Math.Cos(startAngle), center.Y + radius * Math.Sin(startAngle));
            var endPt = new Point(center.X + radius * Math.Cos(endAngle), center.Y + radius * Math.Sin(endAngle));

            var geom = new StreamGeometry();
            using (var gctx = geom.Open())
            {
                gctx.BeginFigure(startPt, false);
                gctx.ArcTo(
                    endPt,
                    new Size(radius, radius),
                    0,
                    pct > 0.5,
                    SweepDirection.Clockwise);
                gctx.EndFigure(false);
            }
            var arcPen = new Pen(activeBrush, stroke, lineCap: PenLineCap.Round);
            context.DrawGeometry(null, arcPen, geom);
        }

        string txt = $"{Math.Round(Value)}{Suffix}";
        var textBrush = new SolidColorBrush(Color.Parse(dark ? "#F1F5F0" : "#111C14"));
        var ft = new FormattedText(
            txt,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.ExtraBold),
            Math.Max(10, size * 0.21),
            textBrush);
        context.DrawText(ft, new Point(center.X - ft.Width / 2.0, center.Y - ft.Height / 2.0));
    }
}

/// <summary>
/// Komponen Grafik Batang Kustom (OnRender) dengan dukungan garis ambang (threshold line 75%)
/// dan gradien halus 180DC Green vs Peringatan Kuning/Oranye.
/// </summary>
public class SimprakBarChart : Control
{
    public static readonly StyledProperty<IList<ChartPointDto>?> ItemsProperty =
        AvaloniaProperty.Register<SimprakBarChart, IList<ChartPointDto>?>(nameof(Items));

    public static readonly StyledProperty<double?> ThresholdValueProperty =
        AvaloniaProperty.Register<SimprakBarChart, double?>(nameof(ThresholdValue), null);

    public static readonly StyledProperty<double> MaxYProperty =
        AvaloniaProperty.Register<SimprakBarChart, double>(nameof(MaxY), 100.0);

    public static readonly StyledProperty<string> ValueSuffixProperty =
        AvaloniaProperty.Register<SimprakBarChart, string>(nameof(ValueSuffix), "%");

    public IList<ChartPointDto>? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public double? ThresholdValue
    {
        get => GetValue(ThresholdValueProperty);
        set => SetValue(ThresholdValueProperty, value);
    }

    public double MaxY
    {
        get => GetValue(MaxYProperty);
        set => SetValue(MaxYProperty, value);
    }

    public string ValueSuffix
    {
        get => GetValue(ValueSuffixProperty);
        set => SetValue(ValueSuffixProperty, value);
    }

    static SimprakBarChart()
    {
        AffectsRender<SimprakBarChart>(ItemsProperty, ThresholdValueProperty, MaxYProperty, ValueSuffixProperty);
    }

    public SimprakBarChart()
    {
        ThemePaletteService.ThemeChanged += InvalidateVisual;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var items = Items;
        double w = Bounds.Width;
        double h = Bounds.Height;
        if (w <= 40 || h <= 40 || items == null || items.Count == 0)
            return;

        bool dark = ThemePaletteService.IsDark;
        var textBrush = new SolidColorBrush(Color.Parse(dark ? "#C0CEC2" : "#435148"));
        var primaryTextBrush = new SolidColorBrush(Color.Parse(dark ? "#F1F5F0" : "#111C14"));
        var gridPen = new Pen(new SolidColorBrush(Color.Parse(dark ? "#223528" : "#E4ECE1")), 1);

        var greenGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse(dark ? "#9CCC65" : "#7CB342"), 0),
                new GradientStop(Color.Parse(dark ? "#6D9E31" : "#557E24"), 1)
            ]
        };

        var warnGradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
            GradientStops =
            [
                new GradientStop(Color.Parse(dark ? "#FBBF24" : "#D97706"), 0),
                new GradientStop(Color.Parse(dark ? "#D97706" : "#B45309"), 1)
            ]
        };

        var trackBrush = new SolidColorBrush(Color.Parse(dark ? "#1A291E" : "#EFF5EC"));

        double padLeft = 52;
        double padRight = 18;
        double padTop = 34;
        double padBottom = 42;
        double plotW = Math.Max(10, w - padLeft - padRight);
        double plotH = Math.Max(10, h - padTop - padBottom);

        double maxVal = Math.Max(MaxY, items.Max(x => x.Value));
        if (maxVal <= 0) maxVal = 100;

        for (int i = 0; i <= 4; i++)
        {
            double frac = i / 4.0;
            double y = padTop + plotH - (frac * plotH);
            context.DrawLine(gridPen, new Point(padLeft, y), new Point(w - padRight, y));
            string lbl = $"{Math.Round(frac * maxVal)}{ValueSuffix}";
            var ft = new FormattedText(lbl, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                Typeface.Default, 12, textBrush);
            context.DrawText(ft, new Point(4, y - ft.Height / 2));
        }

        int n = items.Count;
        double slotW = plotW / n;
        double barW = Math.Clamp(slotW * 0.56, 14, 46);

        for (int i = 0; i < n; i++)
        {
            var pt = items[i];
            double barH = pt.Value <= 0 ? 0 : Math.Clamp((pt.Value / maxVal) * plotH, 4, plotH);
            double x = padLeft + i * slotW + (slotW - barW) / 2;
            double y = padTop + plotH - barH;

            // Subtle column track background
            context.DrawRectangle(trackBrush, null, new RoundedRect(new Rect(x, padTop, barW, plotH), 6));

            if (barH > 0)
            {
                var brush = pt.IsWarning ? warnGradient : greenGradient;
                context.DrawRectangle(brush, null, new RoundedRect(new Rect(x, y, barW, barH), 6, 6, 2, 2));
            }

            string valText = $"{pt.Value:0}{ValueSuffix}";
            var ftVal = new FormattedText(valText, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold), 12, primaryTextBrush);
            context.DrawText(ftVal, new Point(x + (barW - ftVal.Width) / 2, Math.Max(2, y - ftVal.Height - 4)));

            var ftLbl = new FormattedText(pt.Label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.SemiBold), 12, textBrush);
            context.DrawText(ftLbl, new Point(x + (barW - ftLbl.Width) / 2, padTop + plotH + 7));
        }

        if (ThresholdValue.HasValue && ThresholdValue.Value > 0 && ThresholdValue.Value <= maxVal)
        {
            double ty = padTop + plotH - ((ThresholdValue.Value / maxVal) * plotH);
            var dangerColor = Color.Parse(dark ? "#F87171" : "#B91C1C");
            var dangerBg = new SolidColorBrush(Color.Parse(dark ? "#3B1818" : "#FEE2E2"));
            var dashPen = new Pen(new SolidColorBrush(dangerColor), 1.8, new DashStyle([5, 4], 0));
            context.DrawLine(dashPen, new Point(padLeft, ty), new Point(w - padRight, ty));

            var ftTh = new FormattedText($"Ambang Minimum {ThresholdValue.Value:0.#}{ValueSuffix}", CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold), 11.5,
                new SolidColorBrush(dangerColor));
            double pillW = ftTh.Width + 12;
            double pillH = ftTh.Height + 4;
            double pillX = w - padRight - pillW - 2;
            double pillY = Math.Max(2, ty - pillH - 3);
            context.DrawRectangle(dangerBg, new Pen(new SolidColorBrush(dangerColor), 1), new RoundedRect(new Rect(pillX, pillY, pillW, pillH), 4));
            context.DrawText(ftTh, new Point(pillX + 6, pillY + 2));
        }
    }
}

/// <summary>
/// Komponen Grafik Garis Kustom (OnRender) dengan Area Fill halus di bawah garis utama
/// untuk membandingkan Tren Nilai Praktikan (180DC Green) vs Rata-rata Kelompok (Biru).
/// </summary>
public class SimprakLineChart : Control
{
    public static readonly StyledProperty<IList<ChartPointDto>?> ItemsProperty =
        AvaloniaProperty.Register<SimprakLineChart, IList<ChartPointDto>?>(nameof(Items));

    public IList<ChartPointDto>? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    static SimprakLineChart()
    {
        AffectsRender<SimprakLineChart>(ItemsProperty);
    }

    public SimprakLineChart()
    {
        ThemePaletteService.ThemeChanged += InvalidateVisual;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        var items = Items;
        double w = Bounds.Width;
        double h = Bounds.Height;
        if (w <= 40 || h <= 40 || items == null || items.Count == 0)
            return;

        bool dark = ThemePaletteService.IsDark;
        var textBrush = new SolidColorBrush(Color.Parse(dark ? "#C0CEC2" : "#435148"));
        var primaryTextBrush = new SolidColorBrush(Color.Parse(dark ? "#F1F5F0" : "#111C14"));
        var gridPen = new Pen(new SolidColorBrush(Color.Parse(dark ? "#223528" : "#E4ECE1")), 1);
        var greenColor = Color.Parse(dark ? "#8BC34A" : "#6D9E31");
        var greenBrush = new SolidColorBrush(greenColor);
        var greenPen = new Pen(greenBrush, 3.0);
        var avgBrush = new SolidColorBrush(Color.Parse(dark ? "#38BDF8" : "#0369A1"));
        var avgPen = new Pen(avgBrush, 2.0, new DashStyle([4, 3], 0));

        double padLeft = 48;
        double padRight = 24;
        double padTop = 38;
        double padBottom = 42;
        double plotW = Math.Max(10, w - padLeft - padRight);
        double plotH = Math.Max(10, h - padTop - padBottom);

        // Legend at top-right
        var ftLeg1 = new FormattedText("● Nilai Praktikan", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold), 13, greenBrush);
        var ftLeg2 = new FormattedText("--- Rata-rata Kelompok", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.SemiBold), 13, avgBrush);
        double leg2X = w - padRight - ftLeg2.Width;
        double leg1X = leg2X - ftLeg1.Width - 18;
        context.DrawText(ftLeg1, new Point(leg1X, 4));
        context.DrawText(ftLeg2, new Point(leg2X, 4));

        for (int i = 0; i <= 4; i++)
        {
            double frac = i / 4.0;
            double y = padTop + plotH - (frac * plotH);
            context.DrawLine(gridPen, new Point(padLeft, y), new Point(w - padRight, y));
            var ft = new FormattedText($"{i * 25}", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                Typeface.Default, 12, textBrush);
            context.DrawText(ft, new Point(8, y - ft.Height / 2));
        }

        int n = items.Count;
        double stepX = n > 1 ? plotW / (n - 1) : plotW / 2;

        var mainPts = new List<Point>();
        var avgPts = new List<Point>();

        for (int i = 0; i < n; i++)
        {
            var pt = items[i];
            double x = n > 1 ? padLeft + i * stepX : padLeft + plotW / 2;
            double yMain = padTop + plotH - (Math.Clamp(pt.Value, 0, 100) / 100.0 * plotH);
            double yAvg = padTop + plotH - (Math.Clamp(pt.SecondaryValue, 0, 100) / 100.0 * plotH);

            mainPts.Add(new Point(x, yMain));
            avgPts.Add(new Point(x, yAvg));
        }

        if (mainPts.Count > 1)
        {
            var areaGeom = new StreamGeometry();
            using (var gctx = areaGeom.Open())
            {
                gctx.BeginFigure(new Point(mainPts[0].X, padTop + plotH), true);
                foreach (var p in mainPts)
                    gctx.LineTo(p);
                gctx.LineTo(new Point(mainPts[^1].X, padTop + plotH));
                gctx.EndFigure(true);
            }
            var areaFill = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop(Color.FromArgb(75, greenColor.R, greenColor.G, greenColor.B), 0),
                    new GradientStop(Color.FromArgb(5, greenColor.R, greenColor.G, greenColor.B), 1)
                ]
            };
            context.DrawGeometry(areaFill, null, areaGeom);
        }

        for (int i = 1; i < n; i++)
        {
            context.DrawLine(avgPen, avgPts[i - 1], avgPts[i]);
            context.DrawLine(greenPen, mainPts[i - 1], mainPts[i]);
        }

        for (int i = 0; i < n; i++)
        {
            context.DrawEllipse(avgBrush, null, avgPts[i], 3.8, 3.8);
            context.DrawEllipse(Brushes.White, new Pen(greenBrush, 2.5), mainPts[i], 5.0, 5.0);

            if (items[i].Value > 0)
            {
                var ftScore = new FormattedText($"{items[i].Value:0.#}", CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold), 12, primaryTextBrush);
                context.DrawText(ftScore, new Point(mainPts[i].X - ftScore.Width / 2, Math.Max(14, mainPts[i].Y - ftScore.Height - 6)));
            }

            var ftLbl = new FormattedText(items[i].Label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.SemiBold), 12, textBrush);
            context.DrawText(ftLbl, new Point(mainPts[i].X - ftLbl.Width / 2, padTop + plotH + 7));
        }
    }
}
