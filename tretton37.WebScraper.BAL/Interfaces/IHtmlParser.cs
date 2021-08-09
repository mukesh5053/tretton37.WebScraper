
using System.Collections.Generic;
using System.Threading.Tasks;

namespace tretton37.WebScraper.BAL.Interfaces
{

   public interface IHtmlParser
    {
       void DownloadHtmlPage(string path, string response);
    }
}
