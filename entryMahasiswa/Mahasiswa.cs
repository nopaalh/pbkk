using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudentRegistrationApp;

public sealed class Mahasiswa : INotifyPropertyChanged
{
    private string _nim = string.Empty;
    private string _nama = string.Empty;
    private string _prodi = string.Empty;
    private string _jenisKelamin = string.Empty;

    public string Nim
    {
        get => _nim;
        set => SetField(ref _nim, value);
    }

    public string Nama
    {
        get => _nama;
        set => SetField(ref _nama, value);
    }

    public string Prodi
    {
        get => _prodi;
        set => SetField(ref _prodi, value);
    }

    public string JenisKelamin
    {
        get => _jenisKelamin;
        set => SetField(ref _jenisKelamin, value);
    }

    public string Inisial
    {
        get
        {
            var parts = Nama.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length == 0 ? "?" : parts.Length == 1
                ? parts[0][0].ToString().ToUpperInvariant()
                : $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
        }
    }

    public string Detail => $"{Prodi} · {JenisKelamin}";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Detail)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Inisial)));
    }
}
