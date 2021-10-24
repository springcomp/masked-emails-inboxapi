using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InboxApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #if DEBUG
            Console.WriteLine("**Note**: when debugging locally, you might want to listen on an alternate tcp port.");
            Console.WriteLine("Please, run 'dotnet run -- urls=\"http://localhost:5002\" to debug locally.");
            #endif
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
