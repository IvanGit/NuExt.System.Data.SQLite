using System.Data.SQLite;

namespace MoviesAppSample.Converters
{
    internal sealed class MoviesConverter11 : SQLiteDbConverter
    {
        public override Version Version { get; } = new("1.1");

        protected override bool PerformUpdate(SQLiteDbConnection connection)
        {
            CreateTableMovieCasts(connection);
            CreateTableMovieDirectors(connection);
            CreateTableMovieWriters(connection);
            return true;
        }

        private static void CreateTableMovieCasts(SQLiteDbConnection connection)
        {
            connection.ExecuteNonQuery("""
CREATE TABLE IF NOT EXISTS MovieCasts (
    MovieId INTEGER NOT NULL,
    PersonId INTEGER NOT NULL,
    FOREIGN KEY (MovieId) REFERENCES Movies(Id),
    FOREIGN KEY (PersonId) REFERENCES Persons(Id),
    PRIMARY KEY (MovieId, PersonId)
);
""");
        }

        private static void CreateTableMovieDirectors(SQLiteDbConnection connection)
        {
            connection.ExecuteNonQuery("""
CREATE TABLE IF NOT EXISTS MovieDirectors (
    MovieId INTEGER NOT NULL,
    PersonId INTEGER NOT NULL,
    FOREIGN KEY (MovieId) REFERENCES Movies(Id),
    FOREIGN KEY (PersonId) REFERENCES Persons(Id),
    PRIMARY KEY (MovieId, PersonId)
);
""");
        }

        private static void CreateTableMovieWriters(SQLiteDbConnection connection)
        {
            connection.ExecuteNonQuery("""
CREATE TABLE IF NOT EXISTS MovieWriters (
    MovieId INTEGER NOT NULL,
    PersonId INTEGER NOT NULL,
    FOREIGN KEY (MovieId) REFERENCES Movies(Id),
    FOREIGN KEY (PersonId) REFERENCES Persons(Id),
    PRIMARY KEY (MovieId, PersonId)
);
""");
        }
    }
}
