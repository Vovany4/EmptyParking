using Models;
using Npgsql;
using Repositories.Interfaces;
using System.Data;

namespace Repositories
{
    public class MainRepository : BaseRepository, IMainRepository
    {
        public MainRepository()
        {
        }

        public async Task<List<Spot>> GetParkSpotsAsync(NpgsqlConnection conn)
        {
            var commandText = "SELECT * FROM ParkSpots";

            await using NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            return await ToParkSpotListAsync(reader);
        }

        public async Task<List<Spot>> GetParkSpotsAsync(List<int> ids, NpgsqlConnection conn)
        {
            string inClause = string.Join(",", ids.Select(x => x));

            var commandText = $"SELECT * FROM ParkSpots WHERE id IN ({inClause})";

            await using NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            return await ToParkSpotListAsync(reader);
        }

        public async Task<Spot?> GetParkSpotAsync(int id, NpgsqlConnection conn)
        {
            var commandText = $"SELECT * FROM ParkSpots WHERE id = {id}";

            await using NpgsqlCommand cmd = new NpgsqlCommand(commandText, conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            return await ToParkSpotAsync(reader);
        }

        public async Task<bool> UpdateIsEmptyParkSpotAsync(Spot spot, NpgsqlConnection conn)
        {
            var commandText = $@"UPDATE ParkSpots
                SET IsEmpty = @IsEmpty
                WHERE id = @id";

            await using var cmd = new NpgsqlCommand(commandText, conn);
            cmd.Parameters.AddWithValue("id", spot.Id);
            cmd.Parameters.AddWithValue("IsEmpty", spot.IsEmpty);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> BatchUpdateIsEmptyParkSpotAsync(List<Spot> spots, NpgsqlConnection conn)
        {
            var idValuePairsToUpdate = string.Join(",", spots.Select(spot => $"({spot.Id}, '{spot.IsEmpty}')"));

            var commandText = @$"
                CREATE TEMPORARY TABLE TempTable (
                    id INT,
                    new_IsEmpty BOOLEAN
                );

                INSERT INTO TempTable (id, new_IsEmpty)
                VALUES {idValuePairsToUpdate};

                -- Batch update operation using data from the temporary table
                UPDATE ParkSpots t
                SET IsEmpty = temp.new_IsEmpty
                FROM TempTable temp
                WHERE t.id = temp.id;

                -- Remove the temporary table (optional, it will automatically drop at the end of the session)
                DROP TABLE IF EXISTS TempTable;
            ";

            await using var cmd = new NpgsqlCommand(commandText, conn);

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private async Task<List<Spot>> ToParkSpotListAsync(NpgsqlDataReader reader)
        {
            var list = new List<Spot>();

            while (await reader.ReadAsync())
            {
                var parkSpot = new Spot
                {
                    Id = reader.GetInt32("id"),
                    IsEmpty = reader.GetBoolean("isempty"),
                    Latitude = reader.GetDouble("latitude"),
                    Longitude = reader.GetDouble("longitude"),
                };

                list.Add(parkSpot);
            }

            reader.Close();

            return list;
        }

        private async Task<Spot?> ToParkSpotAsync(NpgsqlDataReader reader)
        {
            Spot? parkSpot = null;

            if (await reader.ReadAsync())
            {
                parkSpot = new Spot
                {
                    Id = reader.GetInt32("id"),
                    IsEmpty = reader.GetBoolean("isempty"),
                    Latitude = reader.GetDouble("latitude"),
                    Longitude = reader.GetDouble("longitude"),
                };
            }

            reader.Close();

            return parkSpot;
        }

    }
}
