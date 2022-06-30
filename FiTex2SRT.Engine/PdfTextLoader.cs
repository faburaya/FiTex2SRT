using System.Text;

using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Ladet das Manuskript aus Dateien im Format PDF.
    /// </summary>
    public class PdfTextLoader : ITranscriptLoader
    {
        /// <inheritdoc/>
        public string LoadRawTranscriptFromFile(string filePath)
        {
            StringBuilder buffer = new();

            using PdfReader reader = new(filePath);

            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                string text = PdfTextExtractor.GetTextFromPage(reader, page);
                buffer.AppendLine(text);
            }

            return buffer.ToString();
        }
    }
}