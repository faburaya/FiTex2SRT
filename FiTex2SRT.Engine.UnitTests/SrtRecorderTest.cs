using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace FiTex2SRT.Engine.UnitTests
{
    public class SrtRecorderTest
    {
        [Fact]
        public void Write_ManyEntries()
        {
            Subtitle[] subtitles = new[]
            {
                new Subtitle { startTime = new TimeSpan(0, 0, 0, 0, 1), endTime = new TimeSpan(0, 0, 0, 1, 2), caption = "Erster Satz." },
                new Subtitle { startTime = new TimeSpan(0, 0, 1, 2, 3), endTime = new TimeSpan(0, 1, 2, 3, 4), caption = "Zweiter Satz." },
            };
            string expectedContent = SubtitlesHelper.GenerateExpectedSrtContent(subtitles);
            const string filePath = "dummy_subtitles_of_recorder.srt";
            SrtRecorder recorder = new();
            recorder.Write(subtitles, filePath);
            string actualContent = File.ReadAllText(filePath);
            Assert.Equal(expectedContent, actualContent);
        }
    }
}