<?php

use CodeIgniter\HTTP\Files\UploadedFile;
use CodeIgniter\HTTP\IncomingRequest;
use CodeIgniter\Test\CIUnitTestCase;
use Config\Services;

final class MahasiswaUploadValidationTest extends CIUnitTestCase
{
    public function testFileRulesReadRequestEvenWhenValidationDataContainsOnlyPost(): void
    {
        $path = tempnam(WRITEPATH, 'webp');
        file_put_contents($path, base64_decode('UklGRiIAAABXRUJQVlA4IBYAAAAwAQCdASoBAAEADsD+JaQAA3AAAAAA'));
        try {
            $file = new UploadedFile($path, 'foto.webp', 'image/webp', filesize($path), UPLOAD_ERR_OK);
            $request = $this->createMock(IncomingRequest::class);
            $request->method('getFileMultiple')->willReturn(null);
            $request->method('getFile')->with('foto')->willReturn($file);
            Services::injectMock('request', $request);
            $validation = Services::validation(null, false);
            $rules = ['foto' => 'uploaded[foto]|max_size[foto,2048]|is_image[foto]|mime_in[foto,image/jpeg,image/png,image/webp]'];
            $this->assertTrue($validation->setRules($rules)->run(['nama' => 'Nama']), json_encode($validation->getErrors()));

            $file = new UploadedFile($path, 'foto.webp', 'image/webp', filesize($path), UPLOAD_ERR_INI_SIZE);
            $request = $this->createMock(IncomingRequest::class);
            $request->method('getFileMultiple')->willReturn(null);
            $request->method('getFile')->willReturn($file);
            Services::injectMock('request', $request);
            $validation = Services::validation(null, false);
            $this->assertFalse($validation->setRules($rules)->run(['nama' => 'Nama']));
            $this->assertArrayHasKey('foto', $validation->getErrors());
        } finally {
            unlink($path);
            Services::reset();
        }
    }
}
