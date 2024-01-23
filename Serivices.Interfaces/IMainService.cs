using Models;

namespace Services.Interfaces
{
    public interface IMainService
    {
        Task<List<Spot>> GetParkSpotsAsync();
        Task<bool> UpdateIsEmptyParkSpotAsync(Spot spot);
    }
}
