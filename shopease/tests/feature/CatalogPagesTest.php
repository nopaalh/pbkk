<?php

declare(strict_types=1);

use CodeIgniter\Config\Factories;
use CodeIgniter\Exceptions\PageNotFoundException;
use CodeIgniter\Test\CIUnitTestCase;
use CodeIgniter\Test\FeatureTestTrait;
use Config\DemoCatalog;

final class CatalogPagesTest extends CIUnitTestCase
{
    use FeatureTestTrait;

    public function testDeclaredPagesAndUnknownUrl(): void
    {
        foreach ([
            '/' => 'Temukan barang yang pas untukmu.',
            '/katalog' => 'Katalog',
            '/about' => 'Tentang ShopEase',
            '/contact' => 'Kontak',
        ] as $path => $heading) {
            $page = $this->get($path);
            $page->assertOK();
            $page->assertSee($heading, 'h1');
        }

        $this->expectException(PageNotFoundException::class);
        $this->expectExceptionCode(404);
        $this->get('/halaman-tidak-terdaftar');
    }

    public function testAutoRoutingIsDisabled(): void
    {
        $this->expectException(PageNotFoundException::class);
        $this->expectExceptionCode(404);
        $this->get('/HomeController/index');
    }

    public function testCategoryLinksMatchCatalogSections(): void
    {
        $home = $this->parse($this->get('/')->getBody());
        $catalog = $this->parse($this->get('/katalog')->getBody());

        foreach (['elektronik', 'fashion', 'aksesori'] as $slug) {
            $this->assertGreaterThan(
                0,
                $home->query('//a[contains(@href, "/katalog#' . $slug . '")]')->length,
                'Beranda harus menautkan ke /katalog#' . $slug
            );
            $this->assertSame(
                1,
                $catalog->query('//section[@id="' . $slug . '"]')->length,
                'Katalog harus memiliki section#' . $slug
            );
        }
    }

    public function testFeaturedProductsMatchCatalogAndFollowRankings(): void
    {
        $home = $this->parse($this->get('/')->getBody());
        $catalog = $this->parse($this->get('/katalog')->getBody());

        $latest = $this->readCards($home, '//section[@aria-labelledby="latest-title"]');
        $bestsellers = $this->readCards($home, '//section[@aria-labelledby="bestsellers-title"]');
        $all = $this->readCards($catalog, '//main');

        $this->assertSame(['headphone-studio', 'tote-kanvas', 'jam-tempo'], array_keys($latest));
        $this->assertSame(['tote-kanvas', 'speaker-mini', 'kacamata-senja'], array_keys($bestsellers));
        $this->assertCount(7, $all);
        foreach ($latest + $bestsellers as $id => $label) {
            $this->assertSame($label, $all[$id]);
        }
    }

    public function testEqualRankUsesNameAndMissingFileGetsLocalPlaceholder(): void
    {
        $catalog = new DemoCatalog();
        $product = $catalog->products[0];
        $catalog->products = [
            ['id' => 'z', 'name' => 'Zeta', 'image' => 'tidak-ada.png'] + $product,
            ['id' => 'a', 'name' => 'Alfa', 'image' => null] + $product,
        ];
        Factories::injectMock('config', DemoCatalog::class, $catalog);

        $response = $this->get('/');
        $page = $this->parse($response->getBody());
        $this->assertSame(['a', 'z'], array_keys($this->readCards($page, '//section[@aria-labelledby="latest-title"]')));
        $this->assertSame(['a', 'z'], array_keys($this->readCards($page, '//section[@aria-labelledby="bestsellers-title"]')));
        $this->assertSame(4, $page->query('//article//img[contains(@src, "/placeholder.svg")]')->length);
    }

    public function testEmptyGroupsAndCategoriesKeepNavigation(): void
    {
        $catalog = new DemoCatalog();
        $catalog->products = [];
        Factories::injectMock('config', DemoCatalog::class, $catalog);

        $home = $this->get('/');
        $home->assertSee('Belum ada produk terbaru.');
        $home->assertSee('Belum ada produk terlaris.');
        $this->get('/katalog')->assertSee('Belum ada produk dalam kategori ini.');

        $catalog->categories = [];
        $this->get('/')->assertSee('Belum ada kategori yang tersedia.');
        $emptyCatalog = $this->get('/katalog');
        $emptyCatalog->assertSee('Belum ada kategori di katalog.');
        $emptyCatalog->assertSeeLink('Beranda');
    }

    private function parse(string $html): DOMXPath
    {
        $document = new DOMDocument();
        $document->loadHTML($html, LIBXML_NOERROR | LIBXML_NOWARNING | LIBXML_NONET);

        return new DOMXPath($document);
    }

    private function readCards(DOMXPath $page, string $scope): array
    {
        $cards = [];
        foreach ($page->query($scope . '//article[@data-product]') as $article) {
            $cards[$article->getAttribute('data-product')] = [
                $page->evaluate('normalize-space(.//h3)', $article),
                $page->evaluate('normalize-space(.//p[@class="product-price"])', $article),
            ];
        }

        return $cards;
    }
}
