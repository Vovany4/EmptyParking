using Npgsql;

namespace Repositories
{
    public class BaseRepository
    {
        private const string host = "localhost";
        private const string port = "5433";
        private const string database = "master";
        private const string username = "postgis";
        private const string password = "1111";

        private const string CONNECTION_STRING = $"Host={host};Port={port};Username={username};Password={password};Database={database}";


        public BaseRepository()
        {
        }

        public NpgsqlConnection CreateConnection()
        {
            var _connection = new NpgsqlConnection(CONNECTION_STRING);
            _connection.Open();

            return _connection;
        }
    }
}
