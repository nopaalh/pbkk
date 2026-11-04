<?= $this->extend('layouts/admin') ?>

<?= $this->section('content') ?>
<nav aria-label="breadcrumb">
    <ol class="breadcrumb"><li class="breadcrumb-item"><a href="<?= base_url('dashboard') ?>">Dashboard</a></li><li class="breadcrumb-item active" aria-current="page">Overview</li></ol>
</nav>
<h1 class="sr-only">Dashboard</h1>
<p class="small text-muted">Selamat datang di dashboard admin. Semua data di bawah ini adalah contoh statis.</p>

<div class="row">
    <div class="col-xl-3 col-md-6 mb-3"><div class="card stat-card bg-primary h-100"><div class="card-body"><i class="fas fa-comments stat-icon" aria-hidden="true"></i><strong>26 Pesan Baru</strong></div><div class="card-footer">Contoh pesan</div></div></div>
    <div class="col-xl-3 col-md-6 mb-3"><div class="card stat-card bg-warning h-100"><div class="card-body"><i class="fas fa-list stat-icon" aria-hidden="true"></i><strong>11 Tugas Baru</strong></div><div class="card-footer">Contoh tugas</div></div></div>
    <div class="col-xl-3 col-md-6 mb-3"><div class="card stat-card bg-success h-100"><div class="card-body"><i class="fas fa-shopping-cart stat-icon" aria-hidden="true"></i><strong>123 Pesanan Baru</strong></div><div class="card-footer">Contoh pesanan</div></div></div>
    <div class="col-xl-3 col-md-6 mb-3"><div class="card stat-card bg-danger h-100"><div class="card-body"><i class="fas fa-life-ring stat-icon" aria-hidden="true"></i><strong>13 Tiket Baru</strong></div><div class="card-footer">Contoh tiket</div></div></div>
</div>

<div class="card mb-3">
    <div class="card-header"><h2 class="h6 mb-0"><i class="fas fa-chart-area mr-1" aria-hidden="true"></i> Contoh Grafik Area</h2></div>
    <div class="card-body"><div class="dashboard-chart"><canvas id="myAreaChart" role="img" aria-label="Grafik contoh data tanggal 1 sampai 13 Maret"></canvas></div></div>
    <div class="card-footer small text-muted">Data contoh, bukan data langsung.</div>
</div>

<div class="card mb-3">
    <div class="card-header"><h2 class="h6 mb-0"><i class="fas fa-table mr-1" aria-hidden="true"></i> Contoh Daftar Tugas</h2></div>
    <div class="card-body"><div class="table-responsive">
        <table class="table table-bordered">
            <thead><tr><th scope="col">No.</th><th scope="col">Tugas</th><th scope="col">Status</th></tr></thead>
            <tbody>
                <tr><td>1</td><td>Instalasi CodeIgniter</td><td>Selesai</td></tr>
                <tr><td>2</td><td>Integrasi dashboard</td><td>Dalam proses</td></tr>
                <tr><td>3</td><td>Dokumentasi</td><td>Menunggu</td></tr>
            </tbody>
        </table>
    </div></div>
</div>
<?= $this->endSection() ?>

<?= $this->section('scripts') ?>
<script src="<?= base_url('sb-admin-2/vendor/chart.js/Chart.min.js') ?>"></script>
<script>
new Chart(document.getElementById('myAreaChart'), {
    type: 'line',
    data: {
        labels: ['Mar 1', 'Mar 2', 'Mar 3', 'Mar 4', 'Mar 5', 'Mar 6', 'Mar 7', 'Mar 8', 'Mar 9', 'Mar 10', 'Mar 11', 'Mar 12', 'Mar 13'],
        datasets: [{ label: 'Contoh data', data: [10000, 30000, 26000, 18000, 18000, 29000, 31000, 33000, 26000, 24000, 32000, 31000, 38000], backgroundColor: 'rgba(0, 123, 255, .2)', borderColor: '#007bff', borderWidth: 2, pointRadius: 3, lineTension: .3 }]
    },
    options: { maintainAspectRatio: false, legend: { display: false }, scales: { yAxes: [{ ticks: { beginAtZero: true } }] } }
});
</script>
<?= $this->endSection() ?>
