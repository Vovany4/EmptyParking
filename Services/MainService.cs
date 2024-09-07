using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class MainService : IMainService
	{
		private readonly IMainRepository repositories;
        public MainService(IMainRepository repositories)
        {
            this.repositories = repositories;
        }

        public async Task<List<Spot>> GetParkSpotsAsync()
		{
			using (var conn = repositories.CreateConnection())
			{
				return await repositories.GetParkSpotsAsync(conn);
			}
		}

        public async Task<Spot?> GetParkSpotAsync(int id)
        {
            using (var conn = repositories.CreateConnection())
            {
                return await repositories.GetParkSpotAsync(id, conn);
            }
        }

        public async Task<bool> UpdateIsEmptyParkSpotAsync(Spot spot)
        {
            using (var conn = repositories.CreateConnection())
            {
                return await repositories.UpdateIsEmptyParkSpotAsync(spot, conn);
            }
        }
    }
}
