using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Data.SQLite
{
    /// <summary>
    /// Represents a SQLite database context that provides a connection to the SQLite database
    /// and supports transaction management through commit and rollback operations.
    /// </summary>
    public sealed class SQLiteDbContext: Disposable, IDbContext
    {
        private readonly SQLiteDbTransaction _transaction;
        private readonly Lifetime _lifetime = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLiteDbContext"/> class with the specified SQLite database connection.
        /// </summary>
        /// <param name="connection">The SQLite database connection.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided connection is null.</exception>
        public SQLiteDbContext(SQLiteDbConnection connection)
        {
            try
            {
                _lifetime.Add(() => GC.SuppressFinalize(this));
                Connection = connection ?? throw new ArgumentNullException(nameof(connection));
                CheckAcquired();
                Debug.Assert(!connection.InTransaction, $"{nameof(connection)} in transaction.");
                Throw.InvalidOperationExceptionIf(connection.InTransaction, $"{nameof(connection)} in transaction.");
                Connection.Open(out var originalState);
                _lifetime.Add(() => Connection.Close(originalState));
                _transaction = Connection.BeginTransaction();
                _lifetime.Add(_transaction.Dispose);
            }
            catch (Exception ex)
            {
                Debug.Assert(ex is SQLiteException dbEx && ex.Message.StartsWith("database is locked"), ex.Message);
                _lifetime.Dispose();
                throw;
            }
        }

        #region Properties

        /// <summary>
        /// Gets the SQLite database connection associated with this context.
        /// </summary>
        public SQLiteDbConnection Connection { get; }

        /// <summary>
        /// Gets the database connection associated with this context.
        /// </summary>
        IDbConnection IDbContext.Connection => Connection;

        #endregion

        #region Methods

        /// <summary>
        /// Ensures that the database connection is acquired and not disposed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckAcquired()
        {
            Debug.Assert(Connection.IsAcquired, $"{nameof(Connection)} is not acquired.");
            Throw.InvalidOperationExceptionIf(Connection.IsAcquired == false, $"{nameof(Connection)} is not acquired.");
        }

        /// <summary>
        /// Commits all changes made in this context to the database.
        /// </summary>
        public void Commit()
        {
            CheckAcquired();
            _transaction.Commit();
        }

        /// <summary>
        /// Disposes the resources used by this context.
        /// </summary>
        protected override void OnDispose()
        {
            CheckAcquired();
            _lifetime.Dispose();
            base.OnDispose();
        }

        /// <summary>
        /// Rolls back all changes made in this context.
        /// </summary>
        public void Rollback()
        {
            CheckAcquired();
            _transaction.Rollback();
        }

        #endregion
    }
}
