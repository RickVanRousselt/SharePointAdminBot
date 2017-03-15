using System.Linq;
using System.Text.RegularExpressions;

namespace SharePointAdminBot
{
    public static class Helpers
    {

        /// <summary>
        /// Gets the href value in an anchor element.
        /// </summary>
        ///  Skype transforms raw urls to html. Here we extract the href value from the url
        /// <param name="text">Anchor tag html.</param>
        /// <returns>True if valid anchor element</returns>
        public static string ParseAnchorTag(string text)
        {
            var regex = new Regex("^<a href=\"(?<href>[^\"]*)\">[^<]*</a>$", RegexOptions.IgnoreCase);
            var url = regex.Matches(text).OfType<Match>().Select(m => m.Groups["href"].Value).FirstOrDefault();
            return url;
        }
    }
}