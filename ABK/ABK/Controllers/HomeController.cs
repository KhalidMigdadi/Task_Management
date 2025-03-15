using System.Buffers.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using ABK.Models;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ABK.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyDbContext _context;

        public HomeController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Get the logged-in user's ID from the session
            var userId = HttpContext.Session.GetInt32("id");

            if (userId == null)
            {
                // If the user is not logged in, redirect to the login page
                return RedirectToAction("Log_Reg", "User");
            }

            // Fetch tasks specific to the logged-in user
            var tasks = _context.ToDoTasks
                .Where(t => t.UserId == userId)
                .ToList();

            // Fetch counts for tasks based on their status for the logged-in user
            int pendingTasksCount = _context.ToDoTasks
                .Count(t => t.UserId == userId && (t.Status == "Pending" || t.Status == "pending"));
            int completedTasksCount = _context.ToDoTasks
                .Count(t => t.UserId == userId && (t.Status == "completed" || t.Status == "Completed"));
            int doingTasksCount = _context.ToDoTasks
                .Count(t => t.UserId == userId && (t.Status == "In Progress" || t.Status == "in progress"));

            int allTasks = _context.ToDoTasks
                .Count(t => t.UserId == userId);

            // Send data to the View
            ViewBag.toDo = tasks;
            ViewBag.PendingCount = pendingTasksCount;
            ViewBag.doingTasksCount = doingTasksCount;
            ViewBag.completedTasksCount = completedTasksCount;
            ViewBag.AllTasks = allTasks;

            // Fetch the current user's points (initialize to 0 if null)
            var user = _context.Users.Find(userId);
            int totalPoints = user?.Points ?? 0;  // Use 0 if Points is null

            // Calculate points based on completed tasks
            foreach (var task in tasks)
            {
                if (task.Status == "completed")
                {
                    // Increment points by 10 for each completed task
                    totalPoints += 10;
                }
            }

            // Update the user's points in the database
            if (user != null)
            {
                user.Points = totalPoints;
                _context.SaveChanges();
            }

            // Assuming 200 points are needed for a reward
            int pointsRequiredForReward = 200;
            int pointsLeft = pointsRequiredForReward - totalPoints;

            // Send the total points and points left to the view
            ViewBag.TotalPoints = totalPoints;
            ViewBag.PointsLeft = pointsLeft;

            return View(tasks);
        }


        public IActionResult Tables()
        {

            return View();
        }



















        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
