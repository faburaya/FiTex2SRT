
namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Schnittstelle zum Laden des Manuskripts.
    /// </summary>
    public interface ITranscriptLoader
    {
        /// <summary>
        /// Ladet das Manuskript aus einem Datei.
        /// </summary>
        /// <param name="filePath">Das Dateipfad.</param>
        /// <returns>Der gesamte Inhalt der Datei im Text.</returns>
        string LoadRawTranscriptFromFile(string filePath);
    }
}