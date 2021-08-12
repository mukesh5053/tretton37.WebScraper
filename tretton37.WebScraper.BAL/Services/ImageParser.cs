using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using tretton37.WebScraper.BAL.DTO;
using tretton37.WebScraper.BAL.Interfaces;

namespace tretton37.WebScraper.BAL.Services
{
   public class ImageParser : ParserBase, IImageParser
    {


        private const string IMAGE_XPATH = "//img/@src";

        private List<string> _imageLinks =null;
        private readonly ILogger<ImageParser> _log;


        public List<string> ResourceLinks
        {
            get { return _imageLinks; }
            set { _imageLinks = value; }
        }

        public ImageParser(WebScraperSetting webScraperSetting, string websiteUrl, ILogger<ImageParser> log) : base(webScraperSetting, websiteUrl, log)
        {
            _imageLinks = new List<string>();
            _log = log;

        }

        /// <summary>
        /// Parse the img elements on the page and download the image files locally. 
        /// </summary>
        /// <param name="wPage"></param>
        public void ParseAndDownloadImages(string htmlContent)
        {
            try
            {
                _log.LogInformation($"Parsing Images for {_websiteUrl}");
                Parse(htmlContent, _websiteUrl);

                if (ResourceLinks.Count > 0)
                {  //Only download resources that haven't been downloaded yet. 

                    var diff = Helper.RemainingResources(Helper.globalDownloadedList, ResourceLinks);

                    if (diff.Count > 0)
                    {
                        new Thread(() =>
                           DownloadResources(diff)
                      ).Start();
                    }
                }
            }
            catch (Exception)
            {
                _log.LogError("ImageParser.ParseAndDownloadImages() : Error downlaoding image to local disk.");
                throw;
            }
            
        }

        public void Parse(string htmlContent, string websiteUrl)
        {

            HtmlNodeCollection imageFiles = null;
            _doc.LoadHtml(htmlContent);

            try
            {
                imageFiles = _doc.DocumentNode.SelectNodes(IMAGE_XPATH);
                if (imageFiles != null)
                {
                    Parallel.ForEach(imageFiles, (imageFile) =>
                    {
                        if (imageFile != null)
                        {
                            var src = imageFile.Attributes[_webScraperSetting.SRC_HTML_ATTRIBUTE].Value;

                            if (src != null && src != string.Empty)
                            {
                                var path = new Uri(new Uri(websiteUrl), src).AbsoluteUri;
                                if (!_imageLinks.Contains(path))
                                {
                                    _imageLinks.Add(path);
                                }
                            }
                        }
                    });
                }
            }
            catch (System.ArgumentNullException)
            {
                _log.LogError("ImageParser.Parse() : Error parsing images.");
                throw;
            }

        }
    }
}
