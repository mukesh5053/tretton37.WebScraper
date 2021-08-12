using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.Linq;
using tretton37.WebScraper.BAL.DTO;
using System.IO;
using Microsoft.Extensions.Logging;

namespace tretton37.WebScraper.BAL.Services
{
    public abstract class ParserBase
    {

        public HtmlDocument _doc = new HtmlDocument();
        public  WebScraperSetting _webScraperSetting;
        public  string _websiteUrl;
        private readonly ILogger<ParserBase> _log;
        private static readonly object _lock = new object();

        public ParserBase(WebScraperSetting webScraperSetting, string websiteUrl, ILogger<ParserBase> log)
        {
            this._webScraperSetting = webScraperSetting;
            this._websiteUrl = websiteUrl;
            _log = log;
        }
      
        /// <summary>
        /// Default functionality for the DownloadResources method. 
        /// </summary>
        /// <param name="ResourceUrls"></param>
        public virtual void DownloadResources(List<string> ResourceUrls)
        {

            lock (_lock)
            { 
                Parallel.ForEach(ResourceUrls ,(currentResourceUrl) => {
               try
               {

                        DownloadResourceFile(currentResourceUrl, _websiteUrl);
               }
               catch (Exception)
               {
                   _log.LogError("Base.DownloadResources() : Error downloading resource: {0} \n", currentResourceUrl);
               }
    
           });
            }
            var diff = ResourceUrls.Except(Helper.globalDownloadedList.ToList()).ToList();
           if (diff.Count > 0)
               Helper.globalDownloadedList.AddRange(diff);
        }

        /// <summary>
        /// Download the file from the  passed in resource url. 
        /// </summary>
        /// <param name="resourceUrl"></param>
        private void DownloadResourceFile(string resourceUrl, string websiteUrl)
        {
            try
            {
                //Ignore cdn links. 
                if (resourceUrl != null && resourceUrl != string.Empty && !resourceUrl.Contains(_webScraperSetting.CDN_IDENTIFIER))
                {
                    string path = _webScraperSetting.STORAGE_LOCATION;

                    //Get each folder/dir and sub-dur from the resource url.
                    Uri uri = new Uri(resourceUrl);
                    var seg = uri.Segments;

                    var domainname = uri.GetComponents(UriComponents.Host, UriFormat.Unescaped);

                    //Make sure that the url is within the same domain as the root site. Otherwise, don't bother with downloading it locally. 
                    if (domainname == new Uri(websiteUrl).GetComponents(UriComponents.Host, UriFormat.Unescaped))
                    {
                        //Build the path. 
                        for (int i = 0; i < seg.Length; i++)
                        {
                            string segment = seg[i];
                            segment = segment.Replace(_webScraperSetting.FORWARD_SLASH_STR, @"\");
                            if (segment != string.Empty)
                            {
                                path = path + segment;
                            }
                        }

                        if (path != string.Empty)
                        {
                            //Create the dir if it does not exist. 
                            var folderPath = Path.GetDirectoryName(path);
                            if (!Directory.Exists(folderPath))
                                Directory.CreateDirectory(folderPath);


                            //Delete the file if it exists. This allow us to get a fresh copy from the site. 
                            if (!File.Exists(path))
                            {
                                _log.LogInformation($"Downloading resource : {resourceUrl}");

                                //Download the file 
                                using (var wc = new WebClient())
                                {
                                    wc.Proxy = WebRequest.DefaultWebProxy;
                                    byte[] data = wc.DownloadData(resourceUrl);
                                    _log.LogInformation($"saving resource to the disk {path}");
                                    File.WriteAllBytes(path, data);
                                }
                                Thread.Sleep(10);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                _log.LogError($"DownloadResourceFile() : Error downloading ResourceFile: {resourceUrl} \n" );
                throw;
            }
            
        }

    }
}
