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


        //���E�������n��-------------------------------------------------------------------------------------------------
            public IActionResult Privacy()
            {
            // ����Ҧ� Spot
            var spot = from o in _context.Spots
                       select o;
            var spotid =from o in _context.Spots
                        select o.SpotId;
            var spotsList = spot.ToList();


            // ����Ҧ� Itinerary
            var itinerary = from o in _context.Itineraries
                            select o;
            var itineraryList = itinerary.ToList();


            // ����Ҧ�����
            var cities = _context.Spots
                .Select(s => s.SpotCity)
                .Distinct()
                .ToList();

            // �N cities �ǻ��� View
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
                if (gender.ToLower() == "�k��")
                {
                    Guide = Guide.Where(g => g.GuiderGender == "M");
                }
                else if (gender.ToLower() == "�k��")
                {
                    Guide = Guide.Where(g => g.GuiderGender == "F");
                }
            }

            if (experience.HasValue)
            {
                DateTime cutoffDate = DateTime.Now.AddYears(-experience.Value);
                if (experience == 0)
                {
                    Guide = Guide.Where(g => g.GuiderStartDate >= DateTime.Now.AddMonths(-6)); // �s��ɹC������A�o�̰��]�֩�6�Ӥ�
                }
                else if (experience == 5)
                {
                    Guide = Guide.Where(g => g.GuiderStartDate <= DateTime.Now.AddYears(-5)); // 5�~�H�W������
                }
                else
                {
                    Guide = Guide.Where(g => g.GuiderStartDate <= cutoffDate && g.GuiderStartDate > cutoffDate.AddYears(-1)); // ���w�~��
                }
            }

            

            var GuideList = Guide.ToList();
            ViewBag.GuiderCount = GuideList.Count;

            return View(GuideList);
        }


        //���E�������n��-------------------------------------------------------------------------------------------------
  
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

                ViewData["Message"] = "�Х��n�J";
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
