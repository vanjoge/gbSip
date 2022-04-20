using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GBWeb
{
    public class Program
    {
        static SipServer.SipServer sipServer = new SipServer.SipServer();
        public static void Main(string[] args)
        {
            SIPSorceryLog.RegSIPSorceryLogFactory();

            SQ.Base.ByteHelper.RegisterGBKEncoding();

            sipServer.Start();
            CreateHostBuilder(args).Build().Run();
            sipServer.Stop();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
