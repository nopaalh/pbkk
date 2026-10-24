using Avalonia;
using Avalonia.Headless;
using Simprak.App.ViewModels;
using Simprak.Core.Services;
using Simprak.Data.Services;

namespace Simprak.Tests;

[Collection("Avalonia UI")]
public class ToastTests
{
    [Fact]
    public async Task Toast_HidesAfterTwoSeconds_AndRestartsWhenReplaced()
    {
        using var ui = HeadlessUnitTestSession.StartNew(typeof(ToastTestApp));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await ui.Dispatch(async () =>
        {
            var session = new UserSession();
            var vm = new MainWindowViewModel(new SimprakDataService("Data Source=:memory:", session), session, "unused.db");
            vm.TampilkanToast("Pertama");
            Assert.True(vm.IsToastVisible);
            await Task.Delay(2300);
            Assert.False(vm.IsToastVisible);

            vm.TampilkanToast("Kedua");
            await Task.Delay(1000);
            vm.TampilkanToast("Terbaru", isError: true);
            await Task.Delay(1300);
            Assert.True(vm.IsToastVisible);
            Assert.Equal("Terbaru", vm.ToastMessage);
            Assert.True(vm.IsToastError);
            await Task.Delay(1000);
            Assert.False(vm.IsToastVisible);

            vm.TampilkanToast("Tutup manual");
            vm.TutupToast();
            Assert.False(vm.IsToastVisible);
            return true;
        }, timeout.Token);
    }

    public class ToastTestApp
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<Application>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }
}
