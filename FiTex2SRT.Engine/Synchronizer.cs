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

        private static void AddSyncPoint(List<(TimeSpan time, int pos)> syncPoints, TimeSpan time, int pos)
        {
            int idx = syncPoints.SearchLowerBoundIndex(time, x => x.time);
            if (idx == syncPoints.Count || syncPoints[idx].time != time)
            {
                syncPoints.Insert(idx, (time, pos));
            }
        }

        private static int FindPositionInTranscript(TimeSpan time, Transcript transcript)
        {
            int idx = transcript.SyncPoints.SearchLowerBoundIndex(time, x => x.time);

            if (idx == 0)
                return transcript.SyncPoints.First().pos;

            if (idx == transcript.SyncPoints.Count)
                return transcript.SyncPoints.Last().pos;

            var right = transcript.SyncPoints[idx];
            var left = transcript.SyncPoints[idx - 1];
            return (int)Math.Round(left.pos + (right.pos - left.pos) * (time - left.time) / (right.time - left.time));
        }

        private static (int start, int end) FindStretchInTranscript(
            TimeSpan startTime, TimeSpan endTime, Transcript transcript)
        {
            int end = FindPositionInTranscript(endTime, transcript);
            int start = FindPositionInTranscript(startTime, transcript);

            start = Math.Max((int)Math.Round(start + (start - end) * 0.2), 0);
            start = WordUtils.FindClosestStartOrEndOfWord(transcript.Text, start);
            
            end = Math.Min((int)Math.Round(end + (end - start) * 0.2), transcript.Text.Length);
            end = WordUtils.FindEndOfWord(transcript.Text, end);

            return (start, end);
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

        private static void EnsureOrder(List<(TimeSpan time, int pos)> syncPoints)
        {
            int idx = 1;
            while (idx < syncPoints.Count)
            {
                if (syncPoints[idx].pos > syncPoints[idx - 1].pos)
                {
                    ++idx;
                    continue;
                }

                int outOfOrderIdx;
                if (idx >= 2 && syncPoints[idx].pos > syncPoints[idx - 2].pos)
                {
                    outOfOrderIdx = idx - 1;
                }
                else
                {
                    outOfOrderIdx = idx;
                }

                Console.WriteLine($"Dropping out-of-order synchronization point at {syncPoints[outOfOrderIdx].time}");
                syncPoints.RemoveAt(outOfOrderIdx);
            }
        }
    }
}
