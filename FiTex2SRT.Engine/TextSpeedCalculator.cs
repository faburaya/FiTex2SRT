using System.Diagnostics;

namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Berechnet das Tempo des Texts in den Untertiteln.
    /// </summary>
    public class TextSpeedCalculator
    {
        private readonly int _circularBufferSize;

        private readonly record struct Stats(int charCount, TimeSpan duration);

        private readonly Queue<Stats> _lastSubStats;

        /// <summary>
        /// Erstellt eine neue Instanz von <see cref="TextSpeedCalculator"/>.
        /// </summary>
        /// <param name="circularBufferSize">
        /// Die Größe des Kreispuffers bestimmt, wie viele Untertitel für die Berechnung berücksichtigt werden können.</param>
        public TextSpeedCalculator(int circularBufferSize)
        {
            if (circularBufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException($"Cannot specify a circular buffer with negative size! {nameof(circularBufferSize)}={circularBufferSize}");
            }
            _circularBufferSize = circularBufferSize;
            _lastSubStats = new(capacity: _circularBufferSize);
        }

        /// <summary>
        /// Bezieht den letzten Untertitel in der Berechunung hinein
        /// und schätzt das derzeitige Tempo ein.
        /// </summary>
        /// <param name="subtitle">Der letzte Untertitel.</param>
        /// <returns>Die Geschwigkeit im Zeichen pro Sekunde.</returns>
        /// <remarks>Nur die letzten Untertitel werden berücksichtigt.</remarks>
        public double EstimateCurrentSpeed(Subtitle subtitle)
        {
            _lastSubStats.Enqueue(
                new Stats(subtitle.caption.Length, subtitle.endTime - subtitle.startTime));

            if (_lastSubStats.Count > _circularBufferSize)
                _lastSubStats.Dequeue();

            Debug.Assert(subtitle.endTime > subtitle.startTime);

            var total = _lastSubStats.Aggregate(
                (a, b) => new Stats(a.charCount + b.charCount, a.duration + b.duration));

            return 1000.0 * total.charCount / total.duration.TotalMilliseconds;
        }
    }
}
