using RegistrationAndLogin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RegistrationAndLogin.ViewModels
{
   
    public class StudentViewModel

    {
        [Display(Name = "Student Registration Number")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Registration Number required")]
        public int StudentId { get; set; }

        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "First Name required")]
        [StringLength(100, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last Name required")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; }

        [Display(Name = "Email ID")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email ID required")]
        [DataType(DataType.EmailAddress)]
        public string EmailId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimum 6 characters required")]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirm password must match password")]
        public string ConfirmPassword { get; set; }

        public static implicit operator StudentViewModel(Student student)
        {
            return new StudentViewModel
            {
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                EmailId = student.EmailId,
                Password = student.Password,
                ConfirmPassword =student.Password
            };
        }
        public static implicit operator Student(StudentViewModel studentVM)
        {
            return new Student
            {
                StudentId = (int)studentVM.StudentId,
                FirstName = studentVM.FirstName,
                LastName = studentVM.LastName,
                EmailId = studentVM.EmailId,
                Password = studentVM.Password,
            };
        }
    }
}