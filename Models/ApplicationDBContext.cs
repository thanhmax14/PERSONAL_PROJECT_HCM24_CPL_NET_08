using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Lab01.Models
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }
        public DbSet<User> users { get; set; }
        public DbSet<Role> roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Configure Primary Key using HasKey method
            modelBuilder.Entity<User>().HasKey(s => new { s.ID });

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    ID = 1,
                    UserName = "thanhmax14",
                    FirstName = "Pham",
                    LastName = "Quang Thanh",
                    Email = "phamquangthanhmax14@gmail.com",
                    Password = "827ccb0eea8a706c4c34a16891f84e7b",
                    UserStatus = false,
                    Phone = "0939371017",
                    AccessFailedCount = 0,
                    RoleID = 1,
                    joinin = DateTime.Now,
                    verifyAccount = true,
                    Address = "Lai Vung",
                    Gender = "Male",

                },
                 new User
                 {
                     ID = 2,
                     UserName = "HRM",
                     FirstName = "Le",
                     LastName = "Thi Kiwi",
                     Email = "phamquangthanhmax11@gmail.com",
                     Password = "827ccb0eea8a706c4c34a16891f84e7b",
                     UserStatus = false,
                     AccessFailedCount = 0,
                     Phone = "1254659899",
                     RoleID = 2,
                     joinin = DateTime.Now,
                     verifyAccount = true,
                     Address = "Lai Vung"
                 },
                  new User
                  {
                      ID = 3,
                      UserName = "admin",
                      FirstName = "Pham",
                      LastName = "Quang Thanh 1",
                      Email = "phamquangthanhmax124@gmail.com",
                      Password = "827ccb0eea8a706c4c34a16891f84e7b",
                      UserStatus = false,
                      Phone = "454976486525",
                      AccessFailedCount = 0,
                      RoleID = 3,
                      joinin = DateTime.Now,
                      verifyAccount = true,
                      Address = "Lai Vung"
                  }
            );
            modelBuilder.Entity<Role>().HasKey(s => new { s.ID });
            modelBuilder.Entity<Role>().HasData(
                new Role { ID = 1, Name = "User" },
                new Role { ID = 2, Name = "HRM" },
                   new Role { ID = 3, Name = "Admin" }
            );
        }


    }
}