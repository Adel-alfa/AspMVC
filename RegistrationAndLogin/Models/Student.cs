using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace RegistrationAndLogin.Models
{
    public partial class Student
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.None)]

        public int StudentId { get; set; }

        public string FirstName { get; set; }

         
        public string LastName { get; set; }
        
        public string EmailId { get; set; }
        
        public string Password { get; set; }
        public bool IsEmailVerified { get; set; }
        public System.Guid ActivationCode { get; set; }
        public string ResetPasswordCode { get; set; }


    }

    public class StudentContext : DbContext
    {
        public DbSet<Student> students { get; set; }


    }
}