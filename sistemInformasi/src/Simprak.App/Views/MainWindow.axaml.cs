using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace Simprak.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += OnMainWindowKeyDown;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnMainWindowKeyDown(object? sender, KeyEventArgs e)
    {
        // Pintasan Pencarian Praktikan Cepat (Ctrl+K)
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.K)
        {
            var searchBox = this.FindControl<TextBox>("GlobalSearchBox");
            searchBox?.Focus();
            e.Handled = true;
            return;
        }

        // Pintasan Escape untuk menutup dialog modal atau toast notifikasi
        if (e.Key == Key.Escape && DataContext is ViewModels.MainWindowViewModel vm)
        {
            if (vm.IsDialogPraktikanOpen || vm.IsDialogKelompokOpen || vm.IsDialogModulOpen || vm.IsDialogPenggunaOpen || vm.IsDialogKonfirmasiOpen)
            {
                vm.TutupSemuaDialog();
                e.Handled = true;
            }
            else if (vm.IsToastVisible)
            {
                vm.TutupToast();
                e.Handled = true;
            }
        }
    }
}
