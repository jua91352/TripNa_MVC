using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TripNa_MVC.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripNa_MVC;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;


namespace TripNa_MVC.Controllers
{
    public class TravelController : Controller
    {



        private readonly TripNaContext _context;

        public TravelController(TripNaContext context)
        {
            _context = context;

        }

        public IActionResult weather()
        {
			var items = _context.Restaurant.ToList();
			return View(items);
        }

      


        public async Task<IActionResult> Index(
          string sortOrder,
          string currentFilter,
          string searchString,
          string selectfood,
          string filterfood,

          int? pageNumber)
        {




            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

            ViewData["SERCH"] = selectfood;
            ViewData["SERCHreign"] = filterfood;

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }



            var students = from s in _context.Restaurant
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {

                students = students.Where(s => s.FoodType.Contains(searchString)
                                    || s.RestaurantName.Contains(searchString));
            }
            // 檢查搜索結果是否為空
            var searchResult = students.ToList();
            if (searchResult.Count == 0)
            {
                // 如果搜索結果為空,將"找不到"添加到 ViewBag
                ViewBag.NoResult = "沒有相關資訊";
            }

			// 計算搜索結果的總數
			int searchCount = searchResult.Count;
			ViewData["SearchCount"] = searchCount;

			// 計算總數
			int totalCount = (from t in _context.Restaurant
							  select t).Count();
			ViewData["TotalCount"] = totalCount;



			ViewData["CurrentFilter"] = searchString;
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.CityOrderIndex);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.FoodType);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.FoodType);
                    break;
                default:
                    students = students.OrderBy(s => s.CityOrderIndex);
                    break;
            }

            var restaurants = _context.Restaurant.AsQueryable();

            // 根據選擇的 FoodType 進行篩選
            if (!string.IsNullOrEmpty(selectfood))
            {
                restaurants = restaurants.Where(r => r.FoodType == selectfood);
            }
            else if (!string.IsNullOrEmpty(searchString))
            {
                pageNumber = 1;
                restaurants = restaurants.Where(r => r.RestaurantName.Contains(searchString) || r.Description.Contains(searchString));
            }
            else
            {
                searchString = currentFilter;
            };

            switch (selectfood)
            {
                case "特色小吃":
                    students = students.Where(s => (s.FoodType == "特色小吃"));

                    break;
                case "米其林推薦":
                    students = students.Where(s => (s.FoodType == "米其林推薦"));

                    break;

                case "必比登推薦":
                    students = students.Where(s => (s.FoodType == "必比登推薦"));

                    break;
                case "伴手禮":
                    students = students.Where(s => (s.FoodType == "伴手禮"));

                    break;

            }




            var regions = _context.Restaurant.Select(r => r.LocationName).Distinct().ToList();
            ViewBag.Regions = regions;

            var foodtype = _context.Restaurant.Select(r => r.FoodType).Distinct().ToList();
            ViewBag.FoodType = foodtype;

            int pageSize = 12;
            return View(await PaginatedList<Restaurant>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

		//public IActionResult GetFilteredRestaurants(List<string> regions, List<string> foodType)
		//{
		//    return GetFilteredRestaurants(regions, foodType, _context);
		//}

		//[HttpPost]
		//public IActionResult GetFilteredRestaurants(List<string> regions)
		//{
		//    IQueryable<Restaurants> filteredRestaurants;

		//    if (regions == null || regions.Count==0)
		//    {
		//        // 如果 regions 為空,則返回所有餐廳
		//        filteredRestaurants = _context.Restaurants;
		//    }
		//    else
		//    {
		//        // 根據選擇的地區篩選餐廳
		//        filteredRestaurants = _context.Restaurants.Where(r => regions.Contains(r.Region));
		//    }

		//    return PartialView("_regionlist", filteredRestaurants.ToList());
		//}
		[HttpPost]
		public IActionResult GetFilteredRestaurants(List<string> regions, List<string> foodType)
		{
			IQueryable<Restaurant> filteredRestaurants = _context.Restaurant;


			if ((foodType.Count > 0) && (regions.Count == 0))
			{
				// 根據選擇的食物類型篩選餐廳
				filteredRestaurants = filteredRestaurants.Where(r => foodType.Contains(r.FoodType));

			}
			else if ((regions.Count > 0) && (foodType.Count == 0))
			{
				// 根據選擇的地區進一步篩選餐廳
				filteredRestaurants = filteredRestaurants.Where(r => regions.Contains(r.LocationName));
			}
			else if ((regions.Count > 0) && (foodType.Count > 0))
			{
				filteredRestaurants = filteredRestaurants.Where(r => regions.Contains(r.LocationName) && foodType.Contains(r.FoodType));
			}

			int filteredCount = filteredRestaurants.Count();
			ViewData["FilteredCount"] = filteredCount;

			return PartialView("_regionlist", filteredRestaurants.ToList());
		}

		//[HttpPost]
		//public IActionResult GetFilteredRestaurants(List<string> regions, List<string> foodType)
		//{
		//	IQueryable<Restaurant> filteredRestaurants = _context.Restaurants;

		//	if (regions.Count > 0 && foodType.Count > 0)
		//	{
		//		filteredRestaurants = filteredRestaurants
		//			.Where(r => regions.Contains(r.LocationName) && foodType.Contains(r.FoodType))
		//			.OrderByDescending(r => regions.IndexOf(r.LocationName))
		//			.ThenByDescending(r => foodType.IndexOf(r.FoodType));
		//	}
		//	else if (regions.Count > 0)
		//	{
		//		filteredRestaurants = filteredRestaurants
		//			.Where(r => regions.Contains(r.LocationName))
		//			.OrderByDescending(r => regions.IndexOf(r.LocationName));
		//	}
		//	else if (foodType.Count > 0)
		//	{
		//		filteredRestaurants = filteredRestaurants
		//			.Where(r => foodType.Contains(r.FoodType))
		//			.OrderByDescending(r => foodType.IndexOf(r.FoodType));
		//	}

		//	int filteredCount = filteredRestaurants.Count();
		//	ViewData["FilteredCount"] = filteredCount;

		//	return PartialView("_regionlist", filteredRestaurants.ToList());
		//}

		//public async Task<IActionResult> spot(
		//string sortOrder,
		//string currentFilter,
		//string searchString,
		//string selectcity,
		//string filterfood,

		//int? pageNumber)
		// {




		//     ViewData["CurrentSort"] = sortOrder;
		//     ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
		//     ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";

		//     ViewData["SERCH"] = selectcity;
		//     ViewData["SERCHreign"] = filterfood;

		//     if (searchString != null)
		//     {
		//         pageNumber = 1;
		//     }
		//     else
		//     {
		//         searchString = currentFilter;
		//     }



		//     var students = from s in _context.Spots
		//                    select s;
		//     if (!String.IsNullOrEmpty(searchString))
		//     {

		//         students = students.Where(s => s.SpotCity.Contains(searchString)
		//                             || s.SpotName.Contains(searchString));
		//     }
		//     ViewData["CurrentFilter"] = searchString;
		//     switch (sortOrder)
		//     {
		//         case "name_desc":
		//             students = students.OrderByDescending(s => s.SpotCity);
		//             break;
		//         case "Date":
		//             students = students.OrderBy(s => s.SpotName);
		//             break;
		//         case "date_desc":
		//             students = students.OrderByDescending(s => s.SpotName);
		//             break;
		//         default:
		//             students = students.OrderBy(s => s.SpotCity);
		//             break;
		//     }

		//     var spots = _context.Spots.AsQueryable();

		//     // 根據選擇的 FoodType 進行篩選
		//     if (!string.IsNullOrEmpty(selectcity))
		//     {
		//         spots = spots.Where(r => r.SpotCity == selectcity);
		//     }
		//     else if (!string.IsNullOrEmpty(searchString))
		//     {
		//         pageNumber = 1;
		//         spots = spots.Where(r => r.SpotCity.Contains(searchString) || r.SpotBrief.Contains(searchString));
		//     }
		//     else
		//     {
		//         searchString = currentFilter;
		//     };



		//     var spotcity = _context.Spots.Select(r => r.SpotCity).Distinct().ToList();
		//     ViewBag.SpotCity = spots;

		//     var spotname = _context.Spots.Select(r => r.SpotName).Distinct().ToList();
		//     ViewBag.SpotName = spotname;

		//     int pageSize = 10;
		//     return View(await PaginatedList<Spot>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
		// }
		// [HttpPost]
		// public IActionResult GetFilteredspots(List<string> spotcity,  TripNaContext _context)
		// {
		//     IQueryable<Spot> filteredspots = (IQueryable<Spot>)_context.Spots;


		//     if (spotcity != null && spotcity.Count > 0)
		//     {
		//         // 根據選擇的食物類型篩選餐廳
		//         filteredspots = filteredspots.Where(r => spotcity.Contains(r.SpotCity));
		//     }


		//     //if (regions != null && regions.Count > 0)
		//     //{
		//     //    // 根據選擇的地區進一步篩選餐廳
		//     //    filteredRestaurants = filteredRestaurants.Where(r => regions.Contains(r.Region));
		//     //}
		//     return PartialView("_spotlist", filteredspots.ToList());
		// }


	}
}
