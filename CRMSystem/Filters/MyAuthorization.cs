using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRMSystem.Filters
{
    public class MyAuthorization : AuthorizeFilter
    {

        private static AuthorizationPolicy _policy_ = new AuthorizationPolicy(new[] { new DenyAnonymousAuthorizationRequirement() }, new string[] { });

        public MyAuthorization() : base(_policy_) { }

        public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await base.OnAuthorizationAsync(context);
            if (!context.HttpContext.User.Identity.IsAuthenticated ||
                context.Filters.Any(item => item is IAllowAnonymousFilter))
            {
                Console.WriteLine(context.HttpContext.Request.Headers["Authorization"]);
            }
            else {
                Console.WriteLine("No Auth 1");
            }
            if (context.ActionDescriptor!=null)
            {
                Console.WriteLine(context.ActionDescriptor.ActionConstraints);
            }
            else {
                Console.WriteLine("No Auth 2");
            }
                return;
            //do something
        }
    }
}
