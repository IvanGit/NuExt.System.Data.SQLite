using System.Diagnostics;

namespace System.Data.SQLite
{
    partial class SQLiteDbConnection
    {
        #region Methods

        public SQLiteDataAdapter CreateAdapter()
        {
            CheckDisposed();
            return new SQLiteDataAdapter();
        }

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

        public DataTable Select(SQLiteCommand command)
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
                Fill(adapter, table);
                return table;
            }
            catch (Exception ex)
            {
                Debug.Assert(_assertsDisabled, ex.GetType() + ": " + ex.Message);
                throw;
            }
        }

        public DataTable Select(string sql, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(sql);
#else
            Throw.IfNullOrEmpty(sql);
#endif
            return Select(CreateCommand(sql, parameters));
        }

        public DataTable Select(string tableName, IEnumerable<string> fields, string? where, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            return Select(CreateCommandSelect(tableName, fields, where, parameters));
        }

        public (SQLiteDataAdapter, DataTable) SelectForUpdate(SQLiteCommand command, bool generateCommands)
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
                Fill(adapter, table);
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

        public (SQLiteDataAdapter, DataTable) SelectForUpdate(string sql, bool generateCommands, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(sql);
#else
            Throw.IfNullOrEmpty(sql);
#endif
            return SelectForUpdate(CreateCommand(sql, parameters), generateCommands);
        }

        public (SQLiteDataAdapter, DataTable) SelectForUpdate(string tableName, IEnumerable<string> fields, string? where, bool generateCommands, params SQLiteParameter[] parameters)
        {
#if NET8_0_OR_GREATER
            ArgumentException.ThrowIfNullOrEmpty(tableName);
#else
            Throw.IfNullOrEmpty(tableName);
#endif
            return SelectForUpdate(CreateCommandSelect(tableName, fields, where, parameters), generateCommands);
        }

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
    }
}
