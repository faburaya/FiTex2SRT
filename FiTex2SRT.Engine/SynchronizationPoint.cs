
namespace FiTex2SRT.Engine
{
    /// <summary>
    /// Ein Punkt zur Synchronisierung der Untertitel.
    /// </summary>
    /// <param name="Time">Wann der Punkt stattfindet.</param>
    /// <param name="Position">Wo der Punkt im Manuskript stattfindet.</param>
    public readonly record struct SynchronizationPoint(TimeSpan Time, int Position);
}
