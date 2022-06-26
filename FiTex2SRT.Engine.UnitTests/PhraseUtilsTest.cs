using FiTex2SRT.Engine;
using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using Reusable.Utils;

namespace FiTex2SRT.Engine.UnitTests
{
    public class PhraseUtilsTest
    {
        [Fact]
        public void CalculateCenterOf_NoWords_GiveNull()
        {
            Assert.False(
                PhraseUtils.CalculateCenterOf(Array.Empty<SubstringRef>()).HasValue);
        }

        [Fact]
        public void CalculateCenterOf_OneWord_GiveCenter()
        {
            string text = "Das Kapital";
            var words = new SubstringRef[] { new(text, 4, 7) };
            int? result = PhraseUtils.CalculateCenterOf(words);
            Assert.True(result.HasValue);
            Assert.Equal(7, result.Value);
        }

        [Fact]
        public void CalculateCenterOf_ManyWords_GiveCenter()
        {
            string text = "Das Kapital";
            var words = new SubstringRef[] { new(text, 0, 3), new(text, 4, 7) };
            int? result = PhraseUtils.CalculateCenterOf(words);
            Assert.True(result.HasValue);
            Assert.Equal(5, result.Value);
        }

        [Fact]
        public void FindMatches_NoMatches_GiveEmpty()
        {
            string text1 = "Eine Identitätspolitik.";
            List<SubstringRef> words1 =
                new() { new SubstringRef(text1, 0, 4), new SubstringRef(text1, 5, 17) };

            string text2 = "Der Marxismus.";
            List<SubstringRef> words2 =
                new() { new SubstringRef(text2, 0, 3), new SubstringRef(text2, 4, 9) };

            (IList<SubstringRef> matches1, IList<SubstringRef> matches2) =
                PhraseUtils.FindMatches(words1, words2);

            Assert.Equal(0, matches1.Count);
            Assert.Equal(0, matches2.Count);
        }

        private static (string phrase, List<SubstringRef> words) CreateReferencePhrase()
        {
            string text = "Die Wahrheit ist immer konkret.";
            List<SubstringRef> words = new()
            {
                new SubstringRef(text, 0, 3),
                new SubstringRef(text, 4, 8),
                new SubstringRef(text, 13, 3),
                new SubstringRef(text, 17, 5),
                new SubstringRef(text, 23, 7),
            };
            return (text, words);
        }

        [Fact]
        public void FindMatches_SomeMatches_InOrder_GiveOnlyThem()
        {
            string text1 = "Die Wahrheit kann nur konkret sein.";
            List<SubstringRef> words1 = new()
            {
                new SubstringRef(text1, 0, 3),
                new SubstringRef(text1, 4, 8),
                new SubstringRef(text1, 13, 4),
                new SubstringRef(text1, 18, 3),
                new SubstringRef(text1, 22, 7),
                new SubstringRef(text1, 30, 4),
            };

            (_, List<SubstringRef> words2) = CreateReferencePhrase();

            (IList<SubstringRef> matches1, IList<SubstringRef> matches2) =
                PhraseUtils.FindMatches(words1, words2);

            Assert.Equal(new[] { words1[0], words1[1], words1[4] }, matches1);
            Assert.Equal(new[] { words2[0], words2[1], words2[4] }, matches2);
        }

        [Fact]
        public void FindMatches_SomeMatches_OutOfOrder_GiveOnlyThem()
        {
            string text1 = "Nur konkret kann die Wahrheit sein.";
            List<SubstringRef> words1 = new()
            {
                new SubstringRef(text1, 0, 3),
                new SubstringRef(text1, 4, 7),
                new SubstringRef(text1, 12, 4),
                new SubstringRef(text1, 17, 3),
                new SubstringRef(text1, 21, 8),
                new SubstringRef(text1, 30, 4),
            };

            (_, List<SubstringRef> words2) = CreateReferencePhrase();

            (IList<SubstringRef> matches1, IList<SubstringRef> matches2) =
                PhraseUtils.FindMatches(words1, words2);

            foreach (SubstringRef word in new[] { words1[1], words1[3], words1[4] })
            {
                Assert.Contains(word, matches1);
            }

            foreach (SubstringRef word in new[] { words2[0], words2[1], words2[4] })
            {
                Assert.Contains(word, matches2);
            }

            Assert.Equal(matches1.Count, matches2.Count);
        }

        static private void VerifyText(string text, string[] expectedSentences)
        {
            int start, end, next = 0;
            foreach (string expectation in expectedSentences)
            {
                start = next;
                (end, next) = PhraseUtils.FindEndOfSentence(text, start);
                Assert.Equal(expectation, text.Substring(start, end - start));
            }
            Assert.Equal(text.Length, next);
        }

        [Fact]
        public void FindEndOfSentence_PunktIsDelimiter()
        {
            string text = "Lorem. Ipsum.";
            string[] expectedSentences = new[] { "Lorem.", "Ipsum." };
            VerifyText(text, expectedSentences);
        }

        [Fact]
        public void FindEndOfSentence_ExclamationOrInterrogative()
        {
            string text = "Lorem? Ipsum!";
            string[] expectedSentences = new[] { "Lorem?", "Ipsum!" };
            VerifyText(text, expectedSentences);
        }

        [Fact]
        public void FindEndOfSentence_CommaIsDelimiter()
        {
            string text = "Lorem, ipsum";
            string[] expectedSentences = new[] { "Lorem", "ipsum" };
            VerifyText(text, expectedSentences);
        }

        [Fact]
        public void FindEndOfSentence_ColonIsDelimiter()
        {
            string text = "Lorem: ipsum";
            string[] expectedSentences = new[] { "Lorem", "ipsum" };
            VerifyText(text, expectedSentences);
        }

        [Fact]
        public void FindEndOfSentence_DashIsDelimiter()
        {
            string text = "Lorem - ipsum - dolor";
            string[] expectedSentences = new[] { "Lorem", "ipsum", "dolor" };
            VerifyText(text, expectedSentences);
        }

        [Fact]
        public void FindEndOfSentence_DashIsNotDelimiter()
        {
            string text = "Lorem-ipsum-dolor";
            string[] expectedSentences = new[] { "Lorem-ipsum-dolor" };
            VerifyText(text, expectedSentences);
        }

        [Fact]
        public void FindEndOfSentence_BracketIsNotDelimiter()
        {
            string text = "Lorem (ipsum), dolor (??) sit (amet).";
            string[] expectedSentences = new[] { "Lorem (ipsum)", "dolor (??) sit (amet)." };
            VerifyText(text, expectedSentences);
        }
    }
}