# Laporan Praktikum PBKK: Aplikasi Kalkulator Saintifik Berbasis Desktop (Avalonia UI & .NET)

## Metadata Laporan
- **Mata Kuliah:** Pemrograman Berbasis Kerangka Kerja (PBKK)
- **Topik:** Pemrograman GUI Desktop Event-Driven dengan Avalonia UI (.NET Cross-Platform)
- **Bahasa / Framework:** C# / .NET 10.0 (`net10.0`) + Avalonia UI `11.0.10`
- **Platform Target:** Cross-Platform Desktop (Linux, Windows, macOS)
- **Direktori Proyek:** `kalkulator/`

---

## 1. Gambaran Umum Proyek

Proyek ini merupakan aplikasi desktop **Kalkulator Saintifik** yang dibangun menggunakan kerangka kerja **Avalonia UI** di atas **.NET 10.0**. Aplikasi ini menerapkan paradigma **Event-Driven GUI Programming** dengan memisahkan deklarasi tampilan antarmuka pengguna pada berkas XAML (`MainWindow.axaml`) dan logika pemrosesan matematika pada berkas C# *code-behind* (`MainWindow.axaml.cs`).

### Fitur Utama
1. **Desain Antarmuka Modern Gelap (Grid Tombol Lingkaran $6 \times 5$):**
   - Latar belakang hitam pekat (`#000000`) dengan tampilan angka hasil berukuran besar (`FontSize="96"`) rata kanan dan teks riwayat operasi di atasnya.
   - Tombol berbentuk lingkaran presisi ($90 \times 90\text{ px}$, `CornerRadius="45"`) yang terbagi ke dalam empat kategori warna:
     - **Abu-abu Terang (`#A5A5A5`):** Tombol kontrol `C` (*Clear Entry*), `AC` (*All Clear*), dan `%` (*Persen*).
     - **Abu-abu Gelap (`#333333`):** Tombol angka `0`–`9`, titik desimal (`.`), dan pembalik tanda (`+/-`).
     - **Hitam Keabuan (`#222222`):** Tombol fungsi saintifik (`DEG`/`RAD`, `sin`, `cos`, `tan`, `n!`, `xʸ`, `x²`, `√`, `ln`, `log`).
     - **Oranye (`#FF9F0A`):** Tombol operator aritmatika dasar (`÷`, `×`, `-`, `+`, `=`).
2. **Fungsi Saintifik Lengkap:**
   - **Trigonometri:** `sin`, `cos`, `tan` dengan pengubah satuan sudut **Derajat (`DEG`)** dan **Radian (`RAD`)**.
   - **Eksponensial, Akar, dan Faktorial:** Perpangkatan (`xʸ`), kuadrat (`x²`), akar kuadrat (`√`), dan faktorial (`n!`).
   - **Logaritma:** Logaritma natural berbasis $e$ (`ln`) dan logaritma berbasis 10 (`log`).
3. **Operasi Aritmatika & Kontrol Input:** Penjumlahan (`+`), pengurangan (`-`), perkalian (`×`), pembagian (`÷`), persentase (`%`), pembalik tanda positif/negatif (`+/-`), penghapus entri aktif (`C`), dan penghapus seluruh state (`AC`).
4. **Dukungan Input Ganda (Mouse & Keyboard):** Seluruh operasi dapat dijalankan melalui klik tombol di layar maupun ketikan langsung pada papan ketik.

---

## 2. Struktur Proyek

```text
kalkulator/
├── App.axaml                 # Deklarasi tema global aplikasi (FluentTheme Dark)
├── App.axaml.cs              # Inisialisasi siklus hidup aplikasi Avalonia
├── Kalkulator.csproj         # Konfigurasi proyek .NET 10 dan paket NuGet Avalonia
├── LAPORAN.md                # Dokumentasi laporan praktikum
├── MainWindow.axaml          # Desain tata letak antarmuka pengguna (XAML)
├── MainWindow.axaml.cs       # Logika komputasi kalkulator dan event handler (C#)
└── Program.cs                # Titik masuk utama (Main Entry Point) aplikasi
```

---

## 3. Konfigurasi Proyek & Siklus Hidup Aplikasi

### 3.1. `Kalkulator.csproj`
Berkas konfigurasi proyek `.csproj` menetapkan tipe keluaran sebagai aplikasi berjendela (`WinExe`), menargetkan `.NET 10.0`, serta memuat dependensi pustaka **Avalonia UI 11.0.10**.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>Kalkulator</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <BuiltInComInteropSupport>true</BuiltInComInteropSupport>
    <NoWarn>$(NoWarn);NU1903</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.0.10" />
    <PackageReference Include="Avalonia.Desktop" Version="11.0.10" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.10" />
    <PackageReference Include="Avalonia.Fonts.Inter" Version="11.0.10" />
    <PackageReference Include="Tmds.DBus.Protocol" Version="0.21.0" />
  </ItemGroup>

</Project>
```

### 3.2. `Program.cs`
Berisi titik masuk utama (`Main`) yang mengonfigurasi `AppBuilder` untuk mendeteksi sistem operasi secara otomatis (`UsePlatformDetect()`), memuat fon `Inter`, dan menjalankan siklus hidup aplikasi desktop klasik.

```csharp
using Avalonia;
using System;

namespace Kalkulator;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
```

### 3.3. `App.axaml.cs`
Mengatur inisialisasi XAML dan menetapkan `MainWindow` sebagai jendela utama aplikasi saat kerangka kerja selesai dimuat.

```csharp
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Kalkulator;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
```

---

## 4. Dokumentasi Fungsi dan Snapshot Kode (`MainWindow.axaml.cs`)

Bagian ini menjelaskan seluruh variabel state dan fungsi (method) yang terdapat pada kelas `MainWindow`.

### 4.1. Deklarasi Variabel State dan Konstruktor `MainWindow()`
Menyimpan status perhitungan kalkulator selama aplikasi berjalan dan mendaftarkan *event handler* untuk pengetikan teks dari papan ketik (`TextInputEvent`).

```csharp
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
```

**Penjelasan:**
- `MaxDigits`: Membatasi panjang digit input maksimal sebanyak 15 digit agar sesuai dengan batas presisi tipe data `double` IEEE 754.
- `_inputSekarang`: Menyimpan teks angka yang sedang diketik pengguna di layar utama.
- `_angkaPertama`: Menyimpan nilai operan pertama saat pengguna memilih operator biner (`+`, `-`, `*`, `/`, `^`).
- `_operasi`: Menyimpan simbol operator aritmatika biner yang sedang aktif.
- `_habisSamaDengan`: Menandai apakah layar baru saja menampilkan hasil akhir (`=`) atau hasil fungsi saintifik, sehingga ketikan digit berikutnya akan memulai angka baru.
- `_isDegreeMode`: Menentukan satuan sudut fungsi trigonometri (`true` untuk Derajat/`DEG`, `false` untuk Radian/`RAD`).

---

### 4.2. Fungsi `OnModeToggleClick`
Mengubah mode perhitungan sudut trigonometri antara **Derajat (`DEG`)** dan **Radian (`RAD`)**.

```csharp
private void OnModeToggleClick(object? sender, RoutedEventArgs e)
{
    _isDegreeMode = !_isDegreeMode;
    BtnMode.Content = _isDegreeMode ? "DEG" : "RAD";
    BtnMode.Foreground = _isDegreeMode
        ? Brush.Parse("#FF9F0A")
        : Brush.Parse("#38BDF8");
}
```

**Penjelasan:**
- Membalikkan nilai boolean `_isDegreeMode` setiap kali tombol diklik.
- Memperbarui teks tombol `BtnMode` menjadi `"DEG"` (warna oranye `#FF9F0A`) atau `"RAD"` (warna biru `#38BDF8`) sebagai indikator visual bagi pengguna.

---

### 4.3. Fungsi `OnScientificUnaryClick`
Menangani seluruh operasi matematika saintifik satu operan (*unary operations*), yaitu `sin`, `cos`, `tan`, `x²` (`sqr`), `√` (`sqrt`), `ln`, `log`, dan `n!` (`fact`).

```csharp
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
```

**Penjelasan:**
- Mengidentifikasi fungsi saintifik yang dipanggil melalui properti `Tag` pada `Button`.
- Untuk fungsi `sin`, `cos`, dan `tan`, apabila `_isDegreeMode` bernilai `true`, nilai sudut dikonversi dari derajat ke radian dengan rumus $\text{rad} = \text{deg} \times \frac{\pi}{180}$. Disertakan pula penanganan presisi titik ambang (`1e-9`) untuk sudut kelipatan $90^\circ$ dan $180^\circ$, serta validasi `Domain Error` pada $\tan(90^\circ + k \cdot 180^\circ)$.
- Untuk `sqrt`, `ln`, `log`, dan `fact`, dilakukan validasi domain matematika (misalnya bilangan negatif pada akar/faktorial, bilangan $\le 0$ pada logaritma, atau $n > 170$ pada faktorial yang melebihi batas `double.MaxValue`).

---

### 4.4. Fungsi `HitungFaktorial`
Menghitung nilai faktorial ($n! = 1 \times 2 \times 3 \times \dots \times n$) secara iteratif.

```csharp
private static double HitungFaktorial(int n)
{
    double res = 1;
    for (int i = 2; i <= n; i++)
    {
        res *= i;
    }
    return res;
}
```

**Penjelasan:**
- Menggunakan perulangan `for` dari `2` hingga `n` untuk mengalikan nilai secara berurutan. Pendekatan iteratif dipilih agar tidak membebani *call stack* seperti pada rekursi.

---

### 4.5. Fungsi Event Handler Tombol GUI (`OnAngkaClick`, `OnOperatorClick`, dll.)
Menghubungkan aksi klik tombol pada antarmuka XAML ke fungsi logika internal kalkulator.

```csharp
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
```

**Penjelasan:**
- `OnAngkaClick`: Membaca digit angka (`0`–`9` atau `.`) dari atribut `Tag` tombol lalu meneruskannya ke `InputDigit()`.
- `OnOperatorClick`: Membaca simbol operator (`+`, `-`, `*`, `/`, `^`) dari atribut `Tag` tombol lalu memanggil `InputOperator()`.
- `OnSamaDenganClick`, `OnAllClearClick`, `OnClearEntryClick`, `OnPercentClick`, `OnPlusMinusClick`: Meneruskan klik tombol `=`, `AC`, `C`, `%`, dan `+/-` ke masing-masing fungsi pemrosesnya.

---

### 4.6. Fungsi Input Papan Ketik (`OnWindowTextInput` & `OnWindowKeyDown`)
Memungkinkan pengguna mengoperasikan kalkulator menggunakan papan ketik fisik.

```csharp
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
```

**Penjelasan:**
- `OnWindowTextInput`: Menangkap karakter teks yang diketik pengguna (angka `0`–`9`, koma/titik desimal, simbol operator `+`, `-`, `*`, `/`, `^`, `%`, `=`, dan `c`).
- `OnWindowKeyDown`: Menangkap tombol fungsi non-karakter seperti `Enter` (hitung hasil), `Backspace` (hapus satu digit terakhir), `Delete` (`ClearEntry`), `Escape` (`ClearSemua`), serta tombol operator pada *Numpad*.

---

### 4.7. Fungsi `InputDigit`
Menyusun dan memvalidasi digit angka maupun titik desimal yang dimasukkan pengguna ke dalam variabel `_inputSekarang`.

```csharp
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
```

**Penjelasan:**
- Mereset `_inputSekarang` apabila sebelumnya pengguna baru saja menekan tombol `=` (`_habisSamaDengan == true`).
- Mencegah pengguna memasukkan lebih dari 15 digit angka (`MaxDigits`) atau memasukkan lebih dari satu titik desimal (`.`).
- Menangani kasus awal ketika pengguna langsung menekan `.` (otomatis diubah menjadi `"0."`) dan mencegah penumpukan nol di depan angka (`"05"` menjadi `"5"`).

---

### 4.8. Fungsi `TogglePlusMinus` dan `HitungPersen`
Mengelola perubahan tanda positif/negatif (`+/-`) serta konversi nilai persentase (`%`).

```csharp
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
```

**Penjelasan:**
- `TogglePlusMinus`: Menambahkan atau menghapus karakter minus (`-`) pada awal string `_inputSekarang` jika nilainya bukan nol.
- `HitungPersen`: Mengubah angka aktif menjadi nilai persentase. Jika sedang berada dalam operasi penjumlahan atau pengurangan (`A + B%`), nilai persen dihitung sebagai persentase dari operan pertama ($A \times \frac{B}{100}$); selain itu nilai dibagi langsung dengan $100$.

---

### 4.9. Fungsi `InputOperator` dan `HitungHasil`
Menangani penyimpanan operator biner (`+`, `-`, `*`, `/`, `^`) dan mengeksekusi perhitungan akhir saat tombol `=` ditekan.

```csharp
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
```

**Penjelasan:**
- `InputOperator`: Menyimpan angka di layar ke `_angkaPertama` dan menyimpan simbol operator ke `_operasi`. Apabila sebelumnya sudah ada operasi aktif dan pengguna menekan operator berikutnya (perhitungan berantai seperti `2 + 3 +`), maka `HitungHasil()` dieksekusi terlebih dahulu secara otomatis.
- `HitungHasil`: Mengevaluasi operasi biner antara `_angkaPertama` dan `angkaKedua`, memeriksa pembagian dengan nol (`Error: /0`), memperbarui teks riwayat ekspresi pada `TextRiwayat`, dan menampilkan hasil yang telah diformat ke `TextLayar`.

---

### 4.10. Fungsi Utilitas Format, Konversi, dan Reset (`FormatHasil`, `ClearEntry`, `ClearSemua`, `HapusKarakter`)
Menangani konversi string ke angka, pemformatan hasil numerik bebas artefak *floating-point*, serta fungsi pembersihan layar.

```csharp
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
```

**Penjelasan:**
- `FormatSimbolOperator`: Mengubah operator internal (`*`, `/`) menjadi simbol matematika visual (`×`, `÷`) untuk ditampilkan pada riwayat.
- `TryAmbilAngkaLayar`: Mengonversi teks `_inputSekarang` menjadi tipe `double` secara aman menggunakan `CultureInfo.InvariantCulture` (titik sebagai pemisah desimal).
- `FormatHasil`: Memformat hasil `double` menggunakan spesifikasi `"G15"` untuk menghilangkan galat presisi desimal IEEE 754 (misalnya `0.1 + 0.2` tetap tampil `0.3`), atau `"G12"` (notasi ilmiah) untuk angka yang sangat besar/kecil, serta menangani `NaN` dan `Overflow`.
- `ClearEntry` (`C`): Mengosongkan angka yang sedang diketik saat ini menjadi `0` tanpa menghapus `_angkaPertama` dan `_operasi`.
- `ClearSemua` (`AC`): Mengembalikan seluruh variabel state, layar utama, dan teks riwayat ke kondisi awal.
- `HapusKarakter` (`Backspace`): Menghapus satu karakter paling kanan dari `_inputSekarang` menggunakan operator *range slice* C# (`[..^1]`).
- `UpdateLayar`: Menyinkronkan isi variabel `_inputSekarang` ke komponen `TextLayar` (menampilkan `"0"` jika string kosong).

---

## 5. Tata Cara Menjalankan Aplikasi

Pastikan **.NET 10.0 SDK** telah terpasang pada sistem.

```bash
# 1. Masuk ke direktori kalkulator (sibling dari week1)
cd kalkulator

# 2. Pulihkan dependensi NuGet dan kompilasi proyek
dotnet build

# 3. Jalankan aplikasi desktop kalkulator
dotnet run
```

---

## 6. Skenario Pengujian Sistem

| No | Skenario Uji | Langkah / Input | Hasil yang Diharapkan |
| :-: | :--- | :--- | :--- |
| 1 | Operasi Dasar | `12.5` `+` `7.5` `=` | Riwayat: `12.5 + 7.5 =`, Layar: `20` |
| 2 | Pembagian dengan Nol | `9` `÷` `0` `=` | Layar menampilkan `Error: /0` |
| 3 | Trigonometri (Mode DEG) | Pastikan mode `DEG`, tekan `30` lalu `sin` | Riwayat: `sin(30°)`, Layar: `0.5` |
| 4 | Trigonometri (Mode RAD) | Klik `DEG` hingga berubah menjadi `RAD`, tekan `0` lalu `cos` | Riwayat: `cos(0)`, Layar: `1` |
| 5 | Asimtot Tangen (Mode DEG) | Pastikan mode `DEG`, tekan `90` lalu `tan` | Layar menampilkan `Domain Error` |
| 6 | Pangkat & Akar | `2` `xʸ` `10` `=` lalu tekan `√` | Hasil `1024`, lalu akar menampilkan `32` |
| 7 | Faktorial | `5` `n!` | Riwayat: `5!`, Layar: `120` |
| 8 | Logaritma | `100` `log` | Riwayat: `log(100)`, Layar: `2` |
| 9 | Pembalik Tanda (`+/-`) | Ketik `45` lalu tekan `+/-` | Layar berubah menjadi `-45` |
| 10 | Clear (`C` vs `AC`) | `10` `+` `99`, tekan `C`, lalu `5` `=` | Angka `99` direset menjadi `0`, hasil akhir `15` |
