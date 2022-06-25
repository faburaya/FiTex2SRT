using System.Text;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Loads transcripts from PDF files.
    /// </summary>
    public class PdfTextLoader : ITranscriptLoader
    {
        public string LoadRawTranscriptFromFile(string filePath)
        {
            StringBuilder buffer = new();

            using PdfReader reader = new(filePath);

            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                string text = PdfTextExtractor.GetTextFromPage(reader, page);
                buffer.Append(text);
            }

            return buffer.ToString();
        }
    }
}