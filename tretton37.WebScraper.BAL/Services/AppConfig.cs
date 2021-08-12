using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using tretton37.WebScraper.BAL.DTO;
using tretton37.WebScraper.BAL.Interfaces;

namespace tretton37.WebScraper.BAL.Services
{
    public class AppConfig : IAppConfig
    {

        private readonly ILogger<AppConfig> _logger;

        public AppConfig(ILogger<AppConfig> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// Read and returns scraper setting.
        /// </summary>
        /// <returns></returns>
        public WebScraperSetting ReadSettings()
        {
            WebScraperSetting settings = null;
            StreamReader streamReader = null;
            try
            {
                _logger.LogInformation("ReadSettings() : Reading configuration settings.");

                using (streamReader = new StreamReader("appSettings.json"))
                {
                    string jsonString = streamReader.ReadToEnd();
                    settings = JsonConvert.DeserializeObject<WebScraperSetting>(jsonString);
                }
                    
            }
            catch (Exception)
            {
                _logger.LogError("ReadSettings() : Failed to read configuration settings.");

                throw;
            }

            return settings;
        }
    }

}
