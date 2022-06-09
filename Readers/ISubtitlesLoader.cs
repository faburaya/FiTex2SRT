
namespace Readers
{
    public interface ISubtitlesLoader
    {
        public IList<Subtitle> LoadSubtitlesFromFile(string filePath);
    }
}
