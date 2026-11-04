<?php

use App\Libraries\MahasiswaPhoto;
use CodeIgniter\Test\CIUnitTestCase;

final class MahasiswaPhotoTest extends CIUnitTestCase
{
    public function testRejectsUnsafePathsAndSymlinks(): void
    {
        $dir = WRITEPATH . 'photo-test-' . bin2hex(random_bytes(8));
        mkdir($dir);
        $name = str_repeat('a', 32) . '.jpg';
        file_put_contents($dir . '/' . $name, 'test');
        $photos = new MahasiswaPhoto($dir);
        try {
            $this->assertSame(realpath($dir . '/' . $name), $photos->path($name));
            foreach (['../' . $name, '/' . $name, 'foo.jpg', str_repeat('a', 32) . '.php', null] as $unsafe) {
                $this->assertNull($photos->path($unsafe));
                $photos->delete($unsafe);
            }
            $this->assertFileExists($dir . '/' . $name);
            $webp = str_repeat('c', 32) . '.webp';
            file_put_contents($dir . '/' . $webp, 'test');
            $this->assertSame(realpath($dir . '/' . $webp), $photos->path($webp));
            $photos->delete($webp);
            $this->assertFileDoesNotExist($dir . '/' . $webp);
            $link = str_repeat('b', 32) . '.jpg';
            symlink($dir . '/' . $name, $dir . '/' . $link);
            $this->assertNull($photos->path($link));
            unlink($dir . '/' . $link);
            $photos->delete($name);
            $this->assertFileDoesNotExist($dir . '/' . $name);
        } finally {
            if (is_file($dir . '/' . $name)) {
                unlink($dir . '/' . $name);
            }
            rmdir($dir);
        }
    }
}
