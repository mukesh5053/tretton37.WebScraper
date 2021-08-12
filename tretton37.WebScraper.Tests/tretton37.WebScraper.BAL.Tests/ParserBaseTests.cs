using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tretton37.WebScraper.BAL;
using tretton37.WebScraper.BAL.DTO;
using tretton37.WebScraper.BAL.Interfaces;
using tretton37.WebScraper.BAL.Services;
using Xunit;

namespace tretton37.WebScraper.Tests.tretton37.WebScraper.BAL.Tests
{
    public class ParserBaseTests
    {
        string _websiteUrl;
        Mock<ILogger<ParserBase>> _log;
        public ParserBaseTests()
        {
            _websiteUrl = "https://tretton37.com/";
            _log = new Mock<ILogger<ParserBase>>();
        }

        [Fact]
        public void DownloadResourceFile_Test()
        {
            ////Arrange

            List<string> ResourceUrls = new List<string>()
            {
                 new string("https://tretton37.com/assets/js/lib/polyfills.js"),
            new string("https://unpkg.com/aos@2.3.1/dist/aos.js")
             };
            var mock = new Mock<ParserBase>(null, _websiteUrl, _log.Object)
            {
                CallBase = true,
            };
            mock.Setup(x => x.DownloadResources(ResourceUrls)).Callback<List<string>>((ResourceUrls) => { Helper.globalDownloadedList.AddRange(ResourceUrls); });

            ////Act
            mock.Object.DownloadResources(ResourceUrls);

            ////Assert
            Assert.Equal(Helper.globalDownloadedList.Count, ResourceUrls.Count);
        }
    }
}
