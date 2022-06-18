using Reusable.Utils;

namespace Readers
{
    /// <summary>
    /// 
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

        public static int CalculateCenterOf(IEnumerable<SubstringRef> manyWords)
        {
            int sumOfIndices = 0;
            int countOfChars = 0;

            foreach (SubstringRef word in manyWords)
            {
                countOfChars += word.length;
                sumOfIndices += (2 * word.start + word.length - 1) * word.length / 2;
            }

            return (int)Math.Round((double)sumOfIndices / countOfChars);
        }

        private static (int fromAutoSub, int fromTranscript) CalculateCenterOfMatchedWords(
            List<SubstringRef> autoSubWords, List<SubstringRef> transcriptWords)
        {
            List<SubstringRef> matchedAutoSubWords = new();
            List<SubstringRef> matchedTranscriptWords = new();

            foreach (SubstringRef wordOfAutoSub in autoSubWords)
            {
                int idx = transcriptWords.FindIndex(0,
                    s => s.CompareTo(wordOfAutoSub, StringComparison.OrdinalIgnoreCase) == 0);

                if (idx < 0)
                    continue;

                matchedAutoSubWords.Add(wordOfAutoSub);
                matchedTranscriptWords.Add(transcriptWords[idx]);
                transcriptWords.RemoveAt(idx);
            }

            return (CalculateCenterOf(matchedAutoSubWords),
                    CalculateCenterOf(matchedTranscriptWords));
        }

        public Transcript CreateRefinedTranscript(string transcriptFilePath, string autoSubsFilePath)
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

                (int centerOfMatchInAutoSub, int centerOfMatchInTranscript) =
                    CalculateCenterOfMatchedWords(autoSubWords, transcriptWords);

                TimeSpan avgTimeOfMatchedAutoSub = subtitle.startTime +
                    ((double)centerOfMatchInAutoSub / subtitle.caption.Length)
                    * (subtitle.endTime - subtitle.startTime);

                transcript.SyncTimes.Add(avgTimeOfMatchedAutoSub, centerOfMatchInTranscript);

                nextIdx = WordUtils.FindClosestStartOrEndOfWord(transcript.Text, start +
                    (int)Math.Round(subtitle.caption.Length * lenRatioFromAutoToHumanTranslation));
            }

            return transcript;
        }
    }
}
