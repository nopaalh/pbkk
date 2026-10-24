using Microsoft.Data.Sqlite;

namespace Simprak.Data;

public static class DatabaseInitializer
{
    public static string GetDefaultDatabasePath()
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            localAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
        }
        string dir = Path.Combine(localAppData, "SIMPRAK");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "simprak.db");
    }

    public static string FindSqlScriptPath()
    {
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "database", "simprak.sql"),
            Path.Combine(Directory.GetCurrentDirectory(), "database", "simprak.sql"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "database", "simprak.sql")),
            "/home/nopalh/kuliah/semester5/grafKom/sistemInformasi/database/simprak.sql"
        ];

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        throw new FileNotFoundException("Berkas skrip database simprak.sql tidak ditemukan.");
    }

    /// <summary>
    /// Memastikan database tersedia. Jika belum ada, dibuat terlebih dahulu di berkas sementara (.tmp)
    /// lalu dipindahkan secara atomik ke lokasi tujuan agar kegagalan proses tidak meninggalkan database setengah jadi.
    /// Jika sudah ada, memeriksa kompatibilitas skema (misalnya kolom tautan_tugas FR-39).
    /// </summary>
    public static void EnsureInitialized(string dbPath, string? sqlScriptPath = null)
    {
        string scriptFile = sqlScriptPath ?? FindSqlScriptPath();
        string? directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        if (!File.Exists(dbPath))
        {
            string tempPath = dbPath + ".tmp." + Guid.NewGuid().ToString("N");
            try
            {
                InitializeOnConnection($"Data Source={tempPath};Foreign Keys=True", File.ReadAllText(scriptFile));
                SqliteConnection.ClearAllPools();
                File.Move(tempPath, dbPath, overwrite: true);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
        else
        {
            UpgradeSchemaIfNeeded($"Data Source={dbPath};Foreign Keys=True");
        }
    }

    public static void ResetDatabase(string dbPath, string? sqlScriptPath = null)
    {
        SqliteConnection.ClearAllPools();
        if (File.Exists(dbPath))
            File.Delete(dbPath);
        EnsureInitialized(dbPath, sqlScriptPath);
    }

    public static void InitializeOnConnection(SqliteConnection openConnection, string sqlScript)
    {
        using var cmd = openConnection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;\n" + sqlScript;
        cmd.ExecuteNonQuery();
    }

    public static void InitializeOnConnection(string connectionString, string sqlScript)
    {
        using var conn = new SqliteConnection(connectionString);
        conn.Open();
        InitializeOnConnection(conn, sqlScript);
    }

    /// <summary>
    /// Memeriksa dan menambahkan kolom tautan_tugas pada tabel pengumpulan jika database versi lama belum memilikinya,
    /// tanpa menghapus data pengguna yang sudah ada.
    /// </summary>
    public static void UpgradeSchemaIfNeeded(SqliteConnection openConnection)
    {
        bool hasTautanColumn = false;
        using (var checkCmd = openConnection.CreateCommand())
        {
            checkCmd.CommandText = "PRAGMA table_info(pengumpulan);";
            using var reader = checkCmd.ExecuteReader();
            while (reader.Read())
            {
                string colName = reader.GetString(1);
                if (string.Equals(colName, "tautan_tugas", StringComparison.OrdinalIgnoreCase))
                {
                    hasTautanColumn = true;
                    break;
                }
            }
        }

        if (!hasTautanColumn)
        {
            using var alterCmd = openConnection.CreateCommand();
            alterCmd.CommandText = "ALTER TABLE pengumpulan ADD COLUMN tautan_tugas TEXT NULL;";
            alterCmd.ExecuteNonQuery();
        }
    }

    public static void UpgradeSchemaIfNeeded(string connectionString)
    {
        using var conn = new SqliteConnection(connectionString);
        conn.Open();
        UpgradeSchemaIfNeeded(conn);
    }
}
