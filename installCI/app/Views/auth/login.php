<!doctype html>
<html lang="id">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Login</title>
</head>
<body>
    <main>
        <h1>Login</h1>
        <?php if (session()->getFlashdata('error')): ?>
            <p role="alert"><?= esc(session()->getFlashdata('error')) ?></p>
        <?php endif ?>
        <form action="<?= site_url('login') ?>" method="post">
            <?= csrf_field() ?>
            <label for="email">Email</label>
            <input id="email" name="email" type="email" value="<?= esc(session()->getFlashdata('email') ?? '', 'attr') ?>" autocomplete="username" required>
            <label for="password">Password</label>
            <input id="password" name="password" type="password" autocomplete="current-password" required>
            <button type="submit">Login</button>
            <button type="button" id="fill-demo">Isi akun demo</button>
            <p>Akun ini untuk simulasi; siapa pun dapat mengakses dashboard.</p>
        </form>
    </main>
    <script>
        document.getElementById('fill-demo').addEventListener('click', function () {
            document.getElementById('email').value = <?= json_encode(\Config\DemoAdmin::EMAIL, JSON_HEX_TAG | JSON_HEX_AMP | JSON_HEX_APOS | JSON_HEX_QUOT) ?>;
            document.getElementById('password').value = <?= json_encode(\Config\DemoAdmin::PASSWORD, JSON_HEX_TAG | JSON_HEX_AMP | JSON_HEX_APOS | JSON_HEX_QUOT) ?>;
        });
    </script>
</body>
</html>
