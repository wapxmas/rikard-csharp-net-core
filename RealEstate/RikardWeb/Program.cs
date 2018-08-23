using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace RikardWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights();

            var url = args?.ElementAtOrDefault(0);

            if(url != null)
            {
                builder.UseUrls(url);
            }

            var host = builder.Build();

            host.Run();
        }
    }
}
