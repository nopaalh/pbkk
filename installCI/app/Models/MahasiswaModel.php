<?php

namespace App\Models;

use CodeIgniter\Model;

class MahasiswaModel extends Model
{
    protected $table = 'mahasiswa';
    protected $primaryKey = 'id';
    protected $allowedFields = ['nim', 'nama', 'email', 'jurusan', 'jenis_kelamin', 'foto'];
    protected $useTimestamps = true;
}
