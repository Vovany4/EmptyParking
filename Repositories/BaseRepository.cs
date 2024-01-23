using Npgsql;

namespace Repositories
{
    public class BaseRepository
    {
        internal NpgsqlConnection _connection;
        private const string CONNECTION_STRING = "Server=localhost;" +
            "Port=5432;" +
            "Database=master;" +
            "User Id=postgres;" +
            "Password=1111;";

        public BaseRepository()
        {
        }

        public NpgsqlConnection CreateConnection()
        {
            _connection = new NpgsqlConnection(CONNECTION_STRING);
            _connection.Open();

            return _connection;
        }
    }
}
