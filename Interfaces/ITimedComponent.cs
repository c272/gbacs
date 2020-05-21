using System;
using System.Collections.Generic;
using System.Text;

namespace gbacs.Interfaces
{
    /// <summary>
    /// Represents a timed component that interfaces with the CPU
    /// within GBACS.
    /// </summary>
    public interface ITimedComponent
    {
        /// <summary>
        /// Adds a given number of cycles to this timed component's counter.
        /// </summary>
        public void Cycle(int cycles);
    }
}
