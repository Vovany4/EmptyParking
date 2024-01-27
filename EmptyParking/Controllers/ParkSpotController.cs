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
            var parkSpots = await _mainService.GetParkSpotsAsync();

            return View(MapToViewModel(parkSpots));
        }

        public async Task<IActionResult> Map()
        {
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
                    IsEmpty = spot.IsEmpty,
                    Longitude = spot.Longitude,
                    Latitude = spot.Latitude
                };

                parkSpotsViewModel.Add(parkSpot);
            }

            return parkSpotsViewModel;
        }
    }
}
