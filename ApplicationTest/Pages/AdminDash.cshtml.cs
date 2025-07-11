using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Collections;
using MySql.Data.MySqlClient;

namespace ApplicationTest.Pages
{
    public class AdminDashModel : PageModel
    {
        public DataSet Users { get; set; }
        public DataSet UserCourses { get; set; }
        public DataSet AllSignups { get; set; }

        [BindProperty]
        public int SelectedUserId { get; set; }

        public void OnGet()
        {
            // Auto-login using cookie if session is empty
            if (HttpContext.Session.GetInt32("UserId") == null &&
                Request.Cookies.TryGetValue("UserId", out string userIdStr) &&
                int.TryParse(userIdStr, out int userId))
            {
                HttpContext.Session.SetInt32("UserId", userId);
            }

            // If still no user session, redirect to login
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                Response.Redirect("/LoginSignUp");
                return;
            }

            LoadUsers();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("UserId");
            TempData["Message"] = "You have been logged out.";
            return RedirectToPage("/LoginSignUp");
        }

        public IActionResult OnPostSave([FromForm] int index, [FromForm] string[] username, [FromForm] string[] email, [FromForm] int[] id)
        {
            int userId = id[index];
            string updatedUsername = username[index];
            string updatedEmail = email[index];

            string query = "UPDATE Users SET Username = @Username, Email = @Email WHERE Id = @Id";
            ArrayList parameters = new()
        {
            new MySqlParameter("@Username", updatedUsername),
            new MySqlParameter("@Email", updatedEmail),
            new MySqlParameter("@Id", userId)
        };

            DataCon.ExecNonQuery(query, parameters);
            LoadUsers();
            return Page();
        }

        public IActionResult OnPostDelete([FromForm] int id)
        {
            string query = "DELETE FROM Users WHERE Id = @Id";
            ArrayList parameters = new()
        {
            new MySqlParameter("@Id", id)
        };

            DataCon.ExecNonQuery(query, parameters);
            LoadUsers();
            return Page();
        }

        public IActionResult OnPostViewCourses(int userId)
        {
            SelectedUserId = userId;

            string query = @"
            SELECT C.Id, C.Name 
            FROM UserCourses UC
            JOIN Courses C ON UC.CourseId = C.Id
            WHERE UC.UserId = @UserId";

            ArrayList parameters = new()
        {
            new MySqlParameter("@UserId", userId)
        };

            UserCourses = DataCon.BuildDataSet(query, parameters);

            string allQuery = @"
            SELECT U.Username, C.Name 
            FROM UserCourses UC
            JOIN Users U ON UC.UserId = U.Id
            JOIN Courses C ON UC.CourseId = C.Id";

            AllSignups = DataCon.BuildDataSet(allQuery);

            LoadUsers();
            return Page();
        }

        private void LoadUsers()
        {
            string sql = "SELECT Id, Username, Email FROM Users";
            Users = DataCon.BuildDataSet(sql);
        }
    }
}

