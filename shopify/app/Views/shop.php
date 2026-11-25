<?php
$e=static fn($v)=>esc((string)$v); $url=static fn($v='')=>site_url($v); $rp=static fn($v)=>'Rp '.number_format((int)$v,0,',','.');
$art=static function($p) use($e){ if(!empty($p['image'])) return '<span class="shop-art uploaded" style="background-image:url(\''.base_url('uploads/products/'.$p['image']).'\')"></span>'; return '<span class="shop-art" style="--sprite:'.(int)$p['sprite'].'"></span>'; };
$total=static fn($items)=>array_sum(array_map(static fn($i)=>(int)$i['price']*(int)$i['quantity'],$items));
?>
<!doctype html><html lang="id"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Shopify — <?= $e(ucfirst(str_replace('_',' ',$page))) ?></title><link rel="preconnect" href="https://fonts.googleapis.com"><link rel="preconnect" href="https://fonts.gstatic.com" crossorigin><link href="https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;700;900&family=Playfair+Display:wght@400;500&display=swap" rel="stylesheet"><link rel="stylesheet" href="<?= base_url('assets/showcase/style.css') ?>"><link rel="stylesheet" href="<?= base_url('assets/showcase/shop.css') ?>"></head><body>
<header class="shop-header">
    <a class="logo" href="<?= $url('/') ?>">shopify<span>.</span></a>
    <nav aria-label="Navigasi utama">
        <a href="<?= $url('/') ?>">Katalog</a>
        <a class="nav-icon cart-link" href="<?= $url('cart') ?>" aria-label="Keranjang, <?= (int)$cartCount ?> barang" title="Keranjang"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="9" cy="20" r="1"/><circle cx="19" cy="20" r="1"/><path d="M2.5 3.5h2l2.2 10.7a2 2 0 0 0 2 1.6h9.7a2 2 0 0 0 2-1.6L22 7H5.3"/></svg><span aria-hidden="true"><?= (int)$cartCount ?></span></a>
        <details class="account-menu">
            <summary class="nav-icon" aria-label="Menu akun" title="Akun"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="8" r="4"/><path d="M4.5 21a7.5 7.5 0 0 1 15 0"/></svg></summary>
            <div class="account-dropdown">
                <?php if(session()->get('shop_customer_id')): ?>
                    <a href="<?= $url('profile') ?>">Profil</a>
                    <a href="<?= $url('orders') ?>">Pesanan saya</a>
                    <form action="<?= $url('account/logout') ?>" method="post"><?= csrf_field() ?><button>Keluar</button></form>
                <?php else: ?>
                    <a href="<?= $url('account') ?>">Masuk atau daftar</a>
                <?php endif ?>
                <?php if(session()->get('role')==='admin'): ?>
                    <a href="<?= $url('admin/shop') ?>">Admin toko</a>
                    <a href="<?= $url('admin/shop/invoices') ?>">Invoice toko</a>
                    <form action="<?= $url('admin/logout') ?>" method="post"><?= csrf_field() ?><button>Keluar admin</button></form>
                <?php else: ?>
                    <a href="<?= $url('login') ?>">Masuk admin</a>
                <?php endif ?>
            </div>
        </details>
    </nav>
</header>
<?php if(session()->getFlashdata('error')): ?><div class="notice error"><?= $e(session()->getFlashdata('error')) ?></div><?php endif ?><?php if(session()->getFlashdata('success')): ?><div class="notice"><?= $e(session()->getFlashdata('success')) ?></div><?php endif ?>
<?php if($page==='catalog'): ?>
<main><section class="intro catalog-intro"><div class="background-word" aria-hidden="true">ENJOY IT</div><h1><span>Discover our</span><strong>products</strong></h1><p>Temukan rasa favorit untuk setiap momen.</p></section><section class="promo"><span>SEDIKIT KEBAHAGIAAN</span><strong>Hari yang lebih manis<br>dimulai di sini.</strong><span>Jelajahi koleksi es krim kami ↓</span></section><section class="catalog-wrap"><form class="catalog-filter" method="get" action="<?= $url('/') ?>"><div class="filter-links"><a class="<?= $category===''?'active':'' ?>" href="<?= $url('/') ?>">Semua</a><a class="<?= $category==='Icepops'?'active':'' ?>" href="<?= $url('/') ?>?category=Icepops">Icepops</a><a class="<?= $category==='Gelato'?'active':'' ?>" href="<?= $url('/') ?>?category=Gelato">Gelato</a></div><div><input type="hidden" name="category" value="<?= $e($category) ?>"><input type="search" name="q" placeholder="Cari rasa favorit..." value="<?= $e($q) ?>"><button class="small-button">Cari</button></div></form><div class="catalog-grid"><?php foreach($products as $p): ?><a class="catalog-product" href="<?= $url('product/'.$p['id']) ?>"><div class="catalog-art"><?= $art($p) ?><span class="catalog-flavor" style="background:<?= $e($p['color']) ?>"><b><?= $e($p['name']) ?></b><small><?= $e($p['description']) ?></small></span></div><div class="product-meta"><strong><?= $e($p['name']) ?></strong><span><?= $rp($p['price']) ?></span><small><?= $p['stock']>0?'Stok '.$e($p['stock']):'Stok habis' ?></small></div></a><?php endforeach ?></div><?php if(!$products): ?><p class="empty">Produk tidak ditemukan. Coba kata kunci atau kategori lain.</p><?php endif ?></section></main>
<?php elseif($page==='product'): ?><main class="content"><?= view('components/back_link', ['path' => '/', 'label' => 'Kembali ke katalog']) ?><div class="detail"><div class="detail-image"><?= $art($product) ?></div><div><span class="eyebrow"><?= $e($product['category']) ?></span><h1><?= $e($product['name']) ?></h1><p class="price"><?= $rp($product['price']) ?></p><p><?= $e($product['description']) ?></p><p>Stok tersedia: <b><?= (int)$product['stock'] ?></b></p><?php if($product['stock']>0): ?><form class="detail-purchase" method="post" action="<?= $url('cart/add/'.$product['id']) ?>">
    <?= csrf_field() ?>
    <label class="detail-quantity">
        Jumlah
        <input type="number" name="quantity" min="1" max="<?= (int)$product['stock'] ?>" value="1" required>
    </label>
    <button class="button detail-add" type="submit">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="9" cy="20" r="1"/><circle cx="19" cy="20" r="1"/><path d="M2.5 3.5h2l2.2 10.7a2 2 0 0 0 2 1.6h9.7a2 2 0 0 0 2-1.6L22 7H5.3"/></svg>
        Tambah ke keranjang
    </button>
</form><?php else: ?><p class="notice">Maaf, produk ini sedang habis.</p><?php endif ?></div></div></main>
<?php elseif($page==='cart'): ?><main class="content narrow"><?= view('components/back_link', ['path' => '/', 'label' => 'Kembali ke katalog']) ?><h1>Keranjang belanja</h1><?php if(!$items): ?><p>Keranjang Anda masih kosong.</p><a class="button inline" href="<?= $url('/') ?>">Jelajahi produk</a><?php else: ?><div class="list"><?php foreach($items as $item): ?><div class="cart-row"><div><a href="<?= $url('product/'.$item['id']) ?>"><b><?= $e($item['name']) ?></b></a><p><?= $rp($item['price']) ?> · Stok <?= (int)$item['stock'] ?></p></div><form method="post" action="<?= $url('cart/update') ?>"><?= csrf_field() ?><input type="hidden" name="id" value="<?= (int)$item['id'] ?>"><input aria-label="Jumlah <?= $e($item['name']) ?>" type="number" name="quantity" value="<?= (int)$item['quantity'] ?>" min="1" max="<?= max(1,(int)$item['stock']) ?>"><button name="action" value="update">Ubah</button><button name="action" value="remove" formnovalidate>Hapus</button></form><b><?= $rp($item['price']*$item['quantity']) ?></b></div><?php endforeach ?></div><div class="cart-total"><b>Total</b><b><?= $rp($total($items)) ?></b></div><div class="actions"><a class="button inline" href="<?= $url('checkout') ?>">Lanjut checkout</a><form method="post" action="<?= $url('cart/update') ?>"><?= csrf_field() ?><button class="text-button" name="action" value="clear">Kosongkan keranjang</button></form></div><?php endif ?></main>
<?php elseif($page==='checkout'): ?><main class="content narrow"><?= view('components/back_link', ['path' => 'cart', 'label' => 'Kembali ke keranjang']) ?><h1>Checkout</h1><p>Total pesanan <b><?= $rp($total($items)) ?></b></p><form class="stack-form" method="post" action="<?= $url('checkout') ?>"><?= csrf_field() ?><label>Nama penerima<input name="recipient" value="<?= $e(old('recipient')) ?>" maxlength="100" required></label><label>Nomor WhatsApp<input name="whatsapp" type="tel" value="<?= $e(old('whatsapp')) ?>" placeholder="0812..." required></label><label>Alamat lengkap<textarea name="address" maxlength="500" required><?= $e(old('address')) ?></textarea></label><button class="button">Buat pesanan</button></form></main>
<?php elseif($page==='invoice'): ?><main class="content narrow invoice">
    <?php if (session()->get('role') === 'admin'): ?>
        <?= view('components/back_link', ['path' => 'admin/shop/invoices', 'label' => 'Kembali ke invoice toko']) ?>
    <?php elseif ($order['customer_id'] && (int) $order['customer_id'] === (int) session()->get('shop_customer_id')): ?>
        <?= view('components/back_link', ['path' => 'orders', 'label' => 'Kembali ke pesanan saya']) ?>
    <?php else: ?>
        <?= view('components/back_link', ['path' => '/', 'label' => 'Kembali ke katalog']) ?>
    <?php endif ?>
<div class="invoice-head"><div><span class="eyebrow">Invoice</span><h1><?= $e($order['invoice']) ?></h1></div><button class="button print-button" onclick="window.print()">Cetak invoice</button></div><p><?= $e($order['created_at']) ?> · <?= $e($order['status']) ?></p><h3>Penerima</h3><p><?= $e($order['recipient']) ?><br><?= $e($order['whatsapp']) ?><br><?= nl2br($e($order['address'])) ?></p><div class="list"><?php foreach($order['items'] as $item): ?><div class="cart-row"><span><?= $e($item['product_name']) ?> × <?= (int)$item['quantity'] ?></span><span><?= $rp($item['price']*$item['quantity']) ?></span></div><?php endforeach ?></div><div class="cart-total"><b>Total</b><b><?= $rp($order['total']) ?></b></div><p>Simpan tautan halaman ini untuk melihat invoice kembali.</p></main>
<?php elseif($page==='account'): ?><main class="content"><?= view('components/back_link', ['path' => '/', 'label' => 'Kembali ke katalog']) ?><h1>Akun pembeli</h1><div class="columns"><form class="stack-form" method="post" action="<?= $url('account/login') ?>"><?= csrf_field() ?><h2>Masuk</h2><p class="demo-account">Akun demo pembeli<br><strong><?= $e(\Config\DemoCustomer::EMAIL) ?></strong><br>Kata sandi: <strong><?= $e(\Config\DemoCustomer::PASSWORD) ?></strong></p><label>Email<input type="email" name="email" required></label><label>Kata sandi<input type="password" name="password" required></label><button class="button">Masuk</button><button type="button" class="demo-fill" data-demo-email="<?= $e(\Config\DemoCustomer::EMAIL) ?>" data-demo-password="<?= $e(\Config\DemoCustomer::PASSWORD) ?>">Isi akun demo</button></form><form class="stack-form" method="post" action="<?= $url('account/register') ?>"><?= csrf_field() ?><h2>Buat akun</h2><label>Nama<input name="name" required maxlength="100"></label><label>Email<input type="email" name="email" required></label><label>Kata sandi<input type="password" name="password" minlength="8" required></label><button class="button">Daftar</button></form></div></main>
<?php elseif($page==='profile'): ?><main class="content narrow"><?= view('components/back_link', ['path' => '/', 'label' => 'Kembali ke katalog']) ?><h1>Profil saya</h1><p><strong>Nama</strong><br><?= $e($customer['name']) ?></p><p><strong>Email</strong><br><?= $e($customer['email']) ?></p><a href="<?= $url('orders') ?>">Lihat pesanan saya</a></main>
<?php elseif ($page === 'orders'): ?>
<main class="content narrow">
    <?= view('components/back_link', ['path' => '/', 'label' => 'Kembali ke katalog']) ?>
    <h1>Pesanan saya</h1>
    <?php if (!$orders): ?>
        <p>Belum ada pesanan.</p>
    <?php else: ?>
        <div class="list">
            <?php foreach ($orders as $order): ?>
                <a class="cart-row order-row" href="<?= esc($url('invoice/'.$order['invoice']), 'attr') ?>">
                    <b><?= $e($order['invoice']) ?></b>
                    <span class="order-status"><?= $e($order['status']) ?></span>
                    <span><?= $rp($order['total']) ?></span>
                    <span class="order-detail">
                        <span class="order-detail-icon" role="img" aria-label="Lihat detail"></span>
                    </span>
                </a>
            <?php endforeach ?>
        </div>
    <?php endif ?>
</main>
<?php elseif($page==='admin'): ?><main class="content"><?= view('components/back_link', ['path' => '/', 'label' => 'Kembali ke katalog']) ?><div class="heading-row"><div><span class="eyebrow">Panel toko</span><h1>Dashboard</h1></div><div class="actions">
        <a href="<?= $url('admin/shop/invoices') ?>">Kelola invoice</a>
        <a class="button inline" href="<?= $url('admin/shop/products/new') ?>">Tambah produk</a>
    </div></div><div class="metrics"><div><span>Omzet pesanan selesai</span><b><?= $rp($revenue) ?></b></div><div><span>Pesanan masuk</span><b><?= $orderCount ?></b></div><div><span>Produk aktif</span><b><?= count($products) ?></b></div></div><h2>Produk</h2><div class="table-scroll"><table><tr><th>Produk</th><th>Kategori</th><th>Harga</th><th>Stok</th><th>Aksi</th></tr><?php foreach($products as $p): ?><tr><td><?= $e($p['name']) ?></td><td><?= $e($p['category']) ?></td><td><?= $rp($p['price']) ?></td><td><?= (int)$p['stock'] ?></td><td><a href="<?= $url('admin/shop/products/'.$p['id'].'/edit') ?>">Edit</a> <form class="inline-form" method="post" action="<?= $url('admin/shop/products/'.$p['id'].'/delete') ?>" onsubmit="return confirm('Hapus produk dari katalog?')"><?= csrf_field() ?><button class="text-button">Hapus</button></form></td></tr><?php endforeach ?></table></div></main>
<?php elseif ($page === 'admin_invoices'): ?>
<main class="content">
    <?= view('components/back_link', ['path' => 'admin/shop', 'label' => 'Kembali ke dashboard admin']) ?>
    <h1>Invoice toko</h1>
    <?php if (!$orders): ?>
        <p>Belum ada pesanan masuk.</p>
    <?php else: ?>
        <div class="table-scroll">
            <table>
                <thead><tr><th scope="col">Invoice</th><th scope="col">Penerima</th><th scope="col">Total</th><th scope="col">Status</th></tr></thead>
                <tbody>
                <?php foreach ($orders as $order): ?>
                    <tr>
                        <td><a href="<?= $url('invoice/'.$order['invoice']) ?>"><?= $e($order['invoice']) ?></a></td>
                        <td><?= $e($order['recipient']) ?></td>
                        <td><?= $rp($order['total']) ?></td>
                        <td>
                            <form method="post" action="<?= $url('admin/shop/orders/'.$order['id'].'/status') ?>">
                                <?= csrf_field() ?>
                                <select name="status" aria-label="Status <?= $e($order['invoice']) ?>">
                                    <?php foreach (\App\Libraries\ShopStore::ORDER_STATUSES as $status): ?>
                                        <option <?= $order['status'] === $status ? 'selected' : '' ?>><?= $e($status) ?></option>
                                    <?php endforeach ?>
                                </select>
                                <button class="button button-compact">Simpan</button>
                            </form>
                        </td>
                    </tr>
                <?php endforeach ?>
                </tbody>
            </table>
        </div>
    <?php endif ?>
</main>
<?php elseif($page==='product_form'): ?><main class="content narrow"><?= view('components/back_link', ['path' => 'admin/shop', 'label' => 'Kembali ke dashboard admin']) ?><h1><?= $product?'Edit':'Tambah' ?> produk</h1><form class="stack-form" method="post" enctype="multipart/form-data" action="<?= $url('admin/shop/products'.($product?'/'.$product['id']:'')) ?>"><?= csrf_field() ?><label>Nama<input name="name" value="<?= $e(old('name',$product['name']??'')) ?>" maxlength="100" required></label><label>Deskripsi<textarea name="description" maxlength="1000" required><?= $e(old('description',$product['description']??'')) ?></textarea></label><label>Kategori<select name="category"><?php foreach(['Icepops','Gelato'] as $c): ?><option <?= old('category',$product['category']??'')===$c?'selected':'' ?>><?= $c ?></option><?php endforeach ?></select></label><label>Harga (Rupiah)<input type="number" name="price" min="0" value="<?= $e(old('price',$product['price']??0)) ?>" required></label><label>Stok<input type="number" name="stock" min="0" value="<?= $e(old('stock',$product['stock']??0)) ?>" required></label><label>Foto produk (JPEG, PNG, WebP; maksimal 3 MB)<input type="file" name="image" accept="image/jpeg,image/png,image/webp"></label><button class="button">Simpan produk</button></form></main><?php endif ?>
<footer class="shop-footer"><a class="logo" href="<?= $url('/') ?>">shopify<span>.</span></a><span>Good days taste better together.</span></footer><script src="<?= base_url('assets/showcase/demo-fill.js') ?>" defer></script></body></html>
