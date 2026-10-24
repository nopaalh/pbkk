using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Simprak.Core.Models;
using Simprak.Core.Rules;
using Simprak.Core.Services;
using Simprak.Data.Services;
using Simprak.App.Services;

namespace Simprak.App.ViewModels;

public partial class BarisPenilaianViewModel : ObservableObject
{
    private readonly string _origWaktu;
    private readonly string _origTautan;
    private readonly string _origNilai;
    private readonly string _origCatatan;
    private readonly string _tenggatIso;
    private readonly Action _onDirtyChanged;

    public int IdPraktikan { get; }
    public int IdModul { get; }
    public string Nrp { get; }
    public string NamaLengkap { get; }
    public string NamaPenilai { get; }

    [ObservableProperty]
    private string _waktuKumpulInput;

    [ObservableProperty]
    private string _tautanTugasInput;

    [ObservableProperty]
    private string _nilaiInput;

    [ObservableProperty]
    private string _catatanInput;

    [ObservableProperty]
    private string _statusPengumpulan;

    public bool BisaIsiNilai => !string.IsNullOrWhiteSpace(WaktuKumpulInput);
    public bool PunyaTautanValid => !string.IsNullOrWhiteSpace(TautanTugasInput) && SimprakRules.ValidasiTautanTugas(TautanTugasInput);

    public bool IsDirty =>
        WaktuKumpulInput != _origWaktu ||
        TautanTugasInput != _origTautan ||
        NilaiInput != _origNilai ||
        CatatanInput != _origCatatan;

    public BarisPenilaianViewModel(StatusPengumpulanDto dto, Action onDirtyChanged)
    {
        _onDirtyChanged = onDirtyChanged;
        IdPraktikan = dto.IdPraktikan;
        IdModul = dto.IdModul;
        Nrp = dto.Nrp;
        NamaLengkap = dto.NamaLengkap;
        NamaPenilai = string.IsNullOrWhiteSpace(dto.NamaPenilai) ? "-" : dto.NamaPenilai;
        _tenggatIso = dto.Tenggat;

        _origWaktu = string.IsNullOrWhiteSpace(dto.WaktuKumpul) ? string.Empty : SimprakRules.FormatWaktuTampilan(dto.WaktuKumpul);
        _origTautan = dto.TautanTugas ?? string.Empty;
        _origNilai = dto.Nilai.HasValue ? dto.Nilai.Value.ToString("0.##", CultureInfo.InvariantCulture) : string.Empty;
        _origCatatan = dto.Catatan ?? string.Empty;

        _waktuKumpulInput = _origWaktu;
        _tautanTugasInput = _origTautan;
        _nilaiInput = _origNilai;
        _catatanInput = _origCatatan;
        _statusPengumpulan = dto.StatusPengumpulan;
    }

    partial void OnWaktuKumpulInputChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            StatusPengumpulan = SimprakRules.StatusBelumMengumpulkan;
        }
        else if (SimprakRules.CobaFormatWaktuDatabase(value, out string iso))
        {
            StatusPengumpulan = SimprakRules.HitungStatusPengumpulan(iso, _tenggatIso);
        }
        OnPropertyChanged(nameof(BisaIsiNilai));
        OnPropertyChanged(nameof(IsDirty));
        _onDirtyChanged();
    }

    partial void OnTautanTugasInputChanged(string value)
    {
        OnPropertyChanged(nameof(PunyaTautanValid));
        OnPropertyChanged(nameof(IsDirty));
        _onDirtyChanged();
    }

    partial void OnNilaiInputChanged(string value)
    {
        OnPropertyChanged(nameof(IsDirty));
        _onDirtyChanged();
    }

    partial void OnCatatanInputChanged(string value)
    {
        OnPropertyChanged(nameof(IsDirty));
        _onDirtyChanged();
    }

    [RelayCommand]
    public void TandaiTerkumpulSekarang()
    {
        if (string.IsNullOrWhiteSpace(WaktuKumpulInput))
        {
            WaktuKumpulInput = DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        }
    }

    public void ResetKeAsal()
    {
        WaktuKumpulInput = _origWaktu;
        TautanTugasInput = _origTautan;
        NilaiInput = _origNilai;
        CatatanInput = _origCatatan;
    }

    public (bool Valid, string Error, InputPenilaianBarisDto? Dto) ToDto()
    {
        double? parsedNilai = null;
        if (!string.IsNullOrWhiteSpace(NilaiInput))
        {
            if (!double.TryParse(NilaiInput.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                return (false, $"NRP {Nrp}: Format angka nilai tidak valid.", null);
            parsedNilai = v;
        }

        return (true, string.Empty, new InputPenilaianBarisDto
        {
            IdPraktikan = IdPraktikan,
            IdModul = IdModul,
            WaktuKumpul = string.IsNullOrWhiteSpace(WaktuKumpulInput) ? null : WaktuKumpulInput.Trim(),
            TautanTugas = string.IsNullOrWhiteSpace(TautanTugasInput) ? null : TautanTugasInput.Trim(),
            Nilai = parsedNilai,
            Catatan = string.IsNullOrWhiteSpace(CatatanInput) ? null : CatatanInput.Trim()
        });
    }
}
