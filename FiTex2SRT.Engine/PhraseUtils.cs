using System.Text.RegularExpressions;

using Reusable.Utils;

namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Sammelt Algorithmen, die für die Behandlung von Sätzen nützlich sind.
    /// </summary>
    public static class PhraseUtils
    {
        /// <summary>
        /// Rechnet das Zentrum einer Liste von Wörtern.
        /// </summary>
        /// <param name="manyWords">Die Liste von Wörtern.</param>
        /// <returns>Die Indexposition des Zentrums, wenn mindestens ein Wort vorhanden ist.
        /// Je größer das Wort ist, desto mehr zieht es das Zentrum auf sich selbst.</returns>
        public static int? CalculateCenterOf(IEnumerable<SubstringRef> manyWords)
        {
            int sumOfIndices = 0;
            int countOfChars = 0;

            foreach (SubstringRef word in manyWords)
            {
                countOfChars += word.length;
                sumOfIndices += (2 * word.start + word.length - 1) * word.length / 2;
            }

            if (countOfChars > 0)
            {
                return (int)Math.Round((double)sumOfIndices / countOfChars);
            }

            return null;
        }

        /// <summary>
        /// Stellt fest, welche Wörter aus zwei Sätzen übereinstimmen.
        /// </summary>
        /// <param name="wordsOnTheLeft">Die Wörter des Satzes, der auf der linken Seite steht.</param>
        /// <param name="wordsOnTheRight">Die Wörter des Satzes, der auf der rechten Seite steht.</param>
        /// <remarks>Groß- und Kleinschreibung haben keinen Einfluss.</remarks>
        /// <returns>Die Wörter aus der linken Seite, die in der rechten Seite gefunden wurden. Denn jede Übereinstimmung kommt einmal links und einmal rechts vor, werden sie in zwei Listen aufgeteilt.</returns>
        public static (List<SubstringRef> onTheLeft, List<SubstringRef> onTheRight) FindMatches(
            IEnumerable<SubstringRef> wordsOnTheLeft, IEnumerable<SubstringRef> wordsOnTheRight)
        {
            List<SubstringRef> matchesOnTheLeft = new();
            List<SubstringRef> matchesOnTheRight = new();

            List<SubstringRef> rightList = wordsOnTheRight.ToList();
            foreach (SubstringRef word in wordsOnTheLeft)
            {
                int idx = rightList.FindIndex(0,
                    s => s.CompareTo(word, StringComparison.OrdinalIgnoreCase) == 0);

                if (idx < 0)
                    continue;

                matchesOnTheLeft.Add(word);
                matchesOnTheRight.Add(rightList[idx]);
                rightList.RemoveAt(idx);
            }

            return (matchesOnTheLeft, matchesOnTheRight);
        }

        private static readonly Regex _endPhraseRegex =
            new(@"[\w\)""](?<end>\s+-+\s+|\s*[,;:]+\s*|\s*(?<finalchars>[.?!]+)\s*)", RegexOptions.Compiled);

        /// <summary>
        /// Findet das Ende eines Satzes.
        /// </summary>
        /// <param name="text">Der Text.</param>
        /// <param name="start">Die Indexposition, wo die Suche starten muss.</param>
        /// <returns>Die Indexposition unmittelbar nach dem letzten Zeichen des Satzes,
        /// und die Indexposition des ersten Zeichens des nächsten Satzes.</returns>
        public static (int end, int next) FindEndOfSentence(string text, int start)
        {
            Match nextMatch = _endPhraseRegex.Match(text, start);
            if (!nextMatch.Success)
                return (text.Length, text.Length);

            var capturedInterval = nextMatch.Groups["end"];
            var capturedEndChars = nextMatch.Groups["finalchars"];
            if (capturedEndChars.Success)
            {
                return (capturedEndChars.Index + capturedEndChars.Length,
                        capturedInterval.Index + capturedInterval.Length);
            }

            return (capturedInterval.Index,
                    capturedInterval.Index + capturedInterval.Length);
        }
    }
}
