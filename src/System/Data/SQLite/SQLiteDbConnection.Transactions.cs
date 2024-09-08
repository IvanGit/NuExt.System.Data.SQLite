using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Data.SQLite
{
    partial class SQLiteDbConnection
    {
        #region Methods

        public SQLiteDbTransaction BeginTransaction()
        {
            CheckDisposed();
            Debug.Assert(_inTransaction == 0);
            Interlocked.Increment(ref _inTransaction);
            try
            {
                return new SQLiteDbTransaction(this, _conn.BeginTransaction(), () => Interlocked.Decrement(ref _inTransaction));
            }
            catch (Exception ex)
            {
                Debug.Assert(_assertsDisabled, ex.GetType() + ": " + ex.Message);
                Interlocked.Decrement(ref _inTransaction);
                throw;
            }
        }

        public SQLiteDbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            CheckDisposed();
            Debug.Assert(_inTransaction == 0);
            Interlocked.Increment(ref _inTransaction);
            try
            {
                return new SQLiteDbTransaction(this, _conn.BeginTransaction(isolationLevel), () => Interlocked.Decrement(ref _inTransaction));
            }
            catch (Exception ex)
            {
                Debug.Assert(_assertsDisabled, ex.GetType() + ": " + ex.Message);
                Interlocked.Decrement(ref _inTransaction);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SQLiteDbTransaction BeginTransactionDeferred()
        {
            return BeginTransaction(IsolationLevel.ReadCommitted);
        }

        #endregion
    }
}
