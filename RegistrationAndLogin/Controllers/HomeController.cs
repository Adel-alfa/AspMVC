using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RegistrationAndLogin.Models;

namespace RegistrationAndLogin.Controllers
{
    public class HomeController : Controller
    {
        private StudentContext studentContext = new StudentContext();

        // GET: Home
        [Authorize]
        public ActionResult Index()
        {
            try
            {
                string studentId = System.Web.HttpContext.Current.Session["StudentID"].ToString();
                var student = studentContext.students.Where(s => s.StudentId.Equals( studentId)).FirstOrDefault();
                if (student != null)

                {
                    string message = student.FirstName;
                    ViewBag.Message = message;

                    return View(student);
                }
            }
            catch (Exception )
            {
                return View();

            }
            return View();
        }
    }
}