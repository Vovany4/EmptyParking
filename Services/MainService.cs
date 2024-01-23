using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class MainService : IMainService
	{
		private IMainRepository Repositories;
		public MainService(IMainRepository repositories)
		{
			Repositories = repositories;
		}

		public async Task<List<Spot>> GetParkSpotsAsync()
		{
			using (Repositories.CreateConnection())
			{
				return await Repositories.GetParkSpotsAsync();
			}
		}

        public async Task<bool> UpdateIsEmptyParkSpotAsync(Spot spot)
        {
            using (Repositories.CreateConnection())
            {
                return await Repositories.UpdateIsEmptyParkSpotAsync(spot);
            }
        }
    }
}
