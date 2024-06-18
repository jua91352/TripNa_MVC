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
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data;




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

    //NA+-----------------------------------------------------------

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

            // �ˬd�j�����G�O�_����
            var searchResult = students.ToList();
            if (searchResult.Count == 0)
            {
                // �p�G�j�����G����,�N"�䤣��"�K�[�� ViewBag
                ViewBag.NoResult = "�S��������T";
            }


            // �p��j�����G���`��
            int searchCount = searchResult.Count;
            ViewData["SearchCount"] = searchCount;

            // �p���`��
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

            // �ھڿ�ܪ� FoodType �i��z��
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
            //    return RedirectToAction("Login", "Home"); // �p�G�|�ܤ��S���Τ�H���A���w�V��n������
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
            // �����e�Τ᪺ ID
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return Json(new { success = false, message = "�z�ݭn���n��" });
                //return Redirect("/Members/MemberCenter");
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);
            if (member == null)
            {
                Console.WriteLine($"�����q�l�l�� {memberEmail} ���Τ�");
                return NotFound();
            }

            // �ˬd�O�_�w�g���ùL�Ӵ��I
            var favoriteSpot = _context.FavoriteSpots.FirstOrDefault(fs => fs.SpotId == spotId && fs.MemberId == member.MemberId);

            //if (isFavorite)
            //{
            //    // ��������
            //    if (favoriteSpot != null)
            //    {
            //        _context.FavoriteSpots.Remove(favoriteSpot);
            //        _context.SaveChanges();
            //        return Json(new { success = true, message = "�������æ��\" });
            //    }
            //    else
            //    {
            //        return Json(new { success = false, message = "�Ӵ��I���Q����" });
            //    }
            //}
            //else
            //{
            // �K�[����
            if (favoriteSpot == null)
            {
                var newFavoriteSpot = new FavoriteSpot
                {
                    SpotId = spotId,
                    MemberId = member.MemberId
                };
                _context.FavoriteSpots.Add(newFavoriteSpot);
                _context.SaveChanges();

                return Json(new { success = true, message = "���æ��\" });
            }
            else
            {
                return Json(new { success = false, message = "�Ӵ��I�w�Q����" });
            }
            //}
        }

        //NA+-----------------------------------------------------------



        //���E�������n��-------------------------------------------------------------------------------------------------

        public IActionResult CreateItinerary()
        {

            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                TempData["alertMessage"] = "�Х��n�J�άO���U�|��!!";
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
            var spotid = from o in _context.Spots
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
                Console.WriteLine("Entering CreateOrder method.");

                if (newOrder == null)
                {
                    newOrder = new Orderlist(); // ��l�� Orderlist �C��
                }


                var memberEmail = HttpContext.Session.GetString("memberEmail");
                var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);
                string orderDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
                string orderNumber = DateTime.UtcNow.Date.ToString("MMdd") + member.MemberId.ToString("00") + TempData["ItineraryID"].ToString();
                decimal orderTotalPirce = 0;

                if (TempData["DayCount"] == null)
                {
                    Console.WriteLine("DayCount is missing in TempData.");
                    return BadRequest("DayCount is missing in TempData.");
                }

                if (TempData["DayCount"].ToString() == "�@")
                {
                    orderTotalPirce = 2000M;
                }
                else if (TempData["DayCount"].ToString() == "�G")
                {
                    orderTotalPirce = 3800M;
                }
                else
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

                if (newOrder != null)
                {
                    Console.WriteLine("Preparing to create a new order.");
                    // �]�m�q�����M���A

                    newOrder.MemberId = member.MemberId;
                    newOrder.ItineraryId = (int)TempData["ItineraryID"];
                    newOrder.OrderDate = DateTime.Parse(orderDate);
                    newOrder.OrderNumber = orderNumber;
                    newOrder.OrderStatus = "�|���X�o";
                    newOrder.OrderMatchStatus = "�C�X��";
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
                // ���~�B�z
                Console.WriteLine("Exception: " + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public IActionResult TouristGuide(string? gender = null, int? rating = null, int? experience = null)
        {

            var memberEmail = HttpContext.Session.GetString("memberEmail");
            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            var lastOrder = _context.Orderlists
            .Where(o => o.MemberId == member.MemberId)
            .OrderByDescending(o => o.OrderId)
            .FirstOrDefault();

            var lastItinerary = _context.Itineraries
                .Where(i => i.ItineraryId == lastOrder.ItineraryId)
                .FirstOrDefault();

            string selectedCity = lastItinerary.ItineraryName.Substring(0, 2);

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
                if (gender.ToLower() == "�k��")
                {
                    guiders = guiders.Where(g => g.GuiderGender == "M");
                }
                else if (gender.ToLower() == "�k��")
                {
                    guiders = guiders.Where(g => g.GuiderGender == "F");
                }
            }

            if (rating != null)
            {
                guiders = guiders.Where(g => g.GuiderRating >= rating);
            }

            if (experience.HasValue)
            {
                Console.WriteLine(experience);
                DateOnly cutoffDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-experience.Value));
                if (experience == 0)
                {
                    guiders = guiders.Where(g => g.GuiderStartDate >= DateOnly.FromDateTime(DateTime.Now.AddMonths(-6))); // �s��ɹC������A�o�̰��]�֩�6�Ӥ�
                }
                else if (experience == 5)
                {
                    guiders = guiders.Where(g => g.GuiderStartDate <= DateOnly.FromDateTime(DateTime.Now.AddYears(-5))); // 5�~�H�W������
                }
                else
                {
                    guiders = guiders.Where(g => g.GuiderStartDate <= cutoffDate && g.GuiderStartDate > cutoffDate.AddYears(-1)); // ���w�~��
                }
            }
            if (experience.HasValue && !string.IsNullOrEmpty(gender))
            {
                DateOnly cutoffDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-experience.Value));
                if (experience == 0 && gender?.ToLower() == "�k��")
                {
                    guiders = guiders.Where(g => g.GuiderGender == "M" && g.GuiderStartDate <= DateOnly.FromDateTime(DateTime.Now.AddYears(-6)));
                }
                else if (experience == 5 && gender?.ToLower() == "�k��")
                {
                    guiders = guiders.Where(g => g.GuiderGender == "M" && g.GuiderStartDate <= DateOnly.FromDateTime(DateTime.Now.AddYears(-5)));
                }
                else if (experience == 0 && gender?.ToLower() == "�k��")
                {
                    guiders = guiders.Where(g => g.GuiderGender == "F" && g.GuiderStartDate <= DateOnly.FromDateTime(DateTime.Now.AddYears(-6)));
                }
                else if (experience == 5 && gender?.ToLower() == "�k��")
                {
                    guiders = guiders.Where(g => g.GuiderGender == "F" && g.GuiderStartDate <= DateOnly.FromDateTime(DateTime.Now.AddYears(-5)));
                }
                else if (gender?.ToLower() == "�k��" && experience != 0 && experience != 5)
                {
                    guiders = guiders.Where(g => g.GuiderGender == "M" && g.GuiderStartDate <= cutoffDate && g.GuiderStartDate > cutoffDate.AddYears(-1));
                }
                else
                {
                    guiders = guiders.Where(g => g.GuiderGender == "F" && g.GuiderStartDate <= cutoffDate && g.GuiderStartDate > cutoffDate.AddYears(-1));
                }
            };

            if (experience.HasValue && !string.IsNullOrEmpty(gender))
            {
                DateOnly cutoffDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-experience.Value));
                string genderLower = gender.ToLower();
                if (genderLower == "�k��")
                {
                    if (experience == 0)
                    {
                        guiders = guiders.Where(g => g.GuiderGender == "M" && g.GuiderStartDate <= DateOnly.FromDateTime(DateTime.Now.AddYears(-6)));
                    }
                    else if (experience == 5)
                    {
                        guiders = guiders.Where(g => g.GuiderGender == "M" && g.GuiderStartDate <= DateOnly.FromDateTime(DateTime.Now.AddYears(-5)));
                    }
                    else
                    {
                        guiders = guiders.Where(g => g.GuiderGender == "M" && g.GuiderStartDate <= cutoffDate && g.GuiderStartDate > cutoffDate.AddYears(-1));
                    }
                }
                else if (genderLower == "�k��")
                {
                    if (experience == 0)
                    {
                        guiders = guiders.Where(g => g.GuiderGender == "F" && g.GuiderStartDate <= DateOnly.FromDateTime(DateTime.Now.AddYears(-6)));
                    }
                    else if (experience == 5)
                    {
                        guiders = guiders.Where(g => g.GuiderGender == "F" && g.GuiderStartDate <= DateOnly.FromDateTime(DateTime.Now.AddYears(-5)));
                    }
                    else
                    {
                        guiders = guiders.Where(g => g.GuiderGender == "F" && g.GuiderStartDate <= cutoffDate && g.GuiderStartDate > cutoffDate.AddYears(-1));
                    }
                }
            };

            var guideList = guiders.ToList();
            ViewBag.GuiderCount = guideList.Count;
            ViewBag.SelectedGender = gender?.ToLower();
            ViewBag.SelectedRating = rating;
            ViewBag.SelectedExperience = experience;

            return View(guideList);
        }


        [HttpPost]
        public IActionResult SubmitSelectedGuiders(List<int> guiderIds)
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            var lastOrder = _context.Orderlists
            .Where(o => o.MemberId == member.MemberId)
            .OrderByDescending(o => o.OrderId)
            .FirstOrDefault();

            if (guiderIds == null || !guiderIds.Any())
            {
                return BadRequest("No guiders selected.");
            }

            foreach (var guiderId in guiderIds)
            {
                var selectedGuider = _context.SelectGuiders.FirstOrDefault(sg => sg.GuiderId == guiderId);
                selectedGuider = new SelectGuider
                {
                    OrderId = lastOrder.OrderId,
                    MemberId = member.MemberId,
                    GuiderId = guiderId
                };
                _context.SelectGuiders.Add(selectedGuider);

            }

            _context.SaveChanges();
            return Ok();
        }

        // �}�x�a�[��---------------------------

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

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(string memberEmail)
        {
            if (!string.IsNullOrEmpty(memberEmail))
            {
                // Check if memberEmail exists in the database
                var query = from o in _context.Members
                            where o.MemberEmail == memberEmail
                            select o;

                var result = query.FirstOrDefault();

                if (result != null)
                {
                    // Member found! Proceed with password reset logic...
                    // (Generate password reset token, send email, etc.)

                    // �H�e���ҽX��q�l�l��
                    string verificationCode = GenerateVerificationCode();
                    TempData["VerificationCode"] = verificationCode;
                    TempData["MemberEmail"] = memberEmail;

                    Console.WriteLine(verificationCode);
                    await SendVerificationEmail(memberEmail, verificationCode);
                    ViewData["Message"] = "�w�H�e���ҽX��z���H�c�C";

                }
                else
                {
                    ViewData["Message"] = "�H�c��J���~�A�нT�{���H�c�����U�ɿ�J���H�c�C";
                    return View();
                }
            }
            else
            {
                // Invalid or empty email address
                ModelState.AddModelError("memberEmail", "Please enter a valid email address.");
            }

            return Redirect("/home/EmailVertification");
        }
        private string GenerateVerificationCode()
        {
            // �ͦ�6������ҽX
            return new Random().Next(100000, 999999).ToString();
        }

        private async Task SendVerificationEmail(string email, string verificationCode)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("missingyou520x@gmail.com", "qdvnaopcicwvjpst"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("missingyou520x@gmail.com"),
                Subject = "TripNa ���ҽX",
                Body = $"�z�����ҽX�O {verificationCode}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private async Task ReSendVerificationEmail()
        {
            string verificationCode = GenerateVerificationCode();
            TempData["VerificationCode"] = verificationCode;
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("missingyou520x@gmail.com", "qdvnaopcicwvjpst"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("missingyou520x@gmail.com"),
                Subject = "TripNa ���ҽX",
                Body = $"�z�����ҽX�O {verificationCode}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(TempData["MemberEmail"].ToString());

            await smtpClient.SendMailAsync(mailMessage);
        }
        public IActionResult EmailVertification()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmailVertification(string newPassword)
        {

            var memberEmail = TempData["MemberEmail"]?.ToString();


            var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberEmail == memberEmail);

            // ��s�K�X
            member.MemberPassword = newPassword;
            await _context.SaveChangesAsync();

            return Redirect("/home/Login");
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
