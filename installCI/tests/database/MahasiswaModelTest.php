<?php

use App\Models\MahasiswaModel;
use CodeIgniter\Test\CIUnitTestCase;
use CodeIgniter\Test\DatabaseTestTrait;

final class MahasiswaModelTest extends CIUnitTestCase
{
    use DatabaseTestTrait;

    protected $namespace = 'App';
    protected $migrate = true;
    protected $refresh = true;

    public function testCrud(): void
    {
        $model = new MahasiswaModel();
        $id = $model->insert(['nim' => '12345', 'nama' => 'Nama', 'email' => 'a@example.com', 'jurusan' => 'Informatika', 'jenis_kelamin' => 'Perempuan']);
        $this->assertNotFalse($id);
        $this->assertSame('Nama', $model->find($id)['nama']);
                $this->assertSame('Informatika', $model->find($id)['jurusan']);
                $this->assertSame('Perempuan', $model->find($id)['jenis_kelamin']);
        $this->assertTrue($model->update($id, ['nama' => 'Baru']));
        $this->assertSame('Baru', $model->find($id)['nama']);
        $this->assertTrue($model->delete($id));
        $this->assertNull($model->find($id));
    }
}
