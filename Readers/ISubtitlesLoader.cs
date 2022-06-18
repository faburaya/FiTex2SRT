
namespace Readers
{
    /// <summary>
    /// Interface for loading subtitles.
    /// </summary>
    public interface ISubtitlesLoader
    {
        /// <summary>
        /// Load subtitles from a file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="countOfChars">Receives the count of characters of all captions.</param>
        /// <returns>A list with all subtitles.</returns>
        public IList<Subtitle> LoadSubtitlesFromFile(string filePath, out int countOfChars);
    }
}
