using System;
using System.IO;

namespace apppasteleriav04.Data.Local.Database
{
    /// <summary>
    /// SQLite database constants and configuration
    /// </summary>
    public static class DatabaseConstants
    {
        /// <summary>
        /// Name of the SQLite database file
        /// </summary>
        public const string DatabaseFilename = "pasteleria.db3";

        /// <summary>
        /// SQLite connection flags for optimal performance
        /// </summary>
        public const SQLite.SQLiteOpenFlags Flags =
            SQLite.SQLiteOpenFlags.ReadWrite |
            SQLite.SQLiteOpenFlags.Create |
            SQLite.SQLiteOpenFlags.SharedCache;

        /// <summary>
        /// Gets the full path to the database file
        /// </summary>
        public static string DatabasePath
        {
            get
            {
                var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(basePath, DatabaseFilename);
            }
        }
    }
}
