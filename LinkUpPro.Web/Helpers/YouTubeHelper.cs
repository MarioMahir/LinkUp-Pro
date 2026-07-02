using System.Text.RegularExpressions;

namespace LinkUpPro.Web.Helpers
{
    public static class YouTubeHelper
    {
        private static readonly Regex Pattern = new(
            @"(?:youtube\.com\/(?:watch\?v=|embed\/|shorts\/)|youtu\.be\/)([a-zA-Z0-9_-]{6,15})",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string? ExtractVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            var match = Pattern.Match(url);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
