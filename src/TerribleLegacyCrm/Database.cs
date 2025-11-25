using System.Data.SQLite;

namespace TerribleLegacyCrm;

internal static class Database
{
    public static SQLiteConnection CreateOpenConnection()
    {
        var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        Directory.CreateDirectory(dataDir);

        var dbPath = Path.Combine(dataDir, "terriblecrm.db");
        var connection = new SQLiteConnection($"Data Source={dbPath};Foreign Keys=True;");
        connection.Open();
        return connection;
    }
}
