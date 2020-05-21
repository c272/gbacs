using gbacs.CPU;
using gbacs.Extensions;
using gbacs.Interfaces;
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
        public A7TDRegisters reg = new A7TDRegisters();

        //The current mode of the CPU (ARM or THUMB instructions).
        public CPUMode Mode { get; set; } = CPUMode.ARM;

        //The amount of ticks that have elapsed on the CPU.
        public long Ticks { get; set; } = 0;

        //The list of linked components to this CPU to synchronize cycles with.
        public List<ITimedComponent> Components = new List<ITimedComponent>();
        
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
            //Get the condition out. Should this instruction even be executed?
            uint cond = (instr & 0xF0000000) >> 28;
            if (!EvaluateCond(cond))
            {
                return; //todo: inc sp and pc
            }

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
            else if (combined == 0x121)
            {
                //Branch and exchange. Switches to Thumb execution mode.
                Debug.WriteLine("Branch and exchange detected.");
            }
            //Hacky wizardry: top 2 bits are zero, no "1001" pattern in lower 4 bits.
            else if ((combined & 0xC00) == 0 && (combined & 0x00F) != 0x9)
            {
                //Data processing and FSR transfer.
                Debug.WriteLine("Data processing/FSR transfer detected.");
                ARMOps_32.DataProcessing(this, instr);
            }
            else if ((combined & 0xE40) == 0x040 && (combined & 0x009) == 0x009)
            {
                //Halfword data transfer w/ immediate offset.
                Debug.WriteLine("Halfword data transfer w/ imm. offset detected.");
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
        }

        /// <summary>
        /// Evaluates a conditional from an ARM instruction to determine
        /// whether an instruction should execute.
        /// </summary>
        private bool EvaluateCond(uint cond)
        {
            switch (cond)
            {
                //EQ (Z flag == 1)
                case 0x0:
                    return reg.Get(Flags.Z);
                //NE (Z flag == 0)
                case 0x1:
                    return !reg.Get(Flags.Z);
                //CS (Carry flag set)
                case 0x2:
                    return reg.Get(Flags.C);
                //CC (Carry flag not set)
                case 0x3:
                    return !reg.Get(Flags.C);
                //MI (signed negative, N set)
                case 0x4:
                    return reg.Get(Flags.N);
                //PL (zero or positive, N not set)
                case 0x5:
                    return !reg.Get(Flags.N);
                //VS (signed overflow, V set)
                case 0x6:
                    return reg.Get(Flags.V);
                //VC (signed no overflow, V not set)
                case 0x7:
                    return !reg.Get(Flags.V);
                //HI (unsigned higher, c=1 and z=0)
                case 0x8:
                    return reg.Get(Flags.C) && !reg.Get(Flags.Z);
                //LS (unsigned lower or same, c=0 or z=1)
                case 0x9:
                    return !reg.Get(Flags.C) || reg.Get(Flags.Z);
                //GE (signed >=, n=v)
                case 0xA:
                    return reg.Get(Flags.N) == reg.Get(Flags.V);
                //LT (signed <, n!=v)
                case 0xB:
                    return reg.Get(Flags.N) != reg.Get(Flags.V);
                //GT (signed >, z=0 and n=v)
                case 0xC:
                    return !reg.Get(Flags.Z) && (reg.Get(Flags.N) == reg.Get(Flags.V));
                //LE (signed <=, z=1 or n!=v)
                case 0xD:
                    return reg.Get(Flags.Z) || (reg.Get(Flags.N) != reg.Get(Flags.V));
                //AL (always)
                case 0xE:
                    return true;
                //NE (never, reserved on ARMv4)
                case 0xF:
                    throw new Exception("'Never' (NE) conditional used when it is reserved on ARMv3 and up.");
                default:
                    Debug.WriteLine("WARNING: Unknown condition value passed to evaluator.");
                    return false;
            }
        }

        /// <summary>
        /// Cycles the CPU for the given amount of ticks.
        /// </summary>
        /// <param name="cycles">The amount of cycles to apply.</param>
        public void Cycle(int ticks)
        {
            Ticks += ticks;
            foreach (var component in Components)
            {
                Cycle(ticks);
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
