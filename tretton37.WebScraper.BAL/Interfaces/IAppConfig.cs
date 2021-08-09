using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tretton37.WebScraper.BAL.DTO;

namespace tretton37.WebScraper.BAL.Interfaces
{
   public interface IAppConfig
    {
        WebScraperSetting ReadSettings();
    }
}
