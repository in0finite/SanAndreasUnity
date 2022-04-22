using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public class ETAMeasurer
    {
        System.Diagnostics.Stopwatch m_stopwatch = System.Diagnostics.Stopwatch.StartNew();
        float m_lastProgressPerc = 0f;
        float m_changeInterval = 1f;
        public string ETA { get; private set; } = "0";


        public ETAMeasurer(float changeInterval)
        {
            if (float.IsNaN(changeInterval) || changeInterval < 0f)
                throw new System.ArgumentOutOfRangeException(nameof(changeInterval));

            m_changeInterval = changeInterval;
        }

        public void UpdateETA(float newProgressPerc)
        {
            if (float.IsNaN(newProgressPerc))
                return;

            newProgressPerc = Mathf.Clamp01(newProgressPerc);

            double elapsedSeconds = m_stopwatch.Elapsed.TotalSeconds;

            if (elapsedSeconds < m_changeInterval)
                return;

            m_stopwatch.Restart();

            if (m_lastProgressPerc > newProgressPerc) // progress reduced
            {
                // don't change current ETA
                m_lastProgressPerc = newProgressPerc;
                return;
            }

            double processedPerc = newProgressPerc - m_lastProgressPerc;
            double percPerSecond = processedPerc / elapsedSeconds;

            double percLeft = 1.0 - newProgressPerc;
            double secondsLeft = percLeft / percPerSecond;
            this.ETA = F.FormatElapsedTime(secondsLeft);

            m_lastProgressPerc = newProgressPerc;
        }
    }
}
