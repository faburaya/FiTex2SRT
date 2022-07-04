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

        private static (string text, List<(TimeSpan, int)> syncPoints) CreateExpectations(
            List<(TimeSpan start, TimeSpan end, string text)> paragraphs)
        {
            StringBuilder buffer = new();
            List<(TimeSpan, int)> syncPoints = new();
            foreach (var paragraph in paragraphs)
            {
                syncPoints.Add((paragraph.start, buffer.Length));
                buffer.Append(paragraph.text
                    .Replace("\r", "")
                    .Replace('\n', ' ')
                    .Replace("  ", " ")
                ).Append(' ');
                syncPoints.Add((paragraph.end, buffer.Length - 1));
            }
            return (buffer.ToString(), syncPoints);
        }

        [Fact]
        public void Parse_WellFormed()
        {
            List<(TimeSpan, TimeSpan, string)> paragraphs = new()
            {
                (new TimeSpan(0, 0, 0, 0, 990), new TimeSpan(0, 0, 0, 50, 300), "1er Abschnitt.\nAndere Zeile."),
                (new TimeSpan(0, 0, 0, 52, 100), new TimeSpan(0, 0, 1, 42, 800), "2er Abschnitt. \nAndere Zeile."),
                (new TimeSpan(0, 1, 2, 34, 500), new TimeSpan(0, 2, 3, 45, 670), "3er Abschnitt. \n Andere Zeile."),
            };

            var expected = CreateExpectations(paragraphs);
            string rawText = GenerateRawTranscriptText(paragraphs);
            Transcript transcript = Transcript.Parse(rawText);
            Assert.Equal(expected.text, transcript.Text);

            for (int idx = 0;
                 idx < Math.Min(expected.syncPoints.Count, transcript.SyncPoints.Count);
                 ++idx)
            {
                (TimeSpan expectedTime, int expectedPos) = expected.syncPoints[idx];
                (TimeSpan actualTime, int actualPos) = transcript.SyncPoints[idx];
                Assert.Equal(expectedPos, actualPos);
                Assert.Equal(expectedTime, actualTime);
            }
            Assert.Equal(expected.syncPoints.Count, transcript.SyncPoints.Count);
        }
    }
}