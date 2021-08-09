using System.Collections.Generic;

namespace tretton37.WebScraper.BAL.Interfaces
{
    interface IAnchorParser
    {
        List<string> ResourceLinks { get; set; }

        void Parse(string htmlContent);
    }
}