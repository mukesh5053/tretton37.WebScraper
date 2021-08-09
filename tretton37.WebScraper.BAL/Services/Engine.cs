using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using tretton37.WebScraper.BAL.DTO;
using tretton37.WebScraper.BAL.Interfaces;

namespace tretton37.WebScraper.BAL.Services
{
    public class Engine : IEngine
    {


        #region Properties and Variables

        private ILogger<Engine> _log;
        private IConfiguration _config;
        private readonly  WebScraperSetting _appSettings;

        private static List<string> crawledPages = new List<string>();//Use this to track the parsed sites. 

        private ILogger<ImageParser> imageParserLog = null;
        private ILogger<AnchorParser> anchorParserLog = null;
        private ILogger<StyleParser> styleParserLog = null;
        private ILogger<ScriptParser> scriptParserLog = null;
        private ILogger<HtmlPareser> htmlParserLog = null;


        public Engine(ILogger<Engine> log, IConfiguration config, IAppConfig appConfig)
        {
            _log = log;
            _config = config;
            this._appSettings = appConfig.ReadSettings();

            imageParserLog = LoggerFactory.Create(options => { }).CreateLogger<ImageParser>();
            anchorParserLog = LoggerFactory.Create(options => { }).CreateLogger<AnchorParser>();
            styleParserLog = LoggerFactory.Create(options => { }).CreateLogger<StyleParser>();
            scriptParserLog = LoggerFactory.Create(options => { }).CreateLogger<ScriptParser>();
            htmlParserLog = LoggerFactory.Create(options => { }).CreateLogger<HtmlPareser>();


        }

        #endregion


        public async Task StartAsync()
        {
            try
            {
                _log.LogInformation($"Start : Engine Started...");
                foreach (var url in _appSettings.WebUrls)
                {
                    ParsePage(url);
                }


            }
            catch (Exception ex)
            {
                throw;
            }
        }
        /// <summary>
        /// This is where most of the action occurs. 
        /// It gets the html content of the current page and starts searching for all the internal links. While ignoring external ones. 
        /// Then calls a function to parse the resources on the current page. 
        /// Finally, it starts crawling the found links and goes down the ninja hole. 
        /// </summary>
        /// <param name="urlPath"></param>
        private void ParsePage(string urlPath)
        {
            if (!PageHasBeenCrawled(urlPath)) //Check if the page has already been parsed. 
            {
                if (!crawledPages.Contains(urlPath)) //Keep track of the pages that are already being iterated over. To prevent repeated actions. Irony. 
                    crawledPages.Add(urlPath);

                _log.LogInformation("Begin parsing page {0} \n", urlPath);

                //This should be wrapped in a try catch to catch any exceptions that occur within the GetWebString method. 
                string urlContent = GetWebString(urlPath); //Get the html contents of the current page/url.


                //Note: I got rid of the WebPage object. I thought it unnecessary due to everything I need like the "urlContent" and "urlPath" being centralized. 

                if (urlContent != null)
                {
                    AnchorParser linkParser = new AnchorParser(_appSettings, urlPath, anchorParserLog);//Instantiate a new AnchorParser object. 
                    linkParser.Parse(urlContent); //Call Parse to capture all of the internal links on the current page. 

                    ParseResourceFilesAndLinks(urlContent, urlPath);

                    if (linkParser.ResourceLinks.Count > 0)
                    {
                        //Get a copy of the resource/internal links of the current page. 
                        var copyGlobal = linkParser.ResourceLinks.ToList();
                        var copyCrawledPages = crawledPages;
                        var diff = linkParser.ResourceLinks.Except(copyCrawledPages).ToList();//Get the links that haven't been parsed/crawled yet. 
                        if (diff.Count > 0)
                        {
                            _log.LogInformation("Crawling all of the internal links found on the page: {0}... \n", urlPath);
                            //Concurrently crawl all of the internal links on this current page, recursively calling ParsePage in the process.
                            Parallel.ForEach(diff, (currentInternalLink) =>
                            {
                                if (currentInternalLink != null && currentInternalLink != string.Empty)
                                    ParsePage(currentInternalLink);
                            });
                        }

                    }


                }
            }

        }

        /// <summary>
        /// Concurrently run the three resource parse and download functions.
        /// I separated the three "parse" code snippets into different methods to be able to run the three steps concurrently.
        /// </summary>
        /// <param name="wPage"></param>
        private void ParseResourceFilesAndLinks(string htmlContent, string websiteUrl)
        {
            if (htmlContent != null)
            {
                //Concurrently execute all of three of the resource methods that parse and download the files. 
                Parallel.Invoke(() => new ImageParser(_appSettings, websiteUrl, imageParserLog).ParseAndDownloadImages(htmlContent),//,  //Get the images
                 () => new StyleParser(_appSettings, websiteUrl, styleParserLog).ParseAndDownloadStyleFiles(htmlContent), //Get the style files and fonts
                 () =>new ScriptParser(_appSettings, websiteUrl, scriptParserLog).ParseAndDowloadScriptFiles(htmlContent));   //Get the script files
            }
        }

        /// <summary>
        /// Function that gets the HTML content from the passed in URL if possible. 
        /// Then, saves the page as an html file on the disk. 
        /// Finally, it returns that HTML data.
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        private string GetWebString(string Url)
        {

            Thread.Sleep(500);// Not sure why I had this. 
                              // string filePath = string.Empty; //No longer used anymore
            string pageHtmlName = string.Empty;
            string strResponse = string.Empty;

            Uri uri = new Uri(Url); //Get a uri object from the url. 
            if (uri != null)
            {

                //Get the last segment which is usually the page name. I wonder if I could take care of this in a better way.
                var currentPage = uri.Segments.Last().Replace("/", "");

                //If the current page is not empty, then try to use the name and add a .html extension to it. 
                if (currentPage != string.Empty)
                    if (Url.Contains(_appSettings.HTML_EXTENSION)) //If the page name already contains .html then just use that. 
                        pageHtmlName = currentPage;
                    else
                        pageHtmlName = currentPage + _appSettings.HTML_EXTENSION; //else, add it on.
                else
                    pageHtmlName = _appSettings.MAIN_PAGE_NAME; //If it is empty, then it must be the main url/page. 
                try
                {
                    _log.LogInformation($"Downloading htmlcontent for {currentPage}");
                    HttpWebRequest wrWebRequest = WebRequest.Create(Url) as HttpWebRequest;
                    HttpWebResponse wrWebResponse = (HttpWebResponse)wrWebRequest.GetResponse();
                    StreamReader srResponse;

                    using (srResponse = new StreamReader(wrWebResponse.GetResponseStream()))
                    {
                        //Get the contents of the page as string. I thought about using the async methods. But this code will most likley be running on a separate thread anyway. 
                        //I didn't think it would be wise to use async/await on something that wasn't holding up some sort of UI thread. 
                        strResponse = srResponse.ReadToEnd();
                        //Save the page as a html file on disk on a new thread.
                        //I did this because the main work wasn't already done. 
                        //I figured I could let any filesystem interaction run on another thread while the rest of the code continues on. 
                      

                        new Thread(() =>
                             new HtmlPareser(htmlParserLog).DownloadHtmlPage(_appSettings.STORAGE_LOCATION + pageHtmlName, strResponse)

                        ).Start();
                    }
                }
                catch (Exception e)
                {
                    _log.LogError($"GetWebString() : An error occured while trying to get data from {Url}.");
                    throw;
                }
            }

            return strResponse;
        }


        
        /// <summary>
        /// Determine if the passed in url belongs to a page that has already been crawled through. 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static bool PageHasBeenCrawled(string url)
        {
            var copyCrawledPages = crawledPages;
            foreach (string pageUrl in copyCrawledPages.ToList())
            {
                if (pageUrl == url) //If the current iteration's url matches one in the pages collection, then the page has already been parsed. 
                    return true;
            }

            return false;//Page hasn't been parsed yet. 
        }
         

         
    }

}
