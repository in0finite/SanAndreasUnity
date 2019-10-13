/// <summary>
/// GTA audio sharp namespace
/// </summary>
namespace GTAAudioSharp
{
    /// <summary>
    /// GTA audio beat data structure
    /// </summary>
    public struct GTAAudioBeatData
    {
        /// <summary>
        /// Timing
        /// </summary>
        public readonly uint Timing;

        /// <summary>
        /// Control
        /// </summary>
        public readonly uint Control;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="timing">Timing</param>
        /// <param name="control">Control</param>
        internal GTAAudioBeatData(uint timing, uint control)
        {
            Timing = timing;
            Control = control;
        }
    }
}
