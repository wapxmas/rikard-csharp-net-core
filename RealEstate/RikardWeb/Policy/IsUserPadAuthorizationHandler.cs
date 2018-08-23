using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using RikardLib.AspLog;
using RikardWeb.Lib.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Policy
{
    public class IsUserPadAuthorizationHandler : AuthorizationHandler<IsUserPaid>
    {
        private UserManager<IdentityUser> userManager;
        private readonly IAspLogger logger;
        private readonly IUsersService<IdentityUser> usersService;

        public IsUserPadAuthorizationHandler(
            IAspLogger logger, 
            UserManager<IdentityUser> userManager,
            IUsersService<IdentityUser> usersService
            )
        {
            this.userManager = userManager;
            this.logger = logger;
            this.usersService = usersService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IsUserPaid isUserPaid)
        {
            if(context.User.IsInRole("Admin"))
            {
                context.Succeed(isUserPaid);
            }
            else
            {
                IdentityUser user = usersService.GetUserByIdSync(userManager.GetUserId(context.User));

                if (user != null)
                {
                    if (user.CreatedDate.Add(isUserPaid.TsAllowed) > DateTime.Now)
                    {
                        context.Succeed(isUserPaid);
                    }
                    else
                    {
                        DateTime? ServiceExpired = user.Balance?.ServiceExpired;

                        if (ServiceExpired.HasValue && ServiceExpired.Value > DateTime.Now)
                        {
                            context.Succeed(isUserPaid);
                        }
                    }
                }
            }
            return Task.FromResult(0);
        }
    }
}
