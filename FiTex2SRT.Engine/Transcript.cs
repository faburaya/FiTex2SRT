﻿using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Hält den Text und die knappen Zeitpunkten, die aus dem Manuskript gelesen wurden,
    /// welches von einem Menschen erzeugt wurde.
    /// </summary>
    public class Transcript
    {
        public string Text { get; private init; }

        public List<(TimeSpan time, int pos)> SyncPoints { get; private init; }

        private Transcript(string text, List<(TimeSpan, int)> syncPoints)
        {
            Text = text;
            SyncPoints = syncPoints;
        }

        private static TimeSpan ParseTime(string timestamp)
        {
            int[] parts = (from s in timestamp.Split(':') select int.Parse(s)).ToArray();
            Debug.Assert(parts.Length == 4);
            return new TimeSpan(0, parts[0], parts[1], parts[2], parts[3] * 10);
        }
        
        private static void AppendToSameLine(StringBuilder buffer, string s, int start, int end)
        {
            foreach (char ch in s.AsSpan(start, end - start))
            {
                switch (ch)
                {
                    case '\r':
                        break;
                    case '\n':
                        buffer.Append(' ');
                        break;
                    default:
                        buffer.Append(ch);
                        break;
                }
            }
        }

        private readonly static Regex _timeRegex =
            new(@"(?<start>[\d]{2}:[\d]{2}:[\d]{2}:[\d]{2}) - (?<end>[\d]{2}:[\d]{2}:[\d]{2}:[\d]{2})\s+",
                RegexOptions.Compiled);

        /// <summary>
        /// Zergliedert das Manuskript.
        /// </summary>
        /// <param name="rawText">Der reine Text aus dem Manuskript.</param>
        /// <returns>Der zergliederte Manuskript als ein <see cref="Transcript"/> object.</returns>
        public static Transcript Parse(string rawText)
        {
            StringBuilder buffer = new();
            List<(TimeSpan time, int pos)> syncPoints = new();

            Match? previousMatch = null;
            foreach (Match match in _timeRegex.Matches(rawText))
            {
                if (previousMatch != null)
                {
                    AppendToSameLine(buffer, rawText, previousMatch.Index + previousMatch.Length, match.Index);
                    syncPoints.Add((ParseTime(previousMatch.Groups["end"].Value), buffer.Length - 1));
                }
                syncPoints.Add((ParseTime(match.Groups["start"].Value), buffer.Length));
                previousMatch = match;
            }

            if (previousMatch != null)
            {
                AppendToSameLine(buffer, rawText, previousMatch.Index + previousMatch.Length, rawText.Length);
                syncPoints.Add((ParseTime(previousMatch.Groups["end"].Value), buffer.Length - 1));
            }

            return new Transcript(buffer.ToString(), syncPoints);
        }
    }
}
