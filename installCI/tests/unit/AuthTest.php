<?php

use App\Filters\AuthFilter;
use CodeIgniter\HTTP\RedirectResponse;
use CodeIgniter\Test\CIUnitTestCase;

final class AuthTest extends CIUnitTestCase
{
    protected function tearDown(): void
    {
        session()->remove('user_id');
        parent::tearDown();
    }

    public function testFilterRedirectsGuests(): void
    {
        session()->remove('user_id');
        $response = (new AuthFilter())->before(service('request'));
        $this->assertInstanceOf(RedirectResponse::class, $response);
        $this->assertStringEndsWith('/login', $response->getHeaderLine('Location'));
    }

    public function testFilterAllowsAuthenticatedUsers(): void
    {
        session()->set('user_id', 1);
        $this->assertNull((new AuthFilter())->before(service('request')));
    }

    public function testLoginFormContainsCsrfField(): void
    {
        $html = view('auth/login');
        $this->assertStringContainsString(csrf_token(), $html);
        $this->assertStringContainsString('type="password"', $html);
    }
}
