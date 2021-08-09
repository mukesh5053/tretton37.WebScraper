using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using tretton37.WebScraper.BAL.Interfaces;
using tretton37.WebScraper.BAL.DTO;
using Microsoft.Extensions.Logging;

namespace tretton37.WebScraper.BAL.Services
{
    class StyleParser : ParserBase, IStyleParser
    {

        private const string SCRIPT_XPATH = "//link/@href";

        private List<string> _styleLinks = new List<string>();
        private List<string> _innerStyleLinks = new List<string>();
        private readonly ILogger<StyleParser>  _log;

        public List<string> InnerStyleLinks
        {
            get { return _innerStyleLinks; }
            set { _innerStyleLinks = value; }
        }

        public List<string> ResourceLinks
        {
            get { return _styleLinks; }
            set { _styleLinks = value; }
        }
        public StyleParser(WebScraperSetting webScraperSetting, string websiteUrl, ILogger<StyleParser> log) : base(webScraperSetting, websiteUrl, log)
        {
            _log = log;
        }
        
        /// <summary>
        /// Parse the css files on the page along with the urls links within the css code. Then download the associated resources. 
        /// </summary>
        /// <param name="wPage"></param>
        public void ParseAndDownloadStyleFiles(string htmlContent)
        {

            try
            {
                Parse(htmlContent, _websiteUrl);

                if (ResourceLinks.Count > 0)
                {  //Only download resources that haven't been downloaded yet. 
                    var diff = Helper.RemainingResources(ResourceLinks, InnerStyleLinks);
                    if (diff.Count > 0)
                    {
                        new Thread(() =>
                                     DownloadResources(diff)
                               ).Start();

                    }
                }

                if (InnerStyleLinks.Count > 0)
                {  //Only download resources that haven't been downloaded yet. 

                    var diff = Helper.RemainingResources(Helper.globalDownloadedList, InnerStyleLinks);
                    if (diff.Count() > 0)
                    {
                        new Thread(() =>

                            DownloadResources(diff)

                       ).Start();


                    }

                }
            }
            catch (Exception)
            {
                _log.LogError("StyleParser.ParseAndDownloadStyleFiles() : Error downlaoding style files to local disk.");
                throw;
            }
           
        }

        public void Parse(string htmlContent, string websiteUrl)
        {
            HtmlNodeCollection styleFiles = null;
            try
            {
                _doc.LoadHtml(htmlContent);

                styleFiles = _doc.DocumentNode.SelectNodes(SCRIPT_XPATH);
                 if (styleFiles != null)
                 {
                     foreach( var styleFile in styleFiles)
                     {
                         if (styleFile != null)
                         {
                             if (styleFile.Attributes[_webScraperSetting.REL_HTML_ATTRIBUTE] != null && styleFile.Attributes[_webScraperSetting.REL_HTML_ATTRIBUTE].Value == _webScraperSetting.STYLESHEET_REL_ATTR_VALUE) //Only get link elements that contain stylesheet references. 
                             {
                                 var href = styleFile.Attributes[_webScraperSetting.HREF_HTML_ATTRIBUTE].Value;
                                 if (href != null && href != string.Empty)
                                 {
                                     var path = new Uri( new Uri( websiteUrl), href).AbsoluteUri;
                                     if (!_styleLinks.Contains(path) && !Helper.IsWebPage(path, _webScraperSetting.DOT_EXTENSION_SEPARATOR))
                                     {
                                         _styleLinks.Add(path);
                                         foreach (var csslink in _styleLinks)
                                         {
                                             if (csslink != null && csslink != string.Empty)
                                             {
                                                 WebClient wc = new WebClient();
                                                 var css = wc.DownloadString(csslink);
                                                 //Find "url" or "import"
                                                 Regex cssUrls = new Regex(@"(url|@import)\((?<char>['""])?(?<url>.*?)\k<char>?\)", RegexOptions.IgnoreCase); //Get all url properties in the css. 

                                                 foreach (Match match in cssUrls.Matches(css))
                                                 {
                                                     if (match != null)
                                                     {
                                                         var extractedUrl = match.Groups[_webScraperSetting.CSS_URL_PROPERTY].Value;
                                                         if (extractedUrl != null && extractedUrl != string.Empty)
                                                         {
                                                             var innerStylePath = new Uri(new Uri(websiteUrl), extractedUrl).AbsoluteUri;
                                                             if (!_innerStyleLinks.Contains(innerStylePath) && !Helper.IsWebPage(innerStylePath, _webScraperSetting.DOT_EXTENSION_SEPARATOR))
                                                             {
                                                                 _innerStyleLinks.Add(innerStylePath);
                                                             }
                                                         }                                                       
                                                     }
                                                    
                                                 }
                                             }
                                         }
                                     }
                                 }                    
                             }
                         }
                      
                         Thread.Sleep(500);
                     };
                 }
               
            }
            catch (Exception e)
            {
                _log.LogError("StyleParser.parse() : Error parsing style files.");
                throw;
            }

          

         
          
           
        }
    }
}
