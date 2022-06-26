
namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Schnittstelle zum Laden der Untertitel.
    /// </summary>
    public interface ISubtitlesLoader
    {
        /// <summary>
        /// Ladet die Untertitel aus einer Datei.
        /// </summary>
        /// <param name="filePath">Das Dateipfad.</param>
        /// <param name="countOfChars">Die Zahl von Zeichen der gesamten Untertitel.</param>
        /// <returns>Eine Liste mit einer Instanz von <see cref="Subtitle"/> für jeden Untertitel.</returns>
        public List<Subtitle> LoadSubtitlesFromFile(string filePath, out int countOfChars);
    }
}
