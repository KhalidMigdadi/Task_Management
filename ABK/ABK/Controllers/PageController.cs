using ABK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABK.Controllers
{
    public class PageController : Controller
    {
        private readonly MyDbContext _context;

        public PageController(MyDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult about()
        {
            return View();
        }


        public IActionResult contact()
        {
            return View();
        }

        public IActionResult HandelContact(Feedback feedback)
        {
            if (ModelState.IsValid)
            {



                _context.Add(feedback);
                _context.SaveChanges();

            }


            ViewData["Message"] = "Your feedback has been sent successfully.";
            return View(nameof(contact));
        }

    }
}
