using System.Text;

namespace FiTex2SRT.Engine.UnitTests
{
    static class SubtitlesHelper
    {
        public static string GenerateExpectedSrtContent(Subtitle[] subtitles)
        {
            const string timeFormat = @"hh\:mm\:ss\,fff";

            int id = 0;
            StringBuilder buffer = new();
            foreach (var entry in subtitles)
            {
                buffer.AppendLine((++id).ToString());
                buffer.AppendLine($"{entry.startTime.ToString(timeFormat)} --> {entry.endTime.ToString(timeFormat)}");
                buffer.AppendLine(entry.caption);
                buffer.AppendLine();
            }

            return buffer.ToString();
        }
    }
}
