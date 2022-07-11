using System.Diagnostics;
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

        public List<SynchronizationPoint> SyncPoints { get; private init; }

        private Transcript(string text, List<SynchronizationPoint> syncPoints)
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
            foreach (char c in s.AsSpan(start, end - start))
            {
                switch (c)
                {
                    case '\r':
                        break;
                    case '\n':
                        if (buffer.Length > 0 && buffer[^1] != ' ')
                            buffer.Append(' ');
                        break;
                    default:
                        buffer.Append(c);
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
            List<SynchronizationPoint> syncPoints = new();
            MatchCollection matches = _timeRegex.Matches(rawText);
            for (int idx = 0; idx < matches.Count; ++idx)
            {
                Match match = matches[idx];
                int startPos = match.Index + match.Length;
                int endPos = (idx + 1 < matches.Count ? matches[idx + 1].Index : rawText.Length);
                TimeSpan startTime = ParseTime(match.Groups["start"].Value);
                TimeSpan endTime = ParseTime(match.Groups["end"].Value);
                syncPoints.Add(new SynchronizationPoint(startTime, buffer.Length));
                AppendToSameLine(buffer, rawText, startPos, endPos);
                syncPoints.Add(new SynchronizationPoint(endTime, buffer.Length - 1));
            }
            return new Transcript(buffer.ToString(), syncPoints);
        }
    }
}
