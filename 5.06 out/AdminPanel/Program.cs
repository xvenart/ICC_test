using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog;

namespace AdminPanel
{
    public class Program
    {
        public static object Sync = new object();
        public static List<Entity.ServerEntity> Servers = new List<Entity.ServerEntity>();
        public static Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
