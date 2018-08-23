using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using RikardWeb.Lib.Identity;
using RikardWeb.Lib.Identity.Localization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesExtension
    {
        public static IServiceCollection AddMongoIdentity(this IServiceCollection services)
        {
            services.AddScoped<IUserStore<IdentityUser>, MongoUserStore<IdentityUser>>();
            services.AddScoped<IRoleStore<IdentityRole>, MongoRoleStore>();
            services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders();
            services.AddSingleton<IdentityErrorDescriber, RuIdentityErrorDescriber>();
            services.AddSingleton<IUsersService<IdentityUser>, UsersService<IdentityUser>>();
            return services;
        }
    }
}
