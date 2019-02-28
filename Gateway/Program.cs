using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile("ocelot.json")
                        .AddEnvironmentVariables();
                })
                .ConfigureServices(services => {
                    services.AddOcelot().AddDelegatingHandler<MissingBodyDelegatingHandler>(true);
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                })
                .Configure(app =>
                {
                    app.Use((context, next) =>
                    {
                        context.Request.EnableBuffering();
                        return next();
                    });
                    
                    app.UseOcelot().Wait();
                });
    }
}
