using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Lab01.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

namespace Lab01.Controllers;

public class HomeController : Controller
{

    private string recipientEmail = "";
    private string subject = "";
    private string body = "";
    private readonly ApplicationDBContext _db;
    public HomeController(ApplicationDBContext db)
    {
        this._db = db;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        var model = new InputData.InputModelLogin();

        if (Request.Cookies.ContainsKey("username") && Request.Cookies.ContainsKey("password"))
        {
            model.UserName = Hash.Decrypt(Request.Cookies["username"]);
            model.Password = Hash.Decrypt(Request.Cookies["password"]);
            model.RememberMe = true;
        }

        return View(model);

    }
    [HttpPost]
    public IActionResult Login(InputData.InputModelLogin us)
    {
        // Check if the input model is valid
        if (ModelState.IsValid)
        {
            var check = this._db.users.FirstOrDefault(u => u.UserName == us.UserName || u.Email == us.UserName);
            if (check != null)
            {
                var checkPass = this._db.users.FirstOrDefault(u => u.UserName == check.UserName && u.Password == Hash.CalculateMD5Hash(us.Password));
                if (checkPass != null)
                {
                    System.Console.WriteLine(checkPass.UserName);
                    HttpContext.Session.SetString("UserName", checkPass.UserName);
                    HttpContext.Session.SetString("Email", checkPass.Email);
                    HttpContext.Session.SetInt32("RoleID", checkPass.RoleID);
                    if (us.RememberMe)
                    {
                        CookieOptions option = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(30),
                            Secure = true,
                            HttpOnly = true
                        };
                        Response.Cookies.Append("username", Hash.Encrypt(check.UserName), option);
                        Response.Cookies.Append("password", Hash.Encrypt(us.Password), option);
                    }
                    else
                    {
                        Response.Cookies.Delete("username");
                        Response.Cookies.Delete("password");
                    }
                    if (!check.verifyAccount)
                    {
                        return RedirectToAction("VerifyCode");
                    }

                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Password is incorrect";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Username or Email does not exist";
            }
        }


        return View(us);
    }

    // login with google method  
    public IActionResult LoginWithGoogle()
    {
        string redirectUrl = Url.Action("GoogleResponse", "Home");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }
    public async Task<IActionResult> GoogleResponse()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
            return BadRequest();
        else
        {
            var userInfo = new
            {
                Email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email),
                Name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name),
                GivenName = authenticateResult.Principal.FindFirstValue(ClaimTypes.GivenName),
            };
            if (userInfo != null)
            {
                User checkogin = this._db.users.FirstOrDefault(u => u.Email == userInfo.Email || u.UserName == userInfo.Email);
                if (checkogin == null)
                {

                    HttpContext.Session.SetString("UserName", userInfo.Email);

                    string[] nameParts = userInfo.GivenName.Split(' ');
                    string firstName = nameParts[0];
                    string lastName = nameParts[nameParts.Length - 1];
                    var user = new User
                    {
                        UserName = userInfo.Email,
                        Email = userInfo.Email,
                        FirstName = firstName,
                        LastName = lastName,
                        Password = Hash.CalculateMD5Hash(12345 + ""),
                        RoleID = 1,
                        joinin = DateTime.Now,
                        UserStatus = false,
                        AccessFailedCount = 0,
                        verifyAccount = false,
                    };
                    this._db.users.Add(user);
                    this._db.SaveChanges();
                    return RedirectToAction("VerifyCode");
                }
                else
                {
                    HttpContext.Session.SetInt32("RoleID", checkogin.RoleID);
                    System.Console.WriteLine("role la: " + checkogin.RoleID);
                    HttpContext.Session.SetString("UserName", checkogin.UserName);
                    if (!checkogin.verifyAccount)
                    {
                        return RedirectToAction("VerifyCode");
                    }
                    else if (checkogin.RoleID == 1)
                    {
                        return RedirectToAction("Profile", "User");
                    }
                    else if (checkogin.RoleID == 2)
                    {
                        return RedirectToAction("Manager", "HRM");
                    }
                    else if (checkogin.RoleID == 3)
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index");

                    }
                }
            }
            return RedirectToAction("Login");
        }
    }

    // login with mircosoft  method
    public IActionResult SignInwithMicrosoft()
    {
        var redirectUrl = Url.Action(nameof(MicrosoftResponse), "Home");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, MicrosoftAccountDefaults.AuthenticationScheme);
    }

    public async Task<IActionResult> MicrosoftResponse()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!authenticateResult.Succeeded)
            return BadRequest("Authentication failed"); // Handle authentication failure.
        else
        {

            var claimsPrincipal = authenticateResult.Principal;
            var name = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
            var email = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;


            var userInfo = new
            {
                Name = name,
                Email = email
            };
            if (userInfo != null)
            {
                User checkogin = this._db.users.FirstOrDefault(u => u.Email == userInfo.Email || u.UserName == userInfo.Email);
                if (checkogin == null)
                {

                    HttpContext.Session.SetString("UserName", userInfo.Email);

                    string[] nameParts = userInfo.Name.Split(' ');
                    string firstName = nameParts[0];
                    string lastName = nameParts[nameParts.Length - 1];
                    var user = new User
                    {
                        UserName = userInfo.Email,
                        Email = userInfo.Email,
                        FirstName = firstName,
                        LastName = lastName,
                        Password = Hash.CalculateMD5Hash(12345 + ""),
                        RoleID = 1,
                        joinin = DateTime.Now,
                        UserStatus = false,
                        AccessFailedCount = 0,
                        verifyAccount = false,
                    };
                    this._db.users.Add(user);
                    this._db.SaveChanges();
                    return RedirectToAction("VerifyCode");
                }
                else
                {
                    HttpContext.Session.SetInt32("RoleID", checkogin.RoleID);
                    System.Console.WriteLine("role la: " + checkogin.RoleID);
                    HttpContext.Session.SetString("UserName", checkogin.UserName);
                    if (!checkogin.verifyAccount)
                    {
                        return RedirectToAction("VerifyCode");
                    }
                    else if (checkogin.RoleID == 1)
                    {
                        return RedirectToAction("Profile", "User");
                    }
                    else if (checkogin.RoleID == 2)
                    {
                        return RedirectToAction("Manager", "HRM");
                    }
                    else if (checkogin.RoleID == 3)
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("Index");

                    }
                }
            }
            return RedirectToAction("Login");
        }
    }


    public async Task<IActionResult> SignOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(MicrosoftAccountDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {

        return View();
    }
    [HttpPost]
    public IActionResult Register(InputData.InputModelRegister input)
    {
        if (this._db.users.Any(s => s.UserName == input.UserName))
        {
            ModelState.AddModelError("UserName", "UserName already exists!");

        }
        if (this._db.users.Any(s => s.Email == input.Email))
        {
            ModelState.AddModelError("Email", "Email already exists!");

        }
        if (this._db.users.Any(s => s.Phone == input.Phone))
        {
            ModelState.AddModelError("Phone", "Phone number already exists!");

        }
        if (ModelState.IsValid)
        {
            if (this._db.users.Any(s => s.UserName == input.UserName || s.Email == input.Email || s.Phone == input.Phone))
            {
                return View(input);
            }
            HttpContext.Session.SetString("UserName", input.UserName);
            var user = new User
            {
                UserName = input.UserName,
                Email = input.Email,
                Phone = input.Phone,
                Password = Hash.CalculateMD5Hash(input.Password),
                RoleID = input.RoleID,
                joinin = DateTime.Now,
                UserStatus = false,
                AccessFailedCount = 0,
                verifyAccount = false

            };
            this._db.users.Add(user);
            this._db.SaveChanges();
            return RedirectToAction("VerifyCode");

        }
        else
        {
            return View(input);
        }
    }
    public IActionResult VerifyCode()
    {
        var userName = HttpContext.Session.GetString("UserName");
        if (userName != null)
        {
            var getInfo = this._db.users.FirstOrDefault(u => u.UserName == userName);
            if (getInfo != null)
            {
                if (!getInfo.verifyAccount)
                {
                    SendEmailMail emailService = new SendEmailMail();
                    recipientEmail = getInfo.Email;
                    string code = Hash.RandomCode(5);
                    System.Console.WriteLine("code gui mail: " + code);
                    HttpContext.Session.SetString("code", code);
                    subject = "Veryfi code";
                    bool flag;
                    flag = emailService.SendEmail(recipientEmail, subject, TemplateSendCode(getInfo.LastName + " " + getInfo.FirstName, code));
                    return View();
                }
            }
        }
        return RedirectToAction("Index");


    }
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
    public IActionResult checkCode(string allnumber)
    {

        var code = HttpContext.Session.GetString("code")?.Replace(" ", "");
        System.Console.WriteLine("Code gui ve: " + allnumber);
        if (string.IsNullOrEmpty(code))
        {
            return Json(new { success = false });
        }
        if (code != allnumber)
        {
            return Json(new { success = false });
        }
        var getUserName = HttpContext.Session.GetString("UserName");
        if (string.IsNullOrEmpty(getUserName))
        {
            return Json(new { success = false });
        }
        var user = this._db.users.FirstOrDefault(u => u.UserName == getUserName);
        if (user == null)
        {
            return Json(new { success = false });
        }
        user.verifyAccount = true;
        this._db.users.Update(user);
        this._db.SaveChanges();
        return Json(new { success = true });
    }
    public IActionResult Forgot()
    {

        return View();
    }

    [HttpPost]
    public IActionResult Forgot(InputData.InputModelForgot input)
    {
        User checkExit = this._db.users.FirstOrDefault(u => u.Email == input.Email);
        if (checkExit != null)
        {
            var token = Hash.GenerateRandomString(50);
            checkExit.ResetToken = token;
            var addToken = this._db.users.Update(checkExit);
            if (addToken == null)
            {
                return View(input);
            }
            this._db.SaveChanges();
            SendEmailMail emailService = new SendEmailMail();
            string link = "http://localhost:5069/Home/ResetPass/" + token;
            subject = "Rest Your Password";
            recipientEmail = input.Email;
            emailService.SendEmail(recipientEmail, subject, TemplateResetPass(checkExit.LastName + " " + checkExit.FirstName, link));
            TempData["Mess"] = "Send Email Success, Check Your Email";
            return View();
        }
        else
        {
            ModelState.AddModelError("Email", "Email not exists!");
            return View(input);
        }


    }
    public IActionResult ResetPass()
    {

        return View();
    }

    [HttpPost]
    public IActionResult ResetPass(InputData.InputModelResetPass input)
    {
        var path = HttpContext.Request.Path.Value;
        var token = path.Substring("/Home/ResetPass".Length + 1);

        if (!string.IsNullOrEmpty(token))
        {
            var findUser = this._db.users.FirstOrDefault(u => u.ResetToken == token);
            if (findUser != null)
            {
                findUser.ResetToken = "";
                findUser.Password = Hash.CalculateMD5Hash(input.Password);
                this._db.users.Update(findUser);
                this._db.SaveChanges();
                TempData["MessUpdate"] = "Password changed successfully. You will Redirect to Login";
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }
        return RedirectToAction("Login");
    }


    public IActionResult AccessDenied()
    {

        return View();
    }




    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public string TemplateResetPass(string fullName, string link)
    {
        body = "<!DOCTYPE html>\n"
        + "\n"
        + "<html lang=\"en\" xmlns:o=\"urn:schemas-microsoft-com:office:office\" xmlns:v=\"urn:schemas-microsoft-com:vml\">\n"
        + "\n"
        + "<head>\n"
        + "	<title></title>\n"
        + "	<meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\" />\n"
        + "	<meta content=\"width=device-width, initial-scale=1.0\" name=\"viewport\" />\n"
        + "	<!--[if mso]><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch><o:AllowPNG/></o:OfficeDocumentSettings></xml><![endif]-->\n"
        + "	<style>\n"
        + "		* {\n"
        + "			box-sizing: border-box;\n"
        + "		}\n"
        + "\n"
        + "		body {\n"
        + "			margin: 0;\n"
        + "			padding: 0;\n"
        + "		}\n"
        + "\n"
        + "		a[x-apple-data-detectors] {\n"
        + "			color: inherit !important;\n"
        + "			text-decoration: inherit !important;\n"
        + "		}\n"
        + "\n"
        + "		#MessageViewBody a {\n"
        + "			color: inherit;\n"
        + "			text-decoration: none;\n"
        + "		}\n"
        + "\n"
        + "		p {\n"
        + "			line-height: inherit\n"
        + "		}\n"
        + "\n"
        + "		.desktop_hide,\n"
        + "		.desktop_hide table {\n"
        + "			mso-hide: all;\n"
        + "			display: none;\n"
        + "			max-height: 0px;\n"
        + "			overflow: hidden;\n"
        + "		}\n"
        + "\n"
        + "		.image_block img+div {\n"
        + "			display: none;\n"
        + "		}\n"
        + "\n"
        + "		@media (max-width:660px) {\n"
        + "\n"
        + "			.desktop_hide table.icons-inner,\n"
        + "			.social_block.desktop_hide .social-table {\n"
        + "				display: inline-block !important;\n"
        + "			}\n"
        + "\n"
        + "			.icons-inner {\n"
        + "				text-align: center;\n"
        + "			}\n"
        + "\n"
        + "			.icons-inner td {\n"
        + "				margin: 0 auto;\n"
        + "			}\n"
        + "\n"
        + "			.image_block div.fullWidth {\n"
        + "				max-width: 100% !important;\n"
        + "			}\n"
        + "\n"
        + "			.mobile_hide {\n"
        + "				display: none;\n"
        + "			}\n"
        + "\n"
        + "			.row-content {\n"
        + "				width: 100% !important;\n"
        + "			}\n"
        + "\n"
        + "			.stack .column {\n"
        + "				width: 100%;\n"
        + "				display: block;\n"
        + "			}\n"
        + "\n"
        + "			.mobile_hide {\n"
        + "				min-height: 0;\n"
        + "				max-height: 0;\n"
        + "				max-width: 0;\n"
        + "				overflow: hidden;\n"
        + "				font-size: 0px;\n"
        + "			}\n"
        + "\n"
        + "			.desktop_hide,\n"
        + "			.desktop_hide table {\n"
        + "				display: table !important;\n"
        + "				max-height: none !important;\n"
        + "			}\n"
        + "		}\n"
        + "	</style>\n"
        + "</head>\n"
        + "\n"
        + "<body style=\"background-color: #f8f8f9; margin: 0; padding: 0; -webkit-text-size-adjust: none; text-size-adjust: none;\">\n"
        + "	<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"nl-container\" role=\"presentation\"\n"
        + "		style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #f8f8f9;\" width=\"100%\">\n"
        + "		<tbody>\n"
        + "			<tr>\n"
        + "				<td>\n"
        + "					<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"row row-1\"\n"
        + "						role=\"presentation\"\n"
        + "						style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #1aa19c;\" width=\"100%\">\n"
        + "						<tbody>\n"
        + "							<tr>\n"
        + "								<td>\n"
        + "									<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "										class=\"row-content stack\" role=\"presentation\"\n"
        + "										style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000000; background-color: #1aa19c; width: 640px; margin: 0 auto;\"\n"
        + "										width=\"640\">\n"
        + "										<tbody>\n"
        + "											<tr>\n"
        + "												<td class=\"column column-1\"\n"
        + "													style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;\"\n"
        + "													width=\"100%\">\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"divider_block block-1\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\">\n"
        + "																<div align=\"center\" class=\"alignment\">\n"
        + "																	<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "																		role=\"presentation\"\n"
        + "																		style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "																		width=\"100%\">\n"
        + "																		<tr>\n"
        + "																			<td class=\"divider_inner\"\n"
        + "																				style=\"font-size: 1px; line-height: 1px; border-top: 4px solid #1AA19C;\">\n"
        + "																				<span> </span>\n"
        + "																			</td>\n"
        + "																		</tr>\n"
        + "																	</table>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "												</td>\n"
        + "											</tr>\n"
        + "										</tbody>\n"
        + "									</table>\n"
        + "								</td>\n"
        + "							</tr>\n"
        + "						</tbody>\n"
        + "					</table>\n"
        + "					<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"row row-2\"\n"
        + "						role=\"presentation\" style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\" width=\"100%\">\n"
        + "						<tbody>\n"
        + "							<tr>\n"
        + "								<td>\n"
        + "									<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "										class=\"row-content stack\" role=\"presentation\"\n"
        + "										style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000000; width: 640px; margin: 0 auto;\"\n"
        + "										width=\"640\">\n"
        + "										<tbody>\n"
        + "											<tr>\n"
        + "												<td class=\"column column-1\"\n"
        + "													style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;\"\n"
        + "													width=\"100%\">\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"image_block block-1\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\"\n"
        + "																style=\"width:100%;padding-right:0px;padding-left:0px;\">\n"
        + "																<div align=\"center\" class=\"alignment\"\n"
        + "																	style=\"line-height:10px\">\n"
        + "																	<div style=\"max-width: 160px;\"><a href=\"\"\n"
        + "																			style=\"outline:none\" tabindex=\"-1\"\n"
        + "																			target=\"_blank\"><img alt=\"Your logo.\"\n"
        + "																				height=\"auto\"\n"
        + "																				src=\"https://i.ibb.co/TqMS1x8/imresizer-1710165374749.png?utm_source=zalo&utm_medium=zalo&utm_campaign=zalo\"\n"
        + "																				style=\"display: block; height: auto; border: 0; width: 100%;\"\n"
        + "																				title=\"Your logo.\" width=\"160\" /></a>\n"
        + "																	</div>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "												</td>\n"
        + "											</tr>\n"
        + "										</tbody>\n"
        + "									</table>\n"
        + "								</td>\n"
        + "							</tr>\n"
        + "						</tbody>\n"
        + "					</table>\n"
        + "					<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"row row-3\"\n"
        + "						role=\"presentation\" style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\" width=\"100%\">\n"
        + "						<tbody>\n"
        + "							<tr>\n"
        + "								<td>\n"
        + "									<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "										class=\"row-content stack\" role=\"presentation\"\n"
        + "										style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #fff; color: #000000; width: 640px; margin: 0 auto;\"\n"
        + "										width=\"640\">\n"
        + "										<tbody>\n"
        + "											<tr>\n"
        + "												<td class=\"column column-1\"\n"
        + "													style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;\"\n"
        + "													width=\"100%\">\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"image_block block-1\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\" style=\"width:100%;\">\n"
        + "																<div align=\"center\" class=\"alignment\"\n"
        + "																	style=\"line-height:10px\">\n"
        + "																	<div style=\"max-width: 640px;\"><a\n"
        + "																			href=\"www.example.com\" style=\"outline:none\"\n"
        + "																			tabindex=\"-1\" target=\"_blank\"><img\n"
        + "																				alt=\"Image of lock & key.\" height=\"auto\"\n"
        + "																				src=\"https://i.imgur.com/K8otx6w.gif\"\n"
        + "																				style=\"display: block; height: auto; border: 0; width: 100%;\"\n"
        + "																				title=\"Image of lock & key.\"\n"
        + "																				width=\"640\" /></a></div>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"divider_block block-2\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\" style=\"padding-top:30px;\">\n"
        + "																<div align=\"center\" class=\"alignment\">\n"
        + "																	<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "																		role=\"presentation\"\n"
        + "																		style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "																		width=\"100%\">\n"
        + "																		<tr>\n"
        + "																			<td class=\"divider_inner\"\n"
        + "																				style=\"font-size: 1px; line-height: 1px; border-top: 0px solid #BBBBBB;\">\n"
        + "																				<span> </span>\n"
        + "																			</td>\n"
        + "																		</tr>\n"
        + "																	</table>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"paragraph_block block-3\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\"\n"
        + "																style=\"padding-bottom:10px;padding-left:40px;padding-right:40px;padding-top:10px;\">\n"
        + "																<div\n"
        + "																	style=\"color:#555555;font-family:'Helvetica Neue',Helvetica,Arial,sans-serif;font-size:30px;line-height:120%;text-align:center;mso-line-height-alt:36px;\">\n"
        + "																	<p style=\"margin: 0; word-break: break-word;\"><span\n"
        + "																			style=\"color:#2b303a;\"><strong>Forgot Your\n"
        + "																				Password?</strong></span></p>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"paragraph_block block-4\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\"\n"
        + "																style=\"padding-bottom:10px;padding-left:40px;padding-right:40px;padding-top:10px;\">\n"
        + "																<div\n"
        + "																	style=\"color:#555555;font-family:Montserrat, Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif;font-size:15px;line-height:150%;text-align:center;mso-line-height-alt:22.5px;\">\n"
        + "																	<p style=\"margin: 0; word-break: break-word;\"><span\n"
        + "																			style=\"color:#808389;text-align: left;float: left;\">Hi.\n"
        + "																			<strong>" + fullName + "</strong> ,</span>\n"
        + "																	</p> <br>\n"
        + "																	<br>\n"
        + "																	<p style=\"margin: 0; word-break: break-word;\"><span\n"
        + "																			style=\"color:#808389;\">Congratulations on\n"
        + "																			sending a successful password recovery\n"
        + "																			request, please click on the button below to\n"
        + "																			change the\n"
        + "																			password.</span></p>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"button_block block-5\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\"\n"
        + "																style=\"padding-left:10px;padding-right:10px;padding-top:15px;text-align:center;\">\n"
        + "																<div align=\"center\" class=\"alignment\"><!--[if mso]>\n"
        + "<v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"www.example.com\" style=\"height:62px;width:209px;v-text-anchor:middle;\" arcsize=\"57%\" stroke=\"false\" fillcolor=\"#f7a50c\">\n"
        + "<w:anchorlock/>\n"
        + "<v:textbox inset=\"0px,0px,0px,0px\">\n"
        + "<center dir=\"false\" style=\"color:#ffffff;font-family:Arial, sans-serif;font-size:16px\">\n"
        + "<![endif]-->\n"
        + "																	<a href=\"" + link + "\"\n"
        + "																		style=\"background-color:#f7a50c;border-bottom:0px solid transparent;border-left:0px solid transparent;border-radius:35px;border-right:0px solid transparent;border-top:0px solid transparent;color:#ffffff;display:inline-block;font-family:'Helvetica Neue', Helvetica, Arial, sans-serif;font-size:16px;font-weight:undefined;mso-border-alt:none;padding-bottom:15px;padding-top:15px;text-align:center;text-decoration:none;width:auto;word-break:keep-all;\"><span\n"
        + "																			style=\"padding-left:30px;padding-right:30px;font-size:16px;display:inline-block;letter-spacing:normal;\"><span\n"
        + "																				style=\"margin: 0; word-break: break-word; line-height: 32px;\"><strong>RESET\n"
        + "																					PASSWORD</strong></span></span></a><!--[if mso]></center></v:textbox></v:roundrect><![endif]-->\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"divider_block block-6\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\"\n"
        + "																style=\"padding-bottom:12px;padding-top:60px;\">\n"
        + "																<div align=\"center\" class=\"alignment\">\n"
        + "																	<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "																		role=\"presentation\"\n"
        + "																		style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "																		width=\"100%\">\n"
        + "																		<tr>\n"
        + "																			<td class=\"divider_inner\"\n"
        + "																				style=\"font-size: 1px; line-height: 1px; border-top: 0px solid #BBBBBB;\">\n"
        + "																				<span> </span>\n"
        + "																			</td>\n"
        + "																		</tr>\n"
        + "																	</table>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "												</td>\n"
        + "											</tr>\n"
        + "										</tbody>\n"
        + "									</table>\n"
        + "								</td>\n"
        + "							</tr>\n"
        + "						</tbody>\n"
        + "					</table>\n"
        + "					<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"row row-4\"\n"
        + "						role=\"presentation\" style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\" width=\"100%\">\n"
        + "						<tbody>\n"
        + "							<tr>\n"
        + "								<td>\n"
        + "									<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "										class=\"row-content stack\" role=\"presentation\"\n"
        + "										style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000000; background-color: #410125; width: 640px; margin: 0 auto;\"\n"
        + "										width=\"640\">\n"
        + "										<tbody>\n"
        + "											<tr>\n"
        + "												<td class=\"column column-1\"\n"
        + "													style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;\"\n"
        + "													width=\"100%\">\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"image_block block-1\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\"\n"
        + "																style=\"width:100%;padding-right:0px;padding-left:0px;\">\n"
        + "																<div align=\"center\" class=\"alignment\"\n"
        + "																	style=\"line-height:10px\">\n"
        + "																	<div class=\"fullWidth\" style=\"max-width: 416px;\"><a\n"
        + "																			href=\"www.example.com\" style=\"outline:none\"\n"
        + "																			tabindex=\"-1\" target=\"_blank\"><img\n"
        + "																				alt=\"Your logo. \" height=\"auto\"\n"
        + "																				src=\"https://i.imgur.com/ClCMU7k.png\"\n"
        + "																				style=\"display: block; height: auto; border: 0; width: 100%;\"\n"
        + "																				title=\"Your logo. \" width=\"416\" /></a>\n"
        + "																	</div>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"social_block block-2\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\"\n"
        + "																style=\"padding-bottom:10px;padding-left:10px;padding-right:10px;padding-top:28px;text-align:center;\">\n"
        + "																<div align=\"center\" class=\"alignment\">\n"
        + "																	<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "																		class=\"social-table\" role=\"presentation\"\n"
        + "																		style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; display: inline-block;\"\n"
        + "																		width=\"208px\">\n"
        + "																		<tr>\n"
        + "																			<td style=\"padding:0 10px 0 10px;\"><a\n"
        + "																					href=\"https://www.facebook.com\"\n"
        + "																					target=\"_blank\"><img alt=\"Facebook\"\n"
        + "																						height=\"auto\"\n"
        + "																						src=\"https://i.imgur.com/CphiGv4.png\"\n"
        + "																						style=\"display: block; height: auto; border: 0;\"\n"
        + "																						title=\"Facebook\"\n"
        + "																						width=\"32\" /></a></td>\n"
        + "																			<td style=\"padding:0 10px 0 10px;\"><a\n"
        + "																					href=\"https://www.twitter.com\"\n"
        + "																					target=\"_blank\"><img alt=\"Twitter\"\n"
        + "																						height=\"auto\"\n"
        + "																						src=\"https://i.imgur.com/tL2iA8p.png\"\n"
        + "																						style=\"display: block; height: auto; border: 0;\"\n"
        + "																						title=\"Twitter\"\n"
        + "																						width=\"32\" /></a></td>\n"
        + "																			<td style=\"padding:0 10px 0 10px;\"><a\n"
        + "																					href=\"https://www.instagram.com\"\n"
        + "																					target=\"_blank\"><img alt=\"Instagram\"\n"
        + "																						height=\"auto\"\n"
        + "																						src=\"https://i.imgur.com/FEfpltU.png\"\n"
        + "																						style=\"display: block; height: auto; border: 0;\"\n"
        + "																						title=\"Instagram\"\n"
        + "																						width=\"32\" /></a></td>\n"
        + "																			<td style=\"padding:0 10px 0 10px;\"><a\n"
        + "																					href=\"https://www.linkedin.com\"\n"
        + "																					target=\"_blank\"><img alt=\"LinkedIn\"\n"
        + "																						height=\"auto\"\n"
        + "																						src=\"https://i.imgur.com/m797lAH.png\"\n"
        + "																						style=\"display: block; height: auto; border: 0;\"\n"
        + "																						title=\"LinkedIn\"\n"
        + "																						width=\"32\" /></a></td>\n"
        + "																		</tr>\n"
        + "																	</table>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"divider_block block-3\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\"\n"
        + "																style=\"padding-bottom:10px;padding-left:40px;padding-right:40px;padding-top:25px;\">\n"
        + "																<div align=\"center\" class=\"alignment\">\n"
        + "																	<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "																		role=\"presentation\"\n"
        + "																		style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "																		width=\"100%\">\n"
        + "																		<tr>\n"
        + "																			<td class=\"divider_inner\"\n"
        + "																				style=\"font-size: 1px; line-height: 1px; border-top: 1px solid #555961;\">\n"
        + "																				<span> </span>\n"
        + "																			</td>\n"
        + "																		</tr>\n"
        + "																	</table>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"paragraph_block block-4\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\"\n"
        + "																style=\"padding-bottom:30px;padding-left:40px;padding-right:40px;padding-top:20px;\">\n"
        + "																<div\n"
        + "																	style=\"color:#555555;font-family:Montserrat, Trebuchet MS, Lucida Grande, Lucida Sans Unicode, Lucida Sans, Tahoma, sans-serif;font-size:12px;line-height:120%;text-align:center;mso-line-height-alt:14.399999999999999px;\">\n"
        + "																	<p style=\"margin: 0; word-break: break-word;\"><span\n"
        + "																			style=\"color:#95979c;\">Your Logo Copyright ©\n"
        + "																			2021</span></p>\n"
        + "																	<p style=\"margin: 0; word-break: break-word;\"><span\n"
        + "																			style=\"color:#95979c;\">Want to stop\n"
        + "																			receiving these emails?</span></p>\n"
        + "																	<p style=\"margin: 0; word-break: break-word;\"><span\n"
        + "																			style=\"color:#95979c;\"> <a\n"
        + "																				href=\"http://www.example.com\"\n"
        + "																				rel=\"noopener\"\n"
        + "																				style=\"text-decoration: underline; color: #ffffff;\"\n"
        + "																				target=\"_blank\">Unsubscribe </a></span>\n"
        + "																	</p>\n"
        + "																</div>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "												</td>\n"
        + "											</tr>\n"
        + "										</tbody>\n"
        + "									</table>\n"
        + "								</td>\n"
        + "							</tr>\n"
        + "						</tbody>\n"
        + "					</table>\n"
        + "					<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" class=\"row row-5\"\n"
        + "						role=\"presentation\"\n"
        + "						style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff;\" width=\"100%\">\n"
        + "						<tbody>\n"
        + "							<tr>\n"
        + "								<td>\n"
        + "									<table align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "										class=\"row-content stack\" role=\"presentation\"\n"
        + "										style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000000; background-color: #ffffff; width: 640px; margin: 0 auto;\"\n"
        + "										width=\"640\">\n"
        + "										<tbody>\n"
        + "											<tr>\n"
        + "												<td class=\"column column-1\"\n"
        + "													style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;\"\n"
        + "													width=\"100%\">\n"
        + "													<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\"\n"
        + "														class=\"icons_block block-1\" role=\"presentation\"\n"
        + "														style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; text-align: center;\"\n"
        + "														width=\"100%\">\n"
        + "														<tr>\n"
        + "															<td class=\"pad\"\n"
        + "																style=\"vertical-align: middle; color: #1e0e4b; font-family: 'Inter', sans-serif; font-size: 15px; padding-bottom: 5px; padding-top: 5px; text-align: center;\">\n"
        + "																<table cellpadding=\"0\" cellspacing=\"0\"\n"
        + "																	role=\"presentation\"\n"
        + "																	style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt;\"\n"
        + "																	width=\"100%\">\n"
        + "																	<tr>\n"
        + "																		<td class=\"alignment\"\n"
        + "																			style=\"vertical-align: middle; text-align: center;\">\n"
        + "																			<!--[if vml]><table align=\"center\" cellpadding=\"0\" cellspacing=\"0\" role=\"presentation\" style=\"display:inline-block;padding-left:0px;padding-right:0px;mso-table-lspace: 0pt;mso-table-rspace: 0pt;\"><![endif]-->\n"
        + "																			<!--[if !vml]><!-->\n"
        + "																			<table cellpadding=\"0\" cellspacing=\"0\"\n"
        + "																				class=\"icons-inner\" role=\"presentation\"\n"
        + "																				style=\"mso-table-lspace: 0pt; mso-table-rspace: 0pt; display: inline-block; margin-right: -4px; padding-left: 0px; padding-right: 0px;\">\n"
        + "																				<!--<![endif]-->\n"
        + "																				<tr>\n"
        + "																					<td\n"
        + "																						style=\"vertical-align: middle; text-align: center; padding-top: 5px; padding-bottom: 5px; padding-left: 5px; padding-right: 6px;\">\n"
        + "																						<a href=\"http://designedwithbeefree.com/\"\n"
        + "																							style=\"text-decoration: none;\"\n"
        + "																							target=\"_blank\"><img\n"
        + "																								align=\"center\"\n"
        + "																								alt=\"Beefree Logo\"\n"
        + "																								class=\"icon\"\n"
        + "																								height=\"auto\"\n"
        + "																								src=\"https://i.imgur.com/idGrkt4.png\"\n"
        + "																								style=\"display: block; height: auto; margin: 0 auto; border: 0;\"\n"
        + "																								width=\"34\" /></a>\n"
        + "																					</td>\n"
        + "																					<td\n"
        + "																						style=\"font-family: 'Inter', sans-serif; font-size: 15px; font-weight: undefined; color: #1e0e4b; vertical-align: middle; letter-spacing: undefined; text-align: center;\">\n"
        + "																						<a href=\"http://designedwithbeefree.com/\"\n"
        + "																							style=\"color: #1e0e4b; text-decoration: none;\"\n"
        + "																							target=\"_blank\">Designed\n"
        + "																							with Beefree</a>\n"
        + "																					</td>\n"
        + "																				</tr>\n"
        + "																			</table>\n"
        + "																		</td>\n"
        + "																	</tr>\n"
        + "																</table>\n"
        + "															</td>\n"
        + "														</tr>\n"
        + "													</table>\n"
        + "												</td>\n"
        + "											</tr>\n"
        + "										</tbody>\n"
        + "									</table>\n"
        + "								</td>\n"
        + "							</tr>\n"
        + "						</tbody>\n"
        + "					</table>\n"
        + "				</td>\n"
        + "			</tr>\n"
        + "		</tbody>\n"
        + "	</table><!-- End -->\n"
        + "</body>\n"
        + "\n"
        + "</html>";
        return body;
    }
    public string TemplateSendCode(string fullName, string code)
    {
        body = "<!doctype html>\n"
                          + "<html>\n"
                          + "\n"
                          + "<head>\n"
                          + "    <meta charset='utf-8'>\n"
                          + "    <meta name='viewport' content='width=device-width, initial-scale=1'>\n"
                          + "    <title>Snippet - GoSNippets</title>\n"
                          + "    <link href='https://stackpath.bootstrapcdn.com/bootstrap/5.0.0-alpha1/css/bootstrap.min.css' rel='stylesheet'>\n"
                          + "    <link href='' rel='stylesheet'>\n"
                          + "    <style>\n"
                          + "        @media screen {\n"
                          + "            @font-face {\n"
                          + "                font-family: 'Lato';\n"
                          + "                font-style: normal;\n"
                          + "                font-weight: 400;\n"
                          + "                src: local('Lato Regular'), local('Lato-Regular'), url(https://fonts.gstatic.com/s/lato/v11/qIIYRU-oROkIk8vfvxw6QvesZW2xOQ-xsNqO47m55DA.woff) format('woff');\n"
                          + "            }\n"
                          + "\n"
                          + "            @font-face {\n"
                          + "                font-family: 'Lato';\n"
                          + "                font-style: normal;\n"
                          + "                font-weight: 700;\n"
                          + "                src: local('Lato Bold'), local('Lato-Bold'), url(https://fonts.gstatic.com/s/lato/v11/qdgUG4U09HnJwhYI-uK18wLUuEpTyoUstqEm5AMlJo4.woff) format('woff');\n"
                          + "            }\n"
                          + "\n"
                          + "            @font-face {\n"
                          + "                font-family: 'Lato';\n"
                          + "                font-style: italic;\n"
                          + "                font-weight: 400;\n"
                          + "                src: local('Lato Italic'), local('Lato-Italic'), url(https://fonts.gstatic.com/s/lato/v11/RYyZNoeFgb0l7W3Vu1aSWOvvDin1pK8aKteLpeZ5c0A.woff) format('woff');\n"
                          + "            }\n"
                          + "\n"
                          + "            @font-face {\n"
                          + "                font-family: 'Lato';\n"
                          + "                font-style: italic;\n"
                          + "                font-weight: 700;\n"
                          + "                src: local('Lato Bold Italic'), local('Lato-BoldItalic'), url(https://fonts.gstatic.com/s/lato/v11/HkF_qI1x_noxlxhrhMQYELO3LdcAZYWl9Si6vvxL-qU.woff) format('woff');\n"
                          + "            }\n"
                          + "        }\n"
                          + "\n"
                          + "        /* CLIENT-SPECIFIC STYLES */\n"
                          + "        body,\n"
                          + "        table,\n"
                          + "        td,\n"
                          + "        a {\n"
                          + "            -webkit-text-size-adjust: 100%;\n"
                          + "            -ms-text-size-adjust: 100%;\n"
                          + "        }\n"
                          + "\n"
                          + "        table,\n"
                          + "        td {\n"
                          + "            mso-table-lspace: 0pt;\n"
                          + "            mso-table-rspace: 0pt;\n"
                          + "        }\n"
                          + "\n"
                          + "        img {\n"
                          + "            -ms-interpolation-mode: bicubic;\n"
                          + "        }\n"
                          + "\n"
                          + "        /* RESET STYLES */\n"
                          + "        img {\n"
                          + "            border: 0;\n"
                          + "            height: auto;\n"
                          + "            line-height: 100%;\n"
                          + "            outline: none;\n"
                          + "            text-decoration: none;\n"
                          + "        }\n"
                          + "\n"
                          + "        table {\n"
                          + "            border-collapse: collapse !important;\n"
                          + "        }\n"
                          + "\n"
                          + "        body {\n"
                          + "            height: 100% !important;\n"
                          + "            margin: 0 !important;\n"
                          + "            padding: 0 !important;\n"
                          + "            width: 100% !important;\n"
                          + "            background-color: #f4f4f4;\n"
                          + "        }\n"
                          + "\n"
                          + "        /* iOS BLUE LINKS */\n"
                          + "        a[x-apple-data-detectors] {\n"
                          + "            color: inherit !important;\n"
                          + "            text-decoration: none !important;\n"
                          + "            font-size: inherit !important;\n"
                          + "            font-family: inherit !important;\n"
                          + "            font-weight: inherit !important;\n"
                          + "            line-height: inherit !important;\n"
                          + "        }\n"
                          + "\n"
                          + "        /* MOBILE STYLES */\n"
                          + "        @media screen and (max-width:600px) {\n"
                          + "            h1 {\n"
                          + "                font-size: 32px !important;\n"
                          + "                line-height: 32px !important;\n"
                          + "            }\n"
                          + "        }\n"
                          + "\n"
                          + "        /* ANDROID CENTER FIX */\n"
                          + "        div[style*=\"margin: 16px 0;\"] {\n"
                          + "            margin: 0 !important;\n"
                          + "        }\n"
                          + "    </style>\n"
                          + "    <script type='text/javascript' src=''></script>\n"
                          + "    <script type='text/javascript' src='https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js'></script>\n"
                          + "    <script type='text/javascript'\n"
                          + "        src='https://stackpath.bootstrapcdn.com/bootstrap/5.0.0-alpha1/js/bootstrap.min.js'></script>\n"
                          + "</head>\n"
                          + "\n"
                          + "<body oncontextmenu='return false' class='snippet-body'>\n"
                          + "    <div\n"
                          + "        style=\"display: none; font-size: 1px; color: #fefefe; line-height: 1px; font-family: 'Lato', Helvetica, Arial, sans-serif; max-height: 0px; max-width: 0px; opacity: 0; overflow: hidden;\">\n"
                          + "        We're thrilled to have you here! Get ready to dive into your new account. </div>\n"
                          + "    <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">\n"
                          + "        <!-- LOGO -->\n"
                          + "        <tr>\n"
                          + "            <td bgcolor=\"#FFA73B\" align=\"center\">\n"
                          + "                <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\n"
                          + "                    <tr>\n"
                          + "                        <td align=\"center\" valign=\"top\" style=\"padding: 40px 10px 40px 10px;\"> </td>\n"
                          + "                    </tr>\n"
                          + "                </table>\n"
                          + "            </td>\n"
                          + "        </tr>\n"
                          + "        <tr>\n"
                          + "            <td bgcolor=\"#FFA73B\" align=\"center\" style=\"padding: 0px 10px 0px 10px;\">\n"
                          + "                <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\n"
                          + "                    <tr>\n"
                          + "                        <td bgcolor=\"#ffffff\" align=\"center\" valign=\"top\"\n"
                          + "                            style=\"padding: 40px 20px 20px 20px; border-radius: 4px 4px 0px 0px; color: #111111; font-family: 'Lato', Helvetica, Arial, sans-serif; font-size: 48px; font-weight: 400; letter-spacing: 4px; line-height: 48px;\">\n"
                          + "                            <h1 style=\"font-size: 48px; font-weight: 400; margin: 2;\">Welcome!</h1> <img\n"
                          + "                                src=\" https://img.icons8.com/clouds/100/000000/handshake.png\" width=\"125\" height=\"120\"\n"
                          + "                                style=\"display: block; border: 0px;\" />\n"
                          + "                        </td>\n"
                          + "                    </tr>\n"
                          + "                </table>\n"
                          + "            </td>\n"
                          + "        </tr>\n"
                          + "        <tr>\n"
                          + "            <td bgcolor=\"#f4f4f4\" align=\"center\" style=\"padding: 0px 10px 0px 10px;\">\n"
                          + "                <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\n"
                          + "                    <tr>\n"
                          + "                        <td bgcolor=\"#ffffff\" align=\"left\"\n"
                          + "                            style=\"padding: 20px 30px 40px 30px; color: #666666; font-family: 'Lato', Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;\">\n"
                          + "                            <h5 style=\"font-style: italic;\">Hello,  " + fullName + "</h5>\n"
                          + "                            <p style=\"margin: 0;\">We're excited to have you get started. First, you need to verify code\n"
                          + "                                your\n"
                          + "                                account. Just press the code below.</p>\n"
                          + "                        </td>\n"
                          + "                    </tr>\n"
                          + "                    <tr>\n"
                          + "                        <td bgcolor=\"#ffffff\" align=\"left\">\n"
                          + "                            <table width=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\n"
                          + "                                <tr>\n"
                          + "                                    <td bgcolor=\"#ffffff\" align=\"center\" style=\"padding: 20px 30px 60px 30px;\">\n"
                          + "                                        <table border=\"0\" cellspacing=\"0\" cellpadding=\"0\">\n"
                          + "                                            <tr>\n"
                          + "                                                <td align=\"center\" style=\"border-radius: 3px;\" bgcolor=\"#FFA73B\"><a\n"
                          + "                                                        target=\"_blank\"\n"
                          + "                                                        style=\"font-size: 35px; font-family: Helvetica, Arial, sans-serif; color: #ffffff; text-decoration: none; color: #ffffff; text-decoration: none; padding: 15px 25px; border-radius: 2px; border: 1px solid #FFA73B; display: inline-block;\">\n"
                          + "" + code + " </a></td>\n"
                          + "                                            </tr>\n"
                          + "                                        </table>\n"
                          + "                                    </td>\n"
                          + "                                </tr>\n"
                          + "                            </table>\n"
                          + "                        </td>\n"
                          + "                    </tr> <!-- COPY -->\n"
                          + "\n"
                          + "                    <tr>\n"
                          + "                        <td bgcolor=\"#ffffff\" align=\"left\"\n"
                          + "                            style=\"padding: 0px 30px 20px 30px; color: #666666; font-family: 'Lato', Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;\">\n"
                          + "                            <p style=\"margin: 0;\">If you have any questions, just reply to this email—we're always happy\n"
                          + "                                to help out.</p>\n"
                          + "                        </td>\n"
                          + "                    </tr>\n"
                          + "                    <tr>\n"
                          + "                        <td bgcolor=\"#ffffff\" align=\"left\"\n"
                          + "                            style=\"padding: 0px 30px 40px 30px; border-radius: 0px 0px 4px 4px; color: #666666; font-family: 'Lato', Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;\">\n"
                          + "                            <p style=\"margin: 0;\">Cheers,<br>Shop Thanh</p>\n"
                          + "                        </td>\n"
                          + "                    </tr>\n"
                          + "                </table>\n"
                          + "            </td>\n"
                          + "        </tr>\n"
                          + "        <tr>\n"
                          + "            <td bgcolor=\"#f4f4f4\" align=\"center\" style=\"padding: 30px 10px 0px 10px;\">\n"
                          + "                <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\n"
                          + "                    <tr>\n"
                          + "                        <td bgcolor=\"#FFECD1\" align=\"center\"\n"
                          + "                            style=\"padding: 30px 30px 30px 30px; border-radius: 4px 4px 4px 4px; color: #666666; font-family: 'Lato', Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;\">\n"
                          + "                            <h2 style=\"font-size: 20px; font-weight: 400; color: #111111; margin: 0;\">Need more help?\n"
                          + "                            </h2>\n"
                          + "                            <p style=\"margin: 0;\"><a href=\"https://www.facebook.com/thanhmax1414/\" target=\"_blank\"\n"
                          + "                                    style=\"color: #FFA73B;\">Chat With admin to Support</a></p>\n"
                          + "                        </td>\n"
                          + "                    </tr>\n"
                          + "                </table>\n"
                          + "            </td>\n"
                          + "        </tr>\n"
                          + "        <tr>\n"
                          + "            <td bgcolor=\"#f4f4f4\" align=\"center\" style=\"padding: 0px 10px 0px 10px;\">\n"
                          + "                <table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"max-width: 600px;\">\n"
                          + "                    <tr>\n"
                          + "                        <td bgcolor=\"#f4f4f4\" align=\"left\"\n"
                          + "                            style=\"padding: 0px 30px 30px 30px; color: #666666; font-family: 'Lato', Helvetica, Arial, sans-serif; font-size: 14px; font-weight: 400; line-height: 18px;\">\n"
                          + "                            <br>\n"
                          + "\n"
                          + "                        </td>\n"
                          + "                    </tr>\n"
                          + "                </table>\n"
                          + "            </td>\n"
                          + "        </tr>\n"
                          + "    </table>\n"
                          + "    <script type='text/javascript'></script>\n"
                          + "</body>\n"
                          + "\n"
                          + "</html>";
        return body;
    }

}
