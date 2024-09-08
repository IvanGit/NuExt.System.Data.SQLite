namespace System.Data.SQLite
{
    internal class SQLiteDbConnectionFactory
    {
        #region Internal classes

        internal class AsyncLockCounter : Disposable
        {
            public readonly ReentrantAsyncLock Mutex = new();
            public volatile int ReferenceCount;
#if DEBUG
            public bool IsExpired => ReferenceCount <= 0;
#endif
            protected override void OnDispose()
            {
                Mutex.Dispose();
                base.OnDispose();
            }
        }

        #endregion

        public static readonly SQLiteDbConnectionFactory Instance = new();

        private readonly Dictionary<string, AsyncLockCounter> _sharedLocks = new(StringComparer.CurrentCultureIgnoreCase);
        private SQLiteDbConnectionFactory()
        {

        }

        #region Properties

        public int SharedLockCount { get { lock (_sharedLocks) return _sharedLocks.Count; } }

        public IReadOnlyCollection<string> SharedLockKeys { get { lock (_sharedLocks) return _sharedLocks.Keys; } }

        public IReadOnlyDictionary<string, int> SharedLocks { get { lock (_sharedLocks) return _sharedLocks.ToDictionary(pair => pair.Key, pair => pair.Value.ReferenceCount); } }

        #endregion

        #region Methods

        /// <summary>
        /// Gets an object that can be used to synchronize access to the database.
        /// </summary>
        internal AsyncLockCounter GetSyncObj(string connectionString)
        {
            AsyncLockCounter? syncObject;
            lock (_sharedLocks)
            {
                if (!_sharedLocks.TryGetValue(connectionString, out syncObject))
                {
                    _sharedLocks.Add(connectionString, syncObject = new AsyncLockCounter());
                }
                Interlocked.Increment(ref syncObject.ReferenceCount);
            }
            return syncObject;
        }

        internal bool TryRelease(string connectionString)
        {
            lock (_sharedLocks)
            {
                if (_sharedLocks.TryGetValue(connectionString, out var syncObject) && 
                    Interlocked.Decrement(ref syncObject.ReferenceCount) <= 0)
                {
                    _sharedLocks.Remove(connectionString);
                    syncObject.Dispose();
                    return true;
                }
               
            }
            return false;
        }

        #endregion
    }
}
