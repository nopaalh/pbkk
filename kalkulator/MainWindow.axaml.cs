using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Kalkulator;

public partial class MainWindow : Window
{
    private const int MaxDigits = 15;

    private string _inputSekarang = "";
    private double _angkaPertama = 0;
    private string _operasi = "";
    private bool _habisSamaDengan = false;
    private bool _isDegreeMode = true;

    public MainWindow()
    {
        InitializeComponent();

        AddHandler(TextInputEvent, OnWindowTextInput, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    private void OnModeToggleClick(object? sender, RoutedEventArgs e)
    {
        _isDegreeMode = !_isDegreeMode;
        BtnMode.Content = _isDegreeMode ? "DEG" : "RAD";
        BtnMode.Foreground = _isDegreeMode
            ? Brush.Parse("#FF9F0A")
            : Brush.Parse("#38BDF8");
    }

    private void OnScientificUnaryClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not string func)
            return;

        double val = TryAmbilAngkaLayar(out double parsed) ? parsed : 0;

        double result = 0;
        bool isError = false;
        string errorMsg = "Error";
        string historyText = "";

        switch (func)
        {
            case "sin":
                double angleSin = _isDegreeMode ? (val * Math.PI / 180.0) : val;
                result = Math.Sin(angleSin);
                if (_isDegreeMode && Math.Abs(val % 180) < 1e-9) result = 0;
                historyText = $"sin({FormatHasil(val)}{(_isDegreeMode ? "°" : "")})";
                break;

            case "cos":
                double angleCos = _isDegreeMode ? (val * Math.PI / 180.0) : val;
                result = Math.Cos(angleCos);
                if (_isDegreeMode && Math.Abs((val - 90) % 180) < 1e-9) result = 0;
                historyText = $"cos({FormatHasil(val)}{(_isDegreeMode ? "°" : "")})";
                break;

            case "tan":
                double angleTan = _isDegreeMode ? (val * Math.PI / 180.0) : val;
                if (_isDegreeMode && Math.Abs((val - 90) % 180) < 1e-9)
                {
                    isError = true;
                    errorMsg = "Domain Error";
                }
                else
                {
                    result = Math.Tan(angleTan);
                }
                historyText = $"tan({FormatHasil(val)}{(_isDegreeMode ? "°" : "")})";
                break;

            case "sqr":
                result = val * val;
                historyText = $"sqr({FormatHasil(val)})";
                break;

            case "sqrt":
                if (val < 0)
                {
                    isError = true;
                    errorMsg = "Invalid Input";
                }
                else
                {
                    result = Math.Sqrt(val);
                }
                historyText = $"√({FormatHasil(val)})";
                break;

            case "ln":
                if (val <= 0)
                {
                    isError = true;
                    errorMsg = "Invalid Input";
                }
                else
                {
                    result = Math.Log(val);
                }
                historyText = $"ln({FormatHasil(val)})";
                break;

            case "log":
                if (val <= 0)
                {
                    isError = true;
                    errorMsg = "Invalid Input";
                }
                else
                {
                    result = Math.Log10(val);
                }
                historyText = $"log({FormatHasil(val)})";
                break;

            case "fact":
                if (val < 0 || val != Math.Floor(val) || val > 170)
                {
                    isError = true;
                    errorMsg = "Invalid Input";
                }
                else
                {
                    result = HitungFaktorial((int)val);
                }
                historyText = $"{FormatHasil(val)}!";
                break;
        }

        TextRiwayat.Text = historyText;
        if (isError)
        {
            TextLayar.Text = errorMsg;
            _inputSekarang = "";
            _habisSamaDengan = true;
        }
        else
        {
            _inputSekarang = FormatHasil(result);
            UpdateLayar();
            _habisSamaDengan = true;
        }
    }

    private static double HitungFaktorial(int n)
    {
        double res = 1;
        for (int i = 2; i <= n; i++)
        {
            res *= i;
        }
        return res;
    }

    private void OnAngkaClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string digit)
        {
            InputDigit(digit);
        }
    }

    private void OnOperatorClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            string? op = button.Tag as string;
            string? content = button.Content?.ToString();
            string opSimbol = op ?? content ?? "";
            InputOperator(opSimbol);
        }
    }

    private void OnSamaDenganClick(object? sender, RoutedEventArgs e) => HitungHasil();
    private void OnAllClearClick(object? sender, RoutedEventArgs e) => ClearSemua();
    private void OnClearEntryClick(object? sender, RoutedEventArgs e) => ClearEntry();
    private void OnPercentClick(object? sender, RoutedEventArgs e) => HitungPersen();
    private void OnPlusMinusClick(object? sender, RoutedEventArgs e) => TogglePlusMinus();

    private void OnWindowTextInput(object? sender, TextInputEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text)) return;

        char c = e.Text[0];

        if (char.IsDigit(c))
        {
            InputDigit(c.ToString());
            e.Handled = true;
        }
        else if (c == '.' || c == ',')
        {
            InputDigit(".");
            e.Handled = true;
        }
        else if (c == '+' || c == '-')
        {
            InputOperator(c.ToString());
            e.Handled = true;
        }
        else if (c == '*' || c == 'x' || c == 'X')
        {
            InputOperator("*");
            e.Handled = true;
        }
        else if (c == '/' || c == ':')
        {
            InputOperator("/");
            e.Handled = true;
        }
        else if (c == '^')
        {
            InputOperator("^");
            e.Handled = true;
        }
        else if (c == '%')
        {
            HitungPersen();
            e.Handled = true;
        }
        else if (c == '=')
        {
            HitungHasil();
            e.Handled = true;
        }
        else if (c == 'c' || c == 'C')
        {
            ClearEntry();
            e.Handled = true;
        }
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            HitungHasil();
            e.Handled = true;
        }
        else if (e.Key == Key.Multiply)
        {
            InputOperator("*");
            e.Handled = true;
        }
        else if (e.Key == Key.Divide)
        {
            InputOperator("/");
            e.Handled = true;
        }
        else if (e.Key == Key.Add)
        {
            InputOperator("+");
            e.Handled = true;
        }
        else if (e.Key == Key.Subtract)
        {
            InputOperator("-");
            e.Handled = true;
        }
        else if (e.Key == Key.Back)
        {
            HapusKarakter();
            e.Handled = true;
        }
        else if (e.Key == Key.Delete)
        {
            ClearEntry();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            ClearSemua();
            e.Handled = true;
        }
    }

    private void InputDigit(string digit)
    {
        if (_habisSamaDengan)
        {
            _inputSekarang = "";
            _habisSamaDengan = false;
        }

        int digitCount = 0;
        foreach (char c in _inputSekarang)
        {
            if (char.IsDigit(c)) digitCount++;
        }

        if (digit != "." && digitCount >= MaxDigits)
            return;

        if (digit == "." && _inputSekarang.Contains('.'))
            return;

        if (string.IsNullOrEmpty(_inputSekarang) && digit == ".")
        {
            _inputSekarang = "0.";
        }
        else if (_inputSekarang == "-0" && digit != ".")
        {
            _inputSekarang = "-" + digit;
        }
        else if (_inputSekarang == "0" && digit != ".")
        {
            _inputSekarang = digit;
        }
        else
        {
            _inputSekarang += digit;
        }

        UpdateLayar();
    }

    private void TogglePlusMinus()
    {
        if (string.IsNullOrEmpty(_inputSekarang) || _inputSekarang == "0")
            return;

        if (_inputSekarang.StartsWith('-'))
        {
            _inputSekarang = _inputSekarang[1..];
        }
        else
        {
            _inputSekarang = "-" + _inputSekarang;
        }

        _habisSamaDengan = false;
        UpdateLayar();
    }

    private void HitungPersen()
    {
        if (!TryAmbilAngkaLayar(out double angkaSaatIni))
            return;

        double hasilPersen = !string.IsNullOrEmpty(_operasi) && (_operasi == "+" || _operasi == "-")
            ? _angkaPertama * (angkaSaatIni / 100.0)
            : angkaSaatIni / 100.0;

        _inputSekarang = FormatHasil(hasilPersen);
        _habisSamaDengan = false;
        UpdateLayar();
    }

    private void InputOperator(string opSimbol)
    {
        if (string.IsNullOrEmpty(opSimbol)) return;

        if (!string.IsNullOrEmpty(_operasi) && !string.IsNullOrEmpty(_inputSekarang) && !_habisSamaDengan)
        {
            HitungHasil();
        }

        if (!TryAmbilAngkaLayar(out double angkaSaatIni))
        {
            angkaSaatIni = _angkaPertama;
        }

        _angkaPertama = angkaSaatIni;
        _operasi = opSimbol;

        string displayOp = FormatSimbolOperator(opSimbol);
        TextRiwayat.Text = $"{FormatHasil(_angkaPertama)} {displayOp}";
        _inputSekarang = "";
        _habisSamaDengan = false;
    }

    private void HitungHasil()
    {
        if (string.IsNullOrEmpty(_operasi)) return;

        if (!TryAmbilAngkaLayar(out double angkaKedua))
        {
            angkaKedua = _angkaPertama;
        }

        double hasil = 0;
        bool adaError = false;

        switch (_operasi)
        {
            case "+":
                hasil = _angkaPertama + angkaKedua;
                break;
            case "-":
                hasil = _angkaPertama - angkaKedua;
                break;
            case "*":
                hasil = _angkaPertama * angkaKedua;
                break;
            case "/":
                if (angkaKedua == 0)
                {
                    TextLayar.Text = "Error: /0";
                    adaError = true;
                }
                else
                {
                    hasil = _angkaPertama / angkaKedua;
                }
                break;
            case "^":
                hasil = Math.Pow(_angkaPertama, angkaKedua);
                break;
        }

        string displayOp = FormatSimbolOperator(_operasi);
        TextRiwayat.Text = $"{FormatHasil(_angkaPertama)} {displayOp} {FormatHasil(angkaKedua)} =";

        if (!adaError)
        {
            string formattedHasil = FormatHasil(hasil);
            TextLayar.Text = formattedHasil;
            _inputSekarang = formattedHasil;
        }
        else
        {
            _inputSekarang = "";
        }

        _operasi = "";
        _habisSamaDengan = true;
    }

    private static string FormatSimbolOperator(string opSimbol) => opSimbol switch
    {
        "*" => "×",
        "/" => "÷",
        "^" => "^",
        _ => opSimbol
    };

    private bool TryAmbilAngkaLayar(out double nilai)
    {
        if (string.IsNullOrEmpty(_inputSekarang))
        {
            nilai = 0;
            return false;
        }

        return double.TryParse(_inputSekarang, NumberStyles.Float, CultureInfo.InvariantCulture, out nilai);
    }

    private static string FormatHasil(double val)
    {
        if (double.IsNaN(val)) return "NaN";
        if (double.IsPositiveInfinity(val) || double.IsNegativeInfinity(val)) return "Overflow";
        if (val == 0) return "0";

        double absVal = Math.Abs(val);

        if (absVal >= 1e15 || absVal < 1e-7)
        {
            return val.ToString("G12", CultureInfo.InvariantCulture);
        }

        return val.ToString("G15", CultureInfo.InvariantCulture);
    }

    private void ClearEntry()
    {
        _inputSekarang = "";
        _habisSamaDengan = false;
        TextLayar.Text = "0";
    }

    private void ClearSemua()
    {
        _inputSekarang = "";
        _angkaPertama = 0;
        _operasi = "";
        TextRiwayat.Text = "";
        TextLayar.Text = "0";
        _habisSamaDengan = false;
    }

    private void HapusKarakter()
    {
        if (_habisSamaDengan)
        {
            ClearSemua();
            return;
        }

        if (_inputSekarang.Length > 0)
        {
            _inputSekarang = _inputSekarang[..^1];
            if (_inputSekarang == "-")
            {
                _inputSekarang = "";
            }
            UpdateLayar();
        }
    }

    private void UpdateLayar()
    {
        TextLayar.Text = string.IsNullOrEmpty(_inputSekarang) ? "0" : _inputSekarang;
    }
}
