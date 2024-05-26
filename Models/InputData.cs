using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Lab01.Models
{
    public class InputData
    {
        public InputModelForgot InputForgot { get; set; }
        public InputModelLogin InputLogin { get; set; }
        public InputModelUpdateInfo inputInfo { get; set; }
        public InputModelRegister InputRegi { get; set; }
        public SecurityUser inputSercurity { get; set; }

        public InputModelResetPass InputReset { get; set; }
        public class InputModelLogin
        {

            [Required]
            public string UserName { get; set; }
            [Required]
            public string Password { get; set; }
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
        public class InputModelRegister
        {
            [Required]
            public string UserName { get; set; }
            [Required]
            [EmailAddress(ErrorMessage = "Invalid Email Address")]
            public string Email { get; set; }
            [Required]
            public string? Phone { get; set; }
            public int RoleID { get; set; }
            [Required]
            public string Password { get; set; }
            [NotMapped]  //! day la annotation khoi tao nhưng khong tạo bảng trong DTB
            [Required(ErrorMessage = "Password Confirmation is required.")]
            [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
            public string comfirmPass { get; set; }
        }
        public class InputModelForgot
        {
            [Required]
            public string Email { get; set; }
        }
        public class InputModelResetPass
        {
            [Required]
            public string Password { get; set; }
            [Required(ErrorMessage = "Password Confirmation is required.")]
            [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
            public string comfirmPass { get; set; }
        }

        public class InputModelUpdateInfo
        {
            [Required]
            public string UserName { get; set; }
            [Required]
            public string Email { get; set; }
            [Required]
            public string Phone { get; set; }
            [Required]
            public string FirstName { get; set; }
            [Required]
            public string LastName { get; set; }
            [Required]
            public DateTime Birthday { get; set; }
            [Required]
            public string Gender { get; set; }
            [Required]
            public string Address { get; set; }
        }
        public class SecurityUser
        {
            [Required]
            public string currentPass { get; set; }
            [Required]
            public string NewPass { get; set; }
            [Required(ErrorMessage = "Password Confirmation is required.")]
            [Compare("NewPass", ErrorMessage = "Password and confirmation password do not match.")]
            public string comfirmPass { get; set; }
        }
    }
}