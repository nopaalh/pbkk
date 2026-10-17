using System.Globalization;

namespace HaloLinux;

class Mahasiswa
{
    public string Nim { get; set; } = string.Empty;
    public string Nama { get; set; } = string.Empty;
    public string Prodi { get; set; } = string.Empty;
    public double Ipk { get; set; }

    public Mahasiswa(string nim, string nama, string prodi, double ipk)
    {
        Nim = nim;
        Nama = nama;
        Prodi = prodi;
        Ipk = ipk;
    }
}

class Program
{
    static List<Mahasiswa> daftarMahasiswa = new List<Mahasiswa>();

    static void Main(string[] args)
    {
        bool berjalan = true;
        while (berjalan)
        {
            TampilkanMenu();
            Console.Write("Pilih menu: ");
            string? pilihan = Console.ReadLine()?.Trim();

            switch (pilihan)
            {
                case "1":
                    TambahMahasiswa();
                    break;
                case "2":
                    TampilkanSemuaMahasiswa();
                    break;
                case "3":
                    CariMahasiswa();
                    break;
                case "4":
                    HapusMahasiswa();
                    break;
                case "5":
                    berjalan = false;
                    break;
                default:
                    Console.WriteLine("\nPilihan tidak valid. Silakan coba lagi.");
                    break;
            }
            Console.WriteLine();
        }
    }

    static void TampilkanMenu()
    {
        Console.WriteLine("================================");
        Console.WriteLine("     SISTEM DATA MAHASISWA");
        Console.WriteLine("================================");
        Console.WriteLine("1. Tambah Data Mahasiswa");
        Console.WriteLine("2. Tampilkan Semua Mahasiswa");
        Console.WriteLine("3. Cari Mahasiswa (berdasarkan NIM)");
        Console.WriteLine("4. Hapus Mahasiswa (berdasarkan NIM)");
        Console.WriteLine("5. Keluar");
        Console.WriteLine("--------------------------------");
    }

    static void TambahMahasiswa()
    {
        Console.WriteLine();
        Console.WriteLine("Tambah Data Mahasiswa");
        Console.Write("NIM    : ");
        string nim = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Nama   : ");
        string nama = Console.ReadLine()?.Trim() ?? "";

        Console.Write("Prodi  : ");
        string prodi = Console.ReadLine()?.Trim() ?? "";

        Console.Write("IPK (0.00 - 4.00): ");
        string ipkInput = Console.ReadLine()?.Trim().Replace(',', '.') ?? "0";
        if (!double.TryParse(ipkInput, CultureInfo.InvariantCulture, out double ipk))
        {
            ipk = 0.00;
        }

        daftarMahasiswa.Add(new Mahasiswa(nim, nama, prodi, ipk));
        Console.WriteLine("Data mahasiswa berhasil ditambahkan.");
    }

    static void TampilkanSemuaMahasiswa()
    {
        Console.WriteLine();
        Console.WriteLine("Daftar Mahasiswa");
        string garis = new string('-', 70);
        Console.WriteLine(garis);
        Console.WriteLine($"{"NIM",-15}| {"Nama",-30}| {"Prodi",-20}| IPK");
        Console.WriteLine(garis);

        if (daftarMahasiswa.Count > 0)
        {
            foreach (var m in daftarMahasiswa)
            {
                Console.WriteLine($"{m.Nim,-15}| {m.Nama,-30}| {m.Prodi,-20}| {m.Ipk.ToString("0.00", CultureInfo.InvariantCulture)}");
            }
        }
        Console.WriteLine(garis);
    }

    static void CariMahasiswa()
    {
        Console.WriteLine();
        Console.WriteLine("Cari Mahasiswa");
        Console.Write("Masukkan NIM: ");
        string nim = Console.ReadLine()?.Trim() ?? "";

        Console.WriteLine();
        var mhs = daftarMahasiswa.FirstOrDefault(m => m.Nim.Equals(nim, StringComparison.OrdinalIgnoreCase));
        if (mhs != null)
        {
            Console.WriteLine("Data Ditemukan:");
            Console.WriteLine($"NIM    : {mhs.Nim}");
            Console.WriteLine($"Nama   : {mhs.Nama}");
            Console.WriteLine($"Prodi  : {mhs.Prodi}");
            Console.WriteLine($"IPK    : {mhs.Ipk.ToString("0.00", CultureInfo.InvariantCulture)}");
        }
        else
        {
            Console.WriteLine($"Mahasiswa dengan NIM {nim} tidak ditemukan.");
        }
    }

    static void HapusMahasiswa()
    {
        Console.WriteLine();
        Console.WriteLine("Hapus Mahasiswa");
        Console.Write("Masukkan NIM: ");
        string nim = Console.ReadLine()?.Trim() ?? "";

        var mhs = daftarMahasiswa.FirstOrDefault(m => m.Nim.Equals(nim, StringComparison.OrdinalIgnoreCase));
        if (mhs != null)
        {
            daftarMahasiswa.Remove(mhs);
            Console.WriteLine($"Data mahasiswa dengan NIM {nim} berhasil dihapus.");
        }
        else
        {
            Console.WriteLine($"Mahasiswa dengan NIM {nim} tidak ditemukan.");
        }
    }
}
