using Reusable.Utils;

namespace Readers
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

        /// <summary>
        /// Bereichern das Manuskript mit Zeitpunkten, die mithilfe der Synchronisierung
        /// der automatisch erzeugten Untertitel berechnet werden.
        /// </summary>
        /// <param name="autoSubsFilePath">Das Dateipfad der automatisch erzeugten Untertitel.</param>
        /// <param name="transcriptFilePath">Das Dateipfad des Manuskripts.</param>
        /// <returns></returns>
        public Transcript CreateRefinedTranscript(string autoSubsFilePath, string transcriptFilePath)
        {
            IList<Subtitle> autoSubtitles =
                _subtitlesLoader.LoadSubtitlesFromFile(autoSubsFilePath, out int lengthOfAutoSubs);

            string rawTranscriptText = _transcriptLoader.LoadRawTranscriptFromFile(transcriptFilePath);
            Transcript transcript = Transcript.Parse(rawTranscriptText);
            int lengthOfTranscript = transcript.Text.Length;
            double lenRatioFromAutoToHumanTranslation = (double)lengthOfTranscript / lengthOfAutoSubs;

            int nextIdx = 0;
            foreach (Subtitle subtitle in autoSubtitles)
            {
                int start = nextIdx;
                int end = WordUtils.FindEndOfWord(transcript.Text,
                    start + (int)Math.Round(subtitle.caption.Length * 1.5));

                List<SubstringRef> autoSubWords =
                    WordUtils.SplitIntoWordsAsRefs(subtitle.caption, 0, subtitle.caption.Length);

                List<SubstringRef> transcriptWords =
                    WordUtils.SplitIntoWordsAsRefs(transcript.Text, start, end);

                (IList<SubstringRef> matchesInAutoSubs, IList<SubstringRef> matchesInTranscript) =
                    PhraseUtils.FindMatches(autoSubWords, transcriptWords);

                int centerOfMatchInAutoSub = PhraseUtils.CalculateCenterOf(matchesInAutoSubs);
                int centerOfMatchInTranscript = PhraseUtils.CalculateCenterOf(matchesInTranscript);

                TimeSpan avgTimeOfMatchedAutoSub = subtitle.startTime +
                    ((double)centerOfMatchInAutoSub / subtitle.caption.Length)
                    * (subtitle.endTime - subtitle.startTime);

                transcript.SyncTimes.Add(centerOfMatchInTranscript, avgTimeOfMatchedAutoSub);

                nextIdx = WordUtils.FindClosestStartOrEndOfWord(transcript.Text, start +
                    (int)Math.Round(subtitle.caption.Length * lenRatioFromAutoToHumanTranslation));
            }

            return transcript;
        }
    }
}
