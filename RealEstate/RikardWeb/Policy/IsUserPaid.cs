using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Policy
{
    public class IsUserPaid : IAuthorizationRequirement
    {
        public IsUserPaid(TimeSpan tsAllowed)
        {
            this.TsAllowed = tsAllowed;
        }

        public TimeSpan TsAllowed { get; private set; }
    }
}
