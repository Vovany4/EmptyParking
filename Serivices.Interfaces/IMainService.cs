using Models;

namespace Services.Interfaces
{
    public interface IMainService
    {
        Task<Spot> GetParkSpotAsync(int id);
        Task<List<Spot>> GetParkSpotsAsync();
        Task<bool> UpdateIsEmptyParkSpotAsync(Spot spot);
    }
}
