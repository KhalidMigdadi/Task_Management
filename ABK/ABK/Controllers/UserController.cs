using System.Net.Mail;
using System.Net;
using ABK.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;


namespace ABK.Controllers
{



    public class UserController : Controller
    {
        private readonly MyDbContext _context;

        public UserController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Log_Reg()
        {
            return View();
        }

        [HttpPost]
        public IActionResult HandelReg(string username, string email, string password)
        {

            if (ModelState.IsValid)
            {
                var user = new User
                {

                    Username = username,
                    Email = email,
                    Password = password

                };
                _context.Users.Add(user);
                _context.SaveChanges();
            }


            return RedirectToAction("Log_Reg");
        }


        [HttpPost]
        public IActionResult HandelLogin(string email, string password)
        {


            if (ModelState.IsValid)
            {
                foreach (var user in _context.Users)
                {

                    if (user.Email == email && user.Password == password)
                    {
                        HttpContext.Session.SetInt32("id", user.Id);
                        HttpContext.Session.SetString("username", user.Username);
                        HttpContext.Session.SetString("email", user.Email);
                        HttpContext.Session.SetString("password", password);
                        HttpContext.Session.SetString("image", user.Image ?? "https://via.placeholder.com/150"); // Default if no image

                        // Set the username in the ViewBag
                        ViewBag.Username = user.Username;
                        
                        return RedirectToAction("Index", "Home");
                    }

                }

            }
            ViewData["Message"] = "Invalid email or password!";
            return View("Log_Reg");
        }
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult HandelReset(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {

                ViewData["Message_1"] = "Email not found!";
                return View(nameof(ResetPassword));


            }


            Random rand = new Random();

            int verificationCode = rand.Next(100000, 999999);


            HttpContext.Session.SetString("ResetCode", verificationCode.ToString());
            HttpContext.Session.SetString("ResetEmail", email);


            SendEmailAsync(email, "Password Reset Code", $"Your reset code is: {verificationCode}");
            ViewData["Message_1"] = "A verification code has been sent to your email!";
            Console.WriteLine(email);

            return View(nameof(VerifyCode));

        }


        [HttpPost]
        public IActionResult HandelCode(string code)
        {
            var MyCode = HttpContext.Session.GetString("ResetCode");


            if (MyCode == null || MyCode != code)
            {
                ViewData["Message_2"] = "Invalid verification code!";
                return View("VerifyCode");
            }


            return View("NewPassword");
        }


        static async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Khalid", "cafeuse18@gmail.com"));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                message.Body = new TextPart("plain") { Text = body };

                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect, cancellationToken: new CancellationTokenSource(5000).Token);
                    await client.AuthenticateAsync("cafeuse18@gmail.com", "rare cvvw njkf xqwt\r\n");
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }







        [HttpPost]
        public IActionResult UpdatePassword(string newPassword, string confirmPassword)
        {
            var email = HttpContext.Session.GetString("ResetEmail"); // Get the email from session

            if (email == null)
            {
                ViewData["Message"] = "Session expired. Please try again.";
                return View("NewPassword");
            }

            if (newPassword != confirmPassword)
            {
                ViewData["Message"] = "Passwords do not match!";
                return View("NewPassword");
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewData["Message"] = "User not found!";
                return View("NewPassword");
            }

            // Store the new password (Consider hashing it)
            user.Password = newPassword; // Ideally, use hashed password
            _context.SaveChanges();

            ViewData["Message"] = "Password successfully updated!";
            return RedirectToAction("Log_Reg"); // Redirect user to login page
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }


        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult ChangePssword()
        {
            return View();
        }

       

        public IActionResult HandelChangePassword(int id, string Current_password, string New_password, string ReNew_password)
        {

            if (Current_password != HttpContext.Session.GetString("password"))
            {
                ViewData["Error_user"] = "Incorrect password!";
                return View(nameof(ChangePssword));    
            }
            else if (New_password != ReNew_password)
            {

                ViewData["Error_user"] = "New Password is not match Confirm password";
                return View(nameof(ChangePssword));
            }


            var user = new User
            {

                Id = id,
                Username = HttpContext.Session.GetString("username"),
                Email = HttpContext.Session.GetString("email"),
                Password = New_password,

            };

            _context.Update(user);
            _context.SaveChanges();
            ViewData["Error_user"] = "Change password has been successfully.";
            return View(nameof(Log_Reg));
        }



        public IActionResult EditProfile()
        {
            return View();
        }


        public IActionResult HandelEditProfile(string name, string email, int id, IFormFile profileImage)
        {
            HttpContext.Session.SetString("username", name);
            HttpContext.Session.SetString("email", email);

            var user = new User
            {
                Id = id,
                Username = name,
                Email = email,
                Password = HttpContext.Session.GetString("password"),
            };

            // Handle image upload if a new image is provided
            if (profileImage != null && profileImage.Length > 0)
            {
                // Save the image to a folder (e.g., wwwroot/images)
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", profileImage.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    profileImage.CopyTo(stream);
                }

                // Store the image path in session
                HttpContext.Session.SetString("image", "/images/" + profileImage.FileName);

                // Update the user's profile (optional)
                user.Image = "/images/" + profileImage.FileName;  // Add this if your User model has an 'Image' property
                _context.Update(user);
                _context.SaveChanges();
            }

            return View(nameof(Profile));
        }












        public IActionResult VerifyCode()
        {
            return View();
        }



        public IActionResult NewPassword()
        {
            return View();
        }


        // Logout action
        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();

            // Redirect to the HomeController's Index action
            return RedirectToAction("Log_Reg");
        }


    }
}
