using EmptyParking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services.Interfaces;

namespace EmptyParking.Controllers
{
    public class ParkSpotController : Controller
    {
        private IMainService _mainService;
        public ParkSpotController(IMainService mainService)
        {
            _mainService = mainService;
        }

        public async Task<IActionResult> Index()
        {
           /* var parkSpotList = new List<ParkSpotModel>();
            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost; Port=5432; Database=master; User Id=postgres; Password=1111;");
            conn.Open();

            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT * FROM ParkSpots";

            NpgsqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var parkSpot = new ParkSpotModel
                {
                    Id = reader.GetInt32("id"),
                    IsEmpty = reader.GetBoolean("isempty")
                };

                parkSpotList.Add(parkSpot);
            }*/

            var parkSpots = await _mainService.GetParkSpotsAsync();

            return View(MapToViewModel(parkSpots));
        }

        private List<ParkSpotViewModel> MapToViewModel(List<Spot> spots)
        {
            var parkSpotsViewModel = new List<ParkSpotViewModel>();

            foreach(var spot in spots)
            {
                var parkSpot = new ParkSpotViewModel
                {
                    Id = spot.Id,
                    IsEmpty = spot.IsEmpty
                };

                parkSpotsViewModel.Add(parkSpot);
            }

            return parkSpotsViewModel;
        }
    }
}
