using System.Text.RegularExpressions;
using System.Web;

namespace KursovaHomeGarden.Extensions
{
    public static class StringExtensions
    {
        public static string HighlightText(this string text, string searchTerm)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(searchTerm))
                return HttpUtility.HtmlEncode(text);

            var pattern = Regex.Escape(searchTerm);
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return regex.Replace(
                HttpUtility.HtmlEncode(text),
                match => $"<span class='highlight'>{match.Value}</span>"
            );
        }
    }
}