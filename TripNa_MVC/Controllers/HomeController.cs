using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TripNa_MVC.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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


        //黃浩維的不要動-------------------------------------------------------------------------------------------------
            public IActionResult Privacy()
            {
            // 獲取所有 Spot
            var spot = from o in _context.Spots
                       select o;
            var spotid =from o in _context.Spots
                        select o.SpotId;
            var spotsList = spot.ToList();


            // 獲取所有 Itinerary
            var itinerary = from o in _context.Itineraries
                            select o;
            var itineraryList = itinerary.ToList();


            // 獲取所有縣市
            var cities = _context.Spots
                .Select(s => s.SpotCity)
                .Distinct()
                .ToList();

            // 將 cities 傳遞到 View
            ViewBag.Cities = cities;

            return View(spotsList);
            }

        [HttpGet]
        public IActionResult TouristGuide(string gender = null, int? experience = null, string location = null)
        {
            var Guide = from o in _context.Guiders
                        select o;

            if (!string.IsNullOrEmpty(gender))
            {
                if (gender.ToLower() == "男生")
                {
                    Guide = Guide.Where(g => g.GuiderGender == "M");
                }
                else if (gender.ToLower() == "女生")
                {
                    Guide = Guide.Where(g => g.GuiderGender == "F");
                }
            }

            if (experience.HasValue)
            {
                DateTime cutoffDate = DateTime.Now.AddYears(-experience.Value);
                if (experience == 0)
                {
                    Guide = Guide.Where(g => g.GuiderStartDate >= DateTime.Now.AddMonths(-6)); // 新手導遊的條件，這裡假設少於6個月
                }
                else if (experience == 5)
                {
                    Guide = Guide.Where(g => g.GuiderStartDate <= DateTime.Now.AddYears(-5)); // 5年以上的條件
                }
                else
                {
                    Guide = Guide.Where(g => g.GuiderStartDate <= cutoffDate && g.GuiderStartDate > cutoffDate.AddYears(-1)); // 指定年資
                }
            }

            

            var GuideList = Guide.ToList();
            ViewBag.GuiderCount = GuideList.Count;

            return View(GuideList);
        }


        //黃浩維的不要動-------------------------------------------------------------------------------------------------
  
        public IActionResult Spot(string memberEmail)
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
                var existingMember = await _context.Members.FirstOrDefaultAsync(m => m.MemberEmail == member.MemberPassword);

                if (existingMember != null)
                {
                    ViewData["Message"] = "此帳號已存在";
                    return View();
                }

                _context.Add(member);
                await _context.SaveChangesAsync();
                return Redirect("/home/Login");
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
