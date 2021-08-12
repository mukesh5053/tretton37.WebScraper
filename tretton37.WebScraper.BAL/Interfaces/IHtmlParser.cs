
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tretton37.WebScraper.BAL.Interfaces
{

   public interface IHtmlParser
    {
         Task DownloadHtmlPageAsync(string path, string response);
    }
}
