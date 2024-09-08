using System.Data.SQLite;
using System.Diagnostics;

namespace MoviesAppSample.Converters
{
    internal sealed class MoviesConverter10: SQLiteDbConverter
    {
        public override Version Version { get; } = new("1.0");

        protected override bool PerformUpdate(SQLiteDbConnection connection)
        {
            if (TryAddDbInfo(connection, Version.ToString()) == false)
            {
                Debug.Assert(false);
                return false;
            }
            CreateTableMovies(connection);
            CreateTablePersons(connection);
            return true;
        }

        private static void CreateTableMovies(SQLiteDbConnection connection)
        {
            connection.ExecuteNonQuery("""
CREATE TABLE IF NOT EXISTS Movies (
    Id INTEGER PRIMARY KEY,
    Title TEXT NOT NULL,
    Description TEXT,
    DateReleased INTEGER NOT NULL);
""");
        }

        private static void CreateTablePersons(SQLiteDbConnection connection)
        {
            connection.ExecuteNonQuery("""
CREATE TABLE IF NOT EXISTS Persons (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL UNIQUE);
""");
        }
    }
}
