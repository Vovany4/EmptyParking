using Microsoft.Extensions.Caching.Distributed;
using Models;
using Newtonsoft.Json;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class MainService : IMainService
    {
        private readonly IMainRepository repositories;
        private readonly IDistributedCache cache;
        public MainService(IMainRepository repositories, IDistributedCache cache)
        {
            this.repositories = repositories;
            this.cache = cache;
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
            var key = $"spot-{id}";
            var cachedValue = await cache.GetStringAsync(key);

            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonConvert.DeserializeObject<Spot?>(cachedValue);
            }

            var valueFromDb = default(Spot);
            using (var conn = repositories.CreateConnection())
            {
                valueFromDb = await repositories.GetParkSpotAsync(id, conn);
            }

            if (valueFromDb == null)
            {
                return valueFromDb;
            }

            await cache.SetStringAsync(
                key,
                JsonConvert.SerializeObject(valueFromDb),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) // Set cache expiration
                });

            return valueFromDb;
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
