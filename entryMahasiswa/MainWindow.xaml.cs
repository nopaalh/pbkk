using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace StudentRegistrationApp;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<Mahasiswa> _mahasiswa = new();
    private readonly ICollectionView _mahasiswaView;

    public MainWindow()
    {
        InitializeComponent();
        _mahasiswaView = CollectionViewSource.GetDefaultView(_mahasiswa);
        lstMahasiswa.ItemsSource = _mahasiswaView;
        UpdateTotal();
    }

    private void BtnSimpan_Click(object sender, RoutedEventArgs e)
    {
        if (!TryReadForm(out var nim, out var nama, out var prodi, out var jenisKelamin))
        {
            return;
        }

        if (_mahasiswa.Any(item => string.Equals(item.Nim, nim, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("NIM tersebut sudah terdaftar.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNim.Focus();
            return;
        }

        _mahasiswa.Add(new Mahasiswa
        {
            Nim = nim,
            Nama = nama,
            Prodi = prodi,
            JenisKelamin = jenisKelamin
        });
        UpdateTotal();
        ResetForm();
        MessageBox.Show("Data mahasiswa berhasil disimpan.", "Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnPerbarui_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Mahasiswa selected })
        {
            return;
        }

        if (!ReferenceEquals(lstMahasiswa.SelectedItem, selected))
        {
            lstMahasiswa.SelectedItem = selected;
            return;
        }

        if (!TryReadForm(out var nim, out var nama, out var prodi, out var jenisKelamin))
        {
            return;
        }

        if (_mahasiswa.Any(item => !ReferenceEquals(item, selected) &&
                                   string.Equals(item.Nim, nim, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("NIM tersebut sudah digunakan mahasiswa lain.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNim.Focus();
            return;
        }

        selected.Nim = nim;
        selected.Nama = nama;
        selected.Prodi = prodi;
        selected.JenisKelamin = jenisKelamin;
        _mahasiswaView.Refresh();
        ResetForm();
        MessageBox.Show("Data mahasiswa berhasil diperbarui.", "Berhasil", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BtnHapus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: Mahasiswa selected })
        {
            return;
        }

        var confirmation = MessageBox.Show(
            $"Hapus data mahasiswa {selected.Nama} ({selected.Nim})?",
            "Konfirmasi Hapus",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        _mahasiswa.Remove(selected);
        UpdateTotal();
        ResetForm();
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e) => ResetForm();

    private void TxtCari_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_mahasiswaView is null)
        {
            return;
        }

        var query = txtCari.Text.Trim();
        _mahasiswaView.Filter = item =>
        {
            if (item is not Mahasiswa student)
            {
                return false;
            }

            return string.IsNullOrWhiteSpace(query) ||
                   student.Nim.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                   student.Nama.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                   student.Prodi.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                   student.JenisKelamin.Contains(query, StringComparison.OrdinalIgnoreCase);
        };
    }

    private void LstMahasiswa_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (FindVisualParent<Button>(e.OriginalSource as DependencyObject) is not null)
        {
            return;
        }

        var clickedItem = ItemsControl.ContainerFromElement(
            lstMahasiswa,
            e.OriginalSource as DependencyObject) as ListBoxItem;

        if (clickedItem?.IsSelected != true)
        {
            return;
        }

        e.Handled = true;
        ResetForm();
    }

    private void LstMahasiswa_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (lstMahasiswa.SelectedItem is not Mahasiswa selected)
        {
            return;
        }

        txtNim.Text = selected.Nim;
        txtNama.Text = selected.Nama;
        cmbProdi.SelectedIndex = cmbProdi.Items
            .Cast<ComboBoxItem>()
            .ToList()
            .FindIndex(item => string.Equals(item.Content?.ToString(), selected.Prodi, StringComparison.Ordinal));
        rbLaki.IsChecked = selected.JenisKelamin == "Laki-laki";
        rbPerempuan.IsChecked = selected.JenisKelamin == "Perempuan";
    }

    private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T parent)
            {
                return parent;
            }

            child = VisualTreeHelper.GetParent(child);
        }

        return null;
    }

    private bool TryReadForm(out string nim, out string nama, out string prodi, out string jenisKelamin)
    {
        nim = txtNim.Text.Trim();
        nama = txtNama.Text.Trim();
        prodi = (cmbProdi.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
        jenisKelamin = rbLaki.IsChecked == true ? "Laki-laki" :
            rbPerempuan.IsChecked == true ? "Perempuan" : string.Empty;

        if (string.IsNullOrWhiteSpace(nim))
        {
            MessageBox.Show("NIM wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNim.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(nama))
        {
            MessageBox.Show("Nama mahasiswa wajib diisi.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNama.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(prodi))
        {
            MessageBox.Show("Pilih program studi terlebih dahulu.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
            cmbProdi.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(jenisKelamin))
        {
            MessageBox.Show("Pilih jenis kelamin.", "Validasi", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private void ResetForm()
    {
        lstMahasiswa.SelectedItem = null;
        txtNim.Clear();
        txtNama.Clear();
        cmbProdi.SelectedIndex = -1;
        rbLaki.IsChecked = false;
        rbPerempuan.IsChecked = false;
        txtNim.Focus();
    }

    private void UpdateTotal() => txtTotal.Text = _mahasiswa.Count.ToString();
}
