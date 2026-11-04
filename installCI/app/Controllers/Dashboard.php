<?php

namespace App\Controllers;

use App\Models\MahasiswaModel;

class Dashboard extends BaseController
{
    public function index()
    {
        $rows = (new MahasiswaModel())->orderBy('id', 'DESC')->findAll();
        $jurusan = array_unique(array_filter(array_column($rows, 'jurusan')));

        return view('dashboard/index', [
            'mahasiswa' => array_slice($rows, 0, 5),
            'total' => count($rows),
            'jurusan' => count($jurusan),
            'lakiLaki' => count(array_filter($rows, static fn ($row) => $row['jenis_kelamin'] === 'Laki-laki')),
            'perempuan' => count(array_filter($rows, static fn ($row) => $row['jenis_kelamin'] === 'Perempuan')),
        ]);
    }

    public function profile()
    {
        return view('dashboard/profile');
    }

    public function about()
    {
        return view('dashboard/about');
    }
}
