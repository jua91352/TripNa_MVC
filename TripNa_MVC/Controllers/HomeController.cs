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
using Microsoft.AspNetCore.Authorization;

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
           
        public IActionResult CreateItinerary()
            {
            
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                TempData["alertMessage"] = "Please Login First!!";
                Console.WriteLine(TempData["alertMessage"]);
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }


            var viewModel = new ItineraryViewModel
            {
                Itinerary = new Itinerary(),
                Spot = _context.Spots.ToList(),
                ItineraryDetail = new List<ItineraryDetailViewModel>() // 初始化視圖模型的屬性
            };

            var spot = from o in _context.Spots
                       select o;
            var spotid =from o in _context.Spots
                        select o.SpotId;
            var spotsList = spot.ToList();
            

            // 獲取所有 SpotCity 並去重
            var cities = _context.Spots
                .Select(s => s.SpotCity)
                .Distinct()
                .ToList();

            // 將 cities 傳遞到 View
            ViewBag.Cities = cities;

            
            return View(viewModel);
        }

        //黃浩維的不要動-------------------------------------------------------------------------------------------------

        // 徐庭軒加的---------------------------
       
        [HttpPost]
        public async Task<IActionResult> CreateItinerary([FromBody] ItineraryViewModel dataToSend)
        {
            
            if (!ModelState.IsValid)
            {
                // Log ModelState errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine(error.ErrorMessage);
                    }
                }
                return BadRequest("Invalid itinerary data"); // Or a more specific error message
            }

            try
            {
                if (dataToSend.Itinerary == null)
                {
                    dataToSend.Itinerary = new Itinerary(); // 初始化 Itinerary 對象
                }

                // 確保 dataToSend.ItineraryDetail 不為空
                if (dataToSend.ItineraryDetail == null)
                {
                    dataToSend.ItineraryDetail = new List<ItineraryDetailViewModel>(); // 初始化 ItineraryDetail 列表
                }
                _context.Add(dataToSend.Itinerary);
                await _context.SaveChangesAsync();

                foreach (var detail in dataToSend.ItineraryDetail)
                {
                    var itineraryDetail = new ItineraryDetail
                    {
                        ItineraryId = dataToSend.Itinerary.ItineraryId, // You need to dynamically set this value
                        SpotId = detail.SpotId,
                        ItineraryDate = detail.ItineraryDate,
                        VisitOrder = detail.VisitOrder
                    };

                    _context.ItineraryDetails.Add(itineraryDetail);
                }

                TempData["ItineraryID"] = dataToSend.Itinerary.ItineraryId;
                TempData["DayCount"] = dataToSend.Itinerary.ItineraryName.Substring(2, 1);
                await _context.SaveChangesAsync();
                // Consider returning a success message or redirecting to a confirmation view
                return Ok("Itinerary created successfully!"); // Or a more specific success message
            }
            catch (Exception ex)
            {
                // Log detailed error information
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);

                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] Orderlist newOrder)
        {
            
            try
            {
                if (newOrder == null)
                {
                    newOrder = new Orderlist(); // 初始化 Orderlist 列表
                }
               

                var memberEmail = HttpContext.Session.GetString("memberEmail");
                var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);
                string orderDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
                string orderNumber = DateTime.UtcNow.Date.ToString("yyMMdd") + member.MemberId + TempData["ItineraryID"];
                decimal orderTotalPirce = 0;
                if (TempData["DayCount"].ToString() == "一")
                {
                    orderTotalPirce = 2000M;
                } else if (TempData["DayCount"].ToString() == "二")
                {
                    orderTotalPirce = 3800M;
                } else
                {
                    orderTotalPirce = 5400M;
                }
                if (!ModelState.IsValid)
                {
                    // Log ModelState errors
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            Console.WriteLine(error.ErrorMessage);
                        }
                    }
                    return BadRequest("Invalid itinerary data"); // Or a more specific error message
                }
                if (newOrder != null )
                {
                    // 設置訂單日期和狀態

                    newOrder.MemberId = member.MemberId; // You need to dynamically set this value
                        newOrder.ItineraryId = (int)TempData["ItineraryID"];
                    newOrder.OrderDate = DateTime.Parse(orderDate);
                    newOrder.OrderNumber = int.Parse(orderNumber);
                    newOrder.OrderStatus = "尚未出發";
                    newOrder.OrderMatchStatus = "媒合中";
                    newOrder.OrderTotalPrice = orderTotalPirce;

                    _context.Orderlists.Add(newOrder);
                    _context.SaveChanges();

                    return Ok("Order created successfully");
                }
                else
                {
                    return BadRequest("Invalid order data");
                }
            }
            catch (Exception ex)
            {
                // 錯誤處理
                Console.WriteLine("Exception: " + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public IActionResult TouristGuide(string selectedCity, string gender = null, int? experience = null)
        {
            var guiders = from o in _context.Guiders
                          select o;
            var cityArea = _context.Cityareas.FirstOrDefault(ca => ca.City == selectedCity);

            if (!string.IsNullOrEmpty(selectedCity))
            {
                if (cityArea != null)
                {
                    guiders = guiders.Where(g => g.GuiderArea == cityArea.Area);
                }
            }

            if (!string.IsNullOrEmpty(gender))
            {
                if (gender.ToLower() == "男生")
                {
                    guiders = guiders.Where(g => g.GuiderGender == "M");
                }
                else if (gender.ToLower() == "女生")
                {
                    guiders = guiders.Where(g => g.GuiderGender == "F");
                }
            }

            if (experience.HasValue)
            {
                DateTime cutoffDate = DateTime.Now.AddYears(-experience.Value);
                if (experience == 0)
                {
                    guiders = guiders.Where(g => g.GuiderStartDate >= DateTime.Now.AddMonths(-6)); // 新手導遊的條件，這裡假設少於6個月
                }
                else if (experience == 5)
                {
                    guiders = guiders.Where(g => g.GuiderStartDate <= DateTime.Now.AddYears(-5)); // 5年以上的條件
                }
                else
                {
                    guiders = guiders.Where(g => g.GuiderStartDate <= cutoffDate && g.GuiderStartDate > cutoffDate.AddYears(-1)); // 指定年資
                }
            }

            var guideList = guiders.ToList();
            ViewBag.GuiderCount = guideList.Count;

            return View(guideList);
        }

        // 徐庭軒加的---------------------------

        public IActionResult Spot(string memberEmail)
        {

            var query = from o in _context.Spots
                        select o;

            var spotsList = query.ToList();


            return View(spotsList);

        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.DictionaryKeyPolicy = null;
            });

            services.AddControllersWithViews();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Login"; // 登入頁面路徑
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AuthenticatedUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            });
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

            return View(spotsList) ;
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
