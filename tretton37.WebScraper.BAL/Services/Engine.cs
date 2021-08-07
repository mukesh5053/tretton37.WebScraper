using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using tretton37.WebScraper.BAL.Interfaces;

namespace tretton37.WebScraper.BAL.Services
{
    public class Engine : IEngine
    {


        #region Properties and Variables

        private ILogger<Engine> _log;
        private IConfiguration _config;

        public Engine(ILogger<Engine> log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

        #endregion


        public async Task StartAsync()
        {
            try
            {
                _log.LogInformation($"Start : Engine Started...");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}
