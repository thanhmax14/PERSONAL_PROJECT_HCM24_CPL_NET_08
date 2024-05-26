using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lab01.Models;

namespace Lab01.Middleware
{
    public class RoleCheckingMiddleware
    {
        private readonly RequestDelegate _next;
        public RoleCheckingMiddleware(RequestDelegate next)
        {
            this._next = next;
        }
        public async Task Invoke(HttpContext context, ApplicationDBContext dBContext)
        {
            var path = context.Request.Path.Value;  // Get Path URL
            var role = context.Session.GetInt32("RoleID");
            System.Console.WriteLine(role);
            if (!role.HasValue)
            {
                if (path.StartsWith("/User") && role != 1)
                {
                    context.Response.Redirect("/Home/AccessDenied");
                    return;
                }
                else if (path.StartsWith("/Admin") && role != 3)
                {
                    context.Response.Redirect("/Home/AccessDenied");
                    return;
                }
                else if (path.StartsWith("/HRM") && role != 2)
                {
                    context.Response.Redirect("/Home/AccessDenied");
                    return;
                }
                //===================================================
                // if (role == 1)
                // {
                //     context.Response.Redirect("/User/Profile");
                //     return;
                // }
                // else if (path.StartsWith("/Admin") && role != 3)
                // {
                //     context.Response.Redirect("/Home/AccessDenied");
                //     return;
                // }
                // else if (path.StartsWith("/HRM") && role != 2)
                // {
                //     context.Response.Redirect("/Home/AccessDenied");
                //     return;
                // }
            }

            await _next(context);
        }
    }
}