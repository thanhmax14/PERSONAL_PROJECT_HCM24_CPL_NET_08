using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Lab01.Models
{
    public class User
    {
        [Key]
        public int ID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime joinin { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? Birthday { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Image { get; set; }
        public int? IdCard { get; set; }
        public int RoleID { get; set; } //!khoi tao them bien
        [ForeignKey("RoleID")]            //! ten database
        public Role role { get; set; }    //! khoi tao them model
        public bool UserStatus { get; set; }
        public int? AccessFailedCount { get; set; }
        public DateTime? LockOut { get; set; }
        public bool verifyAccount { get; set; }
        public string? ResetToken { get; set; }


    }
}