using System.Text;

using Reusable.Utils;

namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Erzeugt Untertitel.
    /// </summary>
    public class SubtitlesGenerator
    {
        private static readonly int _maxCaptionLength = 50;

        private static string BreakIfLong(string line)
        {
            if (line.Length > (int)Math.Round(0.5 * _maxCaptionLength))
            {
                int breakIdx = WordUtils.FindEndOfWord(line, line.Length / 2);
                if (breakIdx < line.Length)
                {
                    StringBuilder buffer = new();
                    buffer.Append(line.AsSpan(0, breakIdx));
                    buffer.Append('\n');
                    buffer.Append(line.AsSpan(breakIdx + 1));
                    return buffer.ToString();
                }
            }
            return line;
        }

        private static (int end, int next) GetEndOfCaption(string text, int start)
        {
            (int end, int next) = PhraseUtils.FindEndOfSentence(text, start);

            if (end - start > _maxCaptionLength)
            {
                end = start + (int)Math.Round(0.7 * _maxCaptionLength);
                next = end = WordUtils.FindClosestStartOrEndOfWord(text, end);
                while (next < text.Length && char.IsWhiteSpace(text[next]))
                    ++next;
            }
            return (end, next);
        }

        private static TimeSpan EstimateTimeOf(int pos, List<SynchronizationPoint> syncPoints)
        {
            if (syncPoints.Count < 2)
            {
                throw new ApplicationException("Cannot estimate instant of text position, because not enough synchronization points are available.");
            }
            int idx = syncPoints.SearchUpperBoundIndex(pos, x => x.Position);
            SynchronizationPoint nextSyncPoint = syncPoints[idx];
            SynchronizationPoint prevSyncPoint = syncPoints[idx - 1];
            return prevSyncPoint.Time +
                (nextSyncPoint.Time - prevSyncPoint.Time)
                * (pos - prevSyncPoint.Position)
                / (nextSyncPoint.Position - prevSyncPoint.Position);
        }

        /// <summary>
        /// Erzeugt Untertitel mithilfe eines Manuskripts.
        /// </summary>
        /// <param name="transcript">Das Manuskript.</param>
        /// <returns>Eine Liste von <see cref="Subtitle"/> Instanzen.</returns>
        public static List<Subtitle> CreateSubtitlesFrom(Transcript transcript)
        {
            List<Subtitle> subtitles = new();
            int start = 0;
            while (start < transcript.Text.Length)
            {
                (int end, int next) = GetEndOfCaption(transcript.Text, start);

                subtitles.Add(new Subtitle {
                    startTime = EstimateTimeOf(start, transcript.SyncPoints),
                    endTime = EstimateTimeOf(end, transcript.SyncPoints),
                    caption = BreakIfLong(transcript.Text[start..end])
                });

                start = next;
            }
            return subtitles;
        }
    }
}
