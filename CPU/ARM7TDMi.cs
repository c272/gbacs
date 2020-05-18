using gbacs.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace gbacs
{
    /// <summary>
    /// Represents a virtual ARM7 + 16 bit Thumb + JTAG Debug + fast Multiplier + enhanced ICE CPU.
    /// Great name, guys.
    /// </summary>
    public class ARM7TDMi
    {
        //The registers for the ARM7 TDMi.
        private A7TDRegisters reg = new A7TDRegisters();

        //The current mode of the CPU (ARM or THUMB instructions).
        public CPUMode Mode { get; set; } = CPUMode.ARM;
        
        /// <summary>
        /// Executes a single instruction on the CPU.
        /// </summary>
        public void Execute(uint instr)
        {
            switch (Mode)
            {
                case CPUMode.ARM:
                    ExecuteARM(instr);
                    return;
                case CPUMode.THUMB:
                    ExecuteTHUMB(instr);
                    return;
            }
        }

        /// <summary>
        /// Executes a single 16bit THUMB instruction.
        /// </summary>
        private void ExecuteTHUMB(uint instr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes a single 32bit ARM instruction.
        /// </summary>
        private void ExecuteARM(uint instr)
        {
            //Combine bits 27-21 and bits 7-4 to get an identifiable pattern.
            uint combined = (instr << 4) | ((instr & 0x000000F0) << 16);

            //Print hex.
            Console.WriteLine(combined.ToHexString());
        }
    }

    /// <summary>
    /// The different available modes on the ARM7TDMi.
    /// </summary>
    public enum CPUMode
    {
        ARM,
        THUMB
    }
}
