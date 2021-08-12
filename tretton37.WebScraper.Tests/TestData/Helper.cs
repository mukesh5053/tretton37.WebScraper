using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tretton37.WebScraper.Tests.TestData
{
   public static class Helper
    {

        public static string GetAnchorParserInput()
        {
          return  File.ReadAllText("AnchorData.txt");
        }
    }
}
