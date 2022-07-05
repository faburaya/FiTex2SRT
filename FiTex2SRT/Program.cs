using System.Diagnostics;
using System.Text;

using FiTex2SRT.Engine;

namespace FiTex2SRT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: program.exe input_transcript.pdf input_auto_subtitles.srt output_new_subtitles.srt");

                return;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Synchronizer synchronizer = new(new PdfTextLoader(), new SrtLoader());
            Transcript refinedTranscript = synchronizer.CreateRefinedTranscript(args[0], args[1]);
            List<Subtitle> subtitles = SubtitlesGenerator.CreateSubtitlesFrom(refinedTranscript);
            new SrtRecorder().Write(subtitles, args[2]);
        }
    }
}