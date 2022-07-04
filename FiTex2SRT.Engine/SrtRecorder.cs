
namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Speichert die Untertitel im SRT format.
    /// </summary>
    public class SrtRecorder : ISubtitlesRecorder
    {
        /// <inheritdoc/>
        public void Write(IEnumerable<Subtitle> subtitles, string filePath)
        {
            using FileStream stream = new(filePath, FileMode.Create, FileAccess.Write);
            using StreamWriter writer = new(stream);

            const string timeFormat = @"hh\:mm\:ss\,fff";
            int id = 0;
            foreach (Subtitle subtitle in subtitles)
            {
                writer.WriteLine(++id);
                writer.WriteLine(
                    $"{subtitle.startTime.ToString(timeFormat)} --> {subtitle.endTime.ToString(timeFormat)}");
                writer.WriteLine(subtitle.caption);
                writer.WriteLine();
            }

            writer.Close();
        }
    }
}
