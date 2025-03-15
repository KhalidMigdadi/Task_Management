using ABK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABK.Controllers
{
    public class CRUDController : Controller
    {
        private readonly MyDbContext _context;

        public CRUDController(MyDbContext context)
        {
            _context = context;
        }

        // Show only the logged-in user's tasks
        public IActionResult Show()
        {
            int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("HandelLogin", "Auth");
            }

            var data = _context.ToDoTasks
                .Where(t => t.UserId == userId)
                .Include(t => t.Category) // Include category if needed
                .ToList();

            return View(data);
        }

        // Create Task - Show Form
        public IActionResult Create()
        {
            int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("HandelLogin", "Auth");
            }

            ViewBag.Categories = _context.Categories.ToList(); // Pass categories to view if needed
            return View();
        }

        // Handle Task Creation
        [HttpPost]
        public IActionResult HandelCreate(ToDoTask task)
        {
            int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("HandelLogin", "Auth");
            }

            task.UserId = userId.Value; // Assign logged-in user ID
            task.CreatedAt = DateTime.Now; // Set creation date

            _context.ToDoTasks.Add(task);
            _context.SaveChanges();

            return RedirectToAction("Show");
        }

        // Edit Task
        public IActionResult Edit(int Id)
        {
            int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("HandelLogin", "Auth");
            }

            var task = _context.ToDoTasks.FirstOrDefault(t => t.Id == Id && t.UserId == userId);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categories.ToList(); // Pass categories to view
            return View(task);
        }

        [HttpPost]
        public IActionResult HandelEdit(ToDoTask edit)
        {
            int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("HandelLogin", "Auth");
            }

            var existingTask = _context.ToDoTasks.FirstOrDefault(t => t.Id == edit.Id && t.UserId == userId);
            if (existingTask == null)
            {
                return NotFound();
            }

            // Update all fields
            existingTask.Title = edit.Title;
            existingTask.Description = edit.Description;
            existingTask.DueDate = edit.DueDate;
            existingTask.StartDate = edit.StartDate;
            existingTask.StartTime = edit.StartTime;
            existingTask.EndTime = edit.EndTime;
            existingTask.Status = edit.Status;
            existingTask.CategoryId = edit.CategoryId;

            _context.ToDoTasks.Update(existingTask);
            _context.SaveChanges();

            return RedirectToAction("Show");
        }

        // Delete Task
        public IActionResult Delete(int Id)
        {
            int? userId = HttpContext.Session.GetInt32("id");
            if (userId == null)
            {
                return RedirectToAction("HandelLogin", "Auth");
            }

            var task = _context.ToDoTasks.FirstOrDefault(t => t.Id == Id && t.UserId == userId);
            if (task == null)
            {
                return NotFound();
            }

            var logs = _context.Logs.Where(l => l.TaskId == Id).ToList();
            _context.Logs.RemoveRange(logs);
            _context.ToDoTasks.Remove(task);
            _context.SaveChanges();

            return RedirectToAction("Show");
        }
    }
}
