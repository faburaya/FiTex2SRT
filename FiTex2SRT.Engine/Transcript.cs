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

        public SortedDictionary<int, TimeSpan> SyncTimes { get; private init; }

        private Transcript(string text, SortedDictionary<int, TimeSpan> syncTimes)
        {
            Text = text;
            SyncTimes = syncTimes;
        }

        private static TimeSpan ParseTime(string timestamp)
        {
            int[] parts = (from s in timestamp.Split(':') select int.Parse(s)).ToArray();
            Debug.Assert(parts.Length == 4);
            return new TimeSpan(0, parts[0], parts[1], parts[2], parts[3] * 10);
        }

        private readonly static Regex _timeRegex =
            new(@"(?<start>[\d]{2}:[\d]{2}:[\d]{2}:[\d]{2}) - (?<end>[\d]{2}:[\d]{2}:[\d]{2}:[\d]{2})[\r\n]{1,2}",
                RegexOptions.Compiled);

        /// <summary>
        /// Zergliedert das Manuskript.
        /// </summary>
        /// <param name="rawText">Der reine Text aus dem Manuskript.</param>
        /// <returns>Der zergliederte Manuskript als ein <see cref="Transcript"/> object.</returns>
        public static Transcript Parse(string rawText)
        {
            StringBuilder buffer = new();
            SortedDictionary<int, TimeSpan> syncTimes = new();
            MatchCollection matches = _timeRegex.Matches(rawText);
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                syncTimes.Add(buffer.Length, ParseTime(match.Groups["start"].Value));
                int start = match.Index + match.Length;
                int end = (i + 1 < matches.Count ? matches[i + 1].Index : rawText.Length);
                for (int j = start; j < end; ++j)
                {
                    char ch = rawText[j];
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
                syncTimes.Add(buffer.Length - 1, ParseTime(match.Groups["end"].Value));
            }

            return new Transcript(buffer.ToString(), syncTimes);
        }
    }
}
