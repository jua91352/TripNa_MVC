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

         
        //���E�������n��-------------------------------------------------------------------------------------------------
           
        public IActionResult CreateItinerary()
            {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                TempData["alertMessage"] = "Please Login First!!";
                Console.WriteLine(TempData["alertMessage"]);
                return RedirectToAction("Login", "Home"); // �p�G�|�ܤ��S���Τ�H���A���w�V��n������
            }


            var viewModel = new ItineraryViewModel
            {
                Itinerary = new Itinerary(),
                Spot = _context.Spots.ToList(),
                ItineraryDetail = new List<ItineraryDetailViewModel>() // ��l�Ƶ��ϼҫ����ݩ�
            };

            var spot = from o in _context.Spots
                       select o;
            var spotid =from o in _context.Spots
                        select o.SpotId;
            var spotsList = spot.ToList();
            

            // ����Ҧ� SpotCity �åh��
            var cities = _context.Spots
                .Select(s => s.SpotCity)
                .Distinct()
                .ToList();

            // �N cities �ǻ��� View
            ViewBag.Cities = cities;

            
            return View(viewModel);
        }

        //���E�������n��-------------------------------------------------------------------------------------------------

        // �}�x�a�[��---------------------------
        //, [Bind("ItineraryId,ItineraryName,ItineraryStartDate,ItineraryPeopleNo")] Itinerary itinerary,
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
                    dataToSend.Itinerary = new Itinerary(); // ��l�� Itinerary ��H
                }

                // �T�O dataToSend.ItineraryDetail ������
                if (dataToSend.ItineraryDetail == null)
                {
                    dataToSend.ItineraryDetail = new List<ItineraryDetailViewModel>(); // ��l�� ItineraryDetail �C��
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





        // �}�x�a�[��---------------------------

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
                    options.LoginPath = "/Home/Login"; // �n�J�������|
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

                ViewData["Message"] = "�Х��n�J";
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
                ViewData["Message"] = "�b���αK�X��J���~";
                return View(); // �n�J���Ѿɦ^����
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
                    ViewData["Message"] = "���b���w�s�b";
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
