using System.Globalization;
using System.Text.RegularExpressions;

namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Loads subtitles from SRT files.
    /// </summary>
    public class SrtLoader : ISubtitlesLoader
    {
        private static readonly Regex _subtitleRegex =
            new (@"\d+\r\n(?<start>\d{2}:\d{2}:\d{2},\d{3}) --> (?<end>\d{2}:\d{2}:\d{2},\d{3})\r\n(?<caption>.+(?:\r\n.+)*)\r\n", RegexOptions.Compiled);

        /// <inheritdoc/>
        public IList<Subtitle> LoadSubtitlesFromFile(string filePath, out int countOfChars)
        {
            CultureInfo culture = new("de-DE");
            countOfChars = 0;

            string text = File.ReadAllText(filePath);
            MatchCollection matches = _subtitleRegex.Matches(text);
            List<Subtitle> allSubtitles = new(capacity: matches.Count);

            foreach (Match match in matches)
            {
                Subtitle subtitle = new()
                {
                    caption = match.Groups["caption"].Value,
                    startTime = TimeSpan.Parse(match.Groups["start"].Value, culture),
                    endTime = TimeSpan.Parse(match.Groups["end"].Value, culture)
                };

                allSubtitles.Add(subtitle);
                countOfChars += subtitle.caption.Length;
            }

            return allSubtitles;
        }
    }
}
