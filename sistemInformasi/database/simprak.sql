-- ============================================================================
-- SIMPRAK (Sistem Informasi Monitoring dan Penilaian Praktikum)
-- Skema Database SQLite, Indeks, Trigger, View Analitik, dan Data Contoh
-- ============================================================================

PRAGMA foreign_keys = ON;

DROP VIEW IF EXISTS v_dashboard;
DROP VIEW IF EXISTS v_rekap_kelompok;
DROP VIEW IF EXISTS v_praktikan_berisiko;
DROP VIEW IF EXISTS v_rekap_praktikan;
DROP VIEW IF EXISTS v_modul_status;
DROP VIEW IF EXISTS v_status_pengumpulan;

DROP TRIGGER IF EXISTS trg_pengumpulan_validate_update;
DROP TRIGGER IF EXISTS trg_pengumpulan_validate_insert;
DROP TRIGGER IF EXISTS trg_modul_validate_dates_update;
DROP TRIGGER IF EXISTS trg_modul_validate_dates_insert;
DROP TRIGGER IF EXISTS trg_praktikan_prevent_delete;
DROP TRIGGER IF EXISTS trg_kelompok_validate_asisten_update;
DROP TRIGGER IF EXISTS trg_kelompok_validate_asisten_insert;

DROP TABLE IF EXISTS log_aktivitas;
DROP TABLE IF EXISTS pengumpulan;
DROP TABLE IF EXISTS modul;
DROP TABLE IF EXISTS praktikan;
DROP TABLE IF EXISTS kelompok;
DROP TABLE IF EXISTS pengguna;
DROP TABLE IF EXISTS pengaturan;

-- ============================================================================
-- 1. TABEL UTAMA (7 TABEL)
-- ============================================================================

CREATE TABLE pengguna (
    id_pengguna INTEGER PRIMARY KEY AUTOINCREMENT,
    username TEXT NOT NULL UNIQUE CHECK(length(trim(username)) >= 3),
    nama_lengkap TEXT NOT NULL CHECK(length(trim(nama_lengkap)) >= 2),
    password_hash TEXT NOT NULL,
    peran TEXT NOT NULL CHECK(peran IN ('Admin', 'Asisten')),
    tema TEXT NOT NULL DEFAULT 'Terang' CHECK(tema IN ('Terang', 'Gelap')),
    status_aktif INTEGER NOT NULL DEFAULT 1 CHECK(status_aktif IN (0, 1)),
    login_terakhir TEXT NULL
);

CREATE TABLE kelompok (
    id_kelompok INTEGER PRIMARY KEY AUTOINCREMENT,
    nomor_kelompok INTEGER NOT NULL UNIQUE CHECK(nomor_kelompok > 0),
    nama_kelompok TEXT NOT NULL UNIQUE CHECK(length(trim(nama_kelompok)) >= 3),
    id_asisten INTEGER NULL UNIQUE,
    FOREIGN KEY (id_asisten) REFERENCES pengguna(id_pengguna) ON DELETE SET NULL
);

CREATE TABLE praktikan (
    id_praktikan INTEGER PRIMARY KEY AUTOINCREMENT,
    nrp TEXT NOT NULL UNIQUE CHECK(length(nrp) = 10 AND nrp GLOB '[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
    nama_lengkap TEXT NOT NULL CHECK(length(trim(nama_lengkap)) >= 2),
    angkatan INTEGER NOT NULL CHECK(angkatan BETWEEN 2018 AND 2030),
    mata_kuliah TEXT NOT NULL CHECK(length(trim(mata_kuliah)) >= 2),
    id_kelompok INTEGER NOT NULL,
    FOREIGN KEY (id_kelompok) REFERENCES kelompok(id_kelompok) ON DELETE RESTRICT
);

CREATE TABLE modul (
    id_modul INTEGER PRIMARY KEY AUTOINCREMENT,
    kode_modul TEXT NOT NULL UNIQUE CHECK(length(trim(kode_modul)) >= 2),
    judul_modul TEXT NOT NULL CHECK(length(trim(judul_modul)) >= 3),
    kategori TEXT NOT NULL CHECK(kategori IN ('Modul Mingguan', 'Tugas Pendahuluan', 'Responsi')),
    tanggal_mulai TEXT NOT NULL,
    tenggat TEXT NOT NULL,
    bobot INTEGER NOT NULL CHECK(bobot BETWEEN 1 AND 100),
    CHECK(tenggat >= tanggal_mulai)
);

CREATE TABLE pengumpulan (
    id_pengumpulan INTEGER PRIMARY KEY AUTOINCREMENT,
    id_praktikan INTEGER NOT NULL,
    id_modul INTEGER NOT NULL,
    waktu_kumpul TEXT NULL,
    tautan_tugas TEXT NULL CHECK(
        tautan_tugas IS NULL OR 
        tautan_tugas = '' OR 
        tautan_tugas LIKE 'http://%' OR 
        tautan_tugas LIKE 'https://%'
    ),
    nilai REAL NULL CHECK(nilai IS NULL OR (nilai >= 0 AND nilai <= 100)),
    catatan TEXT NULL,
    id_penilai INTEGER NULL,
    waktu_penilaian TEXT NULL,
    UNIQUE(id_praktikan, id_modul),
    FOREIGN KEY (id_praktikan) REFERENCES praktikan(id_praktikan) ON DELETE RESTRICT,
    FOREIGN KEY (id_modul) REFERENCES modul(id_modul) ON DELETE CASCADE,
    FOREIGN KEY (id_penilai) REFERENCES pengguna(id_pengguna) ON DELETE SET NULL
);

CREATE TABLE log_aktivitas (
    id_log INTEGER PRIMARY KEY AUTOINCREMENT,
    id_pengguna INTEGER NULL,
    aksi TEXT NOT NULL,
    entitas TEXT NOT NULL,
    id_entitas INTEGER NULL,
    keterangan TEXT NOT NULL,
    waktu TEXT NOT NULL,
    FOREIGN KEY (id_pengguna) REFERENCES pengguna(id_pengguna) ON DELETE SET NULL
);

CREATE TABLE pengaturan (
    kunci TEXT PRIMARY KEY,
    nilai TEXT NOT NULL
);

-- ============================================================================
-- 2. INDEKS (9 INDEKS)
-- ============================================================================

CREATE INDEX idx_pengguna_peran ON pengguna(peran, status_aktif);
CREATE INDEX idx_kelompok_asisten ON kelompok(id_asisten);
CREATE INDEX idx_praktikan_kelompok ON praktikan(id_kelompok);
CREATE INDEX idx_praktikan_matkul ON praktikan(mata_kuliah);
CREATE INDEX idx_praktikan_angkatan ON praktikan(angkatan);
CREATE INDEX idx_modul_tenggat ON modul(tanggal_mulai, tenggat);
CREATE INDEX idx_pengumpulan_praktikan ON pengumpulan(id_praktikan);
CREATE INDEX idx_pengumpulan_modul ON pengumpulan(id_modul);
CREATE INDEX idx_log_waktu ON log_aktivitas(waktu DESC);

-- ============================================================================
-- 3. TRIGGER VALIDASI & INTEGRITAS (7 TRIGGER)
-- ============================================================================

-- Trigger 1: Pastikan asisten kelompok pada INSERT adalah Asisten aktif
CREATE TRIGGER trg_kelompok_validate_asisten_insert
BEFORE INSERT ON kelompok
WHEN NEW.id_asisten IS NOT NULL
BEGIN
    SELECT RAISE(ABORT, 'Pembimbing kelompok harus pengguna aktif dengan peran Asisten.')
    WHERE NOT EXISTS (
        SELECT 1 FROM pengguna 
        WHERE id_pengguna = NEW.id_asisten AND peran = 'Asisten' AND status_aktif = 1
    );
END;

-- Trigger 2: Pastikan asisten kelompok pada UPDATE adalah Asisten aktif
CREATE TRIGGER trg_kelompok_validate_asisten_update
BEFORE UPDATE OF id_asisten ON kelompok
WHEN NEW.id_asisten IS NOT NULL
BEGIN
    SELECT RAISE(ABORT, 'Pembimbing kelompok harus pengguna aktif dengan peran Asisten.')
    WHERE NOT EXISTS (
        SELECT 1 FROM pengguna 
        WHERE id_pengguna = NEW.id_asisten AND peran = 'Asisten' AND status_aktif = 1
    );
END;

-- Trigger 3: Tolak penghapusan praktikan yang sudah memiliki riwayat pengumpulan (BR-02)
CREATE TRIGGER trg_praktikan_prevent_delete
BEFORE DELETE ON praktikan
BEGIN
    SELECT RAISE(ABORT, 'Praktikan yang sudah memiliki riwayat pengumpulan tidak dapat dihapus.')
    WHERE EXISTS (
        SELECT 1 FROM pengumpulan WHERE id_praktikan = OLD.id_praktikan
    );
END;

-- Trigger 4: Validasi tanggal modul pada INSERT
CREATE TRIGGER trg_modul_validate_dates_insert
BEFORE INSERT ON modul
BEGIN
    SELECT RAISE(ABORT, 'Tenggat modul tidak boleh lebih awal dari tanggal mulai.')
    WHERE NEW.tenggat < NEW.tanggal_mulai;
END;

-- Trigger 5: Validasi tanggal modul pada UPDATE
CREATE TRIGGER trg_modul_validate_dates_update
BEFORE UPDATE ON modul
BEGIN
    SELECT RAISE(ABORT, 'Tenggat modul tidak boleh lebih awal dari tanggal mulai.')
    WHERE NEW.tenggat < NEW.tanggal_mulai;
END;

-- Trigger 6: Validasi penilaian & penilai pada INSERT pengumpulan
CREATE TRIGGER trg_pengumpulan_validate_insert
BEFORE INSERT ON pengumpulan
BEGIN
    SELECT RAISE(ABORT, 'Nilai tidak dapat diisi sebelum waktu pengumpulan tercatat.')
    WHERE NEW.nilai IS NOT NULL AND (NEW.waktu_kumpul IS NULL OR trim(NEW.waktu_kumpul) = '');

    SELECT RAISE(ABORT, 'Tautan tugas harus menggunakan protokol HTTP atau HTTPS.')
    WHERE NEW.tautan_tugas IS NOT NULL AND trim(NEW.tautan_tugas) != ''
      AND NEW.tautan_tugas NOT LIKE 'http://%' AND NEW.tautan_tugas NOT LIKE 'https://%';

    SELECT RAISE(ABORT, 'Penilai harus Admin aktif atau Asisten aktif untuk kelompok praktikan tersebut.')
    WHERE NEW.id_penilai IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM pengguna u
        LEFT JOIN kelompok k ON k.id_asisten = u.id_pengguna
        JOIN praktikan p ON p.id_praktikan = NEW.id_praktikan
        WHERE u.id_pengguna = NEW.id_penilai
          AND u.status_aktif = 1
          AND (u.peran = 'Admin' OR (u.peran = 'Asisten' AND k.id_kelompok = p.id_kelompok))
    );
END;

-- Trigger 7: Validasi penilaian & penilai pada UPDATE pengumpulan
CREATE TRIGGER trg_pengumpulan_validate_update
BEFORE UPDATE ON pengumpulan
BEGIN
    SELECT RAISE(ABORT, 'Nilai tidak dapat diisi sebelum waktu pengumpulan tercatat.')
    WHERE NEW.nilai IS NOT NULL AND (NEW.waktu_kumpul IS NULL OR trim(NEW.waktu_kumpul) = '');

    SELECT RAISE(ABORT, 'Tautan tugas harus menggunakan protokol HTTP atau HTTPS.')
    WHERE NEW.tautan_tugas IS NOT NULL AND trim(NEW.tautan_tugas) != ''
      AND NEW.tautan_tugas NOT LIKE 'http://%' AND NEW.tautan_tugas NOT LIKE 'https://%';

    SELECT RAISE(ABORT, 'Penilai harus Admin aktif atau Asisten aktif untuk kelompok praktikan tersebut.')
    WHERE NEW.id_penilai IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM pengguna u
        LEFT JOIN kelompok k ON k.id_asisten = u.id_pengguna
        JOIN praktikan p ON p.id_praktikan = NEW.id_praktikan
        WHERE u.id_pengguna = NEW.id_penilai
          AND u.status_aktif = 1
          AND (u.peran = 'Admin' OR (u.peran = 'Asisten' AND k.id_kelompok = p.id_kelompok))
    );
END;

-- ============================================================================
-- 4. ENAM VIEW ANALITIK (6 VIEW)
-- ============================================================================

-- View 1: v_status_pengumpulan (BR-01)
CREATE VIEW v_status_pengumpulan AS
SELECT 
    p.id_praktikan,
    p.nrp,
    p.nama_lengkap,
    p.id_kelompok,
    m.id_modul,
    m.kode_modul,
    m.judul_modul,
    m.kategori,
    m.tanggal_mulai,
    m.tenggat,
    m.bobot,
    g.id_pengumpulan,
    g.waktu_kumpul,
    g.tautan_tugas,
    g.nilai,
    g.catatan,
    g.id_penilai,
    u.nama_lengkap AS nama_penilai,
    g.waktu_penilaian,
    CASE 
        WHEN g.waktu_kumpul IS NULL OR trim(g.waktu_kumpul) = '' THEN 'Belum Mengumpulkan'
        WHEN g.waktu_kumpul <= m.tenggat THEN 'Tepat Waktu'
        ELSE 'Terlambat'
    END AS status_pengumpulan,
    CASE
        WHEN m.tenggat < (SELECT nilai FROM pengaturan WHERE kunci = 'waktu_acuan') THEN 1
        ELSE 0
    END AS sudah_jatuh_tempo
FROM praktikan p
CROSS JOIN modul m
LEFT JOIN pengumpulan g ON g.id_praktikan = p.id_praktikan AND g.id_modul = m.id_modul
LEFT JOIN pengguna u ON u.id_pengguna = g.id_penilai;

-- View 2: v_modul_status
CREATE VIEW v_modul_status AS
SELECT
    m.id_modul,
    m.kode_modul,
    m.judul_modul,
    m.kategori,
    m.tanggal_mulai,
    m.tenggat,
    m.bobot,
    CASE
        WHEN (SELECT nilai FROM pengaturan WHERE kunci = 'waktu_acuan') < m.tanggal_mulai THEN 'Belum Dimulai'
        WHEN (SELECT nilai FROM pengaturan WHERE kunci = 'waktu_acuan') <= m.tenggat THEN 'Berjalan'
        ELSE 'Selesai'
    END AS status_modul,
    (SELECT COUNT(*) FROM praktikan) AS total_praktikan,
    (SELECT COUNT(*) FROM pengumpulan g WHERE g.id_modul = m.id_modul AND g.waktu_kumpul IS NOT NULL AND trim(g.waktu_kumpul) != '') AS jumlah_terkumpul,
    (SELECT COUNT(*) FROM pengumpulan g WHERE g.id_modul = m.id_modul AND g.nilai IS NOT NULL) AS jumlah_dinilai
FROM modul m;

-- View 3: v_rekap_praktikan (BR-03 & BR-04)
CREATE VIEW v_rekap_praktikan AS
WITH base AS (
    SELECT
        p.id_praktikan,
        p.nrp,
        p.nama_lengkap,
        p.angkatan,
        p.mata_kuliah,
        p.id_kelompok,
        k.nomor_kelompok,
        k.nama_kelompok,
        SUM(CASE WHEN v.sudah_jatuh_tempo = 1 THEN 1 ELSE 0 END) AS modul_jatuh_tempo,
        SUM(CASE WHEN v.sudah_jatuh_tempo = 1 AND v.status_pengumpulan != 'Belum Mengumpulkan' THEN 1 ELSE 0 END) AS modul_jatuh_tempo_terkumpul,
        SUM(CASE WHEN v.sudah_jatuh_tempo = 1 AND v.status_pengumpulan = 'Belum Mengumpulkan' THEN 1 ELSE 0 END) AS modul_belum_kumpul,
        SUM(CASE WHEN v.status_pengumpulan = 'Terlambat' THEN 1 ELSE 0 END) AS jumlah_terlambat,
        SUM(CASE WHEN v.nilai IS NOT NULL THEN 1 ELSE 0 END) AS jumlah_modul_dinilai,
        -- Pembilang BR-04: modul yang sudah dinilai (nilai * bobot) + modul jatuh tempo belum kumpul (0 * bobot)
        SUM(
            CASE
                WHEN v.nilai IS NOT NULL THEN v.nilai * v.bobot
                WHEN v.sudah_jatuh_tempo = 1 AND v.status_pengumpulan = 'Belum Mengumpulkan' THEN 0.0
                ELSE 0.0
            END
        ) AS total_poin_berbobot,
        -- Penyebut BR-04: bobot modul yang sudah dinilai + bobot modul jatuh tempo yang belum dikumpulkan
        SUM(
            CASE
                WHEN v.nilai IS NOT NULL THEN v.bobot
                WHEN v.sudah_jatuh_tempo = 1 AND v.status_pengumpulan = 'Belum Mengumpulkan' THEN v.bobot
                ELSE 0
            END
        ) AS total_bobot_pembagi
    FROM praktikan p
    JOIN kelompok k ON k.id_kelompok = p.id_kelompok
    LEFT JOIN v_status_pengumpulan v ON v.id_praktikan = p.id_praktikan
    GROUP BY p.id_praktikan
)
SELECT
    id_praktikan,
    nrp,
    nama_lengkap,
    angkatan,
    mata_kuliah,
    id_kelompok,
    nomor_kelompok,
    nama_kelompok,
    modul_jatuh_tempo,
    modul_jatuh_tempo_terkumpul,
    modul_belum_kumpul,
    jumlah_terlambat,
    jumlah_modul_dinilai,
    CASE
        WHEN modul_jatuh_tempo = 0 THEN 100.0
        ELSE ROUND((CAST(modul_jatuh_tempo_terkumpul AS REAL) * 100.0) / CAST(modul_jatuh_tempo AS REAL), 2)
    END AS ketuntasan,
    CASE
        WHEN total_bobot_pembagi = 0 THEN 0.0
        ELSE ROUND(total_poin_berbobot / CAST(total_bobot_pembagi AS REAL), 2)
    END AS nilai_akhir
FROM base;

-- View 4: v_praktikan_berisiko (BR-05: R1, R2, R3)
CREATE VIEW v_praktikan_berisiko AS
WITH flags AS (
    SELECT
        r.*,
        u.nama_lengkap AS nama_asisten,
        CASE WHEN r.modul_belum_kumpul >= 2 THEN 1 ELSE 0 END AS r1,
        CASE WHEN r.ketuntasan < CAST((SELECT nilai FROM pengaturan WHERE kunci = 'ambang_ketuntasan_r2') AS REAL) THEN 1 ELSE 0 END AS r2,
        CASE WHEN r.jumlah_modul_dinilai >= 2 AND r.nilai_akhir < CAST((SELECT nilai FROM pengaturan WHERE kunci = 'ambang_nilai_r3') AS REAL) THEN 1 ELSE 0 END AS r3
    FROM v_rekap_praktikan r
    JOIN kelompok k ON k.id_kelompok = r.id_kelompok
    LEFT JOIN pengguna u ON u.id_pengguna = k.id_asisten
)
SELECT
    id_praktikan,
    nrp,
    nama_lengkap,
    angkatan,
    mata_kuliah,
    id_kelompok,
    nomor_kelompok,
    nama_kelompok,
    COALESCE(nama_asisten, 'Belum Ditugaskan') AS nama_asisten,
    modul_belum_kumpul,
    jumlah_terlambat,
    jumlah_modul_dinilai,
    ketuntasan,
    nilai_akhir,
    r1,
    r2,
    r3,
    TRIM(
        (CASE WHEN r1 = 1 THEN 'R1 (Belum Kumpul >= 2); ' ELSE '' END) ||
        (CASE WHEN r2 = 1 THEN 'R2 (Ketuntasan < Ambang); ' ELSE '' END) ||
        (CASE WHEN r3 = 1 THEN 'R3 (Nilai Akhir < Ambang); ' ELSE '' END),
        '; '
    ) AS alasan_risiko
FROM flags
WHERE r1 = 1 OR r2 = 1 OR r3 = 1;

-- View 5: v_rekap_kelompok
CREATE VIEW v_rekap_kelompok AS
SELECT
    k.id_kelompok,
    k.nomor_kelompok,
    k.nama_kelompok,
    k.id_asisten,
    COALESCE(u.nama_lengkap, 'Belum Ditugaskan') AS nama_asisten,
    COUNT(r.id_praktikan) AS jumlah_anggota,
    COALESCE(ROUND(AVG(r.ketuntasan), 2), 0.0) AS rata_ketuntasan,
    COALESCE(ROUND(AVG(r.nilai_akhir), 2), 0.0) AS rata_nilai_akhir,
    (SELECT COUNT(*) FROM v_praktikan_berisiko b WHERE b.id_kelompok = k.id_kelompok) AS jumlah_berisiko
FROM kelompok k
LEFT JOIN pengguna u ON u.id_pengguna = k.id_asisten
LEFT JOIN v_rekap_praktikan r ON r.id_kelompok = k.id_kelompok
GROUP BY k.id_kelompok;

-- View 6: v_dashboard
CREATE VIEW v_dashboard AS
SELECT
    (SELECT COUNT(*) FROM praktikan) AS total_praktikan,
    (SELECT COUNT(*) FROM kelompok) AS total_kelompok,
    (SELECT COUNT(*) FROM v_modul_status WHERE status_modul = 'Berjalan') AS modul_aktif,
    COALESCE((SELECT ROUND(AVG(ketuntasan), 2) FROM v_rekap_praktikan), 0.0) AS rata_ketuntasan_global,
    COALESCE((SELECT ROUND(AVG(nilai_akhir), 2) FROM v_rekap_praktikan), 0.0) AS rata_nilai_global,
    (SELECT COUNT(*) FROM v_praktikan_berisiko) AS total_praktikan_berisiko;

-- ============================================================================
-- 5. PENGATURAN AWAL & DATA CONTOH (SEED DATA)
-- ============================================================================

INSERT INTO pengaturan (kunci, nilai) VALUES
('nama_program', 'Laboratorium Informatika & Sistem Informasi — Semester Gasal 2026/2027'),
('ambang_ketuntasan_r2', '75'),
('ambang_nilai_r3', '60'),
('waktu_acuan', '2026-10-21 23:59');

INSERT INTO pengguna (id_pengguna, username, nama_lengkap, password_hash, peran, tema, status_aktif, login_terakhir) VALUES
(1, 'admin', 'Dr. Hendra Wijaya, M.Kom.', '$2b$10$NgmQYs2CpwF7ytkezH9bYO08oC8eKP/hrV/53V/4ReVln18c3E4mi', 'Admin', 'Terang', 1, '2026-10-21 20:15'),
(2, 'asisten01', 'Raka Pratama', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Terang', 1, '2026-10-21 19:11'),
(3, 'asisten02', 'Nadia Kusuma', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Terang', 1, '2026-10-21 19:12'),
(4, 'asisten03', 'Dimas Aditya', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Gelap', 1, '2026-10-21 19:13'),
(5, 'asisten04', 'Alya Rahmawati', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Terang', 1, '2026-10-21 19:14'),
(6, 'asisten05', 'Farhan Maulana', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Terang', 1, '2026-10-21 19:15'),
(7, 'asisten06', 'Siti Nurhaliza', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Gelap', 1, '2026-10-21 19:16'),
(8, 'asisten07', 'Bagas Setiawan', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Terang', 1, '2026-10-21 19:17'),
(9, 'asisten08', 'Tiara Maharani', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Terang', 1, '2026-10-21 19:18'),
(10, 'asisten09', 'Reza Pahlevi', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Gelap', 1, '2026-10-21 19:19'),
(11, 'asisten10', 'Intan Permata', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Terang', 1, '2026-10-21 19:20'),
(12, 'asisten11', 'Ilham Ramadhan', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Terang', 1, '2026-10-21 19:21'),
(13, 'asisten12', 'Larasati Putri', '$2b$10$VVRhLwflE8zWGGmmuGuYZenaFEpFY/6AXAjtoC4RXztoH8Dfv19jW', 'Asisten', 'Gelap', 1, '2026-10-21 19:22');

INSERT INTO kelompok (id_kelompok, nomor_kelompok, nama_kelompok, id_asisten) VALUES
(1, 1, 'Shift A1 - Grafika Alpha', 2),
(2, 2, 'Shift A2 - Grafika Beta', 3),
(3, 3, 'Shift A3 - Grafika Gamma', 4),
(4, 4, 'Shift B1 - Struktur Delta', 5),
(5, 5, 'Shift B2 - Struktur Epsilon', 6),
(6, 6, 'Shift B3 - Struktur Zeta', 7),
(7, 7, 'Shift C1 - Basis Data Eta', 8),
(8, 8, 'Shift C2 - Basis Data Theta', 9),
(9, 9, 'Shift C3 - Basis Data Iota', 10),
(10, 10, 'Shift D1 - Jarkom Kappa', 11),
(11, 11, 'Shift D2 - Jarkom Lambda', 12),
(12, 12, 'Shift D3 - Jarkom Sigma', 13);

INSERT INTO modul (id_modul, kode_modul, judul_modul, kategori, tanggal_mulai, tenggat, bobot) VALUES
(1, 'MOD-01', 'Pengenalan Pipeline & Primitif 2D/3D', 'Tugas Pendahuluan', '2026-09-01 08:00', '2026-09-07 23:59', 10),
(2, 'MOD-02', 'Transformasi Geometri & Matriks Proyeksi', 'Modul Mingguan', '2026-09-08 08:00', '2026-09-14 23:59', 15),
(3, 'MOD-03', 'Struktur Tree, Graph & Traversal Praktis', 'Modul Mingguan', '2026-09-15 08:00', '2026-09-21 23:59', 15),
(4, 'MOD-04', 'Perancangan Skema Relasional, View & Trigger SQL', 'Modul Mingguan', '2026-09-22 08:00', '2026-09-28 23:59', 15),
(5, 'MOD-05', 'Evaluasi Tengah Semester & Responsi 1', 'Responsi', '2026-10-01 08:00', '2026-10-10 23:59', 15),
(6, 'MOD-06', 'Lighting, Shading & Texture Mapping', 'Modul Mingguan', '2026-10-12 08:00', '2026-10-25 23:59', 10),
(7, 'MOD-07', 'Optimasi Query, Indexing & Transaksi ACID', 'Tugas Pendahuluan', '2026-10-18 08:00', '2026-10-28 23:59', 10),
(8, 'MOD-08', 'Proyek Akhir & Responsi Komprehensif', 'Responsi', '2026-11-01 08:00', '2026-11-15 23:59', 10);

INSERT INTO praktikan (id_praktikan, nrp, nama_lengkap, angkatan, mata_kuliah, id_kelompok) VALUES
(1, '5025241001', 'Bima Nugroho', 2024, 'Grafika Komputer', 1),
(2, '5025241002', 'Citra Sari', 2024, 'Grafika Komputer', 2),
(3, '5025241003', 'Dewi Santoso', 2024, 'Grafika Komputer', 3),
(4, '5025231004', 'Eka Mahendra', 2023, 'Struktur Data', 4),
(5, '5025241005', 'Fajar Purnama', 2024, 'Struktur Data', 5),
(6, '5025241006', 'Gita Handayani', 2024, 'Struktur Data', 6),
(7, '5025241007', 'Hafiz Wijaya', 2024, 'Basis Data', 7),
(8, '5025251008', 'Indra Kurniawan', 2025, 'Basis Data', 8),
(9, '5025241009', 'Jihan Utami', 2024, 'Basis Data', 9),
(10, '5025241010', 'Kevin Hakim', 2024, 'Jaringan Komputer', 10),
(11, '5025241011', 'Luthfi Firmansyah', 2024, 'Jaringan Komputer', 11),
(12, '5025231012', 'Maya Aulia', 2023, 'Jaringan Komputer', 12),
(13, '5025241013', 'Naufal Setiawan', 2024, 'Grafika Komputer', 1),
(14, '5025241014', 'Putri Gunawan', 2024, 'Grafika Komputer', 2),
(15, '5025241015', 'Qori Saputra', 2024, 'Grafika Komputer', 3),
(16, '5025251016', 'Rizky Wijaya', 2025, 'Struktur Data', 4),
(17, '5025241017', 'Ulfa Gunawan', 2024, 'Struktur Data', 5),
(18, '5025241018', 'Yusuf Nugroho', 2024, 'Struktur Data', 6),
(19, '5025241019', 'Bella Kurniawan', 2024, 'Basis Data', 7),
(20, '5025231020', 'Erlangga Saputra', 2023, 'Basis Data', 8),
(21, '5025241021', 'Hana Sari', 2024, 'Basis Data', 9),
(22, '5025241022', 'Citra Utami', 2024, 'Jaringan Komputer', 10),
(23, '5025241023', 'Fajar Wibowo', 2024, 'Jaringan Komputer', 11),
(24, '5025251024', 'Indra Santoso', 2025, 'Jaringan Komputer', 12),
(25, '5025241025', 'Luthfi Hakim', 2024, 'Grafika Komputer', 1),
(26, '5025241026', 'Putri Kusuma', 2024, 'Grafika Komputer', 2),
(27, '5025241027', 'Salma Mahendra', 2024, 'Grafika Komputer', 3),
(28, '5025231028', 'Vino Firmansyah', 2023, 'Struktur Data', 4),
(29, '5025241029', 'Zahra Lestari', 2024, 'Struktur Data', 5),
(30, '5025241030', 'Cahyo Purnama', 2024, 'Struktur Data', 6),
(31, '5025241031', 'Febri Aulia', 2024, 'Basis Data', 7),
(32, '5025251032', 'Aditya Pradana', 2025, 'Basis Data', 8),
(33, '5025241033', 'Dewi Handayani', 2024, 'Basis Data', 9),
(34, '5025241034', 'Gita Setiawan', 2024, 'Jaringan Komputer', 10),
(35, '5025241035', 'Jihan Hidayat', 2024, 'Jaringan Komputer', 11),
(36, '5025231036', 'Maya Wijaya', 2023, 'Jaringan Komputer', 12),
(37, '5025241037', 'Qori Gunawan', 2024, 'Grafika Komputer', 1),
(38, '5025241038', 'Taufik Nugroho', 2024, 'Grafika Komputer', 2),
(39, '5025241039', 'Wulan Kurniawan', 2024, 'Grafika Komputer', 3),
(40, '5025251040', 'Arif Saputra', 2025, 'Struktur Data', 4),
(41, '5025241041', 'Dinda Sari', 2024, 'Struktur Data', 5),
(42, '5025241042', 'Gilang Utami', 2024, 'Struktur Data', 6),
(43, '5025241043', 'Bima Wibowo', 2024, 'Basis Data', 7),
(44, '5025231044', 'Eka Santoso', 2023, 'Basis Data', 8),
(45, '5025241045', 'Hafiz Hakim', 2024, 'Basis Data', 9),
(46, '5025241046', 'Kevin Kusuma', 2024, 'Jaringan Komputer', 10),
(47, '5025241047', 'Naufal Mahendra', 2024, 'Jaringan Komputer', 11),
(48, '5025251048', 'Rizky Firmansyah', 2025, 'Jaringan Komputer', 12),
(49, '5025241049', 'Ulfa Lestari', 2024, 'Grafika Komputer', 1),
(50, '5025241050', 'Yusuf Purnama', 2024, 'Grafika Komputer', 2),
(51, '5025241051', 'Bella Aulia', 2024, 'Grafika Komputer', 3),
(52, '5025231052', 'Erlangga Pradana', 2023, 'Struktur Data', 4),
(53, '5025241053', 'Hana Handayani', 2024, 'Struktur Data', 5),
(54, '5025241054', 'Citra Setiawan', 2024, 'Struktur Data', 6),
(55, '5025241055', 'Fajar Hidayat', 2024, 'Basis Data', 7),
(56, '5025251056', 'Indra Wijaya', 2025, 'Basis Data', 8),
(57, '5025241057', 'Luthfi Gunawan', 2024, 'Basis Data', 9),
(58, '5025241058', 'Putri Nugroho', 2024, 'Jaringan Komputer', 10),
(59, '5025241059', 'Salma Kurniawan', 2024, 'Jaringan Komputer', 11),
(60, '5025231060', 'Vino Saputra', 2023, 'Jaringan Komputer', 12),
(61, '5025241061', 'Zahra Sari', 2024, 'Grafika Komputer', 1),
(62, '5025241062', 'Cahyo Utami', 2024, 'Grafika Komputer', 2),
(63, '5025241063', 'Febri Wibowo', 2024, 'Grafika Komputer', 3),
(64, '5025251064', 'Aditya Santoso', 2025, 'Struktur Data', 4),
(65, '5025241065', 'Dewi Hakim', 2024, 'Struktur Data', 5),
(66, '5025241066', 'Gita Kusuma', 2024, 'Struktur Data', 6),
(67, '5025241067', 'Jihan Mahendra', 2024, 'Basis Data', 7),
(68, '5025231068', 'Maya Firmansyah', 2023, 'Basis Data', 8),
(69, '5025241069', 'Qori Lestari', 2024, 'Basis Data', 9),
(70, '5025241070', 'Taufik Purnama', 2024, 'Jaringan Komputer', 10),
(71, '5025241071', 'Wulan Aulia', 2024, 'Jaringan Komputer', 11),
(72, '5025251072', 'Arif Pradana', 2025, 'Jaringan Komputer', 12),
(73, '5025241073', 'Dinda Handayani', 2024, 'Grafika Komputer', 1),
(74, '5025241074', 'Gilang Setiawan', 2024, 'Grafika Komputer', 2),
(75, '5025241075', 'Bima Hidayat', 2024, 'Grafika Komputer', 3),
(76, '5025231076', 'Eka Wijaya', 2023, 'Struktur Data', 4),
(77, '5025241077', 'Hafiz Gunawan', 2024, 'Struktur Data', 5),
(78, '5025241078', 'Kevin Nugroho', 2024, 'Struktur Data', 6),
(79, '5025241079', 'Naufal Kurniawan', 2024, 'Basis Data', 7),
(80, '5025251080', 'Rizky Saputra', 2025, 'Basis Data', 8),
(81, '5025241081', 'Ulfa Sari', 2024, 'Basis Data', 9),
(82, '5025241082', 'Yusuf Utami', 2024, 'Jaringan Komputer', 10),
(83, '5025241083', 'Bella Wibowo', 2024, 'Jaringan Komputer', 11),
(84, '5025231084', 'Erlangga Santoso', 2023, 'Jaringan Komputer', 12),
(85, '5025241085', 'Hana Hakim', 2024, 'Grafika Komputer', 1),
(86, '5025241086', 'Citra Kusuma', 2024, 'Grafika Komputer', 2),
(87, '5025241087', 'Fajar Mahendra', 2024, 'Grafika Komputer', 3),
(88, '5025251088', 'Indra Firmansyah', 2025, 'Struktur Data', 4),
(89, '5025241089', 'Luthfi Lestari', 2024, 'Struktur Data', 5),
(90, '5025241090', 'Putri Purnama', 2024, 'Struktur Data', 6),
(91, '5025241091', 'Salma Aulia', 2024, 'Basis Data', 7),
(92, '5025231092', 'Vino Pradana', 2023, 'Basis Data', 8),
(93, '5025241093', 'Zahra Handayani', 2024, 'Basis Data', 9),
(94, '5025241094', 'Cahyo Setiawan', 2024, 'Jaringan Komputer', 10),
(95, '5025241095', 'Febri Hidayat', 2024, 'Jaringan Komputer', 11),
(96, '5025251096', 'Aditya Wijaya', 2025, 'Jaringan Komputer', 12),
(97, '5025241097', 'Dewi Gunawan', 2024, 'Grafika Komputer', 1),
(98, '5025241098', 'Gita Nugroho', 2024, 'Grafika Komputer', 2),
(99, '5025241099', 'Jihan Kurniawan', 2024, 'Grafika Komputer', 3),
(100, '5025231100', 'Maya Saputra', 2023, 'Struktur Data', 4),
(101, '5025241101', 'Qori Sari', 2024, 'Struktur Data', 5),
(102, '5025241102', 'Taufik Utami', 2024, 'Struktur Data', 6),
(103, '5025241103', 'Wulan Wibowo', 2024, 'Basis Data', 7),
(104, '5025251104', 'Arif Santoso', 2025, 'Basis Data', 8),
(105, '5025241105', 'Dinda Hakim', 2024, 'Basis Data', 9),
(106, '5025241106', 'Gilang Kusuma', 2024, 'Jaringan Komputer', 10),
(107, '5025241107', 'Bima Mahendra', 2024, 'Jaringan Komputer', 11),
(108, '5025231108', 'Eka Firmansyah', 2023, 'Jaringan Komputer', 12),
(109, '5025241109', 'Hafiz Lestari', 2024, 'Grafika Komputer', 1),
(110, '5025241110', 'Kevin Purnama', 2024, 'Grafika Komputer', 2),
(111, '5025241111', 'Naufal Aulia', 2024, 'Grafika Komputer', 3),
(112, '5025251112', 'Rizky Pradana', 2025, 'Struktur Data', 4),
(113, '5025241113', 'Ulfa Handayani', 2024, 'Struktur Data', 5),
(114, '5025241114', 'Yusuf Setiawan', 2024, 'Struktur Data', 6),
(115, '5025241115', 'Bella Hidayat', 2024, 'Basis Data', 7),
(116, '5025231116', 'Erlangga Wijaya', 2023, 'Basis Data', 8),
(117, '5025241117', 'Hana Gunawan', 2024, 'Basis Data', 9),
(118, '5025241118', 'Citra Nugroho', 2024, 'Jaringan Komputer', 10),
(119, '5025241119', 'Fajar Kurniawan', 2024, 'Jaringan Komputer', 11),
(120, '5025251120', 'Indra Saputra', 2025, 'Jaringan Komputer', 12),
(121, '5025241121', 'Luthfi Sari', 2024, 'Grafika Komputer', 1),
(122, '5025241122', 'Putri Utami', 2024, 'Grafika Komputer', 2),
(123, '5025241123', 'Salma Wibowo', 2024, 'Grafika Komputer', 3),
(124, '5025231124', 'Vino Santoso', 2023, 'Struktur Data', 4);

INSERT INTO pengumpulan (id_pengumpulan, id_praktikan, id_modul, waktu_kumpul, tautan_tugas, nilai, catatan, id_penilai, waktu_penilaian) VALUES
(1, 1, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241001-mod01', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-09 14:20'),
(2, 1, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241001-mod02', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-16 14:20'),
(3, 1, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241001-mod03', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-23 14:20'),
(4, 1, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241001-mod04', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-28 14:20'),
(5, 1, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241001-mod05', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-10-12 14:20'),
(6, 2, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241002-mod01', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-09 14:20'),
(7, 2, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241002-mod02', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-16 14:20'),
(8, 2, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241002-mod03', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-23 14:20'),
(9, 2, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241002-mod04', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-28 14:20'),
(10, 2, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241002-mod05', 82.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 3, '2026-10-12 14:20'),
(11, 2, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241002-mod06', NULL, NULL, NULL, NULL),
(12, 3, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241003-mod01', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-09 14:20'),
(13, 3, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241003-mod02', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-16 14:20'),
(14, 3, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241003-mod03', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-23 14:20'),
(15, 3, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241003-mod04', 80.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 4, '2026-09-28 14:20'),
(16, 3, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241003-mod05', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-10-12 14:20'),
(17, 4, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231004-mod01', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-09 14:20'),
(18, 4, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231004-mod02', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-16 14:20'),
(19, 4, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025231004-mod03', 78.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 5, '2026-09-23 14:20'),
(20, 4, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231004-mod04', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-28 14:20'),
(21, 4, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231004-mod05', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-10-12 14:20'),
(22, 4, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231004-mod06', 84.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00'),
(23, 5, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241005-mod01', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-09 14:20'),
(24, 5, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241005-mod02', 76.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 6, '2026-09-16 14:20'),
(25, 5, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241005-mod03', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-23 14:20'),
(26, 5, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241005-mod04', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-28 14:20'),
(27, 5, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241005-mod05', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-10-12 14:20'),
(28, 6, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241006-mod01', 74.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 7, '2026-09-09 14:20'),
(29, 6, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241006-mod02', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-16 14:20'),
(30, 6, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241006-mod03', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-23 14:20'),
(31, 6, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241006-mod04', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-28 14:20'),
(32, 6, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241006-mod05', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-10-12 14:20'),
(33, 6, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241006-mod06', NULL, NULL, NULL, NULL),
(34, 7, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241007-mod01', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-09 14:20'),
(35, 7, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241007-mod02', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-16 14:20'),
(36, 7, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241007-mod03', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-23 14:20'),
(37, 7, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241007-mod04', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-28 14:20'),
(38, 7, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241007-mod05', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-10-12 14:20'),
(39, 8, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251008-mod01', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-09 14:20'),
(40, 8, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251008-mod02', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-16 14:20'),
(41, 8, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251008-mod03', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-23 14:20'),
(42, 8, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251008-mod04', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-28 14:20'),
(43, 8, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251008-mod05', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-10-12 14:20'),
(44, 8, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251008-mod06', 88.0, 'Shader dan pencahayaan sudah sesuai.', 9, '2026-10-20 16:00'),
(45, 9, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241009-mod01', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-09 14:20'),
(46, 9, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241009-mod02', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-16 14:20'),
(47, 9, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241009-mod03', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-23 14:20'),
(48, 9, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241009-mod04', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-28 14:20'),
(49, 9, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241009-mod05', 80.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 10, '2026-10-12 14:20'),
(50, 10, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241010-mod01', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-09 14:20'),
(51, 10, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241010-mod02', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-16 14:20'),
(52, 10, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241010-mod03', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-23 14:20'),
(53, 10, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241010-mod04', 78.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 11, '2026-09-28 14:20'),
(54, 10, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241010-mod05', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-10-12 14:20'),
(55, 10, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241010-mod06', NULL, NULL, NULL, NULL),
(56, 11, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241011-mod01', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-09 14:20'),
(57, 11, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241011-mod02', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-16 14:20'),
(58, 11, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241011-mod03', 76.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 12, '2026-09-23 14:20'),
(59, 12, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231012-mod01', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-09 14:20'),
(60, 12, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025231012-mod02', 74.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 13, '2026-09-16 14:20'),
(61, 12, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231012-mod03', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-23 14:20'),
(62, 12, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231012-mod04', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-28 14:20'),
(63, 12, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231012-mod05', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-10-12 14:20'),
(64, 12, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231012-mod06', 92.0, 'Shader dan pencahayaan sudah sesuai.', 13, '2026-10-20 16:00'),
(65, 13, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241013-mod01', 44.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 2, '2026-09-09 14:20'),
(66, 13, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241013-mod02', 47.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 2, '2026-09-16 14:20'),
(67, 13, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241013-mod03', 50.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 2, '2026-09-23 14:20'),
(68, 13, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241013-mod04', 53.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 2, '2026-09-28 14:20'),
(69, 13, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241013-mod05', 42.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 2, '2026-10-12 14:20'),
(70, 14, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241014-mod01', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-09 14:20'),
(71, 14, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241014-mod02', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-16 14:20'),
(72, 14, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241014-mod03', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-23 14:20'),
(73, 14, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241014-mod04', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-28 14:20'),
(74, 14, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241014-mod05', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-10-12 14:20'),
(75, 14, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241014-mod06', NULL, NULL, NULL, NULL),
(76, 15, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241015-mod01', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-09 14:20'),
(77, 15, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241015-mod02', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-16 14:20'),
(78, 15, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241015-mod03', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-23 14:20'),
(79, 15, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241015-mod04', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-28 14:20'),
(80, 15, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241015-mod05', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-10-12 14:20'),
(81, 16, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251016-mod01', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-09 14:20'),
(82, 16, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251016-mod02', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-16 14:20'),
(83, 16, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251016-mod03', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-23 14:20'),
(84, 16, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251016-mod04', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-28 14:20'),
(85, 16, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025251016-mod05', 78.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 5, '2026-10-12 14:20'),
(86, 16, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251016-mod06', 80.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00'),
(87, 17, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241017-mod01', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-09 14:20'),
(88, 17, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241017-mod02', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-16 14:20'),
(89, 17, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241017-mod03', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-23 14:20'),
(90, 17, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241017-mod04', 76.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 6, '2026-09-28 14:20'),
(91, 17, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241017-mod05', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-10-12 14:20'),
(92, 18, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241018-mod01', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-09 14:20'),
(93, 18, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241018-mod02', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-16 14:20'),
(94, 18, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241018-mod03', 74.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 7, '2026-09-23 14:20'),
(95, 18, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241018-mod04', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-28 14:20'),
(96, 18, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241018-mod05', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-10-12 14:20'),
(97, 18, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241018-mod06', NULL, NULL, NULL, NULL),
(98, 19, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241019-mod01', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-09 14:20'),
(99, 19, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241019-mod02', 95.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 8, '2026-09-16 14:20'),
(100, 19, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241019-mod03', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-23 14:20'),
(101, 19, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241019-mod04', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-28 14:20'),
(102, 19, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241019-mod05', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-10-12 14:20'),
(103, 20, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025231020-mod01', 93.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 9, '2026-09-09 14:20'),
(104, 20, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231020-mod02', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-16 14:20'),
(105, 20, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231020-mod03', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-23 14:20'),
(106, 20, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231020-mod04', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-28 14:20'),
(107, 20, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231020-mod05', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-10-12 14:20'),
(108, 20, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231020-mod06', 84.0, 'Shader dan pencahayaan sudah sesuai.', 9, '2026-10-20 16:00'),
(109, 21, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241021-mod01', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-09 14:20'),
(110, 21, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241021-mod02', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-16 14:20'),
(111, 21, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241021-mod03', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-23 14:20'),
(112, 21, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241021-mod04', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-28 14:20'),
(113, 21, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241021-mod05', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-10-12 14:20'),
(114, 22, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241022-mod01', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-09 14:20'),
(115, 22, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241022-mod02', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-16 14:20'),
(116, 22, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241022-mod06', NULL, NULL, NULL, NULL),
(117, 23, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241023-mod01', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-09 14:20'),
(118, 23, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241023-mod02', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-16 14:20'),
(119, 23, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241023-mod03', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-23 14:20'),
(120, 23, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241023-mod04', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-28 14:20'),
(121, 23, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241023-mod05', 76.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 12, '2026-10-12 14:20'),
(122, 24, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251024-mod01', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-09 14:20'),
(123, 24, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251024-mod02', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-16 14:20'),
(124, 24, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251024-mod03', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-23 14:20'),
(125, 24, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025251024-mod04', 74.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 13, '2026-09-28 14:20'),
(126, 24, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251024-mod05', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-10-12 14:20'),
(127, 24, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251024-mod06', 88.0, 'Shader dan pencahayaan sudah sesuai.', 13, '2026-10-20 16:00'),
(128, 25, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241025-mod01', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-09 14:20'),
(129, 25, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241025-mod02', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-16 14:20'),
(130, 25, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241025-mod03', 95.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 2, '2026-09-23 14:20'),
(131, 25, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241025-mod04', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-28 14:20'),
(132, 25, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241025-mod05', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-10-12 14:20'),
(133, 26, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241026-mod01', 43.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 3, '2026-09-09 14:20'),
(134, 26, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241026-mod02', 46.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 3, '2026-09-16 14:20'),
(135, 26, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241026-mod03', 49.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 3, '2026-09-23 14:20'),
(136, 26, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241026-mod04', 52.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 3, '2026-09-28 14:20'),
(137, 26, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241026-mod05', 55.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 3, '2026-10-12 14:20'),
(138, 26, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241026-mod06', NULL, NULL, NULL, NULL),
(139, 27, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241027-mod01', 91.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 4, '2026-09-09 14:20'),
(140, 27, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241027-mod02', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-16 14:20'),
(141, 27, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241027-mod03', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-23 14:20'),
(142, 27, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241027-mod04', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-28 14:20'),
(143, 27, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241027-mod05', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-10-12 14:20'),
(144, 28, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231028-mod01', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-09 14:20'),
(145, 28, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231028-mod02', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-16 14:20'),
(146, 28, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231028-mod03', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-23 14:20'),
(147, 28, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231028-mod04', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-28 14:20'),
(148, 28, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231028-mod05', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-10-12 14:20'),
(149, 28, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231028-mod06', 92.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00'),
(150, 29, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241029-mod01', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-09 14:20'),
(151, 29, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241029-mod02', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-16 14:20'),
(152, 29, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241029-mod03', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-23 14:20'),
(153, 29, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241029-mod04', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-28 14:20'),
(154, 29, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241029-mod05', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-10-12 14:20'),
(155, 30, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241030-mod01', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-09 14:20'),
(156, 30, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241030-mod02', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-16 14:20'),
(157, 30, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241030-mod03', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-23 14:20'),
(158, 30, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241030-mod04', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-28 14:20'),
(159, 30, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241030-mod05', 74.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 7, '2026-10-12 14:20'),
(160, 30, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241030-mod06', NULL, NULL, NULL, NULL),
(161, 31, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241031-mod01', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-09 14:20'),
(162, 31, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241031-mod02', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-16 14:20'),
(163, 31, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241031-mod03', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-23 14:20'),
(164, 31, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241031-mod04', 95.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 8, '2026-09-28 14:20'),
(165, 31, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241031-mod05', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-10-12 14:20'),
(166, 32, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251032-mod01', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-09 14:20'),
(167, 32, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251032-mod02', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-16 14:20'),
(168, 32, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025251032-mod03', 93.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 9, '2026-09-23 14:20'),
(169, 32, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251032-mod04', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-28 14:20'),
(170, 32, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251032-mod05', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-10-12 14:20'),
(171, 32, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251032-mod06', 80.0, 'Shader dan pencahayaan sudah sesuai.', 9, '2026-10-20 16:00'),
(172, 33, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241033-mod01', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-09 14:20'),
(173, 33, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241033-mod02', 91.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 10, '2026-09-16 14:20'),
(174, 33, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241033-mod03', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-23 14:20'),
(175, 34, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241034-mod01', 89.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 11, '2026-09-09 14:20'),
(176, 34, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241034-mod02', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-16 14:20'),
(177, 34, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241034-mod03', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-23 14:20'),
(178, 34, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241034-mod04', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-28 14:20'),
(179, 34, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241034-mod05', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-10-12 14:20'),
(180, 34, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241034-mod06', NULL, NULL, NULL, NULL),
(181, 35, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241035-mod01', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-09 14:20'),
(182, 35, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241035-mod02', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-16 14:20'),
(183, 35, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241035-mod03', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-23 14:20'),
(184, 35, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241035-mod04', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-28 14:20'),
(185, 35, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241035-mod05', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-10-12 14:20'),
(186, 36, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231036-mod01', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-09 14:20'),
(187, 36, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231036-mod02', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-16 14:20'),
(188, 36, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231036-mod03', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-23 14:20'),
(189, 36, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231036-mod04', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-28 14:20'),
(190, 36, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231036-mod05', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-10-12 14:20'),
(191, 36, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231036-mod06', 84.0, 'Shader dan pencahayaan sudah sesuai.', 13, '2026-10-20 16:00'),
(192, 37, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241037-mod01', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-09 14:20'),
(193, 37, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241037-mod02', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-16 14:20'),
(194, 37, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241037-mod03', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-23 14:20'),
(195, 37, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241037-mod04', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-28 14:20'),
(196, 37, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241037-mod05', 95.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 2, '2026-10-12 14:20'),
(197, 38, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241038-mod01', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-09 14:20'),
(198, 38, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241038-mod02', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-16 14:20'),
(199, 38, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241038-mod03', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-23 14:20'),
(200, 38, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241038-mod04', 93.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 3, '2026-09-28 14:20'),
(201, 38, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241038-mod05', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-10-12 14:20'),
(202, 38, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241038-mod06', NULL, NULL, NULL, NULL),
(203, 39, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241039-mod01', 42.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 4, '2026-09-09 14:20'),
(204, 39, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241039-mod02', 45.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 4, '2026-09-16 14:20'),
(205, 39, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241039-mod03', 48.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 4, '2026-09-23 14:20'),
(206, 39, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241039-mod04', 51.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 4, '2026-09-28 14:20'),
(207, 39, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241039-mod05', 54.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 4, '2026-10-12 14:20'),
(208, 40, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251040-mod01', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-09 14:20'),
(209, 40, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025251040-mod02', 89.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 5, '2026-09-16 14:20'),
(210, 40, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251040-mod03', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-23 14:20'),
(211, 40, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251040-mod04', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-28 14:20'),
(212, 40, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251040-mod05', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-10-12 14:20'),
(213, 40, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251040-mod06', 88.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00'),
(214, 41, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241041-mod01', 87.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 6, '2026-09-09 14:20'),
(215, 41, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241041-mod02', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-16 14:20'),
(216, 41, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241041-mod03', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-23 14:20'),
(217, 41, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241041-mod04', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-28 14:20'),
(218, 41, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241041-mod05', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-10-12 14:20'),
(219, 42, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241042-mod01', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-09 14:20'),
(220, 42, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241042-mod02', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-16 14:20'),
(221, 42, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241042-mod03', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-23 14:20'),
(222, 42, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241042-mod04', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-28 14:20'),
(223, 42, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241042-mod05', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-10-12 14:20'),
(224, 42, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241042-mod06', NULL, NULL, NULL, NULL),
(225, 43, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241043-mod01', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-09 14:20'),
(226, 43, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241043-mod02', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-16 14:20'),
(227, 43, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241043-mod03', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-23 14:20'),
(228, 43, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241043-mod04', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-28 14:20'),
(229, 43, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241043-mod05', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-10-12 14:20'),
(230, 44, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231044-mod01', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-09 14:20'),
(231, 44, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231044-mod02', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-16 14:20'),
(232, 44, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231044-mod06', 92.0, 'Shader dan pencahayaan sudah sesuai.', 9, '2026-10-20 16:00'),
(233, 45, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241045-mod01', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-09 14:20'),
(234, 45, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241045-mod02', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-16 14:20'),
(235, 45, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241045-mod03', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-23 14:20'),
(236, 45, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241045-mod04', 91.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 10, '2026-09-28 14:20'),
(237, 45, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241045-mod05', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-10-12 14:20'),
(238, 46, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241046-mod01', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-09 14:20'),
(239, 46, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241046-mod02', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-16 14:20'),
(240, 46, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241046-mod03', 89.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 11, '2026-09-23 14:20'),
(241, 46, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241046-mod04', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-28 14:20'),
(242, 46, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241046-mod05', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-10-12 14:20'),
(243, 46, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241046-mod06', NULL, NULL, NULL, NULL),
(244, 47, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241047-mod01', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-09 14:20'),
(245, 47, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241047-mod02', 87.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 12, '2026-09-16 14:20'),
(246, 47, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241047-mod03', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-23 14:20'),
(247, 47, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241047-mod04', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-28 14:20'),
(248, 47, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241047-mod05', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-10-12 14:20'),
(249, 48, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025251048-mod01', 85.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 13, '2026-09-09 14:20'),
(250, 48, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251048-mod02', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-16 14:20'),
(251, 48, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251048-mod03', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-23 14:20'),
(252, 48, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251048-mod04', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-28 14:20'),
(253, 48, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251048-mod05', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-10-12 14:20'),
(254, 48, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251048-mod06', 80.0, 'Shader dan pencahayaan sudah sesuai.', 13, '2026-10-20 16:00'),
(255, 49, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241049-mod01', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-09 14:20'),
(256, 49, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241049-mod02', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-16 14:20'),
(257, 49, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241049-mod03', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-23 14:20'),
(258, 49, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241049-mod04', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-28 14:20'),
(259, 49, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241049-mod05', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-10-12 14:20'),
(260, 50, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241050-mod01', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-09 14:20'),
(261, 50, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241050-mod02', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-16 14:20'),
(262, 50, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241050-mod03', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-23 14:20'),
(263, 50, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241050-mod04', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-28 14:20'),
(264, 50, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241050-mod05', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-10-12 14:20'),
(265, 50, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241050-mod06', NULL, NULL, NULL, NULL),
(266, 51, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241051-mod01', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-09 14:20'),
(267, 51, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241051-mod02', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-16 14:20'),
(268, 51, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241051-mod03', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-23 14:20'),
(269, 51, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241051-mod04', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-28 14:20'),
(270, 51, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241051-mod05', 91.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 4, '2026-10-12 14:20'),
(271, 52, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231052-mod01', 55.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 5, '2026-09-09 14:20'),
(272, 52, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231052-mod02', 44.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 5, '2026-09-16 14:20'),
(273, 52, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231052-mod03', 47.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 5, '2026-09-23 14:20'),
(274, 52, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025231052-mod04', 50.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 5, '2026-09-28 14:20'),
(275, 52, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231052-mod05', 53.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 5, '2026-10-12 14:20'),
(276, 52, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231052-mod06', 84.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00'),
(277, 53, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241053-mod01', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-09 14:20'),
(278, 53, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241053-mod02', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-16 14:20'),
(279, 53, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241053-mod03', 87.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 6, '2026-09-23 14:20'),
(280, 53, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241053-mod04', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-28 14:20'),
(281, 53, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241053-mod05', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-10-12 14:20'),
(282, 54, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241054-mod01', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-09 14:20'),
(283, 54, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241054-mod02', 85.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 7, '2026-09-16 14:20'),
(284, 54, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241054-mod03', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-23 14:20'),
(285, 54, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241054-mod04', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-28 14:20'),
(286, 54, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241054-mod05', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-10-12 14:20'),
(287, 54, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241054-mod06', NULL, NULL, NULL, NULL),
(288, 55, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241055-mod01', 83.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 8, '2026-09-09 14:20'),
(289, 55, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241055-mod02', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-16 14:20'),
(290, 55, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241055-mod03', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-23 14:20'),
(291, 56, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251056-mod01', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-09 14:20'),
(292, 56, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251056-mod02', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-16 14:20'),
(293, 56, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251056-mod03', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-23 14:20'),
(294, 56, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251056-mod04', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-28 14:20'),
(295, 56, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251056-mod05', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-10-12 14:20'),
(296, 56, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251056-mod06', 88.0, 'Shader dan pencahayaan sudah sesuai.', 9, '2026-10-20 16:00'),
(297, 57, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241057-mod01', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-09 14:20'),
(298, 57, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241057-mod02', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-16 14:20'),
(299, 57, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241057-mod03', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-23 14:20'),
(300, 57, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241057-mod04', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-28 14:20'),
(301, 57, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241057-mod05', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-10-12 14:20'),
(302, 58, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241058-mod01', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-09 14:20'),
(303, 58, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241058-mod02', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-16 14:20'),
(304, 58, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241058-mod03', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-23 14:20'),
(305, 58, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241058-mod04', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-28 14:20'),
(306, 58, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241058-mod05', 89.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 11, '2026-10-12 14:20'),
(307, 58, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241058-mod06', NULL, NULL, NULL, NULL),
(308, 59, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241059-mod01', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-09 14:20'),
(309, 59, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241059-mod02', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-16 14:20'),
(310, 59, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241059-mod03', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-23 14:20'),
(311, 59, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241059-mod04', 87.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 12, '2026-09-28 14:20'),
(312, 59, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241059-mod05', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-10-12 14:20'),
(313, 60, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231060-mod01', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-09 14:20'),
(314, 60, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231060-mod02', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-16 14:20'),
(315, 60, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025231060-mod03', 85.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 13, '2026-09-23 14:20'),
(316, 60, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231060-mod04', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-28 14:20'),
(317, 60, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231060-mod05', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-10-12 14:20'),
(318, 60, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231060-mod06', 92.0, 'Shader dan pencahayaan sudah sesuai.', 13, '2026-10-20 16:00'),
(319, 61, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241061-mod01', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-09 14:20'),
(320, 61, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241061-mod02', 83.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 2, '2026-09-16 14:20'),
(321, 61, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241061-mod03', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-23 14:20'),
(322, 61, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241061-mod04', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-28 14:20'),
(323, 61, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241061-mod05', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-10-12 14:20'),
(324, 62, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241062-mod01', 81.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 3, '2026-09-09 14:20'),
(325, 62, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241062-mod02', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-16 14:20'),
(326, 62, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241062-mod03', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-23 14:20'),
(327, 62, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241062-mod04', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-28 14:20'),
(328, 62, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241062-mod05', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-10-12 14:20'),
(329, 62, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241062-mod06', NULL, NULL, NULL, NULL),
(330, 63, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241063-mod01', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-09 14:20'),
(331, 63, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241063-mod02', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-16 14:20'),
(332, 63, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241063-mod03', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-23 14:20'),
(333, 63, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241063-mod04', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-28 14:20'),
(334, 63, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241063-mod05', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-10-12 14:20'),
(335, 64, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251064-mod01', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-09 14:20'),
(336, 64, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251064-mod02', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-16 14:20'),
(337, 64, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251064-mod03', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-23 14:20'),
(338, 64, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251064-mod04', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-28 14:20'),
(339, 64, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251064-mod05', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-10-12 14:20'),
(340, 64, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251064-mod06', 80.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00'),
(341, 65, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241065-mod01', 54.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 6, '2026-09-09 14:20'),
(342, 65, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241065-mod02', 43.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 6, '2026-09-16 14:20'),
(343, 65, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241065-mod03', 46.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 6, '2026-09-23 14:20'),
(344, 65, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241065-mod04', 49.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 6, '2026-09-28 14:20'),
(345, 65, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241065-mod05', 52.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 6, '2026-10-12 14:20'),
(346, 66, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241066-mod01', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-09 14:20'),
(347, 66, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241066-mod02', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-16 14:20'),
(348, 66, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241066-mod06', NULL, NULL, NULL, NULL),
(349, 67, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241067-mod01', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-09 14:20'),
(350, 67, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241067-mod02', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-16 14:20'),
(351, 67, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241067-mod03', 83.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 8, '2026-09-23 14:20'),
(352, 67, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241067-mod04', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-28 14:20'),
(353, 67, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241067-mod05', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-10-12 14:20'),
(354, 68, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231068-mod01', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-09 14:20'),
(355, 68, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025231068-mod02', 81.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 9, '2026-09-16 14:20'),
(356, 68, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231068-mod03', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-23 14:20'),
(357, 68, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231068-mod04', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-28 14:20'),
(358, 68, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231068-mod05', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-10-12 14:20'),
(359, 68, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231068-mod06', 84.0, 'Shader dan pencahayaan sudah sesuai.', 9, '2026-10-20 16:00'),
(360, 69, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241069-mod01', 79.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 10, '2026-09-09 14:20'),
(361, 69, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241069-mod02', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-16 14:20'),
(362, 69, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241069-mod03', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-23 14:20'),
(363, 69, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241069-mod04', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-28 14:20'),
(364, 69, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241069-mod05', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-10-12 14:20'),
(365, 70, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241070-mod01', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-09 14:20'),
(366, 70, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241070-mod02', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-16 14:20'),
(367, 70, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241070-mod03', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-23 14:20'),
(368, 70, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241070-mod04', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-28 14:20'),
(369, 70, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241070-mod05', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-10-12 14:20'),
(370, 70, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241070-mod06', NULL, NULL, NULL, NULL),
(371, 71, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241071-mod01', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-09 14:20'),
(372, 71, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241071-mod02', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-16 14:20'),
(373, 71, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241071-mod03', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-23 14:20'),
(374, 71, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241071-mod04', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-28 14:20'),
(375, 71, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241071-mod05', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-10-12 14:20'),
(376, 72, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251072-mod01', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-09 14:20'),
(377, 72, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251072-mod02', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-16 14:20'),
(378, 72, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251072-mod03', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-23 14:20'),
(379, 72, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251072-mod04', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-28 14:20'),
(380, 72, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025251072-mod05', 85.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 13, '2026-10-12 14:20'),
(381, 72, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251072-mod06', 88.0, 'Shader dan pencahayaan sudah sesuai.', 13, '2026-10-20 16:00'),
(382, 73, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241073-mod01', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-09 14:20'),
(383, 73, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241073-mod02', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-16 14:20'),
(384, 73, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241073-mod03', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-23 14:20'),
(385, 73, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241073-mod04', 83.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 2, '2026-09-28 14:20'),
(386, 73, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241073-mod05', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-10-12 14:20'),
(387, 74, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241074-mod01', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-09 14:20'),
(388, 74, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241074-mod02', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-16 14:20'),
(389, 74, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241074-mod03', 81.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 3, '2026-09-23 14:20'),
(390, 74, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241074-mod04', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-28 14:20'),
(391, 74, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241074-mod05', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-10-12 14:20'),
(392, 74, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241074-mod06', NULL, NULL, NULL, NULL),
(393, 75, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241075-mod01', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-09 14:20'),
(394, 75, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241075-mod02', 79.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 4, '2026-09-16 14:20'),
(395, 75, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241075-mod03', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-23 14:20'),
(396, 75, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241075-mod04', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-28 14:20'),
(397, 75, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241075-mod05', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-10-12 14:20'),
(398, 76, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025231076-mod01', 77.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 5, '2026-09-09 14:20'),
(399, 76, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231076-mod02', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-16 14:20'),
(400, 76, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231076-mod03', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-23 14:20'),
(401, 76, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231076-mod04', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-28 14:20'),
(402, 76, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231076-mod05', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-10-12 14:20'),
(403, 76, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231076-mod06', 92.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00'),
(404, 77, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241077-mod01', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-09 14:20'),
(405, 77, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241077-mod02', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-16 14:20'),
(406, 77, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241077-mod03', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-23 14:20'),
(407, 78, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241078-mod01', 53.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 7, '2026-09-09 14:20'),
(408, 78, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241078-mod02', 42.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 7, '2026-09-16 14:20'),
(409, 78, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241078-mod03', 45.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 7, '2026-09-23 14:20'),
(410, 78, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241078-mod04', 48.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 7, '2026-09-28 14:20'),
(411, 78, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241078-mod05', 51.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 7, '2026-10-12 14:20'),
(412, 78, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241078-mod06', NULL, NULL, NULL, NULL),
(413, 79, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241079-mod01', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-09 14:20'),
(414, 79, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241079-mod02', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-16 14:20'),
(415, 79, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241079-mod03', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-23 14:20'),
(416, 79, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241079-mod04', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-28 14:20'),
(417, 79, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241079-mod05', 83.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 8, '2026-10-12 14:20'),
(418, 80, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251080-mod01', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-09 14:20'),
(419, 80, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251080-mod02', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-16 14:20'),
(420, 80, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251080-mod03', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-23 14:20'),
(421, 80, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025251080-mod04', 81.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 9, '2026-09-28 14:20'),
(422, 80, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251080-mod05', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-10-12 14:20'),
(423, 80, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251080-mod06', 80.0, 'Shader dan pencahayaan sudah sesuai.', 9, '2026-10-20 16:00'),
(424, 81, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241081-mod01', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-09 14:20'),
(425, 81, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241081-mod02', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-16 14:20'),
(426, 81, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241081-mod03', 79.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 10, '2026-09-23 14:20'),
(427, 81, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241081-mod04', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-28 14:20'),
(428, 81, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241081-mod05', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-10-12 14:20'),
(429, 82, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241082-mod01', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-09 14:20'),
(430, 82, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241082-mod02', 77.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 11, '2026-09-16 14:20'),
(431, 82, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241082-mod03', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-23 14:20'),
(432, 82, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241082-mod04', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-28 14:20'),
(433, 82, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241082-mod05', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-10-12 14:20'),
(434, 82, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241082-mod06', NULL, NULL, NULL, NULL),
(435, 83, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241083-mod01', 75.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 12, '2026-09-09 14:20'),
(436, 83, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241083-mod02', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-16 14:20'),
(437, 83, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241083-mod03', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-23 14:20'),
(438, 83, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241083-mod04', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-28 14:20'),
(439, 83, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241083-mod05', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-10-12 14:20'),
(440, 84, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231084-mod01', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-09 14:20'),
(441, 84, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231084-mod02', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-16 14:20'),
(442, 84, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231084-mod03', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-23 14:20'),
(443, 84, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231084-mod04', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-28 14:20'),
(444, 84, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231084-mod05', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-10-12 14:20'),
(445, 84, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231084-mod06', 84.0, 'Shader dan pencahayaan sudah sesuai.', 13, '2026-10-20 16:00'),
(446, 85, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241085-mod01', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-09 14:20'),
(447, 85, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241085-mod02', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-16 14:20'),
(448, 85, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241085-mod03', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-23 14:20'),
(449, 85, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241085-mod04', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-28 14:20'),
(450, 85, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241085-mod05', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-10-12 14:20'),
(451, 86, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241086-mod01', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-09 14:20'),
(452, 86, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241086-mod02', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-16 14:20'),
(453, 86, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241086-mod03', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-23 14:20'),
(454, 86, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241086-mod04', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-28 14:20'),
(455, 86, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241086-mod05', 81.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 3, '2026-10-12 14:20'),
(456, 86, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241086-mod06', NULL, NULL, NULL, NULL),
(457, 87, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241087-mod01', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-09 14:20'),
(458, 87, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241087-mod02', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-16 14:20'),
(459, 87, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241087-mod03', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-23 14:20'),
(460, 87, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241087-mod04', 79.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 4, '2026-09-28 14:20'),
(461, 87, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241087-mod05', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-10-12 14:20'),
(462, 88, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251088-mod01', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-09 14:20'),
(463, 88, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251088-mod02', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-16 14:20'),
(464, 88, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251088-mod06', 88.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00'),
(465, 89, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241089-mod01', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-09 14:20'),
(466, 89, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241089-mod02', 75.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 6, '2026-09-16 14:20'),
(467, 89, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241089-mod03', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-23 14:20'),
(468, 89, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241089-mod04', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-28 14:20'),
(469, 89, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241089-mod05', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-10-12 14:20'),
(470, 90, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241090-mod01', 96.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 7, '2026-09-09 14:20'),
(471, 90, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241090-mod02', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-16 14:20'),
(472, 90, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241090-mod03', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-23 14:20'),
(473, 90, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241090-mod04', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-28 14:20'),
(474, 90, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241090-mod05', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-10-12 14:20'),
(475, 90, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241090-mod06', NULL, NULL, NULL, NULL),
(476, 91, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241091-mod01', 52.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 8, '2026-09-09 14:20'),
(477, 91, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241091-mod02', 55.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 8, '2026-09-16 14:20'),
(478, 91, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241091-mod03', 44.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 8, '2026-09-23 14:20'),
(479, 91, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241091-mod04', 47.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 8, '2026-09-28 14:20'),
(480, 91, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241091-mod05', 50.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 8, '2026-10-12 14:20'),
(481, 92, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231092-mod01', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-09 14:20'),
(482, 92, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231092-mod02', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-16 14:20'),
(483, 92, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231092-mod03', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-23 14:20'),
(484, 92, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231092-mod04', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-28 14:20'),
(485, 92, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231092-mod05', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-10-12 14:20'),
(486, 92, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231092-mod06', 92.0, 'Shader dan pencahayaan sudah sesuai.', 9, '2026-10-20 16:00'),
(487, 93, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241093-mod01', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-09 14:20'),
(488, 93, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241093-mod02', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-16 14:20'),
(489, 93, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241093-mod03', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-23 14:20'),
(490, 93, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241093-mod04', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-28 14:20'),
(491, 93, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241093-mod05', 79.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 10, '2026-10-12 14:20'),
(492, 94, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241094-mod01', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-09 14:20'),
(493, 94, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241094-mod02', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-16 14:20'),
(494, 94, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241094-mod03', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-23 14:20'),
(495, 94, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241094-mod04', 77.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 11, '2026-09-28 14:20'),
(496, 94, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241094-mod05', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-10-12 14:20'),
(497, 94, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241094-mod06', NULL, NULL, NULL, NULL),
(498, 95, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241095-mod01', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-09 14:20'),
(499, 95, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241095-mod02', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-16 14:20'),
(500, 95, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241095-mod03', 75.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 12, '2026-09-23 14:20'),
(501, 95, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241095-mod04', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-28 14:20'),
(502, 95, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241095-mod05', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-10-12 14:20'),
(503, 96, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251096-mod01', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-09 14:20'),
(504, 96, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025251096-mod02', 96.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 13, '2026-09-16 14:20'),
(505, 96, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251096-mod03', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-23 14:20'),
(506, 96, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251096-mod04', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-28 14:20'),
(507, 96, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251096-mod05', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-10-12 14:20'),
(508, 96, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251096-mod06', 80.0, 'Shader dan pencahayaan sudah sesuai.', 13, '2026-10-20 16:00'),
(509, 97, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241097-mod01', 94.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 2, '2026-09-09 14:20'),
(510, 97, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241097-mod02', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-16 14:20'),
(511, 97, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241097-mod03', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-23 14:20'),
(512, 97, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241097-mod04', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-28 14:20'),
(513, 97, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241097-mod05', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-10-12 14:20'),
(514, 98, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241098-mod01', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-09 14:20'),
(515, 98, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241098-mod02', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-16 14:20'),
(516, 98, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241098-mod03', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-23 14:20'),
(517, 98, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241098-mod04', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-28 14:20'),
(518, 98, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241098-mod05', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-10-12 14:20'),
(519, 98, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241098-mod06', NULL, NULL, NULL, NULL),
(520, 99, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241099-mod01', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-09 14:20'),
(521, 99, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241099-mod02', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-16 14:20'),
(522, 99, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241099-mod03', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-23 14:20'),
(523, 100, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231100-mod01', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-09 14:20'),
(524, 100, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231100-mod02', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-16 14:20'),
(525, 100, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231100-mod03', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-23 14:20'),
(526, 100, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231100-mod04', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-28 14:20'),
(527, 100, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025231100-mod05', 77.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 5, '2026-10-12 14:20'),
(528, 100, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231100-mod06', 84.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00'),
(529, 101, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241101-mod01', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-09 14:20'),
(530, 101, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241101-mod02', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-16 14:20'),
(531, 101, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241101-mod03', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-23 14:20'),
(532, 101, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241101-mod04', 75.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 6, '2026-09-28 14:20'),
(533, 101, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241101-mod05', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-10-12 14:20'),
(534, 102, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241102-mod01', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-09 14:20'),
(535, 102, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241102-mod02', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-16 14:20'),
(536, 102, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241102-mod03', 96.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 7, '2026-09-23 14:20'),
(537, 102, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241102-mod04', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-28 14:20'),
(538, 102, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241102-mod05', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-10-12 14:20'),
(539, 102, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241102-mod06', NULL, NULL, NULL, NULL),
(540, 103, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241103-mod01', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-09 14:20'),
(541, 103, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241103-mod02', 94.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 8, '2026-09-16 14:20'),
(542, 103, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241103-mod03', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-23 14:20'),
(543, 103, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241103-mod04', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-28 14:20'),
(544, 103, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241103-mod05', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-10-12 14:20'),
(545, 104, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025251104-mod01', 51.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 9, '2026-09-09 14:20'),
(546, 104, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251104-mod02', 54.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 9, '2026-09-16 14:20'),
(547, 104, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251104-mod03', 43.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 9, '2026-09-23 14:20'),
(548, 104, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251104-mod04', 46.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 9, '2026-09-28 14:20'),
(549, 104, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251104-mod05', 49.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 9, '2026-10-12 14:20'),
(550, 104, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251104-mod06', 88.0, 'Shader dan pencahayaan sudah sesuai.', 9, '2026-10-20 16:00'),
(551, 105, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241105-mod01', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-09 14:20'),
(552, 105, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241105-mod02', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-16 14:20'),
(553, 105, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241105-mod03', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-23 14:20'),
(554, 105, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241105-mod04', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-09-28 14:20'),
(555, 105, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241105-mod05', 92.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 10, '2026-10-12 14:20'),
(556, 106, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241106-mod01', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-09 14:20'),
(557, 106, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241106-mod02', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-16 14:20'),
(558, 106, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241106-mod03', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-23 14:20'),
(559, 106, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241106-mod04', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-28 14:20'),
(560, 106, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241106-mod05', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-10-12 14:20'),
(561, 106, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241106-mod06', NULL, NULL, NULL, NULL),
(562, 107, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241107-mod01', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-09 14:20'),
(563, 107, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241107-mod02', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-16 14:20'),
(564, 107, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241107-mod03', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-23 14:20'),
(565, 107, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241107-mod04', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-28 14:20'),
(566, 107, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241107-mod05', 75.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 12, '2026-10-12 14:20'),
(567, 108, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231108-mod01', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-09 14:20'),
(568, 108, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231108-mod02', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-16 14:20'),
(569, 108, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231108-mod03', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-23 14:20'),
(570, 108, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025231108-mod04', 96.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 13, '2026-09-28 14:20'),
(571, 108, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231108-mod05', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-10-12 14:20'),
(572, 108, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231108-mod06', 92.0, 'Shader dan pencahayaan sudah sesuai.', 13, '2026-10-20 16:00'),
(573, 109, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241109-mod01', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-09 14:20'),
(574, 109, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241109-mod02', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-16 14:20'),
(575, 109, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241109-mod03', 94.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 2, '2026-09-23 14:20'),
(576, 109, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241109-mod04', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-28 14:20'),
(577, 109, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241109-mod05', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-10-12 14:20'),
(578, 110, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241110-mod01', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-09 14:20'),
(579, 110, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241110-mod02', 92.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 3, '2026-09-16 14:20'),
(580, 110, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241110-mod06', NULL, NULL, NULL, NULL),
(581, 111, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241111-mod01', 90.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 4, '2026-09-09 14:20'),
(582, 111, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241111-mod02', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-16 14:20'),
(583, 111, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241111-mod03', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-23 14:20'),
(584, 111, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241111-mod04', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-28 14:20'),
(585, 111, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241111-mod05', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-10-12 14:20'),
(586, 112, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251112-mod01', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-09 14:20'),
(587, 112, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251112-mod02', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-16 14:20'),
(588, 112, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251112-mod03', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-23 14:20'),
(589, 112, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251112-mod04', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-28 14:20'),
(590, 112, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251112-mod05', 90.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-10-12 14:20'),
(591, 112, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251112-mod06', 80.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00'),
(592, 113, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241113-mod01', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-09 14:20'),
(593, 113, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241113-mod02', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-16 14:20'),
(594, 113, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241113-mod03', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-23 14:20'),
(595, 113, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241113-mod04', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-09-28 14:20'),
(596, 113, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241113-mod05', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 6, '2026-10-12 14:20'),
(597, 114, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241114-mod01', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-09 14:20'),
(598, 114, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241114-mod02', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-16 14:20'),
(599, 114, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241114-mod03', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-23 14:20'),
(600, 114, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241114-mod04', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 7, '2026-09-28 14:20'),
(601, 114, 5, '2026-10-11 09:30', 'https://github.com/simprak-lab/5025241114-mod05', 96.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 7, '2026-10-12 14:20'),
(602, 114, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241114-mod06', NULL, NULL, NULL, NULL),
(603, 115, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241115-mod01', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-09 14:20'),
(604, 115, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241115-mod02', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-16 14:20'),
(605, 115, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241115-mod03', 89.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-09-23 14:20'),
(606, 115, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241115-mod04', 94.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 8, '2026-09-28 14:20'),
(607, 115, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241115-mod05', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 8, '2026-10-12 14:20'),
(608, 116, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231116-mod01', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-09 14:20'),
(609, 116, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025231116-mod02', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-16 14:20'),
(610, 116, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025231116-mod03', 92.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 9, '2026-09-23 14:20'),
(611, 116, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231116-mod04', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-09-28 14:20'),
(612, 116, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231116-mod05', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 9, '2026-10-12 14:20'),
(613, 116, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231116-mod06', 84.0, 'Shader dan pencahayaan sudah sesuai.', 9, '2026-10-20 16:00'),
(614, 117, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241117-mod01', 50.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 10, '2026-09-09 14:20'),
(615, 117, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025241117-mod02', 53.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 10, '2026-09-16 14:20'),
(616, 117, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241117-mod03', 42.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 10, '2026-09-23 14:20'),
(617, 117, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241117-mod04', 45.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 10, '2026-09-28 14:20'),
(618, 117, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241117-mod05', 48.0, 'Perlu perbaikan logika utama dan kelengkapan analisis laporan.', 10, '2026-10-12 14:20'),
(619, 118, 1, '2026-09-08 09:30', 'https://github.com/simprak-lab/5025241118-mod01', 88.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 11, '2026-09-09 14:20'),
(620, 118, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241118-mod02', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-16 14:20'),
(621, 118, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241118-mod03', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-23 14:20'),
(622, 118, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241118-mod04', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-09-28 14:20'),
(623, 118, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241118-mod05', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 11, '2026-10-12 14:20'),
(624, 118, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241118-mod06', NULL, NULL, NULL, NULL),
(625, 119, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241119-mod01', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-09 14:20'),
(626, 119, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241119-mod02', 96.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-16 14:20'),
(627, 119, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241119-mod03', 78.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-23 14:20'),
(628, 119, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241119-mod04', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-09-28 14:20'),
(629, 119, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241119-mod05', 88.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 12, '2026-10-12 14:20'),
(630, 120, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025251120-mod01', 94.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-09 14:20'),
(631, 120, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025251120-mod02', 76.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-16 14:20'),
(632, 120, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025251120-mod03', 81.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-23 14:20'),
(633, 120, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025251120-mod04', 86.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-09-28 14:20'),
(634, 120, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025251120-mod05', 91.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 13, '2026-10-12 14:20'),
(635, 120, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025251120-mod06', 88.0, 'Shader dan pencahayaan sudah sesuai.', 13, '2026-10-20 16:00'),
(636, 121, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241121-mod01', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-09 14:20'),
(637, 121, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241121-mod02', 79.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-16 14:20'),
(638, 121, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241121-mod03', 84.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 2, '2026-09-23 14:20'),
(639, 122, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241122-mod01', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-09 14:20'),
(640, 122, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241122-mod02', 82.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-16 14:20'),
(641, 122, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025241122-mod03', 87.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-09-23 14:20'),
(642, 122, 4, '2026-09-29 09:30', 'https://github.com/simprak-lab/5025241122-mod04', 92.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 3, '2026-09-28 14:20'),
(643, 122, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241122-mod05', 74.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 3, '2026-10-12 14:20'),
(644, 122, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025241122-mod06', NULL, NULL, NULL, NULL),
(645, 123, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025241123-mod01', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-09 14:20'),
(646, 123, 2, '2026-09-13 18:45', 'https://github.com/simprak-lab/5025241123-mod02', 85.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-16 14:20'),
(647, 123, 3, '2026-09-22 09:30', 'https://github.com/simprak-lab/5025241123-mod03', 90.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 4, '2026-09-23 14:20'),
(648, 123, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025241123-mod04', 95.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-09-28 14:20'),
(649, 123, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025241123-mod05', 77.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 4, '2026-10-12 14:20'),
(650, 124, 1, '2026-09-06 18:45', 'https://github.com/simprak-lab/5025231124-mod01', 83.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-09 14:20'),
(651, 124, 2, '2026-09-15 09:30', 'https://github.com/simprak-lab/5025231124-mod02', 88.0, 'Pengumpulan melewati tenggat, namun fungsi utama berjalan baik.', 5, '2026-09-16 14:20'),
(652, 124, 3, '2026-09-20 18:45', 'https://github.com/simprak-lab/5025231124-mod03', 93.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-23 14:20'),
(653, 124, 4, '2026-09-27 18:45', 'https://github.com/simprak-lab/5025231124-mod04', 75.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-09-28 14:20'),
(654, 124, 5, '2026-10-09 18:45', 'https://github.com/simprak-lab/5025231124-mod05', 80.0, 'Implementasi berjalan baik dan sesuai spesifikasi modul.', 5, '2026-10-12 14:20'),
(655, 124, 6, '2026-10-19 20:15', 'https://github.com/simprak-lab/5025231124-mod06', 92.0, 'Shader dan pencahayaan sudah sesuai.', 5, '2026-10-20 16:00');

INSERT INTO log_aktivitas (id_pengguna, aksi, entitas, id_entitas, keterangan, waktu) VALUES
(1, 'LOGIN', 'Pengguna', 1, 'Koordinator Lab Dr. Hendra Wijaya, M.Kom. berhasil masuk ke sistem.', '2026-10-21 20:15'),
(2, 'NILAI', 'Pengumpulan', 1, 'Asisten Raka Pratama memperbarui nilai MOD-05 dari 78 menjadi 85 untuk NRP 5025241001.', '2026-10-21 19:45'),
(3, 'KUMPUL', 'Pengumpulan', 25, 'Asisten Nadia Kusuma mencatat pengumpulan MOD-06 (https://github.com/simprak-lab/5025241002-mod06).', '2026-10-21 18:30'),
(1, 'UBAH', 'Pengaturan', NULL, 'Admin memperbarui ambang ketuntasan R2 menjadi 75% dan ambang nilai R3 menjadi 60.', '2026-10-21 17:10'),
(4, 'NILAI', 'Pengumpulan', 40, 'Asisten Dimas Aditya menyimpan penilaian batch untuk Shift A3 - Grafika Gamma pada MOD-05.', '2026-10-21 16:05'),
(5, 'TAMBAH', 'Modul', 8, 'Admin menambahkan modul baru MOD-08: Proyek Akhir & Responsi Komprehensif.', '2026-10-20 14:22');
