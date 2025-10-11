using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace NetScad.Core.Interfaces
{
    public interface IConnectionManager : IDisposable
    {
        Task<SqliteConnection> GetConnectionAsync();
    }

    public class ConnectionManager : IConnectionManager
    {
        private readonly SqliteConnection _connection;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _isDisposed;

        public ConnectionManager(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            _connection = new SqliteConnection(connectionString);
            _connection.Open();
        }

        public async Task<SqliteConnection> GetConnectionAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(SqliteConnection));
                if (_connection.State != ConnectionState.Open)
                {
                    await _connection.OpenAsync();
                }
                return _connection;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _connection.Close();
                _connection.Dispose();
                _semaphore.Dispose();
                _isDisposed = true;
            }
        }
    }
}
