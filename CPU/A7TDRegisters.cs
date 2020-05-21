using System;
using System.Threading;

namespace gbacs
{
    /// <summary>
    /// Represents the available registers inside the ARM7TDMi.
    /// </summary>
    public class A7TDRegisters
    {
        //The current mode of the registers.
        public RegisterMode Mode { get; set; }

        //Registers 0-12, with FIQ as an offset of +5. 
        private uint[] standardRegisters = new uint[18];

        //Stack pointer, register 13.
        private uint[] reg13_SP = new uint[6];

        //Link register for BWL (Branch w/ Link) calls.
        private uint[] reg14_LR = new uint[6];

        //Program counter, register 15.
        private uint reg15_PC;

        //CPSR (global).
        private uint CPSR;

        //SPSR (FIQ through Undefined, user mode doesn't have this register.)
        private uint[] SPSR = new uint[5];

        /// <summary>
        /// Gets a given register from the bank.
        /// Indexes 0-15 are standard registers. 16 is CPSR, and 17 is SPSR.
        /// </summary>
        public ref uint Get(int regNum)
        {
            //Verify register number within bounds.
            if (regNum < 0 || regNum > 17)
            {
                throw new ArgumentOutOfRangeException("regNum", "Invalid register number to fetch, register " + regNum + " does not exist.");
            }

            //Switch based on register to fetch.
            if (regNum < 8)
            {
                //Universal register, same across all modes.
                return ref standardRegisters[regNum];
            }
            else if (regNum < 13)
            {
                //Unique register for FIQ, every other mode is the same.
                if (Mode == RegisterMode.FIQ) { regNum += 5; }
                return ref standardRegisters[regNum];
            }
            else if (regNum == 13)
            {
                //Stack pointer (register 13).
                return ref reg13_SP[(int)Mode];
            }
            else if (regNum == 14)
            {
                //Link register (register 14).
                return ref reg14_LR[(int)Mode];
            }
            else if (regNum == 15)
            {
                //Program counter (register 15).
                return ref reg15_PC;
            }
            else if (regNum == 16)
            {
                //CPSR.
                return ref CPSR;
            }
            else if (regNum == 17)
            {
                //SPSR.
                int index = (int)Mode - 1;
                if (index < 0)
                {
                    throw new Exception("Invalid mode to get SPSR for (must be in non-user mode).");
                }

                return ref SPSR[index];
            }

            throw new Exception("Invalid register referenced, not caught by range check.");
        }

        /// <summary>
        /// Returns the value of a specific flag from the CPSR register.
        /// </summary>
        public bool Get(Flags Flag)
        {
            return ((Get(16) >> (int)Flag) & 1) == 1;
        }
    }

    /// <summary>
    /// Represents a register flag for the ARM7TDMi.
    /// </summary>
    public enum Flags
    {
        T = 5, //state (ARM=0 or THUMB=1)
        F = 6, //disable FIQ interrupts
        I = 7, //disable IRQ interrupts
        V = 28, //overflow flag
        C = 29, //carry flag (1 = carry/no borrow, 0 = no carry/borrow)
        Z = 30, //zero flag
        N = 31 //sign flag
    }

    /// <summary>
    /// Represents individual register modes for the ARM7.
    /// </summary>
    public enum RegisterMode
    {
        User = 0,
        FIQ = 1,
        Supervisor = 2,
        Abort = 3,
        IRQ = 4,
        Undefined = 5
    }
}