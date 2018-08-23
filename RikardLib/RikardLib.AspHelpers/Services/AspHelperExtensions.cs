using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace RikardLib.AspHelpers.Services
{
    public static class AspHelperExtensions
    {
        public static string PopString(this ISession session, string key)
        {
            var o = session.GetString(key);
            session.Remove(key);
            return o;
        }
    }
}
