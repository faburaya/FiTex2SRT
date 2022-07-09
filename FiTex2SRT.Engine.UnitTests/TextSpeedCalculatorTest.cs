using System;
using System.Linq;
using System.Text;

using Xunit;

namespace FiTex2SRT.Engine.UnitTests
{
    public class TextSpeedCalculatorTest
    {
        private static Subtitle GenerateSubtitleWith(int numChars, int durationSecs)
        {
            StringBuilder buffer = new();
            for (int i = 0; i < numChars; i++)
                buffer.Append((char)('A' + i));

            return new Subtitle {
                caption = buffer.ToString(),
                startTime = TimeSpan.FromSeconds(0),
                endTime = TimeSpan.FromSeconds(durationSecs)
            };
        }

        [Fact]
        public void EstimateCurrentSpeed_UseOnlyLastSubtitle()
        {
            int[] expectedSpeeds = new[] { 2, 3, 4, 1, 5 };
            const int duration = 3;
            Subtitle[] subtitles =
                (from numCharsPerSec
                 in expectedSpeeds
                 select GenerateSubtitleWith(numCharsPerSec * duration, duration)).ToArray();

            TextSpeedCalculator calculator = new(1);
            for (int i = 0; i < subtitles.Length; i++)
            {
                Assert.Equal(expectedSpeeds[i], calculator.EstimateCurrentSpeed(subtitles[i]));
            }
        }

        [Fact]
        public void EstimateCurrentSpeed_UseCircularBuffer()
        {
            const int duration = 3;
            Subtitle[] subtitles = {
                GenerateSubtitleWith(1 * duration, duration),
                GenerateSubtitleWith(3 * duration, duration),
                GenerateSubtitleWith(5 * duration, duration),
                GenerateSubtitleWith(2 * duration, duration),
            };

            double[] expectedSpeeds = new[] { 1.0, 2.0, 4.0, 3.5 };
            TextSpeedCalculator calculator = new(2);
            for (int i = 0; i < subtitles.Length; i++)
            {
                Assert.Equal(expectedSpeeds[i], calculator.EstimateCurrentSpeed(subtitles[i]));
            }
        }
    }
}