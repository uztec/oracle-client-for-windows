using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace OracleClient.Models
{
    public class DatabaseManager : IDisposable
    {
        private OracleConnection? _connection;
        private bool _disposed = false;

        public bool IsConnected => _connection?.State == ConnectionState.Open;

        public string ConnectionString { get; private set; } = string.Empty;

        public async Task<bool> ConnectAsync(string connectionString)
        {
            try
            {
                if (_connection != null)
                {
                    await DisconnectAsync();
                }

                _connection = new OracleConnection(connectionString);
                await _connection.OpenAsync();
                ConnectionString = connectionString;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to connect to database: {ex.Message}", ex);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    await _connection.CloseAsync();
                }
                _connection.Dispose();
                _connection = null;
            }
        }

        public async Task<DataTable> ExecuteQueryAsync(string sql)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Database connection is not open.");
            }

            var dataTable = new DataTable();
            
            try
            {
                using var command = new OracleCommand(sql, _connection);
                using var adapter = new OracleDataAdapter(command);
                await Task.Run(() => adapter.Fill(dataTable));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing query: {ex.Message}", ex);
            }

            return dataTable;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Database connection is not open.");
            }

            try
            {
                using var command = new OracleCommand(sql, _connection);
                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing command: {ex.Message}", ex);
            }
        }

        public async Task<object?> ExecuteScalarAsync(string sql)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Database connection is not open.");
            }

            try
            {
                using var command = new OracleCommand(sql, _connection);
                return await command.ExecuteScalarAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing scalar query: {ex.Message}", ex);
            }
        }

        public async Task<List<string>> GetTableNamesAsync()
        {
            const string sql = "SELECT table_name FROM user_tables ORDER BY table_name";
            var dataTable = await ExecuteQueryAsync(sql);
            return dataTable.Rows.Cast<DataRow>()
                .Select(row => row["table_name"].ToString() ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }

        public async Task<List<string>> GetColumnNamesAsync(string tableName)
        {
            var sql = $"SELECT column_name FROM user_tab_columns WHERE table_name = '{tableName.ToUpper()}' ORDER BY column_id";
            var dataTable = await ExecuteQueryAsync(sql);
            return dataTable.Rows.Cast<DataRow>()
                .Select(row => row["column_name"].ToString() ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
