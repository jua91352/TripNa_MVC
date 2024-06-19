using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using TripNa_MVC.Models;

namespace TripNa_MVC.Controllers
{
    public class SpotsController : Controller
    {
        private readonly TripNaContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SpotsController(TripNaContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Spots
        //public async Task<IActionResult> Index(string searchString)
        //{

        //    var spots = from s in _context.Spots
        //                   select s;

        //    if (!String.IsNullOrEmpty(searchString))
        //    {
        //        spots.Where(s => s.SpotName.Contains(searchString)
        //                            || s.SpotBrief.Contains(searchString));
        //    }
        //    ViewData["CurrentFilter"] = searchString;
        //    return View(await _context.Spots.ToListAsync());
        //}

        public async Task<IActionResult> Index(string searchString, string city)
        {
            IQueryable<Spot> spots = _context.Spots;

            if (!String.IsNullOrEmpty(searchString))
            {
                spots = spots.Where(s => s.SpotName.Contains(searchString)
                                     || s.SpotBrief.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(city))
            {
                spots = spots.Where(s => s.SpotCity == city);
            }

ViewData["CurrentFilter"] = searchString;
            ViewBag.city = await _context.Spots.Select(a => new { Value = a.SpotCity, Text = a.SpotCity }).Distinct().ToListAsync();


            
            return View(await spots.ToListAsync());
        }






        // GET: Spots/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spot = await _context.Spots
                .FirstOrDefaultAsync(m => m.SpotId == id);
            if (spot == null)
            {
                return NotFound();
            }

            return View(spot);
        }

        // GET: Spots/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Spots/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpotId,SpotName,SpotCity,SpotBrief,SpotIntro")] Spot spot, IFormFile photo)
        {


            string spotImageFileName = $"{spot.SpotName}.jpg";
           
            if (photo != null && photo.Length > 0)
            {
                var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, "Images/Spots", spotImageFileName);
                //string directoryPath = Path.GetDirectoryName(imagePath);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }
            }


            //using (var stream = new FileStream(imagePath, FileMode.Create))
            //{
            //    await photo.CopyToAsync(stream);
            //}

            if (ModelState.IsValid)
            {
                _context.Add(spot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(spot);
        }
        // GET: Spots/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spot = await _context.Spots.FindAsync(id);
            if (spot == null)
            {
                return NotFound();
            }
            return View(spot);
        }

        // POST: Spots/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SpotId,SpotName,SpotCity,SpotBrief,SpotIntro")] Spot spot)
        {
            if (id != spot.SpotId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(spot);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpotExists(spot.SpotId))
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
            return View(spot);
        }

        // GET: Spots/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var spot = await _context.Spots
                .FirstOrDefaultAsync(m => m.SpotId == id);
            if (spot == null)
            {
                return NotFound();
            }

            return View(spot);
        }

        // POST: Spots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var spot = await _context.Spots.FindAsync(id);
            if (spot != null)
            {
                _context.Spots.Remove(spot);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SpotExists(int id)
        {
            return _context.Spots.Any(e => e.SpotId == id);
        }
    }
}
