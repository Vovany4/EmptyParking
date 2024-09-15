using Models;
using Npgsql;

namespace Repositories.Interfaces
{
    public interface IMainRepository
    {
        Task<bool> BatchUpdateIsEmptyParkSpotAsync(List<Spot> spots, NpgsqlConnection conn);
        NpgsqlConnection CreateConnection();
        Task<Spot?> GetParkSpotAsync(int id, NpgsqlConnection conn);
        Task<List<Spot>> GetParkSpotsAsync(NpgsqlConnection conn);
        Task<List<Spot>> GetParkSpotsAsync(List<int> ids, NpgsqlConnection conn);
        Task<bool> UpdateIsEmptyParkSpotAsync(Spot spot, NpgsqlConnection conn);
    }
}
