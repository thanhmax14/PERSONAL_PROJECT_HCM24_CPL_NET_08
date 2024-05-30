using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Lab01.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lab01.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDBContext _db;
        public AdminController(ApplicationDBContext db)
        {
            this._db = db;
        }
        public IActionResult ManagerUser()
        {
            string sql = "select * from users where RoleID = @RoleID and UserStatus != @UserStatus";
            List<User> listUser = this._db.Set<User>().FromSqlRaw(sql, new SqlParameter("@RoleID", 1),
            new SqlParameter("@UserStatus", 1)).ToList();
            ViewBag.listUser = listUser;
            return View();
        }

        [HttpPost]

        public IActionResult ManagerUser(InputData.InputModelUpdateInfo info)
        {
            string sql = "select * from users where RoleID = @RoleID and UserStatus != @UserStatus";
            List<User> listUser = this._db.Set<User>().FromSqlRaw(sql, new SqlParameter("@RoleID", 1),
            new SqlParameter("@UserStatus", 1)).ToList();
            ViewBag.listUser = listUser;

            User getInfo = this._db.users.FirstOrDefault(u => u.UserName == info.UserName);
            if (getInfo != null)
            {
                var checkExitPhone = this._db.users.FirstOrDefault(u => u.Phone == info.Phone && u.UserName != info.UserName);
                if (checkExitPhone != null)
                {
                    ModelState.AddModelError("Phone", $"Phone number {info.Phone}  already exists. Please enter another phone number!");
                    TempData["MessseErro"] = "Profile update was Fail!";

                    return View(info);
                }
                else
                {
                    getInfo.FirstName = info.FirstName;
                    getInfo.LastName = info.LastName;
                    getInfo.Phone = info.Phone;
                    getInfo.Birthday = info.Birthday;
                    getInfo.Gender = info.Gender;
                    getInfo.Address = info.Address;
                    if (ModelState.IsValid)
                    {
                        var update = this._db.users.Update(getInfo);
                        if (update != null)
                        {
                            this._db.SaveChanges();
                            TempData["Messse"] = "Profile update was successful!";
                            return View(info);
                        }
                        else
                        {
                            TempData["Messse"] = "Update failed, please try again!";
                            return View(info);
                        }
                    }
                }
            }
            return View(info);
        }

        public IActionResult GetInfoUser(int id)
        {
            var user = this._db.users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return Json(new
            {
                userName = user.UserName,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                phone = user.Phone,
                birthday = user.Birthday?.ToString("yyyy-MM-dd"),
                gender = user.Gender,
                address = user.Address
            });
        }

        public IActionResult ConfirmBan(int id)
        {
            User findUser = this._db.users.Find(id);
            if (findUser == null)
            {
                return Json(new { success = false });
            }
            else
            {
                findUser.UserStatus = true;
                var ban = this._db.users.Update(findUser);
                if (ban != null)
                {
                    this._db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
        }
        public IActionResult Restore(int id)
        {
            User findUser = this._db.users.Find(id);
            if (findUser == null)
            {
                return Json(new { success = false });
            }
            else
            {
                findUser.UserStatus = false;
                var ban = this._db.users.Update(findUser);
                if (ban != null)
                {
                    this._db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
        }

        public IActionResult ManagerHRM()
        {
            string sql = "select * from users where RoleID =@RoleID  and UserStatus !=@UserStatus";
            List<User> listHRM = this._db.Set<User>().FromSqlRaw(sql, new SqlParameter("@RoleID", 2),
            new SqlParameter("@UserStatus", 1)).ToList();
            ViewBag.listHRM = listHRM;
            return View();
        }
        [HttpPost]

        public IActionResult ManagerHRM(InputData.InputModelUpdateInfo info)
        {
            string sql = "select * from users where RoleID =@RoleID  and UserStatus !=@UserStatus";
            List<User> listHRM = this._db.Set<User>().FromSqlRaw(sql, new SqlParameter("@RoleID", 2),
            new SqlParameter("@UserStatus", 1)).ToList();
            ViewBag.listHRM = listHRM;
            User getInfo = this._db.users.FirstOrDefault(u => u.UserName == info.UserName);
            if (getInfo != null)
            {
                var checkExitPhone = this._db.users.FirstOrDefault(u => u.Phone == info.Phone && u.UserName != info.UserName);
                if (checkExitPhone != null)
                {
                    ModelState.AddModelError("Phone", $"Phone number {info.Phone}  already exists. Please enter another phone number!");
                    TempData["MessseErro"] = "Profile update was Fail!";

                    return View(info);
                }
                else
                {
                    getInfo.FirstName = info.FirstName;
                    getInfo.LastName = info.LastName;
                    getInfo.Phone = info.Phone;
                    getInfo.Birthday = info.Birthday;
                    getInfo.Gender = info.Gender;
                    getInfo.Address = info.Address;
                    if (ModelState.IsValid)
                    {
                        var update = this._db.users.Update(getInfo);
                        if (update != null)
                        {
                            this._db.SaveChanges();
                            TempData["Messse"] = "Profile update was successful!";
                            return View(info);
                        }
                        else
                        {
                            TempData["Messse"] = "Update failed, please try again!";
                            return View(info);
                        }
                    }
                }
            }
            return View(info);
        }
        public IActionResult ManagerBanUser()
        {
            string sql = "select * from users where RoleID =@RoleID and UserStatus = @UserStatus";
            List<User> listBanUser = this._db.Set<User>().FromSqlRaw(sql, new SqlParameter("@RoleID", 1),
            new SqlParameter("@UserStatus", 1)).ToList();

            ViewBag.listBanUser = listBanUser;
            return View();
        }
        public IActionResult ManagerBanHRM()
        {
            string sql = "select * from users where RoleID =@RoleID and UserStatus = @UserStatus";
            List<User> listBanHRM = this._db.Set<User>().FromSqlRaw(sql, new SqlParameter("@RoleID", 2),
            new SqlParameter("@UserStatus", 1)).ToList();
            ViewBag.listBanHRM = listBanHRM;

            return View();
        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}