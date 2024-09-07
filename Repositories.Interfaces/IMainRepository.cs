using Models;
using Npgsql;

namespace Repositories.Interfaces
{
    public interface IMainRepository
    {
        NpgsqlConnection CreateConnection();
        Task<Spot?> GetParkSpotAsync(int id, NpgsqlConnection conn);
        Task<List<Spot>> GetParkSpotsAsync(NpgsqlConnection conn);
        Task<bool> UpdateIsEmptyParkSpotAsync(Spot spot, NpgsqlConnection conn);
    }
}
