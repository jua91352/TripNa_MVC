using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TripNa_MVC.Models;

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
        public async Task<IActionResult> Index()
        {
            return View(await _context.Members.ToListAsync());
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
            return RedirectToAction("UserCollect", "Members");
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
            var OrderId = _context.Orderlists.FirstOrDefault(i => i.OrderId == orderID);

            var orderDetails = from o in _context.Orderlists
                               join m in _context.Members on o.MemberId equals m.MemberId
                               join g in _context.Guiders on o.GuiderId equals g.GuiderId
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
                                   o.OrderPeopleNo,
                                   m.MemberName,
                                   m.MemberEmail,
                                   m.MemberPhone,
                                   ItineraryDetails = j,
                                   Spot = s,
                                   o.ItineraryId,
                                   a.VisitOrder,
                                   g.GuiderArea,
                                   o.OrderId
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
                    OrderPeopleNo = o.OrderPeopleNo,
                    OrderId = o.OrderId,
                    Itinerary = new Itinerary
                    {
                        ItineraryStartDate = o.ItineraryStartDate,
                        ItineraryName = o.ItineraryName,
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
                MemberId = member.MemberId
            };
            Console.WriteLine("--------------------");

            Console.WriteLine(orderID.ToString());
            Console.WriteLine("--------------------");

            return View(model);
        }






































        //public IActionResult UserOrderDetails(int orderID)
        //{
        //    var memberEmail = HttpContext.Session.GetString("memberEmail");
        //    if (string.IsNullOrEmpty(memberEmail))
        //    {
        //        return RedirectToAction("Login", "Home"); // 如果会话中没有用户信息，重定向到登录页面
        //    }

        //    var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

        //    if (member == null)
        //    {
        //        return NotFound();
        //    }

        //    var orderDetails = from o in _context.Orderlists
        //                       join m in _context.Members on o.MemberId equals m.MemberId
        //                       join g in _context.Guiders on o.GuiderId equals g.GuiderId
        //                       join i in _context.Itineraries on o.ItineraryId equals i.ItineraryId
        //                       join a in _context.ItineraryDetails on o.ItineraryId equals a.ItineraryId
        //                       join c in _context.Coupons on o.CouponId equals c.CouponId into couponGroup
        //                       from c in couponGroup.DefaultIfEmpty() // left join
        //                       join s in _context.Spots on a.SpotId equals s.SpotId
        //                       where o.MemberId == member.MemberId && o.OrderId == orderID
        //                       select new
        //                       {
        //                           o.OrderNumber,
        //                           o.OrderDate,
        //                           i.ItineraryStartDate,
        //                           o.OrderTotalPrice,
        //                           o.OrderStatus,
        //                           o.OrderMatchStatus,
        //                           c.CouponCode,
        //                           g.GuiderNickname,
        //                           i.ItineraryName,
        //                           o.OrderPeopleNo,
        //                           m.MemberName,
        //                           m.MemberEmail,
        //                           m.MemberPhone,
        //                           ItineraryDetails = a,
        //                           Spot = s,
        //                           o.ItineraryId,
        //                           a.VisitOrder,
        //                           o.OrderId
        //                       };

        //    var orderDetailsList = orderDetails.ToList();

        //    var model = new OrderDetail
        //    {
        //        Orders = orderDetailsList.Select(o => new Orderlist
        //        {
        //            OrderNumber = o.OrderNumber,
        //            OrderDate = o.OrderDate,
        //            OrderTotalPrice = o.OrderTotalPrice,
        //            OrderStatus = o.OrderStatus,
        //            OrderMatchStatus = o.OrderMatchStatus,
        //            OrderPeopleNo = o.OrderPeopleNo,
        //            OrderId = o.OrderId,
        //            Itinerary = new Itinerary
        //            {
        //                ItineraryStartDate = o.ItineraryStartDate,
        //                ItineraryName = o.ItineraryName,
        //                ItineraryDetails = new List<ItineraryDetail> { o.ItineraryDetails }
        //            },
        //            Coupon = new Coupon
        //            {
        //                CouponCode = o.CouponCode
        //            },
        //            Guider = new Guider
        //            {
        //                GuiderNickname = o.GuiderNickname
        //            },
        //            Member = new Member
        //            {
        //                MemberName = o.MemberName,
        //                MemberEmail = o.MemberEmail,
        //                MemberPhone = o.MemberPhone
        //            },
        //            Spots = o.Spot,
        //            ItineraryDetail = new ItineraryDetail
        //            {
        //                ItineraryId = o.ItineraryId,
        //                VisitOrder = o.VisitOrder
        //            }
        //        }).ToList(),
        //        MemberId = member.MemberId
        //    };

        //    return View(model);
        //}






















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
