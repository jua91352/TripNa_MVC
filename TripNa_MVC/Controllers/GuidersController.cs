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




            if (ModelState.IsValid)
            {
                _context.Add(guider);
                await _context.SaveChangesAsync();
                return Redirect("/Members/MemberCenter");

        }
            return Redirect("/Home/Login");
    }







        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> SignUp([Bind("GuiderId,GuiderNickname,GuiderGender,GuiderArea,GuiderStartDate,GuiderIntro")] Guider guider)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        // Check if the email already exists in the database
        //        var existingGuider = await _context.Guiders.FirstOrDefaultAsync(m => m.GuiderId == guider.GuiderId);

        //        if (existingGuider != null)
        //        {
        //            ViewData["Message"] = "此帳號已存在";
        //            return View();
        //        }

        //        _context.Add(guider);
        //        await _context.SaveChangesAsync();
        //        return Redirect("/Members/MemberCenter");
        //    }
        //    return View();
        //}






       
                var memberEmail = HttpContext.Session.GetString("memberEmail");


                var memberContext = _context.Members.FirstOrDefault(m => m.MemberEmail == memberEmail);

                if (memberContext != null)
                {

                    _context.Add(guider);
                    await _context.SaveChangesAsync();
                    memberContext.GuiderId = guider.GuiderId;
                    await _context.SaveChangesAsync();

                    string guiderImageFileName = $"{guider.GuiderNickname}{guider.GuiderId}.jpg";
                    string guiderVertFileName = $"{guider.GuiderNickname}{guider.GuiderId}_cert.jpg"; // 導遊證文件名


                    // 保存正面照片
                    if (guiderImage != null && guiderImage.Length > 0)
                    {
                        var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "導遊/大頭照", guiderImageFileName);
                        using (var stream = new FileStream(imagePath, FileMode.Create))
                        {
                            await guiderImage.CopyToAsync(stream);
                        }
                    }

                    // 保存導遊證
                    if (guiderVert != null && guiderVert.Length > 0)
                    {
                        var vertPath = Path.Combine(_hostingEnvironment.WebRootPath, "導遊/證照", guiderVertFileName);
                        using (var stream = new FileStream(vertPath, FileMode.Create))
                        {
                            await guiderVert.CopyToAsync(stream);
                        }
                    }

                }

                return Redirect("/members/membercenter");
            }
            
            return View("home");
        }

        

       
        // GET: Guiders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Guiders.ToListAsync());
        }

        // GET: Guiders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guider = await _context.Guiders
                .FirstOrDefaultAsync(m => m.GuiderId == id);
            if (guider == null)
            {
                return NotFound();
            }

            return View(guider);
        }

        // GET: Guiders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Guiders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GuiderId,GuiderNickname,GuiderGender,GuiderArea,GuiderStartDate,GuiderIntro")] Guider guider)
        {
            if (ModelState.IsValid)
            {
                _context.Add(guider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(guider);
        }

        // GET: Guiders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guider = await _context.Guiders.FindAsync(id);
            if (guider == null)
            {
                return NotFound();
            }
            return View(guider);
        }

        // POST: Guiders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GuiderId,GuiderNickname,GuiderGender,GuiderArea,GuiderStartDate,GuiderIntro")] Guider guider)
        {
            if (id != guider.GuiderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(guider);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GuiderExists(guider.GuiderId))
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
            return View(guider);
        }

        // GET: Guiders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guider = await _context.Guiders
                .FirstOrDefaultAsync(m => m.GuiderId == id);
            if (guider == null)
            {
                return NotFound();
            }

            return View(guider);
        }

        // POST: Guiders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var guider = await _context.Guiders.FindAsync(id);
            if (guider != null)
            {
                _context.Guiders.Remove(guider);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GuiderExists(int id)
        {
            return _context.Guiders.Any(e => e.GuiderId == id);
        }
    }
}
