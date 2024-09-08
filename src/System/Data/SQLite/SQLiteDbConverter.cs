using System.Diagnostics;

namespace System.Data.SQLite
{
    /// <summary>
    /// Represents an abstract base class for SQLite database converters.
    /// This class is responsible for managing and applying updates to the SQLite database schema.
    /// </summary>
    public abstract class SQLiteDbConverter: DbConverter<SQLiteDbConnection>
    {
        /// <summary>
        /// The name of the table that stores version information.
        /// </summary>
        internal const string VersionTableName = "db_info";

        /// <summary>
        /// SQL command to create the version table if it does not already exist.
        /// </summary>
        private const string CreateVersionTable = $"""
CREATE TABLE IF NOT EXISTS {VersionTableName} (
    db_key TEXT PRIMARY KEY NOT NULL CHECK(db_key <> ''), 
    db_value TEXT NOT NULL, 
    created_at INTEGER NOT NULL DEFAULT (strftime('%s', 'now')), 
    updated_at INTEGER NOT NULL DEFAULT (strftime('%s', 'now'))
    );
""";

        /// <summary>
        /// SQL trigger to update the timestamp after a version update.
        /// </summary>
        private const string TriggerAfterUpdateDbValue = $"""
CREATE TRIGGER IF NOT EXISTS {VersionTableName}_after_update_db_value 
   AFTER UPDATE OF db_value ON {VersionTableName}
BEGIN
   UPDATE {VersionTableName} SET updated_at=(strftime('%s', 'now')) WHERE db_key=NEW.db_key;
END;
""";

        /// <summary>
        /// Array containing the column names used in version-related operations.
        /// </summary>
        private static readonly string[] s_dbValueArray = { "db_value" };

        /// <summary>
        /// Gets the name of the key used to identify the version information in the database.
        /// </summary>
        /// <remarks>
        /// This property returns the specific key name that is used to store and retrieve version-related 
        /// data from the database. It is essential for ensuring that operations which depend on the 
        /// database version can correctly locate and update the relevant records.
        /// </remarks>
        public virtual string VersionKeyName => "DbVersion";

        /// <summary>
        /// Retrieves the current version of the database schema from the specified database connection.
        /// </summary>
        /// <param name="connection">The database connection to use for retrieving the version.</param>
        /// <returns>The current version of the database schema.</returns>
        public override Version GetDbVersion(SQLiteDbConnection connection)
        {
            var obj = connection.ExecuteScalar($"SELECT db_value FROM {VersionTableName} WHERE db_key = @db_key LIMIT 1",
                DbType.String.CreateInputParam("@db_key", VersionKeyName));
            Debug.Assert(obj != null);
            try
            {
                return new Version(Convert.ToString(obj)!);
            }
            catch
            {
                Debug.Assert(false);
                return new Version();
            }
        }

        /// <summary>
        /// Executes the specific operations required to update the database schema or data.
        /// This method is called during the update process and should be implemented by derived classes
        /// to perform the necessary changes to bring the database to the target version.
        /// </summary>
        /// <param name="connection">The database connection to use for performing the updates.</param>
        /// <returns><c>true</c> if the update operations were successful; otherwise, <c>false</c>.</returns>
        protected abstract bool PerformUpdate(SQLiteDbConnection connection);

        /// <summary>
        /// Determines whether the database requires an update to match the version specified by this converter.
        /// </summary>
        /// <param name="connection">The database connection to check.</param>
        /// <returns><c>true</c> if the database requires an update; otherwise, <c>false</c>.</returns>
        public override bool RequiresUpdate(SQLiteDbConnection connection)
        {
            using var _ = connection.SuspendAsserts();
            try
            {
                return base.RequiresUpdate(connection);
            }
            catch (Exception ex)
            {
                Debug.Assert(ex is SQLiteException && ex.Message.IndexOf("no such table:", StringComparison.Ordinal) > 0, $"{ex.GetType()}: {ex.Message}");
                return true;
            }
        }

        /// <summary>
        /// Attempts to add initial version information to the database by creating the version table
        /// and inserting the provided version value.
        /// </summary>
        /// <param name="connection">The database connection to use for adding version information.</param>
        /// <param name="dbVersion">The version value to insert into the version table.</param>
        /// <returns><c>true</c> if the version information was successfully added; otherwise, <c>false</c>.</returns>
        protected bool TryAddDbInfo(SQLiteDbConnection connection, string dbVersion)
        {
            connection.ExecuteNonQuery(CreateVersionTable);
            connection.ExecuteNonQuery(TriggerAfterUpdateDbValue);
            int affected = connection.ExecuteNonQuery($"INSERT OR IGNORE INTO {VersionTableName} (db_key, db_value) VALUES (@db_key, @db_value)",
                DbType.String.CreateInputParam("@db_key", VersionKeyName),
                DbType.String.CreateInputParam("@db_value", dbVersion));
            return affected == 1;
        }

        /// <summary>
        /// Updates the database schema to the current version defined by this converter.
        /// </summary>
        /// <param name="connection">The database connection to use for performing the update.</param>
        /// <returns><c>true</c> if the update operations were successful; otherwise, <c>false</c>.</returns>
        public sealed override bool Update(SQLiteDbConnection connection)
        {
            Debug.Assert(connection.InTransaction);
            if (PerformUpdate(connection))
            {
                connection.ExecuteNonQuery(TriggerAfterUpdateDbValue);
                using var updateCommand = connection.CreateCommandUpdate($"{VersionTableName}", s_dbValueArray, "WHERE db_key=@db_key",
                    DbType.String.CreateInputParam("@db_value", Version.ToString()),
                    DbType.String.CreateInputParam("@db_key", VersionKeyName));
                int rowsUpdated = connection.ExecuteNonQuery(updateCommand);
                Debug.Assert(rowsUpdated == 1);
                return true;
            }
            return false;
        }
    }

    public static class SQLiteDbConverterExtensions
    {
        public static void Initialize(this IReadOnlyList<SQLiteDbConverter> converters, Func<SQLiteDbConnection> createConnection, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(converters);
            ArgumentNullException.ThrowIfNull(createConnection);
#else
            Throw.IfNull(converters);
            Throw.IfNull(createConnection);
#endif
            using var connection = createConnection();
            connection.AcquireLock(() =>
            {
                converters.Initialize(connection, cancellationToken);
            }, cancellationToken);
        }

        public static void Initialize(this IReadOnlyList<SQLiteDbConverter> converters, SQLiteDbConnection connection, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(converters);
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(converters);
            Throw.IfNull(connection);
#endif
            Debug.Assert(connection.InTransaction == false, $"{nameof(connection)} in transaction state.");
            Throw.InvalidOperationExceptionIf(connection.InTransaction, $"{nameof(connection)} in transaction state.");
            Debug.Assert(connection.IsAcquired, $"{nameof(connection)} is not acquired.");
            Throw.InvalidOperationExceptionIf(connection.IsAcquired == false, $"{nameof(connection)} is not acquired.");

            connection.ExecuteInTransaction(t =>
            {
                converters.Initialize(connection);
                t.Commit();
            }, cancellationToken);
        }
    }
}
