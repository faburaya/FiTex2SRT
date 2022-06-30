using Xunit;

namespace FiTex2SRT.Engine.UnitTests
{
    public class PdfTextLoaderTest
    {
        [Fact]
        public void LoadRawTranscriptFromFile_ManyPages_LoadTextFromAll()
        {
            PdfTextLoader pdfTextLoader = new();
            string expected = " Seite 1 \n Zeile 2 \r\n Seite 2 \n Zeile 4 \r\n";
            string actual = pdfTextLoader.LoadRawTranscriptFromFile("sample.pdf");
            Assert.Equal(expected, actual);
        }
    }
}