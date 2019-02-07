using Logger;
using RegistrationAndLogin.Models;
using RegistrationAndLogin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;


namespace RegistrationAndLogin.Controllers
{
    public class StudentController : Controller
    {
        private ILog _ILog;
        public StudentController()
        {
            _ILog = Log.GetInstance;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            _ILog.LogException(filterContext.Exception.ToString());
            
            filterContext.ExceptionHandled = true;
            this.View("Error").ExecuteResult(this.ControllerContext);

            

        }
        // Student Registration
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }
        // Student Registration post action 
        [HttpPost()]
        [AllowAnonymous]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")]StudentViewModel studentVM)
        {
            Boolean status = false;
            string message = "";
            var student = (Student)studentVM;
            if (!ModelState.IsValid)
                message = "Invalid request";
            else
            {
                

               // verify the Id => is already exist
                if (IsRegistrationNumberExist(student.StudentId))
                {
                    message = "This Student Number is already registred";
                    ModelState.AddModelError("StudentExist", message);
                    ViewBag.Message = message;
                    return View("Error");
                }
                // verify the email => is already exist 

                if (IsEmailExist(student.EmailId))
                {
                    message = "This email is already registred";
                    ModelState.AddModelError("EmailExist", message);
                    ViewBag.Message = message;
                    return View("Error");
                }
                //Generate activation code
                student.ActivationCode = Guid.NewGuid();
                // Password hashing and salting
                student.Password = Cryptography.GenerateKeyHash(student.Password);
                studentVM.ConfirmPassword = Cryptography.GenerateKeyHash(studentVM.ConfirmPassword);
                student.IsEmailVerified = false;
                 
                // Save student data in  DB
                using(StudentContext studentContext = new StudentContext())
                {
                    studentContext.students.Add(student);
                    studentContext.SaveChanges();

                    //Send registration confirmatiom email to student

                    SendVerifcationLinkEmail(student.EmailId, student.ActivationCode.ToString());
                    message = "Registration successfully done. Account activation link" +
                        " has bneen sent to your email id: " + student.EmailId;
                    status = true;
                }
            }

            ViewBag.Message = message;
            ViewBag.Status = status;

            return View(student);                      
        }
        
        // Verify Student Account
          [HttpGet]
          public ActionResult VerifyStudentAccount(string  id)
        {
            bool status = false;
            using(StudentContext studentContext = new StudentContext())
            {
                studentContext.Configuration.ValidateOnSaveEnabled = false;
                var student = studentContext.students.Where(s => s.ActivationCode == new Guid(id)).FirstOrDefault();
                if(student != null)
                {
                    student.IsEmailVerified = true;
                    studentContext.SaveChanges();
                    status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = status;
            return View();
        }

        // Student Login Action
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // Student Login POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(StudentLogin login, string ReturnUrl= "")
        {
            string message = "";
            using (StudentContext studentContext = new StudentContext())
            {
                var student = studentContext.students.Where(s => s.EmailId == login.EmailId).FirstOrDefault();
                if (student != null)
                {
                    

                        if (Cryptography.ValidatePasswords(student.Password, login.Password))
                        {
                            if (student.IsEmailVerified == true)
                            {
                                    // The Remember me cookie
                                    int timeOut = login.RememberMe ? 525600 : 5; //(365d * 24h * 60m ) 
                                    var ticket = new FormsAuthenticationTicket(login.EmailId, login.RememberMe, timeOut);
                                    string encrypted = FormsAuthentication.Encrypt(ticket);
                                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                                    cookie.Expires = DateTime.Now.AddMinutes(timeOut);
                                    cookie.HttpOnly = true;

                                    System.Web.HttpContext.Current.Session["StudentID"] = student.StudentId.ToString();

                                    System.Web.HttpContext.Current.Session["StudentName"] = student.LastName.ToString();

                                    Response.Cookies.Add(cookie);
                                    if (Url.IsLocalUrl(ReturnUrl))
                                    {
                                        return Redirect(ReturnUrl);
                                    }
                                    else
                                    {
                                        return RedirectToAction("Index", "Home", student.StudentId);
                                    }


                            }
                            else
                            {
                                message = "Email address need to be virified";
                            }
                    }
                    else
                    {
                        message = "Invalid Password";
                    }
                
                }
                else
                {
                    message = "Invalid User name ";

                }
            }
                ViewBag.Message = message;
                return View();
        }


        // Logout
        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            System.Web.HttpContext.Current.Session["StudentID"] = string.Empty;
            System.Web.HttpContext.Current.Session["StudentName"] = string.Empty;
            return RedirectToAction("Login", "Student");
           
        }

        // Forget Password

        public ActionResult ForgetPassword()
        {
            return View();
        }

        // Forget Password Post Action
        [HttpPost]
        public ActionResult ForgetPassword(string EmailID)
        {
            string message = "";
            bool status = false;
            using (StudentContext studentContext = new StudentContext())
            {
                var student = studentContext.students.Where(s => s.EmailId == EmailID).FirstOrDefault();
                if (student != null)
                {
                    // send email to rest the password
                    string restCode = Guid.NewGuid().ToString();
                    SendVerifcationLinkEmail(student.EmailId, restCode, "ResetPassword");
                    student.ResetPasswordCode = restCode;
                    studentContext.Configuration.ValidateOnSaveEnabled = false;
                    studentContext.SaveChanges();
                     status = true;
                    message = "Reset password link has been sent to your email.";
                }
                else
                    message = "The email address  is not registered in the system.";
            }
            ViewBag.Message = message;
            ViewBag.Status = status;
            return View();
        }
         public ActionResult ResetPassword(string id)
        {
            using(StudentContext studentContext = new StudentContext())
            {
                var student = studentContext.students.Where(s => s.ResetPasswordCode == id).FirstOrDefault();
                if (student != null)
                {
                    ResetPasswordModel model = new ResetPasswordModel();
                    model.RestCode = id;
                    return View(model);
                }
                else
                {
                    return HttpNotFound();
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            var message = " ";
            bool status = false;
            if (ModelState.IsValid)
            {
                using (StudentContext studentContext = new StudentContext())
                {
                    var student = studentContext.students.Where(s => s.ResetPasswordCode == model.RestCode).FirstOrDefault();
                    if (student != null)
                    {
                        student.Password = Cryptography.GenerateKeyHash(model.NewPassword);
                        student.ResetPasswordCode = "";
                        studentContext.Configuration.ValidateOnSaveEnabled = false;
                        studentContext.SaveChanges();
                        status = true;
                        message = " Password has been updated successfully";
                    }
                }
            }
            else
                message = "somthing invalid";

            ViewBag.Status = status;
            ViewBag.Message = message;
            return View();
        }

        //  verify either the Student Number exists or not
        public bool IsRegistrationNumberExist(int studentId)
        {
            using (StudentContext studentContext = new StudentContext())
            {
                var student = studentContext.students.Where(s => s.StudentId == studentId).FirstOrDefault();
                return student != null;
            }
        }

        //  verify either the email exists or not
        public bool IsEmailExist(string email)
        {
            using (StudentContext studentContext = new StudentContext())
            {
                var student = studentContext.students.Where(s => s.EmailId == email).FirstOrDefault();
                return student !=null;
            }
        }
        //  Send aVerifcation Link Email
        public void SendVerifcationLinkEmail(string email, string activationCode, string emailFor = "VerifyStudentAccount")
        {
            var verifyUrl="/Student/"+emailFor+"/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("YourEmail@gmail.com", "The name of the sender");// Enter your details
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "YourEmailPassword"; // Enter your Password
            string subject = "";
            string body = "";
            if (emailFor == "VerifyStudentAccount")
            {
                subject = "Your account is successfully created!";

                body = "<br/><br/> We are excited to tell you that your account is " +
                   "successfully created. Please click on the below link ot verify your account" +
                   " <br/><br/><a href= '" + link + "'>" + link + "<a/>";

            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Passowrd!";
                body = "Hi, <br/><br/> We got request for reset your account password.  Please click on the below link ot reset your password" +
                     " <br/><br/><a href= " + link + ">Reset Password link<a/>";
            }
        try
            {

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            
                using (var message = new MailMessage(fromEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                    smtp.Send(message);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                throw ex;
            }

        }


    }
}
