using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TripNa_MVC.Models;

namespace TripNa_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TripNaContext _context;

        public HomeController(ILogger<HomeController> logger, TripNaContext context)
        {
            _logger = logger;
            _context = context;

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }






        public async Task<IActionResult> Spot(string sortOrder,
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

            var students = from s in _context.Spots
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {

                students = students.Where(s => s.SpotName.Contains(searchString)
                                    || s.SpotCity.Contains(searchString));
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
            int totalCount = (from t in _context.Spots
                              select t).Count();
            ViewData["TotalCount"] = totalCount;

            ViewData["CurrentFilter"] = searchString;
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.SpotName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.SpotCity);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.SpotCity);
                    break;
                default:
                    students = students.OrderBy(s => s.SpotName);
                    break;
            }

            var spots = _context.Spots.AsQueryable();
            var cityarea = _context.Cityareas.AsQueryable();

            // 根據選擇的 FoodType 進行篩選
            if (!string.IsNullOrEmpty(selectfood))
            {
                spots = spots.Where(r => r.SpotCity == selectfood);
            }
            else if (!string.IsNullOrEmpty(searchString))
            {
                pageNumber = 1;
                spots = spots.Where(r => r.SpotName.Contains(searchString) || r.SpotIntro.Contains(searchString));
            }
            else
            {
                searchString = currentFilter;
            };

            var cityareas = _context.Cityareas.Select(r => r.Area).Distinct().ToList();
            ViewBag.Area = cityareas;

            var spotcity = _context.Spots.Select(r => r.SpotCity).Distinct().ToList();
            ViewBag.SpotCity = spotcity;

            var spotname = _context.Spots.Select(r => r.SpotName).Distinct().ToList();
            ViewBag.SpotName = spotname;



            int pageSize = 32;
            return View(await PaginatedList2<Spot>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        [HttpPost]
        public IActionResult GetFilteredSpots(List<string> spotcity)
        {
            // Start with all spots and city areas
            IQueryable<Spot> filteredspots = _context.Spots;


            if (spotcity != null && spotcity.Count > 0)
            {
                filteredspots = filteredspots.Where(r => spotcity.Contains(r.SpotCity));
            }

            // Get the counts of the filtered results
            int filteredCount = filteredspots.Count();
            ViewData["FilteredCount"] = filteredCount;
            //-------------------------------------test------------------------------------------------
            //var memberEmail = HttpContext.Session.GetString("memberEmail");

            //if (string.IsNullOrEmpty(memberEmail))
            //{
            //    return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            //}
            //var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            //if (member == null)
            //{
            //    return NotFound();
            //}
            //-------------------------------------------------------------------------------------------------

            return PartialView("_spotlist", filteredspots.ToList());
        }


        [HttpPost]
        public IActionResult ToggleFavorite(int spotId, bool isFavorite)
        {
            // 獲取當前用戶的 ID
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return Json(new { success = false, message = "您需要先登錄" });
                //return Redirect("/Members/MemberCenter");
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);
            if (member == null)
            {
                Console.WriteLine($"未找到電子郵件為 {memberEmail} 的用戶");
                return NotFound();
            }

            // 檢查是否已經收藏過該景點
            var favoriteSpot = _context.FavoriteSpots.FirstOrDefault(fs => fs.SpotId == spotId && fs.MemberId == member.MemberId);

            //if (isFavorite)
            //{
            //    // 取消收藏
            //    if (favoriteSpot != null)
            //    {
            //        _context.FavoriteSpots.Remove(favoriteSpot);
            //        _context.SaveChanges();
            //        return Json(new { success = true, message = "取消收藏成功" });
            //    }
            //    else
            //    {
            //        return Json(new { success = false, message = "該景點未被收藏" });
            //    }
            //}
            //else
            //{
            // 添加收藏
            if (favoriteSpot == null)
            {
                var newFavoriteSpot = new FavoriteSpot
                {
                    SpotId = spotId,
                    MemberId = member.MemberId
                };
                _context.FavoriteSpots.Add(newFavoriteSpot);
                _context.SaveChanges();

                return Json(new { success = true, message = "收藏成功" });
            }
            else
            {
                return Json(new { success = false, message = "該景點已被收藏" });
            }
            //}
        }












        public IActionResult createSpot(string memberEmail)
        {

            var query = from o in _context.Spots
                        select o;

            var spotsList = query.ToList();


            return View(spotsList);

        }


        [HttpPost]
        public IActionResult Spot(string spotId, string spotName, string spotCity, string spotIntro, string memberId)
        {
            var query = from o in _context.Spots
                        select o;

            var spotsList = query.ToList();

            if (string.IsNullOrEmpty(memberId))
            {

                ViewData["Message"] = "請先登入";
                //return Redirect("/home/Login");
                return View(spotsList);
            }

            return View(spotsList); ;
        }



        //public IActionResult Spot(string city)
        //{
        //    var spots = _context.Spots;
        //    return View();
        //}


        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(string memberEmail, string memberPassword)
        {
            var query = from o in _context.Members
                        where o.MemberEmail == memberEmail & o.MemberPassword == memberPassword
                        select o;

            var result = query.FirstOrDefault();


            if (result == null)
            {
                ViewData["Message"] = "帳號或密碼輸入錯誤";
                return View(); // 登入失敗導回頁面
            }
            //return Ok(result);
            var memberName = result.MemberName;
            var memberId = result.MemberId.ToString();

            HttpContext.Session.SetString("memberName", memberName);
            HttpContext.Session.SetString("memberEmail", memberEmail);
            HttpContext.Session.SetString("memberId", memberId);

            return Redirect("/Members/MemberCenter");

        }


        public IActionResult Logout()
        {

            HttpContext.Session.Remove("memberEmail");
            return Redirect("/Home/Index");
        }





        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("MemberId,MemberEmail,MemberName,MemberBirthDate,MemberPhone,MemberPassword,MemberCoupon,MemberOrderList,MemberFavorites")] Member member)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists in the database
                var existingMember = await _context.Members.FirstOrDefaultAsync(m => m.MemberEmail == member.MemberEmail);

                if (existingMember != null)
                {
                    ViewData["Message"] = "此帳號已存在";
                    return View();
                }

                _context.Add(member);
                await _context.SaveChangesAsync();
                ViewData["Success"] = "註冊成功！";
                return View();
            }

            return View("home");
        }






        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
