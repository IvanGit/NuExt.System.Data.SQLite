# NuExt.System.Data.SQLite

This NuGet package provides extensions for the SQLite database engine, offering a robust framework for database context management, transaction handling, and data access operations. The package is designed to facilitate seamless integration with existing database code, ensuring consistency and thread-safe concurrent access.

### Commonly Used Types:
- **`System.Data.SQLite.SQLiteDalBase`**: Base class for SQLite-specific Data Access Layer (DAL) operations.
- **`System.Data.SQLite.SQLiteDbConnection`**: Wrapper for SQLite connections enabling thread-safe concurrent access.
- **`System.Data.SQLite.SQLiteDbContext`**: SQLite database context providing connection and transaction management.
- **`System.Data.SQLite.SQLiteDbConverter`**: Base class for managing and applying updates to the SQLite database schema.
- **`System.Data.SQLite.SQLiteDbTransaction`**: Provides methods for committing and rolling back SQLite transactions.

### Samples
For examples of how to use these classes and work with them, see the [samples](samples). These samples provide practical guidance on implementing the library's features in real-world scenarios.

### License
Licensed under the MIT License. See the LICENSE file for details.