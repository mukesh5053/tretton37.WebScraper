using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using tretton37.WebScraper.BAL.Interfaces;

namespace tretton37.WebScraper.BAL.Services
{
    public class HtmlPareser : IHtmlParser
    {

        private readonly ILogger<HtmlPareser> _log;

        public HtmlPareser(ILogger<HtmlPareser> log)
        {
            _log = log;
        }

        /// <summary>
        /// Download Html Page and store to the disk
        /// </summary>
        /// <param name="path"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task DownloadHtmlPageAsync(string path, string response)
        {
            FileInfo fi = null;
            try
            {
                _log.LogInformation($"Downloading html content.");

                //If the file does not exist, then save it to disk. 
                fi = new FileInfo(path);
                if (!fi.Directory.Exists)
                {
                    System.IO.Directory.CreateDirectory(fi.DirectoryName);
                }
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

               await File.WriteAllTextAsync(path, response);
            }
            catch (System.Exception)
            {
                _log.LogError("DownloadHtmlPage() : Error downlaoding html page to local disk.");
                throw;
            }

        }
    }
}
