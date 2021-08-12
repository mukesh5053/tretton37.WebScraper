using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using tretton37.WebScraper.BAL.Interfaces;
using tretton37.WebScraper.BAL.Services;

namespace tretton37.WebScraper
{
    class Program
    {

        #region Variables, Properties and Main
        /// <summary>
        /// Start webscraper engine
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            IHost host = null;
            try
            {
                Console.WriteLine("Start initializing engine configuration.");
                host = Configure();
                var svc = ActivatorUtilities.CreateInstance<Engine>(host.Services);
                Console.WriteLine("Start engine to download website data");
                await svc.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error : {ex.ToString()}");
                Console.WriteLine("Something went wrong please contact support.");
            }

        }
        #endregion

        #region Private Functions and Methods
        /// <summary>
        /// Build configuration
        /// </summary>
        /// <param name="builder"></param>
        private static void BuildConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true);
        }

        /// <summary>
        /// Config Initialization
        /// </summary>
        /// <returns></returns>
        private static IHost Configure()
        {
            IHost host = null;
            IConfigurationBuilder configBuilder = null;
            try
            {

                configBuilder = new ConfigurationBuilder();
                BuildConfig(configBuilder);

                var path = Directory.GetCurrentDirectory();
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(configBuilder.Build())
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File($"{path}\\Logs\\{DateTime.Now.Date.ToString("yyyyMMdd")}.log")
                    .CreateLogger();

                //Dependecny Injection 
                host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddScoped<IEngine, Engine>();
                        services.AddScoped<IAppConfig, AppConfig>();
                        services.AddScoped<IAppConfig, AppConfig>();

                    })
                    .UseSerilog()
                    .Build();

                Console.WriteLine("Finish initializing engine configuration...");

            }
            catch (Exception)
            {
                Console.WriteLine("Failed initializing engine configuration...");
                throw;
            }
            return host;
        }
        #endregion

    }
}
