using gbacs.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
            //Combine bits 27-20 and bits 7-4 to get an identifiable pattern.
            //This is stored in the lower 12 bits of the uint.
            uint combined = ((instr & 0x0FF00000) >> 16) | ((instr >> 4) & 0x0000000F);
            Debug.WriteLine("Instruction Identifier: " + combined.ToBinString());

            //Detect the type of instruction.
            if (combined >= 0xF00)
            {
                //Software interrupt.
                Debug.WriteLine("Software interrupt detected.");
            }
            else if ((combined & 0xE00) == 0xA00)
            {
                //Branch.
                Debug.WriteLine("Branch detected.");
            }
            else if ((combined & 0xE00) == 0xC00 || (combined & 0xE00) == 0xC01)
            {
                //Coprocessor command.
                //The GBA does not have a coprocessor, so we can completely ignore this.
                Debug.WriteLine("Coprocessor call detected.");
            }
            else if ((combined & 0xE00) == 0b10000000)
            {
                //Block data transfer.
                Debug.WriteLine("Block data transfer detected.");
            }
            else if (combined == 0x601)
            {
                //Magic number for an undefined instruction in ARMv4.
                //This is reserved for traps for debuggers and the like.
                throw new Exception("Undefined instruction 0x601 referenced.");
            }
            else if ((combined & 0xE00) == 0x600)
            {
                //Single data transfer.
                Debug.WriteLine("Single data transfer detected.");
            }
            else if ((combined & 0xE40) == 0x040 && (combined & 0x009) == 0x009)
            {
                //Halfword data transfer w/ immediate offset.
                Debug.WriteLine("Halfword data transfer w/ imm. offset detected.");
            }
            else if (combined == 0x121)
            {
                //Branch and exchange. Switches to Thumb execution mode.
                Debug.WriteLine("Branch and exchange detected.");
            }
            else if ((combined & 0xE40) == 0 && (combined & 0x009) != 0x009)
            {
                //Halfword data transfer w/ register offset detected.
                Debug.WriteLine("Halfword data transfer w/ register offset detected.");
            }
            else if ((combined & 0xFBF) == 0x109)
            {
                //Single data swap.
                Debug.WriteLine("Single data swap detected.");
            }
            else if ((combined & 0xF8F) == 0x089)
            {
                //Multiply long.
                Debug.WriteLine("Multiply long detected.");
            }
            else if ((combined & 0xFCF) == 0x009)
            {
                //Multiply.
                Debug.WriteLine("Multiply detected.");
            }
            if ((combined & 0xE00) == 0x200)
            {
                //Data processing and FSR transfer.
                Debug.WriteLine("Data processing/FSR transfer detected.");
            }
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
