using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab01.Models;

namespace Lab01.Middleware
{
    public class AuthenticationMiddleware
    {

        private readonly RequestDelegate _next;
        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context, ApplicationDBContext dBContext)
        {
            var path = context.Request.Path.Value;
            if (path.StartsWith("/Home/ResetPass", StringComparison.OrdinalIgnoreCase))
            {
                if (path.Length > "/Home/ResetPass/".Length)
                {
                    var token = path.Substring("/Home/ResetPass".Length + 1);
                    if (!string.IsNullOrEmpty(token))
                    {
                        var findUser = dBContext.users.FirstOrDefault(u => u.ResetToken == token);
                        if (findUser != null)
                        {
                            await _next(context);
                            return;
                        }
                        else
                        {
                            context.Response.Redirect("/Home/Login");
                            return;
                        }
                    }
                }
                else
                {
                    context.Response.Redirect("/Home/Login");
                    return;
                }
            }


            if (context.Session.TryGetValue("UserName", out _))
            {
                string username = context.Session.GetString("UserName");
                if (username != null)
                {
                    User getInfo = dBContext.users.FirstOrDefault(u => u.UserName == username);
                    if (getInfo != null && getInfo.UserStatus)
                    {
                        if (!context.Request.Path.Equals("/Home/AccessDenied"))
                        {

                            context.Response.Redirect("/Home/AccessDenied");
                            context.Session.Clear();
                            return;
                        }
                    }
                }
                await _next(context);
            }
            else
            {
                if (path.StartsWith("/Admin") || path.StartsWith("/User") || path.StartsWith("/HRM"))
                {
                    context.Response.Redirect("/Home/Login");
                    return;
                }
                await _next(context);
            }



        }
    }
}