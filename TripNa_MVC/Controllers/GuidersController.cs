using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using TripNa_MVC.Models;
using static NuGet.Packaging.PackagingConstants;
using static TripNa_MVC.Models.GuiderRating;
using QAINSERT.Models;

namespace TripNa_MVC.Controllers
{
    public class GuidersController : Controller
    {
        private readonly TripNaContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public GuidersController(TripNaContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        private readonly string _folder;
        private readonly static Dictionary<string, string> _contentTypes = new Dictionary<string, string>
        {
            {".jpg", "image/jpg"}
        };
        private Member Members;
        public IActionResult SignUp()
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            if (member == null)
            {
                return NotFound();
            }
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("GuiderId,GuiderNickname,GuiderGender,GuiderArea,GuiderStartDate,GuiderIntro")] Guider guider, IFormFile guiderImage, IFormFile guiderVert)
        {
            if (ModelState.IsValid)
            {

                var memberEmail = HttpContext.Session.GetString("memberEmail");
                var memberContext = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);
                Console.WriteLine("---------------------"+memberContext+ "---------------------");

                if (memberContext != null )
                {

                    _context.Add(guider);
                    await _context.SaveChangesAsync();
                    memberContext.GuiderId = guider.GuiderId;
                    await _context.SaveChangesAsync();

                    string guiderImageFileName = $"{guider.GuiderNickname}.jpg";
                    string guiderVertFileName = $"{guider.GuiderNickname}.jpg"; // 導遊證文件名


                    // 保存正面照片
                    if (guiderImage != null && guiderImage.Length > 0)
                    {
                        var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, $"導遊/大頭照/{guider.GuiderArea}", guiderImageFileName);
                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            await guiderImage.CopyToAsync(stream);
                        }
                    }

                    // 保存導遊證
                    if (guiderVert != null && guiderVert.Length > 0)
                    {
                        var vertPath = Path.Combine(_hostingEnvironment.WebRootPath, $"導遊/證照/{guider.GuiderArea}", guiderVertFileName);
                        using (var stream = new FileStream(vertPath, FileMode.Create))
                        {
                            await guiderVert.CopyToAsync(stream);
                        }
                    }

                }

                return Redirect("/Guiders/GuiderCenter");
            }


            return Redirect("/Guiders/SignUp");
        }


        // GET: /Members/MemberCenter
        public IActionResult GuiderCenter()
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }
            var memberContext = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            var MemberId = _context.Members.FirstOrDefault(m => m.MemberId == memberContext.MemberId);

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);


            var GuiderId = memberContext.GuiderId;

            Console.WriteLine(GuiderId + "----------------------------------------------");

            HttpContext.Session.SetString("GuiderId", GuiderId.ToString());

            if (member == null)
            {
                return NotFound();
            }



            if (memberContext.GuiderId == null)
            {
                return Redirect("/Members/MemberCenter"); // 如果該使用者沒有，重定向到會員中心頁面
            }


            var guidermember = from g in _context.Guiders
                               join m in _context.Members on g.GuiderId equals m.GuiderId into membersGroup
                               from m in membersGroup.DefaultIfEmpty()
                               where g.GuiderId == memberContext.GuiderId
                               select new
                               {
                                   Guider = g,
                                   Member = m
                               };

            var guidermemberList = guidermember.Select(x => new guidermemberlist
            {
                Guider = x.Guider,
                Member = x.Member
            }).ToList();

            return View(guidermemberList);
        }



        //NEW 更新導遊

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiderCenter(Guider updatedGuider, IFormFile guiderImage)
        {

            var memberEmail = HttpContext.Session.GetString("memberEmail");

            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home");
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            var guider = _context.Guiders.FirstOrDefault(g => g.GuiderId == member.GuiderId);


            if (member.GuiderId == null)
            {
                return Redirect("/Members/MemberCenter"); // 如果該使用者沒有，重定向到會員中心頁面
            }

            bool isUpdated = false;

            string originalNickname = guider.GuiderNickname;
            string originalArea = guider.GuiderArea;

            // 更新會員的資訊
            // 檢查 GuiderNickname 是否被修改過
            if (updatedGuider.GuiderNickname != null && updatedGuider.GuiderNickname != guider.GuiderNickname)
            {
                guider.GuiderNickname = updatedGuider.GuiderNickname;
            }

            // 檢查 GuiderArea 是否被修改過
            if (updatedGuider.GuiderArea != null && updatedGuider.GuiderArea != guider.GuiderArea)
            {
                guider.GuiderArea = updatedGuider.GuiderArea;
            }

            // 檢查 GuiderIntro 是否被修改過
            if (updatedGuider.GuiderIntro != null && updatedGuider.GuiderIntro != guider.GuiderIntro)
            {
                guider.GuiderIntro = updatedGuider.GuiderIntro;
            }

            // 檢查並更新照片

            string guiderImageFileName = $"{guider.GuiderNickname}.jpg";


            if (originalNickname != guider.GuiderNickname || originalArea != guider.GuiderArea)
            {
                var oldImagePath = Path.Combine(_hostingEnvironment.WebRootPath, $"導遊/大頭照/{originalArea}", $"{originalNickname}.jpg");
                var newImagePath = Path.Combine(_hostingEnvironment.WebRootPath, $"導遊/大頭照/{guider.GuiderArea}", $"{guider.GuiderNickname}.jpg");

                if (System.IO.File.Exists(oldImagePath))
                {
                    if (!Directory.Exists(Path.Combine(_hostingEnvironment.WebRootPath, $"導遊/大頭照/{guider.GuiderArea}")))
                    {
                        Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, $"導遊/大頭照/{guider.GuiderArea}"));
                    }

                    // 移動圖片到新的路徑
                    System.IO.File.Move(oldImagePath, newImagePath);
                }
            }
            else
            {
                // 更新正面照片
                if (guiderImage != null && guiderImage.Length > 0)
                {
                    Console.WriteLine("------------------------------------- 更改照片 ----------------------------------------");
                    var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, $"導遊/大頭照/{guider.GuiderArea}");
                    if (!Directory.Exists(imagePath))
                    {
                        Directory.CreateDirectory(imagePath);
                    }
                    var fullImagePath = Path.Combine(imagePath, guiderImageFileName);
                    using (var stream = new FileStream(fullImagePath, FileMode.Create))
                    {
                        await guiderImage.CopyToAsync(stream);
                    }
                    isUpdated = true;
                }
            }
            // 如果有任何欄位被修改過，就儲存變更
            if (guider.GuiderNickname != null || guider.GuiderArea != null || guider.GuiderIntro != null || isUpdated)
            {
                _context.SaveChanges();
            }



            return RedirectToAction("GuiderCenter");
        }








        public IActionResult Guiderintroduce()
        {
            var GuiderId = HttpContext.Session.GetString("GuiderId");
            //HttpContext.Session.SetString("GuiderId", GuiderId.ToString());

            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);
            var MemberId = _context.Members.FirstOrDefault(m => m.MemberId == member.MemberId);
            var MemberName = _context.Members.FirstOrDefault(m => m.MemberName == member.MemberName);
            //var GuiderId = _context.Guiders.FirstOrDefault(g => g.GuiderId == member.GuiderId);

            if (member == null)
            {
                return NotFound();
            }

            var guider = from g in _context.Guiders
                                           join o in _context.Orderlists on g.GuiderId equals o.GuiderId into OrderGroup
                                           from o in OrderGroup.DefaultIfEmpty()
                                           join r in _context.Ratings on g.GuiderId equals r.GuiderId into RatingGroup
                                           from r in RatingGroup.DefaultIfEmpty()
                                           join m in _context.Members on g.GuiderId equals m.GuiderId into MemberGroup
                                           from m in MemberGroup.DefaultIfEmpty()
                                           where g.GuiderId == member.GuiderId
                                           select new
                                           {
                                               g.GuiderNickname,
                                               g.GuiderId,
                                               g.GuiderGender,
                                               g.GuiderArea,
                                               g.GuiderStartDate,
                                               g.GuiderIntro,
                                               //r.RatingComment,
                                               //r.RatingStars,
                                               RatingComment = r.RatingComment == null ? "無評論" : r.RatingComment, // 如果 r.RatingComment 為 null,則使用 "無評論" 作為預設值
                                               RatingStars = r.RatingStars == null ? (byte)0 : r.RatingStars, // 使用三元運算子
                                               m.MemberName,
                                               m.MemberId
                                               //,                                   
                                               //Order = o
                                           };


            var guiderList = guider.ToList();



            var ratings = from q in _context.Ratings
                            join m in _context.Members on q.MemberId equals m.MemberId
                            join g in _context.Guiders on q.GuiderId equals g.GuiderId
                            where q.GuiderId == member.GuiderId 
                            select new
                            {
                                g.GuiderNickname,
                                m.MemberName,
                                q.RatingComment,
                                q.RatingStars
                            };



            // 構建 GuiderRating
            var model = new GuiderRating
            {
                Guiders = guiderList.Select(g => new Guider

                {
                    GuiderId = g.GuiderId,
                    GuiderNickname = g.GuiderNickname,
                    GuiderGender = g.GuiderGender,
                    GuiderArea = g.GuiderArea,
                    GuiderStartDate = g.GuiderStartDate,
                    GuiderIntro = g.GuiderIntro,
                    Rating = new Rating
                    {
                        RatingComment = g.RatingComment,
                        RatingStars = g.RatingStars
                    },
                    Members = new Member
                    {
                        MemberName = g.MemberName,
                        MemberId = g.MemberId
                    }
                }).ToList(),

                Rates = ratings.Select(q => new GuiderRatingData
                {
                    GuiderNickname = q.GuiderNickname,
                    MemberName = q.MemberName,
                    RatingComment = q.RatingComment,
                    RatingStars = q.RatingStars
                }).ToList(),

            };

            //計算該導遊總共有幾筆訂單
            int orderCount = _context.Orderlists.Count(o => (o.GuiderId.HasValue ? o.GuiderId.Value.ToString() : string.Empty) == GuiderId);

            //計算該導遊總共有幾筆評價
            int ratingCount = _context.Ratings.Count(r => r.GuiderId == Convert.ToInt32(GuiderId));
            //(r.GuiderId.HasValue ? r.GuiderId.Value.ToString() : string.Empty) == GuiderId);
            //(o => (o.GuiderId.HasValue ? o.GuiderId.Value.ToString() : string.Empty) == GuiderId);

            double ratingAvg = _context.Ratings
                        .Where(r => r.GuiderId == Convert.ToDouble(GuiderId))
                        .Select(r => (double)r.RatingStars)
                        .DefaultIfEmpty()
                        .Average();


            Console.WriteLine("orderCount為:"+orderCount + "----------------------------------------------");
            Console.WriteLine("ratingCount:" + ratingCount + "----------------------------------------------");
            Console.WriteLine("ratingAvg:" + ratingAvg + "----------------------------------------------");


            //將該導遊總共有幾筆評價筆數傳給HTML
            ViewData["ratingCount"] = ratingCount;

            //將該導遊總共有幾筆訂單筆數傳給HTML
            ViewData["orderCount"] = orderCount;

            //將該導遊總評價的平均傳給HTML
            ViewData["ratingAvg"] = ratingAvg;

            return View(model);
        }






        public IActionResult GuiderOrder()
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果Session 裡面沒有東西 重導回主頁
            }
            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            var MemberId = _context.Members.FirstOrDefault(m => m.MemberId == member.MemberId);

            if (member != null && member.GuiderId == null)
            {
                // GuiderID 為空,可以註冊
                ViewData["Message"] = "前往註冊";
            }
            else
            {
                // GuiderID 不為空,不能註冊
            }

            var orderDetails = from o in _context.Orderlists
                               join i in _context.Itineraries on o.ItineraryId equals i.ItineraryId
                               where o.GuiderId == member.GuiderId
                               orderby o.OrderNumber descending //按訂單日期降序排序
                               select new
                               {
                                   o.OrderNumber,
                                   o.OrderDate,
                                   i.ItineraryStartDate,
                                   o.OrderTotalPrice,
                                   o.OrderStatus,
                                   o.OrderMatchStatus,
                                   o.OrderId
                               };


            // 將查詢結果轉換為列表
            var orderDetailsList = orderDetails.ToList();

            // 構建 OrderDetail
            var model = new OrderDetail
            {

                Orders = orderDetailsList.Select(o => new Orderlist
                {
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    OrderTotalPrice = o.OrderTotalPrice,
                    OrderStatus = o.OrderStatus,
                    OrderMatchStatus = o.OrderMatchStatus,
                    OrderId = o.OrderId,
                    Itinerary = new Itinerary
                    {
                        ItineraryStartDate = o.ItineraryStartDate
                    },                  
                }).ToList(),
                MemberId = member.MemberId
            };

            return View(model);
        }



        public IActionResult GuiderMatchDetails()
        {

            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            var MemberId = _context.Members.FirstOrDefault(m => m.MemberId == member.MemberId);



            if (member != null && member.GuiderId == null)
            {
                // GuiderID 為空,可以註冊
                ViewData["Message"] = "前往註冊";
            }
            else
            {
                // GuiderID 不為空,不能註冊
            }

            var orderDetails =  from sg in _context.SelectGuiders
                                
                                join o in _context.Orderlists on sg.OrderId equals o.OrderId

                                join m in _context.Members on sg.MemberId equals m.MemberId


                                from g in _context.Guiders.Where(x => x.GuiderId == (int?)sg.GuiderId).DefaultIfEmpty()

                                join i in _context.Itineraries on o.ItineraryId equals i.ItineraryId

                                join a in _context.ItineraryDetails on o.ItineraryId equals a.ItineraryId into details

                                where o.OrderMatchStatus == "媒合中" && sg.GuiderId == member.GuiderId

                                from j in _context.ItineraryDetails.Where(x => x.ItineraryId == i.ItineraryId)
                                  
                                join s in _context.Spots on j.SpotId equals s.SpotId

                                select new
                                {
                                    o.OrderNumber,
                                    o.OrderDate,
                                    i.ItineraryStartDate,
                                    o.OrderStatus,
                                    o.OrderMatchStatus,
                                    g.GuiderNickname,
                                    i.ItineraryName,
                                    i.ItineraryPeopleNo,
                                    m.MemberName,
                                    m.MemberEmail,
                                    m.MemberPhone,
                                    m.MemberId,
                                    o.ItineraryId,                                    
                                    g.GuiderArea,
                                    g.GuiderId,
                                    o.OrderId,
                                    Spot = s,
                                    ItineraryDetails = details.ToList() // 將 ItineraryDetails 作為子集合
                                };

            // 將查詢結果轉換為列表
            var orderDetailsList = orderDetails.ToList();

            if (orderDetailsList == null)
            {
                return NotFound();
            }


            var odp = from o in _context.Orderlists
                      join i in _context.Itineraries on o.ItineraryId equals i.ItineraryId
                      select o;

            int orderCount = _context.Orderlists
                         .Where(o => o.GuiderId == member.GuiderId)
                         .Count();


            Console.WriteLine("---------------------------"+ orderCount + "---------------------------");
            Console.WriteLine("---------------------------" + member.GuiderId + "---------------------------");

            //Console.WriteLine("---------------------------" + memberID + "---------------------------");


            //將該導遊總共有幾筆評價筆數傳給HTML
            ViewData["GuiderId"] = member.GuiderId;


            // 構建 OrderDetail
            var model = new OrderDetail
            {

                Orders = orderDetailsList.Select(o => new Orderlist
                {

                    OrderNumber = o.OrderNumber,
                    ItineraryId = o.ItineraryId,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    OrderMatchStatus = o.OrderMatchStatus,
                    OrderId = o.OrderId,

                    Itinerary = new Itinerary
                    {
                        ItineraryStartDate = o.ItineraryStartDate,
                        ItineraryName = o.ItineraryName,
                        ItineraryPeopleNo = o.ItineraryPeopleNo,
                        ItineraryDetails = o.ItineraryDetails // 將 ItineraryDetails 作為子屬性
                    },
                    Guider = new Guider
                    {
                        GuiderNickname = o.GuiderNickname,
                        GuiderArea = o.GuiderArea,
                        GuiderId=  o.GuiderId
                    },                    
                    Member = new Member
                    {
                        MemberName = o.MemberName,
                        MemberEmail = o.MemberEmail,
                        MemberPhone = o.MemberPhone,
                        MemberId =o.MemberId
                    },
                    Spots = o.Spot
                    
                }).ToList(),
                MemberId = member.MemberId,
                
            };

            return View(model);
        }







        //[HttpPut("{id}")]
        [HttpPut]
        public async Task<IActionResult> MatchDetails( [FromBody] Match dataToSend)
        {

            var existingOrder = await _context.Orderlists.FindAsync(dataToSend.OrderId);

            if (existingOrder == null)
            {
                return NotFound();
            }
            existingOrder.OrderId = dataToSend.OrderId;
            existingOrder.MemberId = dataToSend.MemberId;
            existingOrder.GuiderId = dataToSend.GuiderId;
            existingOrder.OrderStatus = dataToSend.OrderStatus;
            existingOrder.OrderMatchStatus = dataToSend.OrderMatchStatus;
            existingOrder.ItineraryId = dataToSend.ItineraryId;

            var member = _context.Members.FirstOrDefault(m => m.MemberId == existingOrder.MemberId);
            var memberEmail = member.MemberEmail;

            var guider = _context.Guiders.FirstOrDefault(g => g.GuiderId == existingOrder.GuiderId);
            var guiderNickname = guider.GuiderNickname;

            var itinerary = _context.Itineraries.FirstOrDefault(i => i.ItineraryId == existingOrder.ItineraryId);
            var itineraryName = itinerary.ItineraryName;
            var itineraryStartDate = itinerary.ItineraryStartDate;


            try
            {
                _context.Entry(existingOrder).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await SendAcceptEmail(memberEmail, guiderNickname, itineraryName, itineraryStartDate);
                return Ok("成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return StatusCode(500, "更新錯誤");
            }

        }










        [HttpDelete("/Guiders/GuiderMatchDetails")]
        public async Task<IActionResult> GuiderMatchDetails([FromQuery] int orderId, [FromQuery] int guideId, [FromQuery] int memberId)

        {
            var existingOrder = await (from o in _context.SelectGuiders
                                       where o.OrderId == orderId && o.MemberId == memberId && o.GuiderId == guideId
                                       select o).FirstOrDefaultAsync();
            //return Ok(existingOrder);


            if (existingOrder == null)
            {
                return NotFound();
            }


            _context.SelectGuiders.Remove(existingOrder);


            var member = _context.Members.FirstOrDefault(m => m.MemberId == existingOrder.MemberId);
            var memberEmail = member.MemberEmail;

            var guider = _context.Guiders.FirstOrDefault(g => g.GuiderId == existingOrder.GuiderId);
            var guiderNickname = guider.GuiderNickname;


            var order = _context.Orderlists.FirstOrDefault(o => o.OrderId == existingOrder.OrderId);
            var itinerary = _context.Itineraries.FirstOrDefault(i => i.ItineraryId == order.ItineraryId);
            var itineraryName = itinerary.ItineraryName;
            var itineraryStartDate = itinerary.ItineraryStartDate;


            try
            {
                //_context.Entry(existingOrder).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await SendRejectEmail(memberEmail, guiderNickname, itineraryName, itineraryStartDate);
                return Ok("婉拒了了了");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("Stack Trace: " + ex.StackTrace);
                return StatusCode(500, "錯誤");
            }
        }



        public async Task SendAcceptEmail(string memberEmail, string guiderNickName, string itineraryName, DateTime itineraryStartDate)
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
                Subject = "導遊媒合成功通知",
                Body = $"您的{itineraryName}訂單(出發日{itineraryStartDate})，已與導遊 {guiderNickName} 媒合成功",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(memberEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendRejectEmail(string memberEmail, string guiderNickName, string itineraryName, DateTime itineraryStartDate)
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
                Subject = "導遊婉拒訂單通知",
                Body = $"您的{itineraryName}訂單(出發日{itineraryStartDate})，已被導遊 {guiderNickName} 婉拒",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(memberEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }
       



















        public IActionResult GuiderOrderDetails(int orderID)
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            var orderDetails =  from o in _context.Orderlists
                                join m in _context.Members on o.MemberId equals m.MemberId
                                from g in _context.Guiders.Where(x => x.GuiderId == (int?)o.GuiderId).DefaultIfEmpty()
                                join i in _context.Itineraries on o.ItineraryId equals i.ItineraryId
                                join a in _context.ItineraryDetails on o.ItineraryId equals a.ItineraryId
                                join c in _context.Coupons on o.MemberId equals c.MemberId
                                join r in _context.Ratings on o.MemberId equals r.MemberId
                                where o.GuiderId == member.GuiderId && o.OrderId == orderID
                                from j in _context.ItineraryDetails.Where(x => x.ItineraryId == i.ItineraryId)
                                join s in _context.Spots on j.SpotId equals s.SpotId
                                select new
                                {
                                    o.OrderNumber,
                                    o.OrderDate,
                                    i.ItineraryStartDate,
                                    o.OrderTotalPrice,
                                    o.OrderStatus,
                                    o.OrderMatchStatus,
                                    c.CouponCode,
                                    g.GuiderNickname,
                                    i.ItineraryName,
                                    i.ItineraryPeopleNo,
                                    m.MemberName,
                                    m.MemberEmail,
                                    m.MemberPhone,
                                    ItineraryDetails = j,
                                    Spot = s,
                                    o.ItineraryId,
                                    a.VisitOrder,
                                    g.GuiderArea,
                                    o.OrderId,
                                    r.RatingComment,
                                    r.RatingStars,
                                };

            // 將查詢結果轉換為列表
            var orderDetailsList = orderDetails.ToList();

            if (orderDetailsList == null)
            {
                return NotFound();
            }

            // 構建 OrderDetail
            var model = new OrderDetail
            {

                Orders = orderDetailsList.Select(o => new Orderlist
                {

                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    OrderTotalPrice = o.OrderTotalPrice,
                    OrderStatus = o.OrderStatus,
                    OrderMatchStatus = o.OrderMatchStatus,
                    OrderId = o.OrderId,

                    Itinerary = new Itinerary
                    {
                        ItineraryStartDate = o.ItineraryStartDate,
                        ItineraryName = o.ItineraryName,
                        ItineraryPeopleNo = o.ItineraryPeopleNo,
                        ItineraryDetails = new List<ItineraryDetail> { o.ItineraryDetails }
                    },
                    Coupon = new Coupon
                    {
                        CouponCode = o.CouponCode
                    },
                    Guider = new Guider
                    {
                        GuiderNickname = o.GuiderNickname,
                        GuiderArea = o.GuiderArea
                    },
                    Rating = new Rating
                    {
                        RatingComment = o.RatingComment,
                        RatingStars = o.RatingStars
                    },
                    Member = new Member
                    {
                        MemberName = o.MemberName,
                        MemberEmail = o.MemberEmail,
                        MemberPhone = o.MemberPhone
                    },
                    Spots = o.Spot,
                    ItineraryDetail = new ItineraryDetail
                    {
                        ItineraryId = o.ItineraryId,
                        VisitOrder = o.VisitOrder
                    }
                }).ToList(),


                MemberId = member.MemberId,
                OrderId = orderID
            };

            return View(model);
        }


        public IActionResult GuiderQA(int orderID)
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);
            var orderDetails = (from o in _context.Orderlists
                                join m in _context.Members on o.MemberId equals m.MemberId
                                from g in _context.Guiders.Where(x => x.GuiderId == (int?)o.GuiderId).DefaultIfEmpty()
                                join i in _context.Itineraries on o.ItineraryId equals i.ItineraryId
                                join a in _context.ItineraryDetails on o.ItineraryId equals a.ItineraryId
                                from c in _context.Coupons.Where(x => x.MemberId == o.MemberId).DefaultIfEmpty()
                                where o.GuiderId == member.GuiderId && o.OrderId == orderID
                                from j in _context.ItineraryDetails.Where(x => x.ItineraryId == i.ItineraryId)
                                join s in _context.Spots on j.SpotId equals s.SpotId
                                select new
                                {
                                    o.OrderNumber,
                                    o.OrderDate,
                                    i.ItineraryStartDate,
                                    o.OrderTotalPrice,
                                    o.OrderStatus,
                                    o.OrderMatchStatus,
                                    c.CouponCode,
                                    g.GuiderNickname,
                                    g.GuiderId,
                                    i.ItineraryName,
                                    i.ItineraryPeopleNo,
                                    m.MemberName,
                                    m.MemberEmail,
                                    m.MemberPhone,
                                    m.MemberId,
                                    ItineraryDetails = j,
                                    Spot = s,
                                    o.ItineraryId,
                                    a.VisitOrder,
                                    g.GuiderArea,
                                    o.OrderId
                                });



            // 將查詢結果轉換為列表
            var orderDetailsList = orderDetails.ToList();

            if (orderDetailsList == null)
            {
                return NotFound();
            }

            //var questions = from ga in _context.GuiderAnswers
            //                join g in _context.Guiders on ga.GuiderId equals g.GuiderId
            //                from q in _context.MemberQuestions.Where(g => g.OrderId == (int?)ga.OrderId).DefaultIfEmpty()
            //                where g.GuiderId == member.GuiderId && q.OrderId == orderID
            //                select new
            //                {
            //                    q.QuestionContent,
            //                    q.QuestionTime,
            //                    ga.AnswerContent,
            //                    ga.AnswerTime
            //                };


            var questions = from q in _context.Qas
                            where q.GuiderId == member.GuiderId && q.OrderId == orderID
                            //.Where(q => q.OrderId == orderID)
                            select new
                            {
                                q.Qaid,
                                q.QuestionContent,
                                q.QuestionTime,
                                q.AnswerContent,
                                q.AnswerTime
                            };

            int answerCount = _context.GuiderAnswers
                               .Where(a => a.OrderId == orderID)
                               .Count();

            // 顯示結果
            Console.WriteLine($"OrderID = {orderID} 的 AnswerCount 筆數為: {answerCount}");

            //將問答筆數傳給HTML
            ViewData["AnswerCount"] = answerCount;


            // 構建 OrderDetail
            var model = new OrderDetail
            {

                Orders = orderDetailsList.Select(o => new Orderlist
                {

                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    OrderTotalPrice = o.OrderTotalPrice,
                    OrderStatus = o.OrderStatus,
                    OrderMatchStatus = o.OrderMatchStatus,
                    OrderId = o.OrderId,
                    Itinerary = new Itinerary
                    {
                        ItineraryStartDate = o.ItineraryStartDate,
                        ItineraryName = o.ItineraryName,
                        ItineraryPeopleNo = o.ItineraryPeopleNo,
                        ItineraryDetails = new List<ItineraryDetail> { o.ItineraryDetails }
                    },
                    Guider = new Guider
                    {
                        GuiderNickname = o.GuiderNickname,
                        GuiderArea = o.GuiderArea,
                        GuiderId = o.GuiderId
                    },
                    Member = new Member
                    {
                        MemberName = o.MemberName,
                        MemberId = o.MemberId,
                    },

                    Spots = o.Spot,
                    ItineraryDetail = new ItineraryDetail
                    {
                        ItineraryId = o.ItineraryId,
                        VisitOrder = o.VisitOrder
                    }
                }).ToList(),
                Questions = questions.Select(q => new Qa
                {   Qaid = q.Qaid,
                    QuestionContent = q.QuestionContent,
                    QuestionTime = (DateTime)q.QuestionTime,
                    AnswerContent = q.AnswerContent,
                    AnswerTime = q.AnswerTime
                }).ToList(),
                MemberId = member.MemberId,
                OrderId = orderID
            };

            return View(model);
        }


        [HttpPost]
        public IActionResult SubmitAnswer([FromBody] Qa DataUpdate, string answer, int orderId)
        {

            var existingQa =  _context.Qas.FindAsync(DataUpdate.OrderId);

            if (existingQa == null)
            {
                return NotFound();
            }

            // 檢查輸入資料的有效性
            if (string.IsNullOrWhiteSpace(answer))
            {
                return BadRequest("問題內容不能為空白。");
            }


            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }
            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);
            var guider = member.GuiderId;

            Console.WriteLine("------------------"+ guider + "------------------");


            // 建立新的 GuiderAnswer 實體並儲存到資料庫

            var newAnswer = new GuiderAnswer
            {
                GuiderId = DataUpdate.GuiderId,
                OrderId = DataUpdate.OrderId,
                AnswerContent = DataUpdate.AnswerContent,
                AnswerTime = DateTime.Now
            };

            _context.GuiderAnswers.Add(newAnswer);
            _context.SaveChanges();

            return Ok("回覆提交成功。");

        }




        //[HttpPut]
        //public async Task<IActionResult> tails([FromBody] Match dataToSend)
        //{

        //    var existingOrder = await _context.Orderlists.FindAsync(dataToSend.OrderId);

        //    if (existingOrder == null)
        //    {
        //        return NotFound();
        //    }
        //    existingOrder.OrderId = dataToSend.OrderId;
        //    existingOrder.MemberId = dataToSend.MemberId;
        //    existingOrder.GuiderId = dataToSend.GuiderId;
        //    existingOrder.OrderStatus = dataToSend.OrderStatus;
        //    existingOrder.OrderMatchStatus = dataToSend.OrderMatchStatus;
        //    existingOrder.ItineraryId = dataToSend.ItineraryId;

        //    var member = _context.Members.FirstOrDefault(m => m.MemberId == existingOrder.MemberId);
        //    var memberEmail = member.MemberEmail;

        //    var guider = _context.Guiders.FirstOrDefault(g => g.GuiderId == existingOrder.GuiderId);
        //    var guiderNickname = guider.GuiderNickname;

        //    var itinerary = _context.Itineraries.FirstOrDefault(i => i.ItineraryId == existingOrder.ItineraryId);
        //    var itineraryName = itinerary.ItineraryName;
        //    var itineraryStartDate = itinerary.ItineraryStartDate;


        //    try
        //    {
        //        _context.Entry(existingOrder).State = EntityState.Modified;
        //        await _context.SaveChangesAsync();
        //        await SendAcceptEmail(memberEmail, guiderNickname, itineraryName, itineraryStartDate);
        //        return Ok("成功");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Exception: " + ex.Message);
        //        Console.WriteLine("Stack Trace: " + ex.StackTrace);
        //        return StatusCode(500, "更新錯誤");
        //    }

        //}























    }
}
