using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TripNa_MVC.Controllers
{
    public class ItineraryController : Controller
    {
        // GET: ItineraryController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ItineraryController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ItineraryController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ItineraryController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ItineraryController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ItineraryController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ItineraryController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ItineraryController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
