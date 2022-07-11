using System.Diagnostics;
using System.Text;

using Reusable.Utils;

namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Verwendet die automatisch erzeugten Untertiteln, die verlässliche Zeitpunkte haben,
    /// um das Manuskript zu synchronisieren, das ursprünglich über knappe Zeitpunkte verfügt.
    /// </summary>
    public class Synchronizer
    {
        private readonly ITranscriptLoader _transcriptLoader;
        private readonly ISubtitlesLoader _subtitlesLoader;

        public Synchronizer(ITranscriptLoader transcriptLoader, ISubtitlesLoader subtitlesLoader)
        {
            _transcriptLoader = transcriptLoader;
            _subtitlesLoader = subtitlesLoader;
        }

        private static string ToString(IEnumerable<SubstringRef> autoSubWords,
                                       IEnumerable<SubstringRef> transcriptWords,
                                       IEnumerable<SubstringRef> matches)
        {
            StringBuilder buffer = new();
            buffer.Append("[S]:");
            foreach (SubstringRef s in autoSubWords)
            {
                buffer.Append(s.AsSpan());
                buffer.Append(',');
            }
            buffer.Append("[T]:");
            foreach (SubstringRef s in transcriptWords)
            {
                buffer.Append(s.AsSpan());
                buffer.Append(',');
            }
            buffer.Append("[M]:");
            foreach (SubstringRef s in matches)
            {
                buffer.Append(s.AsSpan());
                buffer.Append(',');
            }
            return buffer.ToString();
        }

        private static void AddSyncPoint(
            List<SynchronizationPoint> syncPoints, TimeSpan time, int pos)
        {
            int idx = syncPoints.SearchUpperBoundIndex(time, x => x.Time);
            syncPoints.Insert(idx, new SynchronizationPoint(time, pos));
        }

        private static int FindLikelyPositionInTranscript(TimeSpan time, Transcript transcript)
        {
            int idx = transcript.SyncPoints.SearchUpperBoundIndex(time, x => x.Time);
            if (idx == 0)
                return 0;

            if (idx >= transcript.SyncPoints.Count)
                return transcript.Text.Length - 1;

            SynchronizationPoint nextSyncPoint = transcript.SyncPoints[idx];
            SynchronizationPoint previousSyncPoint = transcript.SyncPoints[idx - 1];
            return previousSyncPoint.Position + (int)Math.Round(
                (nextSyncPoint.Position - previousSyncPoint.Position)
                * (time - previousSyncPoint.Time)
                / (nextSyncPoint.Time - previousSyncPoint.Time));
        }

        private static (int start, int end) FindStretchInTranscript(
            TimeSpan startTime, TimeSpan endTime, Transcript transcript)
        {
            int start = FindLikelyPositionInTranscript(startTime, transcript);
            int end = FindLikelyPositionInTranscript(endTime, transcript);
            int safeStart = Math.Clamp(start + (start - end) / 3, 0, transcript.Text.Length - 1);
            int safeEnd = Math.Min(transcript.Text.Length - 1, end + (end - start) / 3);
            int adjustedStart = WordUtils.FindClosestStartOrEndOfWord(transcript.Text, safeStart);
            int adjustedEnd = WordUtils.FindEndOfWord(transcript.Text, safeEnd);
            return (adjustedStart, adjustedEnd);
        }

        /// <summary>
        /// Bereichern das Manuskript mit Zeitpunkten, die mithilfe der Synchronisierung
        /// der automatisch erzeugten Untertitel berechnet werden.
        /// </summary>
        /// <param name="autoSubsFilePath">Das Dateipfad der automatisch erzeugten Untertitel.</param>
        /// <param name="transcriptFilePath">Das Dateipfad des Manuskripts.</param>
        /// <returns>Das Bereicherte Manuskript.</returns>
        public Transcript CreateRefinedTranscript(string transcriptFilePath, string autoSubsFilePath)
        {
            IList<Subtitle> autoSubtitles = _subtitlesLoader.LoadSubtitlesFromFile(autoSubsFilePath);
            string rawTranscriptText = _transcriptLoader.LoadRawTranscriptFromFile(transcriptFilePath);
            Transcript transcript = Transcript.Parse(rawTranscriptText);
            int countInitialSyncPoints = transcript.SyncPoints.Count;
            transcript.SyncPoints.Capacity = countInitialSyncPoints + autoSubtitles.Count;

            foreach (Subtitle subtitle in autoSubtitles)
            {
                (int start, int end) =
                    FindStretchInTranscript(subtitle.startTime, subtitle.endTime, transcript);

                List<SubstringRef> autoSubWords =
                    WordUtils.SplitIntoWordsAsRefs(subtitle.caption, 0, subtitle.caption.Length);

                List<SubstringRef> transcriptWords =
                    WordUtils.SplitIntoWordsAsRefs(transcript.Text, start, end);

                (IList<SubstringRef> matchesInAutoSubs, IList<SubstringRef> matchesInTranscript) =
                    PhraseUtils.FindMatches(autoSubWords, transcriptWords);

                Debug.WriteLine(ToString(autoSubWords, transcriptWords, matchesInTranscript));

                if ((float)matchesInAutoSubs.Count / autoSubWords.Count < 0.5f)
                    continue;

                int? centerOfMatchInAutoSub = PhraseUtils.CalculateCenterOf(matchesInAutoSubs);
                int? centerOfMatchInTranscript = PhraseUtils.CalculateCenterOf(matchesInTranscript);

                Debug.Assert(centerOfMatchInAutoSub.HasValue == centerOfMatchInTranscript.HasValue);

                if (centerOfMatchInAutoSub.HasValue && centerOfMatchInTranscript.HasValue)
                {
                    TimeSpan avgTimeOfMatchedAutoSub = subtitle.startTime +
                        ((double)centerOfMatchInAutoSub.Value / subtitle.caption.Length)
                        * (subtitle.endTime - subtitle.startTime);

                    AddSyncPoint(transcript.SyncPoints,
                                 avgTimeOfMatchedAutoSub,
                                 centerOfMatchInTranscript.Value);
                }
            }

            EnsureOrder(transcript.SyncPoints);
            int countGoodSyncPoints = transcript.SyncPoints.Count - countInitialSyncPoints;
            Console.WriteLine($"Transcript has been enriched with {countGoodSyncPoints} synchronization points: success rate of calculation is {100.0 * countGoodSyncPoints / autoSubtitles.Count:F1}%");

            return transcript;
        }

        private static void EnsureOrder(List<SynchronizationPoint> syncPoints)
        {
            int idx = 1;
            while (idx < syncPoints.Count)
            {
                if (syncPoints[idx].Position > syncPoints[idx - 1].Position)
                {
                    ++idx;
                    continue;
                }

                int outOfOrderIdx;
                if (idx >= 2 && syncPoints[idx].Position > syncPoints[idx - 2].Position)
                {
                    outOfOrderIdx = idx - 1;
                }
                else
                {
                    outOfOrderIdx = idx;
                }

                Console.WriteLine($"Dropping out-of-order synchronization point at {syncPoints[outOfOrderIdx].Time}");
                syncPoints.RemoveAt(outOfOrderIdx);
            }
        }
    }
}
