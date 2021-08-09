using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tretton37.WebScraper.BAL.DTO;
using tretton37.WebScraper.BAL.Interfaces;

namespace tretton37.WebScraper.BAL.Services
{
    class AnchorParser : ParserBase, IAnchorParser
    {
        private const string LINK_XPATH = "//a/@href";
        private List<string> _internalUrls = null;
        private readonly ILogger<AnchorParser> _log;

        public List<string> ResourceLinks
        {
            get { return _internalUrls; }
            set { _internalUrls = value; }
        }

        public AnchorParser(WebScraperSetting webScraperSetting, string websiteUrl, ILogger<AnchorParser> log) : base(webScraperSetting, websiteUrl, log)
        {
            _internalUrls = new List<string>();
            _log = log;
        }

        public void Parse(string htmlContent)
        {

            HtmlNodeCollection linkPaths = null;
            try
            {
                _log.LogInformation("parsing anchor/links {0} \n");
                _doc.LoadHtml(htmlContent);

                linkPaths = _doc.DocumentNode.SelectNodes(LINK_XPATH); //Select all of the anchor tags with the 'href' attribute. 

                if (linkPaths != null) //If there are some...
                {
                    //Iterate through linkPaths using Parrallel programming.
                    //This is to try and concurrently grab  the urls within the href attributes. 
                    Parallel.ForEach(linkPaths, (item) =>
                    {
                        if (item != null)
                        {
                            var href = item.Attributes[_webScraperSetting.HREF_HTML_ATTRIBUTE].Value; //Get the contents within the 'href' attribute. 

                            if (href != null && href != string.Empty)
                            {
                                var path = new Uri(new Uri(_websiteUrl), href).AbsoluteUri;
                                if (path.Count(x => x == _webScraperSetting.FORWARD_SLASH_CHAR) >= 4)
                                {
                                    var last = path.LastIndexOf(_webScraperSetting.FORWARD_SLASH_CHAR);
                                    path = path.Substring(0, last);
                                }
                                if (!path.Contains(_webScraperSetting.MAILTO_LINKS) && !path.Contains(_webScraperSetting.TELEPHONE_LINKS)) //Leave out email and phone links
                                {
                                    if (!path.Contains(_webScraperSetting.POUND_SYMBOL_CHAR)) //Avoid sub-sections of pages
                                    {

                                        if (!IsExternalUrl(path) && path != _websiteUrl)
                                        {
                                            if (Helper.IsWebPage(path, _webScraperSetting.DOT_EXTENSION_SEPARATOR))
                                            {
                                                if (!_internalUrls.Contains(path))
                                                    _internalUrls.Add(path);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    });
                }
            }
            catch (Exception e)
            {
                _log.LogError("AnchorParser.Parse() : Error parsing Anchor/links.");
                throw;
            }
        }

        private bool IsExternalUrl(string url)
        {
            try
            {
                string domain = new Uri(_websiteUrl).GetLeftPart(UriPartial.Authority);
                if (url.IndexOf(domain) > -1)
                    return false;
                else if ((url.Length >= 7) && (url.IndexOf("http://") > -1 || url.IndexOf("www") > -1 || url.IndexOf("https://") > -1))
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                _log.LogError("AnchorParser.IsExternalUrl() : Error evaluating external url .");
                throw;
            }

        }

    }
}
