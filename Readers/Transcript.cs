using System.Text;
using System.Text.RegularExpressions;

namespace Readers
{
    public class Transcript
    {
        public string Text { get; private init; }

        public SortedDictionary<TimeSpan, int> SyncTimes { get; private init; }

        private Transcript(string text, SortedDictionary<TimeSpan, int> syncTimes)
        {
            Text = text;
            SyncTimes = syncTimes;
        }

        private readonly static Regex _timeRegex =
            new(@"^(?<start>[\d]{2}:[\d]{2}:[\d]{2}:[\d]{2}) - (?<end>[\d]{2}:[\d]{2}:[\d]{2}:[\d]{2})",
                RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawText"></param>
        /// <returns></returns>
        public static Transcript Parse(string rawText)
        {
            List<(int idxPrevEnd, int idxStart, TimeSpan startTime, TimeSpan endTime)> paragraphs = new();

            MatchCollection matches = _timeRegex.Matches(rawText);
            foreach (Match match in matches)
            {
                paragraphs.Add((
                    match.Index - 1,
                    match.Index + match.Length + 1,
                    TimeSpan.Parse(match.Groups["start"].Value),
                    TimeSpan.Parse(match.Groups["end"].Value)));
            }

            SortedDictionary<TimeSpan, int> syncTimes = new();
            StringBuilder buffer = new();
            for (int idx = 0; idx < paragraphs.Count; ++idx)
            {
                syncTimes.Add(paragraphs[idx].startTime, buffer.Length);
                int idxStart = paragraphs[idx].idxStart;
                int idxEnd = (idx + 1 < paragraphs.Count ? paragraphs[idx + 1].idxPrevEnd : rawText.Length);
                buffer.Append(rawText.AsSpan(idxStart, idxEnd - idxStart));
                syncTimes.Add(paragraphs[idx].endTime, buffer.Length);
            }

            return new Transcript(buffer.ToString(), syncTimes);
        }
    }
}
