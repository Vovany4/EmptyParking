using Models;
using Npgsql;

namespace Repositories.Interfaces
{
    public interface IMainRepository
    {
        NpgsqlConnection CreateConnection();
        Task<List<Spot>> GetParkSpotsAsync();
        Task<bool> UpdateIsEmptyParkSpotAsync(Spot spot);
    }
}
