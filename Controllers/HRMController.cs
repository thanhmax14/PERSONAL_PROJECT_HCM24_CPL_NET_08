using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lab01.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lab01.Controllers
{

    public class HRMController : Controller
    {
        private readonly ApplicationDBContext _db;
        public HRMController(ApplicationDBContext db)
        {
            this._db = db;
        }
        public IActionResult Manager()
        {
            string sql = "select * from users where RoleID = @RoleID and UserStatus != @UserStatus";
            List<User> listUser = this._db.Set<User>().FromSqlRaw(sql, new SqlParameter("@RoleID", 1),
            new SqlParameter("@UserStatus", 1)).ToList();
            ViewBag.listUser = listUser;
            return View();
        }
        [HttpPost]
        public IActionResult Manager(InputData.InputModelUpdateInfo info)
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
                    ModelState.AddModelError("Phone", "Phone number already exists. Please enter another phone number!");
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


        public IActionResult Profile()
        {

            var username = HttpContext.Session.GetString("UserName");
            if (username != null)
            {
                var user = _db.users.FirstOrDefault(u => u.UserName == username);
                if (user != null)
                {
                    var model = new InputData.InputModelUpdateInfo
                    {
                        UserName = user.UserName,
                        Email = user.Email,
                        Phone = user.Phone,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Birthday = user.Birthday ?? default(DateTime),
                        Gender = user.Gender,
                        Address = user.Address
                    };
                    ViewBag.Fullname = user.LastName + " " + user.FirstName;
                    ViewBag.Role = "HRM";
                    ViewBag.jonin = user.joinin;
                    ViewBag.img = user.Image;

                    return View(model);
                }
                return NotFound("User not found");
            }
            return BadRequest("Invalid session");
        }
        [HttpPost]
        public IActionResult Profile(InputData.InputModelUpdateInfo info)
        {
            User getInfo = this._db.users.FirstOrDefault(u => u.UserName == info.UserName);
            if (getInfo != null)
            {
                var checkExitPhone = this._db.users.FirstOrDefault(u => u.Phone == info.Phone && u.UserName != info.UserName);
                if (checkExitPhone != null)
                {
                    ModelState.AddModelError("Phone", "Phone number already exists. Please enter another phone number!");
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
                            TempData["Messse"] = "Your profile update was successful!";
                            return View();
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

        public IActionResult Security()
        {
            var username = HttpContext.Session.GetString("UserName");

            var user = _db.users.FirstOrDefault(u => u.UserName == username);
            ViewBag.Fullname = user.LastName + " " + user.FirstName;

            ViewBag.Role = "User";
            ViewBag.jonin = user.joinin;
            ViewBag.img = user.Image;

            return View();
        }

        [HttpPost]
        public IActionResult Security(InputData.SecurityUser input)
        {
            var username = HttpContext.Session.GetString("UserName");
            if (username != null)
            {
                User getInfo = this._db.users.FirstOrDefault(u => u.UserName == username);
                if (getInfo != null)
                {
                    if (getInfo.Password != Hash.CalculateMD5Hash(input.currentPass))
                    {
                        ModelState.AddModelError("currentPass", "Password incorrect please enter again!!");
                    }
                    else if (getInfo.Password == Hash.CalculateMD5Hash(input.NewPass))
                    {
                        ModelState.AddModelError("NewPass", "Please enter other password!!");
                    }
                    else
                    {
                        getInfo.Password = Hash.CalculateMD5Hash(input.NewPass);
                        if (ModelState.IsValid)
                        {
                            var update = this._db.users.Update(getInfo);
                            if (update != null)
                            {
                                this._db.SaveChanges();
                                TempData["Messse"] = "Your Password update was successful!";
                                return View();
                            }
                            else
                            {
                                TempData["Messse"] = "Update failed, please try again!";
                                return View(input);
                            }
                        }
                    }
                }
            }
            return View();
        }
        public IActionResult BlockAccount()
        {
            var username = HttpContext.Session.GetString("UserName");
            if (username != null)
            {
                User getInfo = this._db.users.FirstOrDefault(u => u.UserName == username);
                if (getInfo != null)
                {
                    getInfo.UserStatus = true;
                    var block = this._db.users.Update(getInfo);
                    if (block != null)
                    {
                        this._db.SaveChanges();
                        //  HttpContext.Session.Clear();
                        return Json(new { success = true });
                    }
                    else
                    {
                        return Json(new { success = false });
                    }
                }
            }
            return Json(new { success = false });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(IFormFile file)
        {

            var username = HttpContext.Session.GetString("UserName");
            if (username != null)
            {
                User getInfo = this._db.users.FirstOrDefault(u => u.UserName == username);
                if (getInfo != null)
                {
                    try
                    {
                        // Handle file upload logic here
                        if (file != null && file.Length > 0)
                        {
                            string fileExtension = Path.GetExtension(file.FileName).ToLower();
                            string[] allowedExtensions = { ".jpg", ".png" };

                            if (allowedExtensions.Contains(fileExtension))
                            {
                                var maxFileSize = 5 * 1024 * 1024; // 5MB
                                if (file.Length > maxFileSize)
                                {
                                    TempData["MessseErro"] = "File size exceeds the maximum limit of 5MB";
                                    return RedirectToAction("Profile");
                                }

                                // Save the file to the server or perform any other necessary operations
                                var fileName = Path.GetFileName(file.FileName);
                                //  Format  Name - Year - Month - day - house - minus i second
                                var timestamp = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                                var newFileName = $"{fileName}_{timestamp}{fileExtension}";

                                // Define the uploads folder path
                                var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

                                // Ensure the directory exists
                                if (!Directory.Exists(uploadsFolderPath))
                                {
                                    Directory.CreateDirectory(uploadsFolderPath);
                                }

                                var filePath = Path.Combine(uploadsFolderPath, newFileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    file.CopyTo(stream);
                                }

                                getInfo.Image = newFileName;
                                this._db.Update(getInfo);
                                this._db.SaveChanges();
                                TempData["Messse"] = "Your avatar update was successful!";
                                return RedirectToAction("Profile");
                            }
                            else
                            {

                                TempData["MessseErro"] = "Only jpg, png files are allowed.";
                                return RedirectToAction("Profile");
                            }
                        }
                        else
                        {

                            TempData["MessseErro"] = "Please select a file to upload.";
                            return RedirectToAction("Profile");
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["MessseErro"] = ex.Message;
                        return RedirectToAction("Profile");
                    }
                }
            }
            TempData["MessseErro"] = "Can't Find Info User!";
            return RedirectToAction("Profile");

        }
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}