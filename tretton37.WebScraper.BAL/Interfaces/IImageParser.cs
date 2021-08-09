
using System.Collections.Generic;
namespace tretton37.WebScraper.BAL.Interfaces
{

  public interface IImageParser
    {
     
        List<string> ResourceLinks {get; set;} 
      
        void Parse(string page, string websiteUrl);

    }
}
