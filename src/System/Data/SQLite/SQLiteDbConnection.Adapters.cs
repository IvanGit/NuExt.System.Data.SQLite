using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Data.SQLite
{
    partial class SQLiteDbConnection
    {
        #region Methods

        #region CreateAdapter

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SQLiteDataAdapter CreateAdapter()
        {
            CheckDisposed();
            return new SQLiteDataAdapter();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SQLiteDataAdapter CreateAdapter(SQLiteCommand command)
        {
            CheckDisposed();
            Debug.Assert(command != null, $"{nameof(command)} is null");
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(command);
#else
            Throw.IfNull(command);
#endif
            return new SQLiteDataAdapter() { SelectCommand = command };
        }

        #endregion

        #region Select

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataTable Select(string tableName, IEnumerable<string> fields, string? expr, params SQLiteParameter[] parameters)
        {
            return Select(tableName, fields, expr, default, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataTable Select(string tableName, IEnumerable<string> fields, string? expr, CancellationToken cancellationToken, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            return Select(CreateCommandSelect(tableName, fields, expr, parameters), cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataTable Select(string sql, params SQLiteParameter[] parameters)
        {
            return Select(sql, default, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataTable Select(string sql, CancellationToken cancellationToken, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(sql);
#else
            Throw.IfNullOrEmpty(sql);
#endif
            return Select(CreateCommand(sql, parameters), cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataTable Select(SQLiteCommand command)
        {
            return Select(command, default);
        }

        public DataTable Select(SQLiteCommand command, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(command);
#else
            Throw.IfNull(command);
#endif
            using var adapter = CreateAdapter();
            try
            {
                adapter.SelectCommand = command;
                var table = new DataTable();
                Fill(adapter, table, cancellationToken);
                return table;
            }
            catch (Exception ex)
            {
                Debug.Assert(_assertsDisabled, ex.GetType() + ": " + ex.Message);
                throw;
            }
        }

        #endregion

        #region SelectForUpdate

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (SQLiteDataAdapter, DataTable) SelectForUpdate(string tableName, IEnumerable<string> fields, string? expr, bool generateCommands, params SQLiteParameter[] parameters)
        {
            return SelectForUpdate(tableName, fields, expr, generateCommands, default, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (SQLiteDataAdapter, DataTable) SelectForUpdate(string tableName, IEnumerable<string> fields, string? expr, bool generateCommands, CancellationToken cancellationToken, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            return SelectForUpdate(CreateCommandSelect(tableName, fields, expr, parameters), generateCommands, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (SQLiteDataAdapter, DataTable) SelectForUpdate(string sql, bool generateCommands, params SQLiteParameter[] parameters)
        {
            return SelectForUpdate(sql, generateCommands, default, parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (SQLiteDataAdapter, DataTable) SelectForUpdate(string sql, bool generateCommands, CancellationToken cancellationToken, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(sql);
#else
            Throw.IfNullOrEmpty(sql);
#endif
            return SelectForUpdate(CreateCommand(sql, parameters), generateCommands, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (SQLiteDataAdapter, DataTable) SelectForUpdate(SQLiteCommand command, bool generateCommands)
        {
            return SelectForUpdate(command, generateCommands, default);
        }

        public (SQLiteDataAdapter, DataTable) SelectForUpdate(SQLiteCommand command, bool generateCommands, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(command);
#else
            Throw.IfNull(command);
#endif
            var adapter = CreateAdapter();
            try
            {
                adapter.SelectCommand = command;
                var table = new DataTable();
                Fill(adapter, table, cancellationToken);
                if (generateCommands)
                {
                    var cmdBuilder = new SQLiteCommandBuilder(adapter);
#if DEBUG_
                var insertCommand = cmdBuilder.GetInsertCommand();
                var updateCommand = cmdBuilder.GetUpdateCommand();
                var deleteCommand = cmdBuilder.GetDeleteCommand();
                insertCommand?.Dispose();
                updateCommand?.Dispose();
                deleteCommand?.Dispose();
#endif
                }
                return (adapter, table);
            }
            catch (Exception ex)
            {
                Debug.Assert(_assertsDisabled, ex.GetType() + ": " + ex.Message);
                adapter.Dispose();
                throw;
            }
        }

        #endregion

        #region Fill

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fill(SQLiteDataAdapter adapter, DataTable table)
        {
            Fill(adapter, table, default);
        }

        public void Fill(SQLiteDataAdapter adapter, DataTable table, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(adapter);
            ArgumentNullException.ThrowIfNull(table);
#else
            Throw.IfNull(adapter);
            Throw.IfNull(table);
#endif

            AcquireLock(() => adapter.Fill(table), cancellationToken);
        }

        #endregion

        #region Update

        public int Update(SQLiteDataAdapter adapter, DataTable table)
        {
            return Update(adapter, table, default);
        }

        public int Update(SQLiteDataAdapter adapter, DataTable table, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(adapter);
            ArgumentNullException.ThrowIfNull(table);
#else
            Throw.IfNull(adapter);
            Throw.IfNull(table);
#endif
            //Update requires a valid UpdateCommand when passed DataRow collection with modified rows.
            return AcquireLock(() => adapter.Update(table), cancellationToken);
        }

        #endregion

        #endregion
    }
}
