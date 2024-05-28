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
            string username = context.Session.GetString("UserName");
            var getinfo = dBContext.users.FirstOrDefault(u => u.UserName == username);

            if (getinfo != null)
            {
                System.Console.WriteLine(getinfo.RoleID);
                bool redirect = false;
                string redirectUrl = "/Home/AccessDenied";

                if (path.StartsWith("/User") && getinfo.RoleID != 1)
                {
                    redirect = true;
                }
                else if (path.StartsWith("/Admin") && getinfo.RoleID != 3)
                {
                    redirect = true;
                }
                else if (path.StartsWith("/HRM") && getinfo.RoleID != 2)
                {
                    redirect = true;
                }

                if (getinfo.RoleID == 1 && !path.StartsWith("/User"))
                {
                    redirect = true;
                    redirectUrl = "/User/Profile";
                }
                else if (getinfo.RoleID == 2 && !path.StartsWith("/HRM"))
                {
                    redirect = true;
                    redirectUrl = "/HRM/Manager";
                }

                if (redirect)
                {
                    context.Response.Redirect(redirectUrl);
                    return;
                }
            }



            await _next(context);
        }
    }
}