using System.Diagnostics;
using System.Text;

using FiTex2SRT.Engine;

namespace FiTex2SRT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: program.exe transcript.pdf auto_subtitles.srt");
                return;
            }
            
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Synchronizer synchronizer = new(new PdfTextLoader(), new SrtLoader());
            Transcript refinedTranscript = synchronizer.CreateRefinedTranscript(args[0], args[1]);

            for (int idx = 1; idx < refinedTranscript.SyncPoints.Count; ++idx)
            {
                var prevSyncPoint = refinedTranscript.SyncPoints[idx - 1];
                var syncPoint = refinedTranscript.SyncPoints[idx];
                if (syncPoint.time > prevSyncPoint.time && syncPoint.pos > prevSyncPoint.pos)
                    continue;

                Console.WriteLine($"Synchronization point #{idx} is out-of-order: {syncPoint}");
            }

            Console.WriteLine("\n%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%\n");

            List<Subtitle> subtitles = SubtitlesGenerator.CreateSubtitlesFrom(refinedTranscript);
            foreach (Subtitle subtitle in subtitles)
            {
                Console.WriteLine($"[{subtitle.endTime - subtitle.startTime}] : {subtitle.caption}");
            }
        }
    }
}