<?php

use App\Models\UserModel;
use CodeIgniter\Test\CIUnitTestCase;
use CodeIgniter\Test\DatabaseTestTrait;

final class UserModelTest extends CIUnitTestCase
{
    use DatabaseTestTrait;

    public function testStoresPasswordHash(): void
    {
        $users = new UserModel();
        $hash = password_hash('test-password', PASSWORD_DEFAULT);
        $id = $users->insert(['email' => 'admin@example.com', 'password_hash' => $hash]);
        $user = $users->find($id);
        $this->assertTrue(password_verify('test-password', $user['password_hash']));
        $this->assertFalse(password_verify('wrong-password', $user['password_hash']));
    }
}
