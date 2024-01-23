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

        public async Task<List<Spot>> GetParkSpotsAsync()
        {
            var commandText = "SELECT * FROM ParkSpots";
            NpgsqlCommand cmd = new NpgsqlCommand(commandText, _connection);

            return ToParkSpotList(cmd.ExecuteReader());
        }

        public async Task<bool> UpdateIsEmptyParkSpotAsync(Spot spot)
        {
            var commandText = $@"UPDATE ParkSpots
                SET IsEmpty = @IsEmpty
                WHERE id = @id";

            await using (var cmd = new NpgsqlCommand(commandText, _connection))
            {
                cmd.Parameters.AddWithValue("id", spot.Id);
                cmd.Parameters.AddWithValue("IsEmpty", spot.IsEmpty);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
        }

        private List<Spot> ToParkSpotList(NpgsqlDataReader reader)
        {
            var list = new List<Spot>();

            while (reader.Read())
            {
                var parkSpot = new Spot
                {
                    Id = reader.GetInt32("id"),
                    IsEmpty = reader.GetBoolean("isempty")
                };

                list.Add(parkSpot);
            }

            return list;
        }
    }
}
