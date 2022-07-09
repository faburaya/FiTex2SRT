using System.Globalization;
using System.Text.RegularExpressions;

namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Ladet Untertitel von dem Dateiformat SRT.
    /// </summary>
    public class SrtLoader : ISubtitlesLoader
    {
        private static readonly Regex _subtitleRegex =
            new (@"(?<id>\d+)\r\n(?<start>\d{2}:\d{2}:\d{2},\d{3}) --> (?<end>\d{2}:\d{2}:\d{2},\d{3})\r\n(?<caption>.+(?:\r\n.+)*)\r\n", RegexOptions.Compiled);

        /// <inheritdoc/>
        public List<Subtitle> LoadSubtitlesFromFile(string filePath)
        {
            CultureInfo culture = new("de-DE");
            string text = File.ReadAllText(filePath);
            MatchCollection matches = _subtitleRegex.Matches(text);
            List<Subtitle> allSubtitles = new(capacity: matches.Count);

            foreach (Match match in matches)
            {
                int id = int.Parse(match.Groups["id"].Value);

                Subtitle subtitle = new()
                {
                    caption = match.Groups["caption"].Value,
                    startTime = TimeSpan.Parse(match.Groups["start"].Value, culture),
                    endTime = TimeSpan.Parse(match.Groups["end"].Value, culture)
                };

                if (subtitle.endTime < subtitle.startTime)
                {
                    throw new ApplicationException($"Subtitle #{id} has invalid times: {subtitle.startTime} -> {subtitle.endTime}");
                }

                if (allSubtitles.Count > 0
                    && subtitle.startTime < allSubtitles.Last().endTime)
                {
                    throw new ApplicationException($"Subtitle #{id} is out of order: {allSubtitles.Last().endTime} -> {subtitle.startTime}");
                }

                allSubtitles.Add(subtitle);
            }

            return allSubtitles;
        }
    }
}
