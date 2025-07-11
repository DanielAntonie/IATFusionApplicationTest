using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections;
using System.Data;
using MySql.Data.MySqlClient;

namespace ApplicationTest.Pages
{
    public class UserDashModel : PageModel
    {
        public DataSet UserCourses { get; set; }
        public DataSet AllCourses { get; set; }

        [BindProperty]
        public int SelectedCourseId { get; set; }

        public int LoggedInUserId => Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));

        public void OnGet()
        {
            // Auto-login using cookie if session is empty
            if (HttpContext.Session.GetInt32("UserId") == null &&
                Request.Cookies.TryGetValue("UserId", out string userIdStr) &&
                int.TryParse(userIdStr, out int userId))
            {
                HttpContext.Session.SetInt32("UserId", userId);
            }

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                Response.Redirect("/LoginSignUp");
                return;
            }

            LoadUserCourses();
            LoadAllCourses();
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("UserId");
            TempData["Message"] = "You have been logged out.";
            return RedirectToPage("/LoginSignUp");
        }

        public IActionResult OnPostApply()
        {
            string query = "INSERT INTO UserCourses (UserId, CourseId) VALUES (@UserId, @CourseId)";
            ArrayList paramList = new()
        {
            new MySqlParameter("@UserId", LoggedInUserId),
            new MySqlParameter("@CourseId", SelectedCourseId)
        };

            try
            {
                DataCon.ExecNonQuery(query, paramList);
                TempData["Message"] = "Course applied successfully.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: " + ex.Message;
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int courseId)
        {
            string query = "DELETE FROM UserCourses WHERE UserId = @UserId AND CourseId = @CourseId";
            ArrayList paramList = new()
        {
            new MySqlParameter("@UserId", LoggedInUserId),
            new MySqlParameter("@CourseId", courseId)
        };

            try
            {
                DataCon.ExecNonQuery(query, paramList);
                TempData["Message"] = "Course removed.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error: " + ex.Message;
            }

            return RedirectToPage();
        }

        private void LoadUserCourses()
        {
            string query = @"
            SELECT c.Id, c.Name 
            FROM UserCourses uc
            JOIN Courses c ON uc.CourseId = c.Id
            WHERE uc.UserId = @UserId";

            ArrayList paramList = new()
        {
            new MySqlParameter("@UserId", LoggedInUserId)
        };

            UserCourses = DataCon.BuildDataSet(query, paramList);
        }

        private void LoadAllCourses()
        {
            string query = "SELECT Id, Name FROM Courses";
            AllCourses = DataCon.BuildDataSet(query);
        }
    }
}



