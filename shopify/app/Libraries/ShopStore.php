<?php
namespace App\Libraries;

use PDO;
use RuntimeException;

class ShopStore
{
    public PDO $db;
    public function __construct(?string $path = null)
    {
        $path ??= WRITEPATH . 'shop.db';
        $dsn = $path === ':memory:' ? 'sqlite::memory:' : (getenv('SHOP_DSN') ?: 'sqlite:' . $path);
        $this->db = new PDO($dsn, getenv('SHOP_DB_USER') ?: null, getenv('SHOP_DB_PASSWORD') ?: null, [PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION, PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC]);
        if ($this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'sqlite') {
        $this->db->exec('PRAGMA foreign_keys=ON; PRAGMA busy_timeout=5000');
        $this->db->exec("CREATE TABLE IF NOT EXISTS products (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, description TEXT NOT NULL, category TEXT NOT NULL, price INTEGER NOT NULL, stock INTEGER NOT NULL, image TEXT, sprite INTEGER DEFAULT 0, color TEXT DEFAULT '#e8a540', active INTEGER NOT NULL DEFAULT 1); CREATE TABLE IF NOT EXISTS customers (id INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, email TEXT NOT NULL UNIQUE, password_hash TEXT NOT NULL); CREATE TABLE IF NOT EXISTS orders (id INTEGER PRIMARY KEY AUTOINCREMENT, invoice TEXT NOT NULL UNIQUE, customer_id INTEGER, recipient TEXT NOT NULL, whatsapp TEXT NOT NULL, address TEXT NOT NULL, total INTEGER NOT NULL, status TEXT NOT NULL DEFAULT 'Pending', created_at TEXT NOT NULL, access_token TEXT NOT NULL, FOREIGN KEY(customer_id) REFERENCES customers(id)); CREATE TABLE IF NOT EXISTS order_items (id INTEGER PRIMARY KEY AUTOINCREMENT, order_id INTEGER NOT NULL, product_id INTEGER, product_name TEXT NOT NULL, price INTEGER NOT NULL, quantity INTEGER NOT NULL, FOREIGN KEY(order_id) REFERENCES orders(id), FOREIGN KEY(product_id) REFERENCES products(id));");
        }
        if ((int) $this->db->query('SELECT COUNT(*) FROM products')->fetchColumn() === 0) {
            $stmt = $this->db->prepare('INSERT INTO products (name,description,category,price,stock,sprite,color) VALUES (?,?,?,?,?,?,?)');
            foreach ([['Raspberry ripple','Kesegaran raspberry dalam setiap gigitan.','Icepops',22000,30,0,'#ef7774'],['Coconut cloud','Gelato kelapa yang lembut dan creamy.','Gelato',25000,25,1,'#e6ded2'],['Passion fruit','Rasa markisa yang segar dan cerah.','Icepops',24000,25,2,'#e8a540'],['Golden mango','Manisnya mangga untuk hari yang cerah.','Icepops',23000,20,3,'#f2bb35'],['Brownie bliss','Cokelat dan brownie dalam satu sajian.','Gelato',28000,20,4,'#c49b89'],['Cookies & cream','Perpaduan klasik dengan potongan kukis.','Gelato',28000,20,5,'#c5b7a8']] as $row) $stmt->execute($row);
        }
    }
    public function product(int $id): ?array { $s=$this->db->prepare('SELECT * FROM products WHERE id=? AND active=1'); $s->execute([$id]); return $s->fetch() ?: null; }
    public function cart(array $cart): array { $items=[]; foreach ($cart as $id=>$qty) { $p=$this->product((int)$id); if ($p && (int)$qty>0) { $p['quantity']=(int)$qty; $items[]=$p; } } return $items; }
    public function checkout(array $cart, array $data, ?int $customer): array
    {
        if (!$cart) throw new RuntimeException('Keranjang masih kosong.');
        if ($this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'sqlite') $this->db->exec('BEGIN IMMEDIATE'); else $this->db->beginTransaction();
        try {
            $items=$this->cart($cart); if(count($items)!==count($cart)) throw new RuntimeException('Produk tidak lagi tersedia. Periksa keranjang Anda.');
            $total=0;
            foreach($items as $item) { if($item['quantity'] > $item['stock']) throw new RuntimeException('Stok '.$item['name'].' tidak mencukupi.'); $total += $item['quantity'] * $item['price']; }
            $invoice='INV-'.date('Ymd').'-'.strtoupper(bin2hex(random_bytes(3))); $invoice=substr($invoice,0,-1);
            $token=bin2hex(random_bytes(24));
            $s=$this->db->prepare('INSERT INTO orders (invoice,customer_id,recipient,whatsapp,address,total,created_at,access_token) VALUES (?,?,?,?,?,?,?,?)');
            $s->execute([$invoice,$customer,$data['recipient'],$data['whatsapp'],$data['address'],$total,date('Y-m-d H:i:s'),$token]); $id=(int)$this->db->lastInsertId();
            $line=$this->db->prepare('INSERT INTO order_items (order_id,product_id,product_name,price,quantity) VALUES (?,?,?,?,?)');
            $stock=$this->db->prepare('UPDATE products SET stock=stock-? WHERE id=? AND stock>=? AND active=1');
            foreach($items as $item) { $stock->execute([$item['quantity'],$item['id'],$item['quantity']]); if($stock->rowCount()!==1) throw new RuntimeException('Stok berubah. Silakan periksa kembali keranjang.'); $line->execute([$id,$item['id'],$item['name'],$item['price'],$item['quantity']]); }
            if ($this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'sqlite') $this->db->exec('COMMIT'); else $this->db->commit(); return ['invoice'=>$invoice,'token'=>$token];
        } catch (\Throwable $e) { if ($this->db->getAttribute(PDO::ATTR_DRIVER_NAME) === 'sqlite') $this->db->exec('ROLLBACK'); else $this->db->rollBack(); throw $e; }
    }
    public function order(string $invoice): ?array { $s=$this->db->prepare('SELECT * FROM orders WHERE invoice=?'); $s->execute([$invoice]); $o=$s->fetch(); if(!$o)return null; $s=$this->db->prepare('SELECT * FROM order_items WHERE order_id=?'); $s->execute([$o['id']]); $o['items']=$s->fetchAll(); return $o; }
}
