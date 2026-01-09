using System.IO;

namespace apppasteleriav04.Data.Local.Database
{
    //
    public static class DatabaseConstants
    {
        public const string DatabaseFilename = "pasteleria.db3";
        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);
    }
}
