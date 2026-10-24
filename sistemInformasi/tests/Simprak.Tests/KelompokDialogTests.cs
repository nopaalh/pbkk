using Avalonia.Headless;
using Microsoft.Data.Sqlite;
using Simprak.App.ViewModels;
using Simprak.Core.Services;
using Simprak.Data;
using Simprak.Data.Services;

namespace Simprak.Tests;

[Collection("Avalonia UI")]
public class KelompokDialogTests
{
    [Fact]
    public async Task TambahAsistenDariKelompok_MemuatPilihanBaruDanMempertahankanForm()
    {
        using var ui = HeadlessUnitTestSession.StartNew(typeof(ToastTests.ToastTestApp));
        await ui.Dispatch(() =>
        {
            using var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
            connection.Open();
            DatabaseInitializer.InitializeOnConnection(connection, File.ReadAllText(DatabaseInitializer.FindSqlScriptPath()));
            var session = new UserSession();
            var service = new SimprakDataService(connection, session);
            Assert.True(service.Login("admin", "admin123").Sukses);
            var vm = new MainWindowViewModel(service, session, "unused.db") { IsAdmin = true, IsLoggedIn = true };
            vm.MuatUlangSemuaData();
            vm.BukaTambahKelompok();
            Assert.Empty(vm.PilihanAsistenTersedia);
            vm.FormKelompokNama = "Kelompok baru pengujian";
            int nomor = vm.FormKelompokNomor;

            vm.BukaTambahPengguna();
            vm.FormPenggunaUsername = "asisten_baru";
            vm.FormPenggunaNama = "Asisten Baru";
            vm.FormPenggunaPassword = "asisten123";
            vm.SimpanPengguna();

            Assert.True(vm.IsDialogKelompokOpen);
            Assert.False(vm.IsDialogPenggunaOpen);
            Assert.Equal("Kelompok baru pengujian", vm.FormKelompokNama);
            Assert.Equal(nomor, vm.FormKelompokNomor);
            Assert.Equal("asisten_baru", Assert.Single(vm.PilihanAsistenTersedia).Username);
            Assert.Equal("asisten_baru", vm.FormKelompokAsisten?.Username);

            vm.SimpanKelompok();
            Assert.False(vm.IsDialogKelompokOpen);
            var kelompok = Assert.Single(service.GetRekapKelompok(), k => k.NomorKelompok == nomor);
            Assert.Equal(vm.FormKelompokAsisten!.IdPengguna, kelompok.IdAsisten);
            Assert.Empty(service.GetAsistenTersediaUntukKelompok());
            vm.BukaEditKelompok(kelompok);
            Assert.Equal("asisten_baru", Assert.Single(vm.PilihanAsistenTersedia).Username);

            vm.BukaTambahPengguna();
            vm.TutupSemuaDialog();
            Assert.False(vm.IsDialogPenggunaOpen);
            Assert.True(vm.IsDialogKelompokOpen);
            Assert.Equal("Kelompok baru pengujian", vm.FormKelompokNama);
            vm.TutupSemuaDialog();
            return true;
        }, CancellationToken.None);
    }
}
