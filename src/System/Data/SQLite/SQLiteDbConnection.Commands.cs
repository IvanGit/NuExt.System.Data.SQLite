using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Data.SQLite
{
    partial class SQLiteDbConnection
    {
        #region Methods

        public SQLiteCommand CreateCommand()
        {
            return _conn.CreateCommand();
        }

        public SQLiteCommand CreateCommand(string sql, params SQLiteParameter[] parameters)
        {
            SQLiteCommand command = CreateCommand();
            command.CommandText = sql;
            if (parameters is { Length: > 0 })
            {
                Debug.Assert(new HashSet<string>(parameters.Select(p => p.ParameterName)).Count == parameters.Length);
                command.Parameters.AddRange(parameters);
            }
            return command;
        }

        public SQLiteCommand CreateCommandInsert(string tableName, IEnumerable<string> fields, params SQLiteParameter[] parameters)
        {
            return CreateCommandInsert(tableName, fields, null, parameters);
        }

        public SQLiteCommand CreateCommandInsert(string tableName, IEnumerable<string> fields, string? expr, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            if (fields.Count() != parameters.Length)
            {
                Debug.Assert(false, $"{nameof(CreateCommandInsert)}: argument count mismatch");
                Throw.ArgumentException($"{nameof(CreateCommandInsert)}: argument count mismatch");
            }
            var builder = new ValueStringBuilder();
            builder.Append($"INSERT INTO \"{tableName}\" (");
            string delimiter = string.Empty;
            foreach (string field in fields)
            {
                builder.Append($"{delimiter}\"{field}\"");
                delimiter = ", ";
            }
            builder.Append(") VALUES (");
            delimiter = string.Empty;
            foreach (string field in fields)
            {
                builder.Append($"{delimiter}@{field}");
                delimiter = ", ";
            }
            builder.Append(')');
            if (!string.IsNullOrEmpty(expr))
            {
                builder.Append(' ');
                builder.Append(expr);
            }
            return CreateCommand(builder.ToString(), parameters);
        }

        public SQLiteCommand CreateCommandInsertOrIgnore(string tableName, IEnumerable<string> fields, params SQLiteParameter[] parameters)
        {
            return CreateCommandInsertOrIgnore(tableName, fields, null, parameters);
        }

        public SQLiteCommand CreateCommandInsertOrIgnore(string tableName, IEnumerable<string> fields, string? expr, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            if (fields.Count() != parameters.Length)
            {
                Debug.Assert(false, $"{nameof(CreateCommandInsertOrIgnore)}: argument count mismatch");
                Throw.ArgumentException($"{nameof(CreateCommandInsertOrIgnore)}: argument count mismatch");
            }
            var builder = new ValueStringBuilder();
            builder.Append($"INSERT OR IGNORE INTO \"{tableName}\" (");
            string delimiter = string.Empty;
            foreach (string field in fields)
            {
                builder.Append($"{delimiter}\"{field}\"");
                delimiter = ", ";
            }
            builder.Append(") VALUES (");
            delimiter = string.Empty;
            foreach (string field in fields)
            {
                builder.Append($"{delimiter}@{field}");
                delimiter = ", ";
            }
            builder.Append(')');
            if (!string.IsNullOrEmpty(expr))
            {
                builder.Append(' ');
                builder.Append(expr);
            }
            return CreateCommand(builder.ToString(), parameters);
        }

        public SQLiteCommand CreateCommandInsertOrReplace(string tableName, IEnumerable<string> fields, params SQLiteParameter[] parameters)
        {
            return CreateCommandInsertOrReplace(tableName, fields, null, parameters);
        }

        public SQLiteCommand CreateCommandInsertOrReplace(string tableName, IEnumerable<string> fields, string? expr, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            if (fields.Count() != parameters.Length)
            {
                Debug.Assert(false, $"{nameof(CreateCommandInsertOrReplace)}: argument count mismatch");
                Throw.ArgumentException($"{nameof(CreateCommandInsertOrReplace)}: argument count mismatch");
            }
            var builder = new ValueStringBuilder();
            builder.Append($"REPLACE INTO \"{tableName}\" (");//Alias for INSERT OR REPLACE
            string delimiter = string.Empty;
            foreach (string field in fields)
            {
                builder.Append($"{delimiter}\"{field}\"");
                delimiter = ", ";
            }
            builder.Append(") VALUES (");
            delimiter = string.Empty;
            foreach (string field in fields)
            {
                builder.Append($"{delimiter}@{field}");
                delimiter = ", ";
            }
            builder.Append(')');
            if (!string.IsNullOrEmpty(expr))
            {
                builder.Append(' ');
                builder.Append(expr);
            }
            return CreateCommand(builder.ToString(), parameters);
        }

        public SQLiteCommand CreateCommandSelect(string tableName, IEnumerable<string> fields, string? expr, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            var builder = new ValueStringBuilder();
            builder.Append("SELECT ");
            string delimiter = string.Empty;
            foreach (string field in fields)
            {
                builder.Append($"{delimiter}\"{field}\"");
                delimiter = ", ";
            }
            builder.Append($" FROM \"{tableName}\"");
            if (!string.IsNullOrEmpty(expr))
            {
                builder.Append(' ');
                builder.Append(expr);
            }
            return CreateCommand(builder.ToString(), parameters);
        }

        public SQLiteCommand CreateCommandUpdate(string tableName, IEnumerable<string> fields, string? expr, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            if (fields.Count() > parameters.Length)
            {
                Debug.Assert(false, $"{nameof(CreateCommandUpdate)}: argument count mismatch");
                Throw.ArgumentException($"{nameof(CreateCommandUpdate)}: argument count mismatch");
            }
            var builder = new ValueStringBuilder();
            builder.Append($"UPDATE \"{tableName}\" SET ");
            string delimiter = string.Empty;
            foreach (string field in fields)
            {
                builder.Append($"{delimiter}\"{field}\"=@{field}");
                delimiter = ", ";
            }
            if (!string.IsNullOrEmpty(expr))
            {
                builder.Append(' ');
                builder.Append(expr);
            }
            return CreateCommand(builder.ToString(), parameters);
        }

        public SQLiteCommand CreateCommandDelete(string tableName, string? expr, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            var builder = new ValueStringBuilder();
            builder.Append($"DELETE FROM \"{tableName}\"");
            if (!string.IsNullOrEmpty(expr))
            {
                builder.Append(' ');
                builder.Append(expr);
            }
            return CreateCommand(builder.ToString(), parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteNonQuery(SQLiteCommand command)
        {
            return ExecuteNonQuery(command, default);
        }

        public int ExecuteNonQuery(SQLiteCommand command, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(command);
#else
            Throw.IfNull(command);
#endif
            return AcquireLock(() =>
                {
                    QuietOpen(out var open);
                    try
                    {
                        return command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Debug.Assert(_assertsDisabled, ex.GetType() + ": " + ex.Message);
                        throw;
                    }
                    finally
                    {
                        QuietClose(open);
                    }
                }, cancellationToken);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            return ExecuteNonQuery(sql, default, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteNonQuery(string sql, CancellationToken cancellationToken, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(sql);
#else
            Throw.IfNullOrEmpty(sql);
#endif
            using var command = CreateCommand(sql, parameters);
            return ExecuteNonQuery(command);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteReader(SQLiteCommand command, Action<SQLiteDataReader>? read)//1a=>2a
        {
            return ExecuteReader(command, read, default(CancellationToken));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteReader(SQLiteCommand command, Action<SQLiteDataReader>? read, CancellationToken cancellationToken)//2a=>4a
        {
            return ExecuteReader(command, read, CommandBehavior.Default, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteReader(SQLiteCommand command, Action<SQLiteDataReader>? read, CommandBehavior behavior)//3a=>4a
        {
            return ExecuteReader(command, read, behavior, default);
        }

        public int ExecuteReader(SQLiteCommand command, Action<SQLiteDataReader>? read, CommandBehavior behavior, CancellationToken cancellationToken)//4a
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(command);
#else
            Throw.IfNull(command);
#endif
            return AcquireLock(() =>
                {
                    int count = 0;
                    QuietOpen(out var open);
                    try
                    {
                        using var reader = command.ExecuteReader(behavior);
                        while (reader.Read())
                        {
                            try
                            {
                                read?.Invoke(reader);
                            }
                            catch
                            {
                                command.Cancel();
                                throw;
                            }
                            count++;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Debug.Assert(_assertsDisabled, ex.GetType() + ": " + ex.Message);
                        throw;
                    }
                    finally
                    {
                        QuietClose(open);
                    }
                    return count;
                }, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteReader(string sql, Action<SQLiteDataReader>? read, params SQLiteParameter[] parameters)//1b=>2b
        {
            return ExecuteReader(sql, read, default(CancellationToken), parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ExecuteReader(string sql, Action<SQLiteDataReader>? read, CancellationToken cancellationToken, params SQLiteParameter[] parameters)//2b=>4b
        {
            return ExecuteReader(sql, read, CommandBehavior.Default, cancellationToken, parameters);
        }

        public int ExecuteReader(string sql, Action<SQLiteDataReader>? read, CommandBehavior behavior, params SQLiteParameter[] parameters)//3b=>3a
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(sql);
#else
            Throw.IfNullOrEmpty(sql);
#endif
            using var command = CreateCommand(sql, parameters);
            return ExecuteReader(command, read, behavior);
        }

        public int ExecuteReader(string sql, Action<SQLiteDataReader>? read, CommandBehavior behavior,
            CancellationToken cancellationToken, params SQLiteParameter[] parameters)//4b=>4a
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(sql);
#else
            Throw.IfNullOrEmpty(sql);
#endif
            using var command = CreateCommand(sql, parameters);
            return ExecuteReader(command, read, behavior, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? ExecuteScalar(SQLiteCommand command)
        {
            return ExecuteScalar(command, default);
        }

        public object? ExecuteScalar(SQLiteCommand command, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(command);
#else
            Throw.IfNull(command);
#endif
            return AcquireLock(() =>
            {
                QuietOpen(out var open);
                try
                {
                    return command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    Debug.Assert(_assertsDisabled, ex.GetType() + ": " + ex.Message);
                    throw;
                }
                finally
                {
                    QuietClose(open);
                }
            }, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            return ExecuteScalar(sql, default, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? ExecuteScalar(string sql, CancellationToken cancellationToken, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(sql);
#else
            Throw.IfNullOrEmpty(sql);
#endif
            using var command = CreateCommand(sql, parameters);
            return ExecuteScalar(command);
        }

        #endregion
    }
}
