using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tretton37.WebScraper.BAL
{
   static  class Helper
    {
        public static List<string> globalDownloadedList = new List<string>(); //Use this to track the already downloaded resources. 

        public static List<string> RemainingResources(List<string> downloadedList, List<string> resourceList)
        {
            var copyGlobal = downloadedList.ToList();
            var diff = resourceList.Except(copyGlobal).ToList();
            return diff;
        }

        /// <summary>
        /// Determin if the passed in string is a web page or a resource path.
        /// </summary>
        /// <param name="anchorElm"></param>
        /// <returns></returns>
        public static bool IsWebPage(string anchorElm, string DOT_EXTENSION_SEPARATOR)
        {
            if (anchorElm.IndexOf("javascript:") == 0)
                return false;
            string urlAssetExt = anchorElm.Substring(anchorElm.LastIndexOf(DOT_EXTENSION_SEPARATOR) + 1, anchorElm.Length - anchorElm.LastIndexOf(DOT_EXTENSION_SEPARATOR) - 1);
            switch (urlAssetExt)
            {
                case "jpg":
                case "png":
                case "ico":
                case "css":
                case "woff":
                case "eot":
                    return false;
            }
            if (urlAssetExt.Contains("css")) //I looked for this because the "css" indicator might not be the "extension" in the string. Ex. 'css?f34793c9-17e6-439f-8809-7009f2b25de5' in  main.css?f34793c9-17e6-439f-8809-7009f2b25de5"
                return false; //I wonder if I could do this a better way. 

            return true;
        }

    }
}
