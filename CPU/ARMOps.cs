using System;
using System.Collections.Generic;
using System.Text;

namespace gbacs.CPU
{
    /// <summary>
    /// Operations for the ARM 7TDMi.
    /// </summary>
    public static class ARMOps_32
    {
        /// <summary>
        /// Processes a data processing instruction for the given CPU.
        /// </summary>
        public static void DataProcessing(ARM7TDMi cpu, uint instr)
        {
            //Get the flags out ([I]mmediate, [S]et condition codes).
            bool op2Immediate = (instr & 0x02000000) == 0x2000000;
            bool setCondCodes = (instr & 0x00100000) == 0x00100000;

            //Get the opcode, which is bytes 24-21, and switch on it.
            switch ((instr & 0x01E00000) >> 21)
            {
                //Logical AND.
                case 0:

            }
        }
    }
}
