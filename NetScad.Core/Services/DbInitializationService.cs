using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetScad.Core.Services
{
    // Background service for embedded DB extraction (runs on non-UI thread at startup)
    public class DbInitializationService : BackgroundService
    {
        private readonly SqliteConnection _connection;
        private readonly ILogger<DbInitializationService> _logger;

        public DbInitializationService(SqliteConnection connection, ILogger<DbInitializationService> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Clear any open pools
            SqliteConnection.ClearPool(_connection);
            GC.Collect();
            _logger.LogInformation("DB all pools cleared.");

            await _connection.OpenAsync();  // Open connection on background thread

            if (_connection.State == System.Data.ConnectionState.Open)
            {
                _logger.LogInformation("SQLite connection opened.");
            }
            else
            {
                _logger.LogError("Failed to open SQLite connection.");
            }
        }
    }
}
