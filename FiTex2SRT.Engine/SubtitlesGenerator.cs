using System.Diagnostics;

using Reusable.Utils;

namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Erzeugt Untertitel.
    /// </summary>
    public class SubtitlesGenerator
    {
        private static readonly int _maxCaptionLength = 60;

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

        private static TimeSpan EstimateTimeOf(int pos, List<(TimeSpan time, int pos)> syncPoints)
        {
            Debug.Assert(syncPoints.Count > 0);
            int idx = syncPoints.SearchLowerBoundIndex(pos, x => x.pos);

            if (idx == 0)
                return syncPoints.First().time;

            if (idx == syncPoints.Count)
                return syncPoints.Last().time;

            var left = syncPoints[idx - 1];
            var right = syncPoints[idx];
            return left.time + (right.time - left.time) * (pos - left.pos) / (right.pos - left.pos);
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
                    caption = transcript.Text[start..end]
                });

                start = next;
            }
            return subtitles;
        }
    }
}
