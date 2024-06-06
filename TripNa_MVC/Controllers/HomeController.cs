using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TripNa_MVC.Models;
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

        public IActionResult Privacy()
        {
            return View();
        }


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

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(string memberEmail)
        {
            Console.WriteLine(123123);
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

                    // 寄送驗證碼到電子郵件
                    string verificationCode = GenerateVerificationCode();
                    TempData["VerificationCode"] = verificationCode;
                    TempData["MemberEmail"] = memberEmail;

                    Console.WriteLine(verificationCode);
                    // await SendVerificationEmail(memberEmail, verificationCode);
                    ViewData["Message"] = "已寄送驗證碼到您的信箱。";


                    // 你可以在這裡添加其他邏輯，比如保存驗證碼到數據庫

                    //return Ok(new { Message = "驗證碼已寄送到您的電子郵件。" });

                }
                else
                {
                    ViewData["Message"] = "信箱輸入錯誤，請確認此信箱為註冊時輸入的信箱。";
                    return View();
                }
            }
            else
            {
                // Invalid or empty email address
                ModelState.AddModelError("memberEmail", "Please enter a valid email address.");
            }
            
            return View();
        }
        private string GenerateVerificationCode()
        {
            // 生成6位數驗證碼
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
                Subject = "TripNa 驗證碼",
                Body = $"您的驗證碼是 {verificationCode}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public IActionResult EmailVertification()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EmailVerification(string verificationCode, string newPassword)
        {
            var storedCode = TempData["VerificationCode"]?.ToString();
            var memberEmail = TempData["MemberEmail"]?.ToString();

   
            if (storedCode != verificationCode)
            {
                ViewData["Message"] = "驗證碼不正確!" ;
            }

            var member = await _context.Members.FirstOrDefaultAsync(m => m.MemberEmail == memberEmail);

            // 更新密碼
            member.MemberPassword = newPassword;
            await _context.SaveChangesAsync();

            return Redirect("/Home/Login");
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
