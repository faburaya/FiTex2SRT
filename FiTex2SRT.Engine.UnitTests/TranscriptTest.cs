using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;

namespace FiTex2SRT.Engine.UnitTests
{
    public class TranscriptTest
    {
        private static string GenerateRawTranscriptText(
            List<(TimeSpan start, TimeSpan end, string text)> paragraphs)
        {
            StringBuilder buffer = new();
            buffer.AppendLine("Transcript Export");
            buffer.AppendLine("FULL TRANSCRIPT");

            const string timeFormat = @"hh\:mm\:ss\:ff";
            foreach (var paragraph in paragraphs)
            {
                buffer.AppendLine($"{paragraph.start.ToString(timeFormat)} - {paragraph.end.ToString(timeFormat)}");
                buffer.AppendLine(paragraph.text);
            }

            return buffer.ToString();
        }

        private static (string text, SortedDictionary<int, TimeSpan> syncTimes) CreateExpectations(
            List<(TimeSpan start, TimeSpan end, string text)> paragraphs)
        {
            StringBuilder buffer = new();
            SortedDictionary<int, TimeSpan> syncTimes = new();
            foreach (var paragraph in paragraphs)
            {
                syncTimes.Add(buffer.Length, paragraph.start);
                buffer.Append(paragraph.text);
                syncTimes.Add(buffer.Length - 1, paragraph.end);
                buffer.AppendLine();
            }
            buffer.Replace("\r", "");
            buffer.Replace('\n', ' ');
            return (buffer.ToString(), syncTimes);
        }

        [Fact]
        public void Parse_WellFormed()
        {
            List<(TimeSpan, TimeSpan, string)> paragraphs = new()
            {
                (new TimeSpan(0, 0, 0, 0, 990), new TimeSpan(0, 0, 0, 50, 300), "Erster Abschnitt.\nAndere Zeile."),
                (new TimeSpan(0, 0, 0, 52, 100), new TimeSpan(0, 0, 1, 42, 800), "Zweiter Abschnitt.\nAndere Zeile."),
                (new TimeSpan(0, 1, 2, 34, 500), new TimeSpan(0, 2, 3, 45, 670), "Dritter Abschnitt.\nAndere Zeile."),
            };

            var (expectedText, expectedSyncTimes) = CreateExpectations(paragraphs);
            string rawText = GenerateRawTranscriptText(paragraphs);
            Transcript transcript = Transcript.Parse(rawText);
            Assert.Equal(expectedText, transcript.Text);

            (TimeSpan time, int pos)[] expectedSyncTimesAsPairs =
                (from pair in expectedSyncTimes select (pair.Value, pair.Key)).ToArray();

            (TimeSpan time, int pos)[] actualSyncTimesAsPairs =
                (from pair in transcript.SyncTimes select (pair.Value, pair.Key)).ToArray();

            for (int idx = 0;
                 idx < Math.Min(expectedSyncTimesAsPairs.Length, actualSyncTimesAsPairs.Length);
                 ++idx)
            {
                var expected = expectedSyncTimesAsPairs[idx];
                var actual = actualSyncTimesAsPairs[idx];
                Assert.InRange(actual.pos, expected.pos - 2, expected.pos + 2);
                Assert.Equal(expected.time, actual.time);
            }
            Assert.Equal(expectedSyncTimesAsPairs.Length, actualSyncTimesAsPairs.Length);
        }
    }
}