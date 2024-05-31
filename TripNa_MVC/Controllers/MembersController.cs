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

            //var query = from o in _context.FavoriteSpots
            //            where o.MemberId == member.MemberId
            //            select o;

            var favoriteSpots = _context.FavoriteSpots
                                .Where(f => f.MemberId == member.MemberId)
                                .Select(f => f.SpotId)
                                .ToList();

            var favoriteSpotDetails = _context.Spots
                                              .Where(s => favoriteSpots.Contains(s.SpotId))
                                              .ToList();

            
            var model = new SpotViewModel
            {
                FavoriteSpots = favoriteSpotDetails
                //MemberId = memberId
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
        public async Task<IActionResult> UserCoupon()
        {
            var memberEmail = HttpContext.Session.GetString("memberEmail");
            if (string.IsNullOrEmpty(memberEmail))
            {
                return RedirectToAction("Login", "Home"); // 如果Session 裡面沒有東西 重導回主頁
            }
            var member = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);


            var query = from o in _context.Coupons
                        where o.MemberId == member.MemberId
                        select o;


            if (member == null)
            {
                return NotFound();
            }

            return View(await query.ToListAsync());

        }




        public IActionResult UserOrder()
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
