﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Xunit;

namespace FiTex2SRT.Engine.UnitTests
{
    public class SrtLoaderTest
    {
        private static string CreateSrtFile(Subtitle[] subtitles)
        {
            const string timeFormat = @"hh\:mm\:ss\,fff";

            int id = 0;
            StringBuilder buffer = new();
            foreach (var entry in subtitles)
            {
                buffer.AppendLine(id++.ToString());
                buffer.AppendLine($"{entry.startTime.ToString(timeFormat)} --> {entry.endTime.ToString(timeFormat)}");
                buffer.AppendLine(entry.caption);
                buffer.AppendLine();
            }

            const string filePath = "dummy_subtitles.srt";
            File.WriteAllText(filePath, buffer.ToString());
            return filePath;
        }

        [Fact]
        public void LoadSubtitlesFromFile_WellFormed()
        {
            Subtitle[] expectedSubtitles = new[]
            {
                new Subtitle {
                    startTime = new TimeSpan(0, 0, 0, 1, 166),
                    endTime = new TimeSpan(0, 0, 2, 5, 333),
                    caption = "Caros companheiros"
                },
                new Subtitle {
                    startTime = new TimeSpan(0, 0, 2, 10, 500),
                    endTime = new TimeSpan(0, 0, 4, 15, 666),
                    caption = "ontem finalmente"
                },
                new Subtitle {
                    startTime = new TimeSpan(0, 0, 8, 20, 833),
                    endTime = new TimeSpan(0, 0, 16, 25, 999),
                    caption = "ocorreu a revolução"
                },
            };

            string filePath = CreateSrtFile(expectedSubtitles);
            IList<Subtitle> actualSubtitles =
                new SrtLoader().LoadSubtitlesFromFile(filePath, out int actualCharCount);

            Assert.Equal(expectedSubtitles, actualSubtitles);
            Assert.Equal(expectedSubtitles.Sum(x => x.caption.Length), actualCharCount);
        }
    }
}