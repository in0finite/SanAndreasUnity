using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CielaSpike
{
    /// <summary>
    /// Ninja jumps between threads.
    /// </summary>
    public static class Ninja
    {
        /// <summary>
        /// Yield return it to switch to Unity main thread.
        /// </summary>
        public static readonly object JumpToUnity;
        /// <summary>
        /// Yield return it to switch to background thread.
        /// </summary>
        public static readonly object JumpBack;

        static Ninja()
        {
            JumpToUnity = new object();
            JumpBack = new object();
        }
    }
}