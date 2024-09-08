using System.Diagnostics;

namespace System.Data.SQLite
{
    public static class SQLiteDbConnectionExtensions
    {
        public static void ExecuteInTransaction(this SQLiteDbConnection connection, Action<SQLiteDbTransaction> action, CancellationToken cancellationToken = default)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
            Debug.Assert(action is not null, $"{nameof(action)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
            ArgumentNullException.ThrowIfNull(action);
#else
            Throw.IfNull(connection);
            Throw.IfNull(action);
#endif
            Debug.Assert(!connection.InTransaction, $"{nameof(connection)} in transaction.");
            Throw.InvalidOperationExceptionIf(connection.InTransaction, $"{nameof(connection)} in transaction.");
            connection.AcquireLock(() =>
                {
                    connection.QuietOpen(out var open);
                    try
                    {
                        using var transaction = connection.BeginTransaction();
                        try
                        {
                            action(transaction);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            Debug.Assert(connection.AssertsDisabled, ex.GetType() + ": " + ex.Message);
                            transaction.Rollback();
                            throw;
                        }
                    }
                    finally
                    {
                        connection.QuietClose(open);
                    }
                }, cancellationToken);
        }

        public static T ExecuteInTransaction<T>(this SQLiteDbConnection connection, Func<SQLiteDbTransaction, T> action, CancellationToken cancellationToken = default)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
            Debug.Assert(action is not null, $"{nameof(action)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
            ArgumentNullException.ThrowIfNull(action);
#else
            Throw.IfNull(connection);
            Throw.IfNull(action);
#endif
            Debug.Assert(!connection.InTransaction, $"{nameof(connection)} in transaction.");
            Throw.InvalidOperationExceptionIf(connection.InTransaction, $"{nameof(connection)} in transaction.");
            return connection.AcquireLock(() =>
                {
                    connection.QuietOpen(out var open);
                    try
                    {
                        using var transaction = connection.BeginTransaction();
                        try
                        {
                            return action(transaction);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception ex)
                        {
                            Debug.Assert(connection.AssertsDisabled, ex.GetType() + ": " + ex.Message);
                            transaction.Rollback();
                            throw;
                        }
                    }
                    finally
                    {
                        connection.QuietClose(open);
                    }
                }, cancellationToken);
        }

        public static ICollection<string> GetTableColumns(this SQLiteDbConnection connection, string tableName)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
            Debug.Assert(!string.IsNullOrEmpty(tableName), $"{nameof(tableName)} is null or empty");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(connection);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            var columns = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            int? index = null;
            void Read(SQLiteDataReader reader)
            {
                index ??= reader.GetOrdinal("name");
                string name = reader.GetString(index.Value);
                columns.Add(name);
            }
            connection.ExecuteReader($"PRAGMA table_info('{tableName}')", Read);
            return columns;
        }

        public static DataTable GetTableSchema(this SQLiteDbConnection connection, string tableName)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
            Debug.Assert(!string.IsNullOrEmpty(tableName), $"{nameof(tableName)} is null or empty");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(connection);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            var schema = connection.Select($"SELECT * FROM \"{tableName}\" WHERE 0=1");
            schema.TableName = tableName;
            return schema;
        }

        public static ICollection<string> GetAllTables(this SQLiteDbConnection connection)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(connection);
#endif
            var tables = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            int? index = null;
            void Read(SQLiteDataReader reader)
            {
                index ??= reader.GetOrdinal("name");
                string name = reader.GetString(index.Value);
                tables.Add(name);
            }
            connection.ExecuteReader("SELECT * FROM sqlite_master WHERE type='table'", Read);
            return tables;
        }

        public static ICollection<string> GetAllViews(this SQLiteDbConnection connection)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(connection);
#endif
            var tables = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            int? index = null;
            void Read(SQLiteDataReader reader)
            {
                index ??= reader.GetOrdinal("name");
                string name = reader.GetString(index.Value);
                tables.Add(name);
            }
            connection.ExecuteReader("SELECT * FROM sqlite_master WHERE type='view'", Read);
            return tables;
        }

        public static ICollection<string> GetTableIndexes(this SQLiteDbConnection connection, string tableName)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
            Debug.Assert(!string.IsNullOrEmpty(tableName), $"{nameof(tableName)} is null or empty");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(connection);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            var indexes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            int? index = null;
            void Read(SQLiteDataReader reader)
            {
                index ??= reader.GetOrdinal("name");
                string name = reader.GetString(index.Value);
                indexes.Add(name);
            }
            connection.ExecuteReader("SELECT * FROM sqlite_master WHERE type='index' AND tbl_name=@tbl_name", Read, CommandBehavior.Default, DbType.String.CreateInputParam("@tbl_name", tableName));
            return indexes;
        }

        public static bool IsTableEmpty(this SQLiteDbConnection connection, string tableName)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
            Debug.Assert(!string.IsNullOrEmpty(tableName), $"{nameof(tableName)} is null or empty");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(connection);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            var res = connection.ExecuteScalar($"SELECT 1 FROM \"{tableName}\" LIMIT 1");
            return res is null;
        }

        public static bool IsTableExist(this SQLiteDbConnection connection, string tableName)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
            Debug.Assert(!string.IsNullOrEmpty(tableName), $"{nameof(tableName)} is null or empty");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(connection);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            var res = connection.ExecuteScalar("SELECT 1 FROM sqlite_master WHERE name=@name and type='table' LIMIT 1",
                DbType.String.CreateInputParam("@name", tableName));
            return res is not null;
        }

        public static bool IsTableIndexExist(this SQLiteDbConnection connection, string tableName, string indexName)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
            Debug.Assert(!string.IsNullOrEmpty(tableName), $"{nameof(tableName)} is null or empty");
            Debug.Assert(!string.IsNullOrEmpty(indexName), $"{nameof(indexName)} is null or empty");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(connection);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
            ArgumentException.ThrowIfNullOrEmpty(indexName);
#else
            Throw.IfNullOrEmpty(tableName);
            Throw.IfNullOrEmpty(indexName);
#endif
            var res = connection.ExecuteScalar(
                "SELECT 1 FROM sqlite_master WHERE tbl_name=@tbl_name AND name=@name and type='index' LIMIT 1",
                DbType.String.CreateInputParam("@tbl_name", tableName),
                DbType.String.CreateInputParam("@name", indexName));
            return res is not null;
        }

        public static bool IsTriggerExist(this SQLiteDbConnection connection, string triggerName)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
            Debug.Assert(!string.IsNullOrEmpty(triggerName), $"{nameof(triggerName)} is null or empty");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(connection);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(triggerName);
#else
            Throw.IfNullOrEmpty(triggerName);
#endif
            var res = connection.ExecuteScalar("SELECT 1 FROM sqlite_master WHERE name=@name and type='trigger' LIMIT 1",
                DbType.String.CreateInputParam("@name", triggerName));
            return res is not null;
        }

        public static bool IsViewExist(this SQLiteDbConnection connection, string viewName)
        {
            Debug.Assert(connection is not null, $"{nameof(connection)} is null");
            Debug.Assert(!string.IsNullOrEmpty(viewName), $"{nameof(viewName)} is null or empty");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(connection);
#else
            Throw.IfNull(connection);
#endif
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(viewName);
#else
            Throw.IfNullOrEmpty(viewName);
#endif
            var res = connection.ExecuteScalar("SELECT 1 FROM sqlite_master WHERE name=@name and type='view' LIMIT 1",
                DbType.String.CreateInputParam("@name", viewName));
            return res is not null;
        }
    }
}
