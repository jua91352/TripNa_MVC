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
using Microsoft.EntityFrameworkCore;
using TripNa_MVC.Models;
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

        // GET: Members
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.Members.ToListAsync());
        //}


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

            var orderDetails =( from o in _context.Orderlists
                               join m in _context.Members on o.MemberId equals m.MemberId
                                from g in _context.Guiders.Where(x => x.GuiderId == (int?)o.GuiderId).DefaultIfEmpty()
                                join i in _context.Itineraries on o.ItineraryId equals i.ItineraryId
                               join a in _context.ItineraryDetails on o.ItineraryId equals a.ItineraryId
                               join c in _context.Coupons on o.MemberId equals c.MemberId
                               join r in _context.Ratings on o.MemberId equals r.MemberId
                               where  o.MemberId == member.MemberId  && o.OrderId == orderID
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
                                join c in _context.Coupons on o.MemberId equals c.MemberId
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



            //join q in _context.MemberQuestions on o.MemberId equals q.MemberId


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
















        




        //step1 : 網頁導入傳值到前端
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


            string TotalAmount = (Decimal.ToInt32(orderTotalPrice)).ToString();

            var orderId = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 20);

            //var orderId = "EC" + latestOrder + ;

            //需填入你的網址
            var website = $"http://localhost:5226/";
            //var neworder = new OrderDetail
            //{
            //    MerchantTradeNo = orderId,
            //    MerchantTradeDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
            //    TotalAmount = TotalAmount,
            //    TradeDesc = "無",
            //    ItemName = "測試商品",
            //    ExpireDate = "3",
            //    CustomField1 = "",
            //    CustomField2 = "",
            //    CustomField3 = "",
            //    CustomField4 = "",
            //    ReturnURL = $"{website}/api/Ecpay/AddPayInfo",
            //    OrderResultURL = $"{website}/Members/PayInfo/{orderId}",
            //    PaymentInfoURL = $"{website}/api/Ecpay/AddAccountInfo",
            //    ClientRedirectURL = $"{website}/Members/AccountInfo/{orderId}",
            //    MerchantID = "2000132",
            //    IgnorePayment = "GooglePay#WebATM#CVS#BARCODE",
            //    PaymentType = "aio",
            //    ChoosePayment = "ALL",
            //    EncryptType = "1"
            //};

            //檢查碼
            //neworder.CheckMacValue = GetCheckMacValue(ConvertToDictionary(neworder));
            return View(order);
        }



        private Dictionary<string, string> ConvertToDictionary(OrderCheckOut neworder)
        {
            var dictionary = new Dictionary<string, string>
    {
        { "MerchantTradeNo", neworder.MerchantTradeNo },
        { "MerchantTradeDate", neworder.MerchantTradeDate },
        { "TotalAmount", neworder.TotalAmount },
        { "TradeDesc", neworder.TradeDesc},
        { "ItemName", neworder.ItemName},
        { "ExpireDate", neworder.ExpireDate},
        { "CustomField1", neworder.CustomField1},
        { "CustomField2", neworder.CustomField2},
        { "CustomField3", neworder.CustomField3},
        { "CustomField4", neworder.CustomField4},
        { "ReturnURL", neworder.ReturnURL},
        { "OrderResultURL", neworder.OrderResultURL},
        { "PaymentInfoURL", neworder.PaymentInfoURL},
        { "ClientRedirectURL", neworder.ClientRedirectURL},
        { "MerchantID", neworder.MerchantID},
        { "PaymentType", neworder.PaymentType},
        { "ChoosePayment", neworder.ChoosePayment},
        { "EncryptType", neworder.EncryptType},
        { "CheckMacValue", neworder.CheckMacValue}

        
    };

            return dictionary;
        }



        /// <summary>
        /// 產生檢查碼。
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        //private string BuildCheckMacValue(string parameters, int encryptType = 0)
        //{
        //    string szCheckMacValue = String.Empty;
        //    // 產生檢查碼。
        //    var HashKey = "5294y06JbISpM5x9";
        //    var HashIV = "v77hoKGq4kWxNNIS";

        //    szCheckMacValue = String.Format("HashKey={0}{1}&HashIV={2}", HashKey, parameters, HashIV);
        //    szCheckMacValue = HttpUtility.UrlEncode(szCheckMacValue).ToLower();
        //    if (encryptType == 1)
        //    {
        //        szCheckMacValue = SHA256Encoder.Encrypt(szCheckMacValue);
        //    }
        //    else
        //    {
        //        szCheckMacValue = MD5Encoder.Encrypt(szCheckMacValue);
        //    }

        //    return szCheckMacValue;
        //}


        //private string GetCheckMacValue(Dictionary<string, string> order)
        //{
        //    var param = order.Keys.OrderBy(x => x).Select(key => key + "=" + order[key]).ToList();
        //    string checkValue = string.Join("&", param);
        //    //測試用的 HashKey
        //    var HashKey = "5294y06JbISpM5x9";
        //    //測試用的 HashIV
        //    var HashIV = "v77hoKGq4kWxNNIS";
        //    checkValue = $"HashKey={HashKey}" + "&" + checkValue + $"&HashIV={HashIV}";
        //    checkValue = HttpUtility.UrlEncode(checkValue).ToLower();
        //    checkValue = GetSHA256(checkValue);
        //    return checkValue.ToUpper();
        //}


        //private string GetSHA256(string value)
        //{
        //    var result = new StringBuilder();

        //    // 創建 SHA256 實例
        //    using (var sha256 = SHA256.Create())
        //    {
        //        // 將輸入字符串轉換為 UTF-8 編碼的字節數組
        //        var bytes = Encoding.UTF8.GetBytes(value);

        //        // 計算哈希值
        //        var hash = sha256.ComputeHash(bytes);

        //        // 將每個字節轉換為十六進制表示，並附加到結果字符串中
        //        for (int i = 0; i < hash.Length; i++)
        //        {
        //            result.Append(hash[i].ToString("X2")); // X2 表示以十六進制格式輸出，並且保證每個字節的表示都是兩位數
        //        }
        //    }

        //    // 返回計算得到的 SHA256 哈希值的字符串表示
        //    return result.ToString();
        //}



        //private string GetSHA256(string value)
        //{
        //    var result = new StringBuilder();
        //    var sha256 = SHA256Managed.Create();
        //    var bts = Encoding.UTF8.GetBytes(value);
        //    var hash = sha256.ComputeHash(bts);
        //    for (int i = 0; i < hash.Length; i++)
        //    {
        //        result.Append(hash[i].ToString("X2"));
        //    }
        //    return result.ToString();
        //}



        /// step5 : 取得付款資訊，更新資料庫
        [HttpPost]
        public ActionResult PayInfo(FormCollection id)
        {
            var data = new Dictionary<string, string>();
            foreach (string key in id.Keys)
            {
                data.Add(key, id[key]);
            }
            TripNaContext db = new TripNaContext();
            string temp = id["MerchantTradeNo"]; //寫在LINQ(下一行)會出錯，
            var ecpayOrder = db.EcpayOrders.Where(m => m.MerchantTradeNo == temp).FirstOrDefault();
            if (ecpayOrder != null)
            {
                ecpayOrder.RtnCode = int.Parse(id["RtnCode"]);
                if (id["RtnMsg"] == "Succeeded") ecpayOrder.RtnMsg = "已付款";
                ecpayOrder.PaymentDate = Convert.ToDateTime(id["PaymentDate"]);
                ecpayOrder.SimulatePaid = int.Parse(id["SimulatePaid"]);
                db.SaveChanges();
            }
            return View("EcpayView", data);
        }





        /// step5 : 取得虛擬帳號 資訊
        [HttpPost]
        public ActionResult AccountInfo(FormCollection id)
        {
            var data = new Dictionary<string, string>();
            foreach (string key in id.Keys)
            {
                data.Add(key, id[key]);
            }
            TripNaContext db = new TripNaContext();
            string temp = id["MerchantTradeNo"]; //寫在LINQ會出錯
            var ecpayOrder = db.EcpayOrders.Where(m => m.MerchantTradeNo == temp).FirstOrDefault();
            if (ecpayOrder != null)
            {
                ecpayOrder.RtnCode = int.Parse(id["RtnCode"]);
                if (id["RtnMsg"] == "Succeeded") ecpayOrder.RtnMsg = "已付款";
                ecpayOrder.PaymentDate = Convert.ToDateTime(id["PaymentDate"]);
                ecpayOrder.SimulatePaid = int.Parse(id["SimulatePaid"]);
                db.SaveChanges();
            }
            return View("EcpayView", data);
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
