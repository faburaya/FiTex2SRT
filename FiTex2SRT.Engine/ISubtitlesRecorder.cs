
namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Schnittstelle zum Speichern der Untertitel.
    /// </summary>
    public interface ISubtitlesRecorder
    {
        /// <summary>
        /// Speichert die Untertiteln auf einer Datei.
        /// </summary>
        /// <param name="subtitles">Die Liste mit den Untertiteln.</param>
        /// <param name="filePath">Das Dateipfad.</param>
        void Write(IEnumerable<Subtitle> subtitles, string filePath);
    }
}
