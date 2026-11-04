<?php

namespace App\Controllers;

use App\Libraries\MahasiswaPhoto;
use App\Models\MahasiswaModel;
use CodeIgniter\Exceptions\PageNotFoundException;
use Throwable;

class Mahasiswa extends BaseController
{
    protected $helpers = ['form', 'url'];

    public function index()
    {
        return view('mahasiswa/index', ['mahasiswa' => (new MahasiswaModel())->orderBy('id', 'DESC')->findAll()]);
    }

    public function create()
    {
        return view('mahasiswa/form', ['mahasiswa' => null]);
    }

    public function edit($id)
    {
        return view('mahasiswa/form', ['mahasiswa' => $this->find($id)]);
    }

    public function store()
    {
        return $this->save();
    }

    public function update($id)
    {
        return $this->save($this->find($id));
    }

    private function save(?array $current = null)
    {
        $model = new MahasiswaModel();
        $rules = [
            'nim' => 'required|max_length[30]|alpha_numeric|is_unique[mahasiswa.nim,id,' . ($current['id'] ?? '0') . ']',
            'nama' => 'required|max_length[100]',
            'email' => 'required|valid_email|max_length[254]',
            'jurusan' => 'required|max_length[100]',
                        'jenis_kelamin' => 'required|in_list[Laki-laki,Perempuan]',
        ];
        $file = $this->request->getFile('foto');
        if ($file && $file->getError() !== UPLOAD_ERR_NO_FILE) {
            $rules['foto'] = 'uploaded[foto]|max_size[foto,2048]|is_image[foto]|mime_in[foto,image/jpeg,image/png,image/webp]';
        }
        if (! $this->validateData($this->request->getPost(), $rules)) {
            return redirect()->back()->withInput();
        }
        $data = [];
        foreach (['nim', 'nama', 'email', 'jurusan', 'jenis_kelamin'] as $field) {
            $data[$field] = $this->request->getPost($field);
        }
        $photos = new MahasiswaPhoto();
        $newPhoto = null;
        try {
            if ($file && $file->getError() !== UPLOAD_ERR_NO_FILE) {
                $newPhoto = $photos->store($file);
                $data['foto'] = $newPhoto;
            }
            $success = $current ? $model->update($current['id'], $data) : $model->insert($data);
            if ($success === false) {
                throw new \RuntimeException('Data mahasiswa tidak dapat disimpan.');
            }
        } catch (Throwable $e) {
            $photos->delete($newPhoto);
            log_message('error', 'Gagal menyimpan mahasiswa: {error}', ['error' => $e->getMessage()]);
            return redirect()->back()->withInput()->with('error', 'Data tidak dapat disimpan. Silakan coba lagi.');
        }
        if ($newPhoto !== null && $current) {
            $photos->delete($current['foto']);
        }

        return redirect()->to(site_url('mahasiswa'))->with('success', 'Data mahasiswa berhasil disimpan.');
    }

    public function delete($id)
    {
        $current = $this->find($id);
        if (! (new MahasiswaModel())->delete($id)) {
            return redirect()->to(site_url('mahasiswa'))->with('error', 'Data tidak dapat dihapus.');
        }
        (new MahasiswaPhoto())->delete($current['foto']);

        return redirect()->to(site_url('mahasiswa'))->with('success', 'Data mahasiswa berhasil dihapus.');
    }

    public function photo($id)
    {
        $current = $this->find($id);
        $path = (new MahasiswaPhoto())->path($current['foto']);
        if ($path === null || ! in_array(mime_content_type($path), ['image/jpeg', 'image/png', 'image/webp'], true)) {
            throw PageNotFoundException::forPageNotFound();
        }

        return $this->response->setHeader('X-Content-Type-Options', 'nosniff')
            ->setContentType(mime_content_type($path))->setBody(file_get_contents($path));
    }

    private function find($id): array
    {
        $row = (new MahasiswaModel())->find($id);
        if (! $row) {
            throw PageNotFoundException::forPageNotFound();
        }

        return $row;
    }
}
