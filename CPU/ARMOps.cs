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

            //Get the value of the first operand (op1) register.
            uint Rn = cpu.reg.Get((int)((instr & 0x000F0000) >> 16));

            //The shift's carry bit (to be set later).
            bool? shiftCarryBit = null;

            //Get the second operand out (based on the I flag).
            uint op2 = 0x0;
            if (op2Immediate)
            {
                //8-bit immediate value with 4-bit ROR.
                //Done in steps of 2, so have to multiply.
                int shiftAmt = (int)((op2Data & 0xF00) >> 8) * 2;
                uint toShift = op2Data & 0x0FF;
                op2 = (toShift >> shiftAmt) | (toShift << (32 - shiftAmt));

                //Set the carry flag.
                if (shiftAmt != 0) 
                {
                    cpu.reg.Set(Flags.C, (toShift & 1) == 1);
                }
            }
            else
            {
                //Register value with 8-bit shift.
                uint Rm = cpu.reg.Get((int)(op2Data & 0x00F));
                int shiftAmt = (int)((op2Data & 0xFF0) >> 4);

                //Get the type of shift that's being performed, from bits 6 & 5.
                uint shiftType = (op2Data & 0x060) >> 4;

                //Are we shifting by register, or immediate?
                bool shiftByReg = (op2Data & 0x010) == 0x010;
                if (shiftByReg)
                {
                    //Set the shift amount based on the value of the register.
                    //Only the lower 8 bits of the register are used.
                    shiftAmt = (int)cpu.reg.Get((int)((op2Data & 0xF00) >> 8)) & 0xFF;
                    if (shiftAmt == 0)
                    {
                        op2 = Rm;
                    }
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
                            //LSL, no shift (c unaffected)
                            case 0:
                                op2 = Rm;
                                break;

                            //LSR, taken as LSR#32 (Op2 zeroed, C flag set to bit 31)
                            case 1:
                                op2 = 0;
                                shiftCarryBit = (Rm & 0x80000000) == 0x80000000;
                                break;

                            //ASR, taken as ASR#32 (Op2 is all bit 31, C is bit 31)
                            case 2:
                                shiftCarryBit = (Rm & 0x80000000) == 0x80000000;
                                if ((bool)shiftCarryBit)
                                {
                                    op2 = 0xFFFFFFFF;
                                    break;
                                }
                                op2 = 0;
                                break;

                            //ROR, taken as RRX#1.
                            case 3:
                                shiftCarryBit = (Rm & 1) == 1;
                                op2 = ((Rm >> 1) & 0x7FFFFFFF);
                                if (cpu.reg.Get(Flags.C))
                                {
                                    op2 |= 0x80000000;
                                }
                                break;

                            default:
                                throw new Exception("Unrecognized zero shift type, were shift bits 5-6 parsed improperly?");
                        }
                    }
                }

                //Complete the shift (if shift amount > 0).
                if (shiftAmt != 0)
                {
                    switch (shiftType)
                    {
                        //logical shift left
                        case 0x0:
                            op2 = Rm << shiftAmt;
                            shiftCarryBit = (Rm >> 31) == 1;
                            break;

                        //logical shift right
                        case 0x1:
                            op2 = Rm >> shiftAmt;
                            shiftCarryBit = (Rm & 1) == 1;
                            break;

                        //arithmetic shift right
                        case 0x2:
                            op2 = (Rm >> shiftAmt) | (Rm & 0x80000000);
                            shiftCarryBit = (Rm & 1) == 1;
                            break;

                        //rotate right
                        case 0x3:
                            op2 = (Rm >> shiftAmt) | (Rm << (32 - shiftAmt));
                            shiftCarryBit = (Rm & 1) == 1;
                            break;

                        //unknown
                        default:
                            throw new Exception("Unknown shift type for DataProc, were shift bits 5-6 parsed improperly?");
                    }
                }
            }

            //Switch on the opcode and perform the actual operation.
            bool? carryFlag = null;
            bool? overflowFlag = null;
            uint result = 0x0;
            switch (opCode)
            {
                //Logical AND.
                case 0x0:
                    result = Rn & op2;
                    carryFlag = shiftCarryBit;
                    break;

                //Logical XOR.
                case 0x1:
                    result = Rn ^ op2;
                    carryFlag = shiftCarryBit;
                    break;

                //Subtraction.
                case 0x2:
                    result = Rn - op2;
                    carryFlag = false;
                    if (op2 > Rn)
                    {
                        carryFlag = true;
                    }
                    break;

                //Reverse subtraction.
                case 0x3:
                    result = op2 - Rn;
                    carryFlag = false;
                    if (Rn > op2)
                    {
                        carryFlag = true;
                    }
                    break;

                //Addition.
                case 0x4:
                    result = Rn + op2;
                    //Set overflow (MSB change).
                    if ((result & 0x80000000) != (Rn & 0x80000000))
                    {
                        overflowFlag = true;
                    }

                    //Set carry flag.
                    carryFlag = false;
                    if (Rn + op2 > uint.MaxValue)
                    {
                        carryFlag = true;
                    }

                    break;

                //Add with carry.
                case 0x5:
                    result = Rn + op2;
                    if (cpu.reg.Get(Flags.C))
                    {
                        result++;
                    }

                    //Overflow flag (MSB change).
                    if ((result & 0x80000000) != (Rn & 0x80000000))
                    {
                        overflowFlag = true;
                    }

                    //Set carry flag.
                    carryFlag = false;
                    if (Rn + op2 + Convert.ToInt32(cpu.reg.Get(Flags.C)) > uint.MaxValue)
                    {
                        carryFlag = true;
                    }
                    break;

                //Subtract with carry.
                case 0x6:
                    result = Rn - op2 - 1;
                    if (cpu.reg.Get(Flags.C))
                    {
                        result++;
                    }

                    //Set carry flag.
                    carryFlag = false;
                    if (Rn - op2 - 1 + Convert.ToInt32(cpu.reg.Get(Flags.C)) < 0)
                    {
                        carryFlag = true;
                    }
                    break;

                //Subtract with carry reversed.
            }
        }
    }
}
