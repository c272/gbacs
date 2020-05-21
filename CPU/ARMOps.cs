using gbacs.Extensions;
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
            //Get the flags & small data out ([I]mmediate, [S]et condition codes).
            bool op2Immediate = (instr & 0x02000000) == 0x2000000;
            bool setCondCodes = (instr & 0x00100000) == 0x00100000;
            uint op2Data = (instr & 0x00000FFF) >> 12;
            uint opCode = (instr & 0x01E00000) >> 21;

            //Get the destination register as a reference.
            ref uint Rd = ref cpu.reg.Get((int)((instr & 0x0000F000) >> 12));

            //Get the value of the first operand register.
            uint Rn = cpu.reg.Get((int)((instr & 0x000F0000) >> 16));

            //Get the second operand out (based on the I flag).
            uint op2;
            if (op2Immediate)
            {
                //8-bit immediate value with 4-bit rotate right.
                //Done in steps of 2, so have to multiply.
                int shiftAmt = (int)((op2Data & 0xF00) >> 8) * 2;
                uint toShift = op2Data & 0x0FF;
                op2 = (toShift >> shiftAmt) | (toShift << (32 - shiftAmt));

                //todo: determine if ROR sets the carry flag
            }
            else
            {
                //Register value with 8-bit shift.
                //Only the lower 8 bits of the register are used.
                uint Rm = cpu.reg.Get((int)(op2Data & 0x00F)) & 0x000000FF;
                int shiftAmt = (int)((op2Data & 0xFF0) >> 4);

                //Get the type of shift that's being performed, from bits 6 & 5.
                uint shiftType = (op2Data & 0x060) >> 4;

                //Are we shifting by register, or immediate?
                bool shiftByReg = (op2Data & 0x010) == 0x010;
                if (shiftByReg)
                {
                    //Set the shift amount based on the value of the register.
                    //Register for shift is in bits 11-8.
                    shiftAmt = (int)cpu.reg.Get((int)((op2Data & 0xF00) >> 8));
                }
                else
                {
                    //Shifting by immediate. Data is in bytes 11-7.
                    shiftAmt = (int)((op2Data & 0xF80) >> 7);

                    //Is the shift zero? If so, some funky stuff happens.
                    if (shiftAmt == 0)
                    {
                        switch (shiftType)
                        {
                            //todo
                        }
                    }
                }

                //Complete the shift.
                bool carryBit = false;
                switch (shiftType)
                {
                    //logical shift left
                    case 0x0:
                        op2 = Rm << shiftAmt;
                        carryBit = (Rm >> 31) == 1;
                        break;

                    //logical shift right
                    case 0x1:
                        op2 = Rm >> shiftAmt;
                        carryBit = (Rm & 1) == 1;
                        break;

                    //arithmetic shift right
                    case 0x2:
                        op2 = (Rm >> shiftAmt) | (Rm & 0x80000000);
                        carryBit = (Rm & 1) == 1;
                        break;

                    //rotate right
                    case 0x3:
                        op2 = (Rm >> shiftAmt) | (Rm << (32 - shiftAmt));
                        carryBit = cpu.reg.Get(Flags.C);
                        break;

                    //unknown
                    default:
                        throw new Exception("Unknown shift type for DataProc, were shift bits 5-6 parsed improperly?");
                }

                //Should the carry flag be set? (for logical operations)
                if (setCondCodes && (opCode.Between(0x0, 0x1) || opCode.Between(0x8, 0x9) || opCode >= 0xC))
                {
                    //Set it as whether the calculation returned the expected result.
                    cpu.reg.Set(Flags.C, carryBit);
                }
            }

            //Switch on the opcode and perform the actual operation.
            switch (opCode)
            {
                //Logical AND.
                case 0:
                    Rd = Rn & op2;
                    break;
            }
        }
    }
}
