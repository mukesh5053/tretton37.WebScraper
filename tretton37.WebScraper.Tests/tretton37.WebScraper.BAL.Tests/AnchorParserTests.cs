using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
using tretton37.WebScraper.BAL.DTO;
using tretton37.WebScraper.BAL.Interfaces;
using tretton37.WebScraper.BAL.Services;
using tretton37.WebScraper.Tests.TestData;
using Xunit;

namespace tretton37.WebScraper.Tests.tretton37.WebScraper.BAL.Tests
{
  public  class AnchorParserTests
    {
        public static Microsoft.Extensions.Configuration.IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(@"appSettings.json")
                .Build();
            return config;
        }

        private readonly IAnchorParser _anchorParser;
        private WebScraperSetting _webScraperSetting;
        private string _websiteUrl;
        private Mock<ILogger<AnchorParser>> _log;
        public AnchorParserTests()
        {
            var config = InitConfiguration();
            AppConfig appConfig = new AppConfig(new Mock<ILogger<AppConfig>>().Object);
            _webScraperSetting = appConfig.ReadSettings();
            _websiteUrl = "https://tretton37.com/";
            _log = new Mock<ILogger<AnchorParser>>();
            _anchorParser = new AnchorParser(_webScraperSetting, _websiteUrl, _log.Object);

        }

        [Fact]
        private void ParseTest()
        {
            //Arrange
            string htmlContent = Helper.GetAnchorParserInput();

            //Act
            try
            {
                _anchorParser.Parse(htmlContent);
                Assert.True(_anchorParser.ResourceLinks.Count > 0);

            }
            catch (System.Exception)
            {
                Assert.True(false);

            }

            //Assert
        }

    }
}
