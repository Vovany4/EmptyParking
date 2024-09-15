using Models;

namespace Services.Interfaces
{
    public interface IMainService
    {
        Task<bool> BatchUpdateIsEmptyParkSpotAsync(List<Spot> spots);
        Task<Spot?> GetParkSpotAsync(int id);
        Task<List<Spot>> GetParkSpotsAsync();
        Task<List<Spot>> GetParkSpotsAsync(List<int> ids);
        Task<bool> UpdateIsEmptyParkSpotAsync(Spot spot);
    }
}
