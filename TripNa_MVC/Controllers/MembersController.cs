using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TripNa_MVC.Models;
using XAct;
using XSystem.Security.Cryptography;

namespace TripNa_MVC.Controllers
{
    public class MembersController : Controller
    {
        private readonly TripNaContext _context;

        public MembersController(TripNaContext context)
        {
            _context = context;
        }



        // GET: /Members/MemberCenter
        public IActionResult MemberCenter()
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

            return View(member);
        }


        [HttpPost]
        public IActionResult MemberCenter(Member updatedMember)
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home");
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            if (member == null)
            {
                return NotFound();
            }

            // 更新會員的資訊
            member.MemberName = updatedMember.MemberName;
            member.MemberPhone = updatedMember.MemberPhone;
            if (!string.IsNullOrEmpty(updatedMember.MemberPassword))
            {
                member.MemberPassword = updatedMember.MemberPassword;
            }

            _context.SaveChanges();
            return RedirectToAction("MemberCenter");
        }



        public IActionResult UserCollect()
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); //如果Session 裡面沒有東西 重導回主頁
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

            var favoriteSpots = from fs in _context.FavoriteSpots
                                join m in _context.Members on fs.MemberId equals m.MemberId
                                join s in _context.Spots on fs.SpotId equals s.SpotId
                                where fs.MemberId == member.MemberId
                                select new
                                {
                                    fs.FavoriteSpotId,
                                    fs.MemberId,
                                    fs.SpotId,
                                    s.SpotCity,
                                    s.SpotName,
                                    s.SpotIntro
                                };

            // 將查詢結果轉換為列表
            var favoriteSpotList = favoriteSpots.ToList();

            // 構建 SpotViewModel
            var model = new SpotViewModel
            {
                FavoriteSpots = favoriteSpotList.Select(fs => new FavoriteSpot
                {

                    FavoriteSpotId = fs.FavoriteSpotId,
                    MemberId = fs.MemberId,
                    SpotId = fs.SpotId,

                    Spot = new Spot
                    {
                        SpotId = fs.SpotId,
                        SpotCity = fs.SpotCity,
                        SpotName = fs.SpotName,
                        SpotIntro = fs.SpotIntro
                    }

                }).ToList(),

                Spots = favoriteSpotList.Select(fs => new Spot
                {
                    SpotId = fs.SpotId,
                    SpotCity = fs.SpotCity,
                    SpotName = fs.SpotName,
                    SpotIntro = fs.SpotIntro

                }).ToList(),

                MemberId = member.MemberId
            };
            return View(model);
        }


        // 刪除被選擇的ID 的該筆收藏資料
        [HttpPost, ActionName("UserCollect")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCollect(int id)
        {
            var favoriteSpot = await _context.FavoriteSpots.FindAsync(id);
            if (favoriteSpot != null)
            {
                _context.FavoriteSpots.Remove(favoriteSpot);
            }

            await _context.SaveChangesAsync();
            //return RedirectToAction("UserCollect", "Members");
            return Redirect("/Members/UserCollect");

        }






        // GET: /Members/UserCoupon
        public IActionResult UserCoupon()
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

            if (member == null)
            {
                return NotFound();
            }

            var query = from c in _context.Coupons
                        join m in _context.Members on c.MemberId equals m.MemberId
                        join i in _context.Itineraries on c.ItineraryId equals i.ItineraryId
                        where c.MemberId == member.MemberId
                        select new
                        {
                            c.CouponCode,
                            c.CouponDueDate,
                            i.ItineraryName
                        };
            // 將查詢結果轉換為列表
            var result = query.ToList();

            // 構建 OrderDetail
            var model = new UserCoupon
            {

                Coupon = result.Select(x => new Coupon
                {
                    CouponCode = x.CouponCode,
                    CouponDueDate = x.CouponDueDate,

                    Itinerary = new Itinerary
                    {
                        ItineraryName = x.ItineraryName
                    }

                }).ToList(),
            };

            return View(model);
        }




        public IActionResult UserOrder()
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
                               join c in _context.Coupons on o.CouponId equals c.CouponId into couponGroup
                               from c in couponGroup.DefaultIfEmpty() // left join
                               where o.MemberId == member.MemberId
                               orderby o.OrderNumber descending //按訂單日期降序排序
                               select new
                               {
                                   o.OrderNumber,
                                   o.OrderDate,
                                   i.ItineraryStartDate,
                                   o.OrderTotalPrice,
                                   o.OrderStatus,
                                   o.OrderMatchStatus,
                                   c.CouponCode,
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
                    Coupon = new Coupon
                    {
                        CouponCode = o.CouponCode
                    }
                }).ToList(),
                MemberId = member.MemberId
            };

            return View(model);
        }




        public IActionResult UserOrderDetails(int orderID)
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
            var orderDetails = (from o in _context.Orderlists
                                join m in _context.Members on o.MemberId equals m.MemberId
                                from g in _context.Guiders.Where(x => x.GuiderId == (int?)o.GuiderId).DefaultIfEmpty()
                                join i in _context.Itineraries on o.ItineraryId equals i.ItineraryId
                                join a in _context.ItineraryDetails on o.ItineraryId equals a.ItineraryId
                                from c in _context.Coupons.Where(x => x.MemberId == o.MemberId).DefaultIfEmpty()
                                from r in _context.Ratings.Where(x => x.MemberId == o.MemberId).DefaultIfEmpty()
                                where o.MemberId == member.MemberId && o.OrderId == orderID
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
                                    CouponCode = c.CouponCode ?? string.Empty,
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
                                    o.OrderId,
                                    RatingComment = r.RatingComment ?? string.Empty,
                                    RatingStars = r.RatingStars == null ? (byte)0 : r.RatingStars
                                });

            // 將查詢結果轉換為列表
            var orderDetailsList = orderDetails.ToList();

            if (orderDetailsList == null)
            {
                return NotFound();
            }


            var odp = from o in _context.Orderlists
                      join i in _context.Itineraries on o.ItineraryId equals i.ItineraryId
                      select o;





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
                        GuiderArea = o.GuiderArea,
                        GuiderId = o.GuiderId
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
                        MemberPhone = o.MemberPhone,
                        MemberId = o.MemberId

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


        public IActionResult MemberQA(int orderID)
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
                                where o.MemberId == member.MemberId && o.OrderId == orderID
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
                                    o.OrderId
                                });



            // 將查詢結果轉換為列表
            var orderDetailsList = orderDetails.ToList();

            if (orderDetailsList == null)
            {
                return NotFound();
            }


            var questions = from q in _context.MemberQuestions
                            from ga in _context.GuiderAnswers.Where(g => g.OrderId == (int?)q.OrderId).DefaultIfEmpty()
                            where q.MemberId == member.MemberId && q.OrderId == orderID
                            //.Where(q => q.OrderId == orderID)
                            select new
                            {
                                q.QuestionContent,
                                q.QuestionTime,
                                ga.AnswerContent,
                                ga.AnswerTime
                            };







            int questionCount = _context.MemberQuestions
                               .Where(mq => mq.OrderId == orderID)
                               .Count();

            // 顯示結果
            Console.WriteLine($"OrderID = {orderID} 的 QuestionContent 筆數為: {questionCount}");

            //將問答筆數傳給HTML
            ViewData["QuestionCount"] = questionCount;


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
                Questions = questions.Select(q => new QuestionAnswer
                {
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
        public IActionResult SubmitQuestion(string question, int orderId)
        {
            // 檢查輸入資料的有效性
            if (string.IsNullOrWhiteSpace(question))
            {
                return BadRequest("問題內容不能為空白。");
            }

            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }
            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);



            // 建立新的 MemberQuestion 實體並儲存到資料庫
            var newQuestion = new MemberQuestion
            {
                MemberId = member.MemberId,
                OrderId = orderId,
                QuestionContent = question,
                QuestionTime = DateTime.Now
            };


            _context.MemberQuestions.Add(newQuestion);
            _context.SaveChanges();

            return Ok("問題提交成功。");

        }




        [HttpPost]
        public IActionResult SaveRating([FromBody] MemberRating dataToSend)
        {
            try
            {
                // 創建一個新的評價實體
                var rating = new Rating
                {
                    RatingStars = dataToSend.RatingStars,
                    RatingComment = dataToSend.RatingComment,
                    MemberId = dataToSend.MemberId,
                    GuiderId = dataToSend.GuiderId,
                    OrderId = dataToSend.OrderId
                };

                Console.WriteLine("-------------------MemberId: " + dataToSend.MemberId);
                Console.WriteLine("-------------------OrderId: " + dataToSend.OrderId);
                Console.WriteLine("-------------------GuiderId: " + dataToSend.GuiderId);
                Console.WriteLine("--------------------RatingComment: " + dataToSend.RatingComment);
                Console.WriteLine("---------------------RatingStars: " + dataToSend.RatingStars);

                // 添加評價到 DbContext
                _context.Ratings.Add(rating);

                // 保存到資料庫
                _context.SaveChanges();

                Console.WriteLine("Creating rating: " + Newtonsoft.Json.JsonConvert.SerializeObject(rating));

                return Ok("評價提交成功");
            }
            catch (DbUpdateException ex)
            {
                // 處理資料庫更新相關的異常
                Console.WriteLine("DbUpdateException: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
                return StatusCode(500, "資料庫更新異常：" + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
                return StatusCode(500, "伺服器錯誤：" + ex.Message);
            }
        }










        //判斷該會員是否真的有該筆優惠券
        [HttpPost]
        public IActionResult ValidateCoupon(string couponCode, int memberId, decimal orderTotal)
        {
            var coupon = _context.Coupons
                .FirstOrDefault(c => c.MemberId == memberId && c.CouponCode == couponCode);

            if (coupon != null)
            {
                // 這裡假設優惠券金額是固定的 50 元，您可以根據實際情況進行調整
                return Json(new { isValid = true, discountAmount = 50m });
            }
            else
            {
                return Json(new { isValid = false });
            }
        }



        public ActionResult MemberCheckOut(int orderID)
        {

            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }

            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

            var latestOrder = _context.Orderlists
                             .Where(o => o.MemberId == member.MemberId)
                             .OrderByDescending(o => o.OrderId)
                             .Select(o => o.OrderId)
                             .FirstOrDefault();


            var orderTotalPrice = _context.Orderlists
                            .Where(o => o.OrderId == latestOrder)
                            .OrderByDescending(o => o.OrderId)                            
                            .Select(o => o.OrderTotalPrice)
                            .FirstOrDefault();
          

            var newOrder = (from o in _context.Orderlists
                            join i in _context.Itineraries on o.ItineraryId equals i.ItineraryId
                            join id in _context.ItineraryDetails on i.ItineraryId equals id.ItineraryId
                            join s in _context.Spots on id.SpotId equals s.SpotId
                            join c in _context.Coupons on o.CouponId equals c.CouponId into couponGroup

                            from c in couponGroup.DefaultIfEmpty()
                            where o.MemberId == member.MemberId && o.OrderId == latestOrder
                            orderby o.OrderId descending

                            select new
                            {
                                o.OrderId,
                                o.MemberId,
                                o.OrderNumber,
                                o.OrderDate,
                                c.CouponCode,
                                o.OrderTotalPrice,
                                i.ItineraryId,
                                i.ItineraryName,
                                i.ItineraryStartDate,
                                i.ItineraryPeopleNo,
                                s.SpotId,
                                s.SpotName,
                                s.SpotCity,
                                id.VisitOrder,
                                id.ItineraryDate
                            }).ToList();



            // 查詢會員的所有優惠券
            var memberCoupons = _context.Coupons
                .Where(c => c.MemberId == member.MemberId)
                .ToList();


            var order = new OrderDetail
            {
                Orders = newOrder.Select(o => new Orderlist
                {
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    OrderTotalPrice = o.OrderTotalPrice,
                    OrderId = o.OrderId,
                    Itinerary = new Itinerary
                    {
                        ItineraryStartDate = o.ItineraryStartDate,
                        ItineraryName = o.ItineraryName,
                        ItineraryPeopleNo = o.ItineraryPeopleNo,
                    },
                    Coupon = new Coupon
                    {
                        CouponCode = o.CouponCode
                    },
                    Spots = new Spot
                    {
                        SpotId = o.SpotId,
                        SpotCity = o.SpotCity,
                        SpotName = o.SpotName
                    },
                    ItineraryDetail = new ItineraryDetail
                    {
                        ItineraryId = o.ItineraryId,
                        VisitOrder = o.VisitOrder,
                        ItineraryDate = o.ItineraryDate
                    }
                }).ToList(),
                MemberCoupons = memberCoupons,  // 添加會員的所有優惠券
                MemberId = member.MemberId,
                OrderId = latestOrder
            };
            return View(order);
        }


        public ActionResult PaySuccess()
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果會話中沒有用戶信息，重定向到登錄頁面
            }
            return View();
        }









        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MemberId,MemberEmail,MemberName,MemberBirthDate,MemberPhone,MemberPassword")] Member member)
        {
            if (ModelState.IsValid)
            {
                _context.Add(member);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MemberId,MemberEmail,MemberName,MemberBirthDate,MemberPhone,MemberPassword")] Member member)
        {
            if (id != member.MemberId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.MemberId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(member);
        }






        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.MemberId == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }






        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }





        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }
    }
}
