using System;
using System.Collections.Generic;
using System.Text;

namespace gbacs.Extensions
{
    /// <summary>
    /// Extends the "Uint" class with extensions for debug purposes.
    /// </summary>
    public static class UIntExtensions
    {
        /// <summary>
        /// Returns the unsigned integer as a hex string.
        /// </summary>
        public static string ToHexString(this uint u)
        {
            return string.Format("{0}", u.ToString("X"));
        }

        /// <summary>
        /// Returns the unsigned integer as a binary string.
        /// </summary>
        public static string ToBinString(this uint u)
        {
            return Convert.ToString(u, 2);
        }
    }
}
