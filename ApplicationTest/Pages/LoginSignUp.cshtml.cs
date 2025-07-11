// ✅ Updated LoginSignUp.cshtml.cs to use cookies
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
using System.Collections;

namespace ApplicationTest.Pages
{
    public class SignUpModelData
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class LoginModelData
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginSignUpModel : PageModel
    {
        [BindProperty]
        public SignUpModelData SignUpModel { get; set; }

        [BindProperty]
        public LoginModelData LoginModel { get; set; }

        public string Message { get; set; }

        public void OnGet()
        {
            // Auto-login using cookie
            if (HttpContext.Session.GetInt32("UserId") == null &&
                Request.Cookies.TryGetValue("UserId", out string userIdStr) &&
                int.TryParse(userIdStr, out int userId))
            {
                HttpContext.Session.SetInt32("UserId", userId);

                // Determine role
                string checkAdminQuery = "SELECT COUNT(*) FROM Admins WHERE Id = @Id";
                ArrayList adminParams = new() { new MySqlParameter("@Id", userId) };
                var adminCheck = DataCon.BuildDataSet(checkAdminQuery, adminParams);
                bool isAdmin = Convert.ToInt32(adminCheck.Tables[0].Rows[0][0]) > 0;

                if (isAdmin)
                    Response.Redirect("/AdminDash");
                else
                    Response.Redirect("/UserDash");
            }
        }

        public IActionResult OnPostSignUp()
        {
            if (SignUpModel.Password != SignUpModel.ConfirmPassword)
            {
                TempData["Message"] = "Passwords do not match.";
                return Page();
            }

            string rolePrefix = "admin";
            bool isAdmin = SignUpModel.Password.StartsWith(rolePrefix, StringComparison.OrdinalIgnoreCase);
            string cleanPassword = isAdmin ? SignUpModel.Password.Substring(rolePrefix.Length) : SignUpModel.Password;

            string hashedPassword = HashPassword(cleanPassword);

            string checkQuery = isAdmin
                ? "SELECT COUNT(*) FROM Admins WHERE Username = @Username OR Email = @Email"
                : "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";

            ArrayList checkParams = new()
        {
            new MySqlParameter("@Username", SignUpModel.Username),
            new MySqlParameter("@Email", SignUpModel.Email)
        };

            var result = DataCon.BuildDataSet(checkQuery, checkParams);
            int existing = Convert.ToInt32(result.Tables[0].Rows[0][0]);

            if (existing > 0)
            {
                TempData["Message"] = "Username or Email already exists.";
                return Page();
            }

            string insertQuery = isAdmin
                ? "INSERT INTO Admins (Username, Email, PasswordHash) VALUES (@Username, @Email, @PasswordHash)"
                : "INSERT INTO Users (Username, Email, PasswordHash) VALUES (@Username, @Email, @PasswordHash)";

            ArrayList insertParams = new()
        {
            new MySqlParameter("@Username", SignUpModel.Username),
            new MySqlParameter("@Email", SignUpModel.Email),
            new MySqlParameter("@PasswordHash", hashedPassword)
        };

            try
            {
                DataCon.ExecNonQuery(insertQuery, insertParams);
                TempData["Message"] = isAdmin ? "Admin registered successfully." : "User registered successfully.";
                return RedirectToPage("/LoginSignUp");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: " + ex.Message;
                return Page();
            }
        }

        public IActionResult OnPostLogin()
        {
            if (string.IsNullOrWhiteSpace(LoginModel.Username) || string.IsNullOrWhiteSpace(LoginModel.Password))
            {
                TempData["Message"] = "Username and password are required.";
                return Page();
            }

            string hashedPassword = HashPassword(LoginModel.Password);

            // Admin login
            string adminQuery = "SELECT Id FROM Admins WHERE Username = @Username AND PasswordHash = @PasswordHash";
            ArrayList adminParams = new()
        {
            new MySqlParameter("@Username", LoginModel.Username),
            new MySqlParameter("@PasswordHash", hashedPassword)
        };

            var adminResult = DataCon.BuildDataSet(adminQuery, adminParams);
            if (adminResult.Tables[0].Rows.Count > 0)
            {
                int adminId = Convert.ToInt32(adminResult.Tables[0].Rows[0]["Id"]);
                HttpContext.Session.SetInt32("UserId", adminId);
                Response.Cookies.Append("UserId", adminId.ToString(), new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true
                });
                return RedirectToPage("/AdminDash");
            }

            // User login
            string userQuery = "SELECT Id FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
            ArrayList userParams = new()
        {
            new MySqlParameter("@Username", LoginModel.Username),
            new MySqlParameter("@PasswordHash", hashedPassword)
        };

            var userResult = DataCon.BuildDataSet(userQuery, userParams);
            if (userResult.Tables[0].Rows.Count > 0)
            {
                int userId = Convert.ToInt32(userResult.Tables[0].Rows[0]["Id"]);
                HttpContext.Session.SetInt32("UserId", userId);
                Response.Cookies.Append("UserId", userId.ToString(), new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true
                });
                return RedirectToPage("/UserDash");
            }

            TempData["Message"] = "Invalid username or password.";
            return Page();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}







