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
    class ScriptParser : ParserBase, IScriptParser
    {
        private const string SCRIPT_XPATH = "//script/@src";

        private List<string> _scriptLinks = new List<string>();
        private readonly ILogger<ScriptParser>  _log;

        public List<string> ResourceLinks
        {
            get { return _scriptLinks; }
            set { _scriptLinks = value; }
        }

        public ScriptParser(WebScraperSetting webScraperSetting, string websiteUrl, ILogger<ScriptParser> log) : base(webScraperSetting, websiteUrl, log)
        {
            _log = log;
        }

        public void ParseAndDowloadScriptFiles(string htmlContent)
        {

            try
            {
                Parse(htmlContent, _websiteUrl);

                if (ResourceLinks.Count > 0)
                {

                    //Only download resources that haven't been downloaded yet. 
                    var diff = Helper.RemainingResources(Helper.globalDownloadedList, ResourceLinks); //scriptParser.ResourceLinks.Except(copyGlobal).ToList();
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
                _log.LogError("ScriptParser.ParseAndDowloadScriptFiles() : Error downlaoding script files to local disk.");
                throw;
            }
            
        }


        public void Parse(string htmlContent, string websiteUrl)
        {
            HtmlNodeCollection scriptfiles = null;
            try
            {
                _doc.LoadHtml(htmlContent);
                scriptfiles = _doc.DocumentNode.SelectNodes(SCRIPT_XPATH); //Get all of the script files with the src attribute. 

                if (scriptfiles != null)
                {
                    Parallel.ForEach(scriptfiles, (script) =>
                    {
                        if (script != null)
                        {
                            var src = script.Attributes[_webScraperSetting.SRC_HTML_ATTRIBUTE].Value;

                            if (src != null & src != string.Empty)
                                if (!src.Contains(_webScraperSetting.CDN_IDENTIFIER))
                                {
                                    var path = new Uri(new Uri(websiteUrl), src).AbsoluteUri;
                                    if (!_scriptLinks.Contains(path))
                                    {
                                        _scriptLinks.Add(path);
                                    }
                                }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                _log.LogError("ScriptParser.Parse() : Error parsing script files to local disk.");
                throw;
            }


        }

    }
}
