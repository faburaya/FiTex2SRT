﻿using System.Globalization;
using System.Text.RegularExpressions;

namespace Readers
{
    /// <summary>
    /// Loads subtitles from SRT files.
    /// </summary>
    public class SrtLoader : ISubtitlesLoader
    {
        private static readonly Regex _subtitleRegex =
            new (@"\d+\r\n(?<start>\d{2}:\d{2}:\d{2},\d{3}) --> (?<end>\d{2}:\d{2}:\d{2},\d{3})\r\n(?<caption>.+(?:\r\n.+)*)\r\n", RegexOptions.Compiled);

        public IList<Subtitle> LoadSubtitlesFromFile(string filePath)
        {
            string text = File.ReadAllText(filePath);
            MatchCollection matches = _subtitleRegex.Matches(text);

            CultureInfo culture = new("de-DE");
            return (from match in matches
                    select new Subtitle
                    {
                        caption = match.Groups["caption"].Value,
                        startTime = TimeSpan.Parse(match.Groups["start"].Value, culture),
                        endTime = TimeSpan.Parse(match.Groups["end"].Value, culture)
                    }).ToArray();
        }
    }
}