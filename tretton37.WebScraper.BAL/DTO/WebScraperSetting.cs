using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tretton37.WebScraper.BAL.DTO
{
    public record WebScraperSetting(List<string> WebUrls,string STORAGE_LOCATION, string CDN_IDENTIFIER, string SRC_HTML_ATTRIBUTE,
        char FORWARD_SLASH_CHAR, string FORWARD_SLASH_STR, string HTML_EXTENSION, string HTTP_PREFIX, string HTTPS_PREFIX, string TELEPHONE_LINKS,
        string MAILTO_LINKS, string DOT_EXTENSION_SEPARATOR, string HREF_HTML_ATTRIBUTE, string REL_HTML_ATTRIBUTE, string CSS_URL_PROPERTY,
        string MAIN_PAGE_NAME, string STYLESHEET_REL_ATTR_VALUE, char POUND_SYMBOL_CHAR);
    
}
