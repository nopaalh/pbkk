<?php

namespace Tests\Unit;

use App\Libraries\ShopStore;
use CodeIgniter\Test\CIUnitTestCase;

final class ShopStoreTest extends CIUnitTestCase
{
    public function testDemoCustomerIsCreatedWithHashedPassword(): void
    {
        $store = new ShopStore(':memory:');
        $query = $store->db->prepare('SELECT * FROM customers WHERE email = ?');
        $query->execute([\Config\DemoCustomer::EMAIL]);
        $customer = $query->fetch();
        self::assertNotFalse($customer);
        self::assertTrue(password_verify(\Config\DemoCustomer::PASSWORD, $customer['password_hash']));
        self::assertNotSame(\Config\DemoCustomer::PASSWORD, $customer['password_hash']);
    }

    public function testCheckoutUpdatesStockAndRecordsInvoice(): void
    {
        $store = new ShopStore(':memory:');
        $result = $store->checkout([1 => 2, 2 => 1], ['recipient' => 'Ayu', 'whatsapp' => '08123456789', 'address' => 'Jalan Mawar 1'], null);
        self::assertMatchesRegularExpression('/^INV-\d{8}-[A-F0-9]{5}$/', $result['invoice']);
        self::assertSame(28, $store->product(1)['stock']);
        $order = $store->order($result['invoice']);
        self::assertSame(69000, $order['total']);
        self::assertCount(2, $order['items']);
    }

    public function testInsufficientStockLeavesInventoryAndOrdersUnchanged(): void
    {
        $store = new ShopStore(':memory:');
        try {
            $store->checkout([1 => 31], ['recipient' => 'Ayu', 'whatsapp' => '08123456789', 'address' => 'Jalan Mawar 1'], null);
            self::fail('Checkout must reject insufficient stock.');
        } catch (\RuntimeException $error) {
            self::assertStringContainsString('Stok', $error->getMessage());
        }
        self::assertSame(30, $store->product(1)['stock']);
        self::assertSame(0, (int) $store->db->query('SELECT COUNT(*) FROM orders')->fetchColumn());
    }
}
