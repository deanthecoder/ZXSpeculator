// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

namespace Speculator.Core;

public partial class Z80Instructions
{
    private Instruction[] m_instructions;
    private Instruction[] m_instructionSubSetDDCB;
    private Instruction[] m_instructionSubSetFDCB;
    private Instruction[] m_fdcbByOp;
    private Instruction[] m_opcodeToInstructionLUT;
    private Instruction m_nop;
    private Instruction m_nopnop;

    private Instruction[] InstructionSubSetDDCB =>
        m_instructionSubSetDDCB ??= GetDdCbInstructions().ToArray();

    private Instruction[] InstructionSubSetFDCB =>
        m_instructionSubSetFDCB ??= GetFdCbInstructions().ToArray();

    private Instruction[] InitFdcbLookup()
    {
        var lut = new Instruction[256];
        var list = InstructionSubSetFDCB;
        for (var i = 0; i < list.Length; i++)
        {
            var inst = list[i];
            var tokens = inst.HexTemplate.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // Expect pattern: FD CB d xx (or equivalent 4-byte template). The last token is the CB opcode (xx).
            var last = tokens[^1];
            var op = Convert.ToByte(last, 16);
            lut[op] = inst;
        }
        return lut;
    }

    public Instruction Nop => m_nop ??= new(InstructionID.NOP, "NOP", "00", 4);
    public Instruction NopNop => m_nopnop ??= m_instructions.First(t => t.Id == InstructionID.NOPNOP);
    
    public Z80Instructions()
    {
        InitList();
    }
    
    public Instruction FindInstructionAtMemoryLocation(Memory mainMemory, ushort addr)
    {
        var opcode = mainMemory.Peek(addr);
        var instruction = m_opcodeToInstructionLUT[opcode];
        if (instruction != null)
            return instruction;

        // Handle instruction prefixes.
        switch (opcode)
        {
            case 0xCB:
                return HandleCBPrefix(mainMemory, addr);
            case 0xDD:
                if (HandleDDPrefix(mainMemory, addr, out var ddInstruction))
                    return ddInstruction;
                break;
            case 0xED:
                if (HandleEDPrefix(mainMemory, addr, out var edInstruction))
                    return edInstruction;
                break;
            case 0xFD:
                if (HandleFDPrefix(mainMemory, addr, out var fdInstruction))
                    return fdInstruction;
                break;
        }

        return null;
    }

    private Instruction[] InitPrimaryInstructionLookup()
    {
        var lookup = new[]
        {
            m_instructions[445], // 0x00 NOP
            m_instructions[279], // 0x01 LD BC,nn
            m_instructions[410], // 0x02 LD (BC),A
            m_instructions[207], // 0x03 INC BC
            m_instructions[206], // 0x04 INC B
            m_instructions[174], // 0x05 DEC B
            m_instructions[294], // 0x06 LD B,n
            m_instructions[578], // 0x07 RLCA
            m_instructions[196], // 0x08 EX AF,AF'
            m_instructions[34], // 0x09 ADD HL,BC
            m_instructions[272], // 0x0A LD A,(BC)
            m_instructions[175], // 0x0B DEC BC
            m_instructions[208], // 0x0C INC C
            m_instructions[176], // 0x0D DEC C
            m_instructions[309], // 0x0E LD C,n
            m_instructions[601], // 0x0F RRCA
            m_instructions[142], // 0x10 DJNZ n
            m_instructions[311], // 0x11 LD DE,nn
            m_instructions[411], // 0x12 LD (DE),A
            m_instructions[210], // 0x13 INC DE
            m_instructions[209], // 0x14 INC D
            m_instructions[177], // 0x15 DEC D
            m_instructions[326], // 0x16 LD D,n
            m_instructions[577], // 0x17 RLA
            m_instructions[253], // 0x18 JR d
            m_instructions[35], // 0x19 ADD HL,DE
            m_instructions[273], // 0x1A LD A,(DE)
            m_instructions[178], // 0x1B DEC DE
            m_instructions[211], // 0x1C INC E
            m_instructions[179], // 0x1D DEC E
            m_instructions[341], // 0x1E LD E,n
            m_instructions[600], // 0x1F RRA
            m_instructions[251], // 0x20 JR NZ,d
            m_instructions[344], // 0x21 LD HL,nn
            m_instructions[412], // 0x22 LD (nn),HL
            m_instructions[213], // 0x23 INC HL
            m_instructions[212], // 0x24 INC H
            m_instructions[180], // 0x25 DEC H
            m_instructions[355], // 0x26 LD H,n
            m_instructions[172], // 0x27 DAA
            m_instructions[252], // 0x28 JR Z,d
            m_instructions[36], // 0x29 ADD HL,HL
            m_instructions[342], // 0x2A LD HL,(nn)
            m_instructions[181], // 0x2B DEC HL
            m_instructions[220], // 0x2C INC L
            m_instructions[188], // 0x2D DEC L
            m_instructions[403], // 0x2E LD L,n
            m_instructions[156], // 0x2F CPL
            m_instructions[250], // 0x30 JR NC,d
            m_instructions[409], // 0x31 LD SP,nn
            m_instructions[438], // 0x32 LD (nn),A
            m_instructions[221], // 0x33 INC SP
            m_instructions[222], // 0x34 INC (HL)
            m_instructions[190], // 0x35 DEC (HL)
            m_instructions[421], // 0x36 LD (HL),n
            m_instructions[650], // 0x37 SCF
            m_instructions[249], // 0x38 JR C,d
            m_instructions[37], // 0x39 ADD HL,SP
            m_instructions[271], // 0x3A LD A,(nn)
            m_instructions[189], // 0x3B DEC SP
            m_instructions[205], // 0x3C INC A
            m_instructions[173], // 0x3D DEC A
            m_instructions[277], // 0x3E LD A,n
            m_instructions[151], // 0x3F CCF
            m_instructions[281], // 0x40 LD B,B
            m_instructions[282], // 0x41 LD B,C
            m_instructions[283], // 0x42 LD B,D
            m_instructions[284], // 0x43 LD B,E
            m_instructions[285], // 0x44 LD B,H
            m_instructions[290], // 0x45 LD B,L
            m_instructions[291], // 0x46 LD B,(HL)
            m_instructions[280], // 0x47 LD B,A
            m_instructions[296], // 0x48 LD C,B
            m_instructions[297], // 0x49 LD C,C
            m_instructions[298], // 0x4A LD C,D
            m_instructions[299], // 0x4B LD C,E
            m_instructions[300], // 0x4C LD C,H
            m_instructions[305], // 0x4D LD C,L
            m_instructions[306], // 0x4E LD C,(HL)
            m_instructions[295], // 0x4F LD C,A
            m_instructions[313], // 0x50 LD D,B
            m_instructions[314], // 0x51 LD D,C
            m_instructions[315], // 0x52 LD D,D
            m_instructions[316], // 0x53 LD D,E
            m_instructions[317], // 0x54 LD D,H
            m_instructions[322], // 0x55 LD D,L
            m_instructions[323], // 0x56 LD D,(HL)
            m_instructions[312], // 0x57 LD D,A
            m_instructions[328], // 0x58 LD E,B
            m_instructions[329], // 0x59 LD E,C
            m_instructions[330], // 0x5A LD E,D
            m_instructions[331], // 0x5B LD E,E
            m_instructions[332], // 0x5C LD E,H
            m_instructions[337], // 0x5D LD E,L
            m_instructions[338], // 0x5E LD E,(HL)
            m_instructions[327], // 0x5F LD E,A
            m_instructions[346], // 0x60 LD H,B
            m_instructions[347], // 0x61 LD H,C
            m_instructions[348], // 0x62 LD H,D
            m_instructions[349], // 0x63 LD H,E
            m_instructions[350], // 0x64 LD H,H
            m_instructions[351], // 0x65 LD H,L
            m_instructions[352], // 0x66 LD H,(HL)
            m_instructions[345], // 0x67 LD H,A
            m_instructions[394], // 0x68 LD L,B
            m_instructions[395], // 0x69 LD L,C
            m_instructions[396], // 0x6A LD L,D
            m_instructions[397], // 0x6B LD L,E
            m_instructions[398], // 0x6C LD L,H
            m_instructions[399], // 0x6D LD L,L
            m_instructions[400], // 0x6E LD L,(HL)
            m_instructions[393], // 0x6F LD L,A
            m_instructions[415], // 0x70 LD (HL),B
            m_instructions[416], // 0x71 LD (HL),C
            m_instructions[417], // 0x72 LD (HL),D
            m_instructions[418], // 0x73 LD (HL),E
            m_instructions[419], // 0x74 LD (HL),H
            m_instructions[420], // 0x75 LD (HL),L
            m_instructions[201], // 0x76 HALT
            m_instructions[414], // 0x77 LD (HL),A
            m_instructions[259], // 0x78 LD A,B
            m_instructions[260], // 0x79 LD A,C
            m_instructions[261], // 0x7A LD A,D
            m_instructions[262], // 0x7B LD A,E
            m_instructions[263], // 0x7C LD A,H
            m_instructions[269], // 0x7D LD A,L
            m_instructions[274], // 0x7E LD A,(HL)
            m_instructions[258], // 0x7F LD A,A
            m_instructions[20], // 0x80 ADD A,B
            m_instructions[21], // 0x81 ADD A,C
            m_instructions[22], // 0x82 ADD A,D
            m_instructions[23], // 0x83 ADD A,E
            m_instructions[24], // 0x84 ADD A,H
            m_instructions[29], // 0x85 ADD A,L
            m_instructions[30], // 0x86 ADD A,(HL)
            m_instructions[19], // 0x87 ADD A,A
            m_instructions[1], // 0x88 ADC A,B
            m_instructions[2], // 0x89 ADC A,C
            m_instructions[3], // 0x8A ADC A,D
            m_instructions[4], // 0x8B ADC A,E
            m_instructions[5], // 0x8C ADC A,H
            m_instructions[10], // 0x8D ADC A,L
            m_instructions[11], // 0x8E ADC A,(HL)
            m_instructions[0], // 0x8F ADC A,A
            m_instructions[770], // 0x90 SUB B
            m_instructions[771], // 0x91 SUB C
            m_instructions[772], // 0x92 SUB D
            m_instructions[773], // 0x93 SUB E
            m_instructions[774], // 0x94 SUB H
            m_instructions[779], // 0x95 SUB L
            m_instructions[780], // 0x96 SUB (HL)
            m_instructions[769], // 0x97 SUB A
            m_instructions[632], // 0x98 SBC A,B
            m_instructions[633], // 0x99 SBC A,C
            m_instructions[634], // 0x9A SBC A,D
            m_instructions[635], // 0x9B SBC A,E
            m_instructions[636], // 0x9C SBC A,H
            m_instructions[641], // 0x9D SBC A,L
            m_instructions[642], // 0x9E SBC A,(HL)
            m_instructions[631], // 0x9F SBC A,A
            m_instructions[47], // 0xA0 AND B
            m_instructions[48], // 0xA1 AND C
            m_instructions[49], // 0xA2 AND D
            m_instructions[50], // 0xA3 AND E
            m_instructions[51], // 0xA4 AND H
            m_instructions[56], // 0xA5 AND L
            m_instructions[57], // 0xA6 AND (HL)
            m_instructions[46], // 0xA7 AND A
            m_instructions[785], // 0xA8 XOR B
            m_instructions[786], // 0xA9 XOR C
            m_instructions[787], // 0xAA XOR D
            m_instructions[788], // 0xAB XOR E
            m_instructions[789], // 0xAC XOR H
            m_instructions[794], // 0xAD XOR L
            m_instructions[795], // 0xAE XOR (HL)
            m_instructions[784], // 0xAF XOR A
            m_instructions[448], // 0xB0 OR B
            m_instructions[449], // 0xB1 OR C
            m_instructions[450], // 0xB2 OR D
            m_instructions[451], // 0xB3 OR E
            m_instructions[452], // 0xB4 OR H
            m_instructions[457], // 0xB5 OR L
            m_instructions[458], // 0xB6 OR (HL)
            m_instructions[447], // 0xB7 OR A
            m_instructions[158], // 0xB8 CP B
            m_instructions[159], // 0xB9 CP C
            m_instructions[160], // 0xBA CP D
            m_instructions[161], // 0xBB CP E
            m_instructions[162], // 0xBC CP H
            m_instructions[167], // 0xBD CP L
            m_instructions[168], // 0xBE CP (HL)
            m_instructions[157], // 0xBF CP A
            m_instructions[572], // 0xC0 RET NZ
            m_instructions[475], // 0xC1 POP BC
            m_instructions[240], // 0xC2 JP NZ,nn
            m_instructions[248], // 0xC3 JP nn
            m_instructions[145], // 0xC4 CALL NZ,nn
            m_instructions[481], // 0xC5 PUSH BC
            m_instructions[33], // 0xC6 ADD A,n
            m_instructions[623], // 0xC7 RST 00
            m_instructions[576], // 0xC8 RET Z
            m_instructions[566], // 0xC9 RET
            m_instructions[244], // 0xCA JP Z,nn
            null, // 0xCB
            m_instructions[149], // 0xCC CALL Z,nn
            m_instructions[150], // 0xCD CALL nn
            m_instructions[14], // 0xCE ADC A,n
            m_instructions[624], // 0xCF RST 08
            m_instructions[571], // 0xD0 RET NC
            m_instructions[476], // 0xD1 POP DE
            m_instructions[239], // 0xD2 JP NC,nn
            m_instructions[473], // 0xD3 OUT (n),A
            m_instructions[144], // 0xD4 CALL NC,nn
            m_instructions[482], // 0xD5 PUSH DE
            m_instructions[783], // 0xD6 SUB n
            m_instructions[625], // 0xD7 RST 10
            m_instructions[569], // 0xD8 RET C
            m_instructions[195], // 0xD9 EXX
            m_instructions[237], // 0xDA JP C,nn
            m_instructions[230], // 0xDB IN A,(n)
            m_instructions[141], // 0xDC CALL C,nn
            null, // 0xDD
            m_instructions[645], // 0xDE SBC A,n
            m_instructions[626], // 0xDF RST 18
            m_instructions[574], // 0xE0 RET PO
            m_instructions[477], // 0xE1 POP HL
            m_instructions[242], // 0xE2 JP PO,nn
            m_instructions[198], // 0xE3 EX (SP),HL
            m_instructions[147], // 0xE4 CALL PO,nn
            m_instructions[483], // 0xE5 PUSH HL
            m_instructions[60], // 0xE6 AND n
            m_instructions[627], // 0xE7 RST 20
            m_instructions[573], // 0xE8 RET PE
            m_instructions[245], // 0xE9 JP (HL)
            m_instructions[241], // 0xEA JP PE,nn
            m_instructions[197], // 0xEB EX DE,HL
            m_instructions[146], // 0xEC CALL PE,nn
            null, // 0xED
            m_instructions[798], // 0xEE XOR n
            m_instructions[628], // 0xEF RST 28
            m_instructions[575], // 0xF0 RET P
            m_instructions[474], // 0xF1 POP AF
            m_instructions[243], // 0xF2 JP P,nn
            m_instructions[193], // 0xF3 DI
            m_instructions[148], // 0xF4 CALL P,nn
            m_instructions[480], // 0xF5 PUSH AF
            m_instructions[461], // 0xF6 OR n
            m_instructions[629], // 0xF7 RST 30
            m_instructions[570], // 0xF8 RET M
            m_instructions[405], // 0xF9 LD SP,HL
            m_instructions[238], // 0xFA JP M,nn
            m_instructions[194], // 0xFB EI
            m_instructions[143], // 0xFC CALL M,nn
            null, // 0xFD
            m_instructions[171], // 0xFE CP n
            m_instructions[630], // 0xFF RST 38
        };

        return lookup;
    }

    /// <summary>
    /// Treat HL in next instruction as IY.
    /// </summary>
    private bool HandleFDPrefix(Memory mainMemory, ushort addr, out Instruction instr)
    {
        // Single read of the next opcode using the underlying array to avoid repeated Peek(...). 
        var data = mainMemory.Data;
        var nextIndex = addr + 1;
        if (nextIndex >= data.Length)
        {
            instr = null;
            return true; // Treat as a recognized prefix with incomplete stream.
        }

        var nextOpcode = data[nextIndex];
        switch (nextOpcode)
        {
            case 0xCB:
            {
                // FD CB d xx. Use a direct LUT keyed by the final CB opcode (xx) instead of scanning the subset.
                m_fdcbByOp ??= InitFdcbLookup();

                var finalIndex = nextIndex + 2; // addr+3 overall (skip displacement d at addr+2).
                if (finalIndex < data.Length)
                {
                    var cbOp = data[finalIndex];
                    instr = m_fdcbByOp[cbOp];
                }
                else
                {
                    instr = null;
                }
                return true;
            }
            case 0x09: // ADD IY,BC.
            {
                instr = m_instructions[42];
                return true;
            }
            case 0x19: // ADD IY,DE
            {
                instr = m_instructions[43];
                return true;
            }
            case 0x21: // LD IY,nn
            {
                instr = m_instructions[391];
                return true;
            }
            case 0x22: // LD (nn),IY
            {
                instr = m_instructions[442];
                return true;
            }
            case 0x23: // INC IY.
            {
                instr = m_instructions[217];
                return true;
            }
            case 0x24: // INC IYH.
            {
                instr = m_instructions[218];
                return true;
            }
            case 0x25: // DEC IYH.
            {
                instr = m_instructions[186];
                return true;
            }
            case 0x26: // LD IYH,n.
            {
                instr = m_instructions[381];
                return true;
            }
            case 0x29: // ADD IY,IY.
            {
                instr = m_instructions[44];
                return true;
            }
            case 0x2A: // LD IY,(nn).
            {
                instr = m_instructions[390];
                return true;
            }
            case 0x2B: // DEC IY.
            {
                instr = m_instructions[185];
                return true;
            }
            case 0x2C: // INC IYL.
            {
                instr = m_instructions[219];
                return true;
            }
            case 0x2D: // DEC IYL.
            {
                instr = m_instructions[187];
                return true;
            }
            case 0x2E: // LD IYL,n.
            {
                instr = m_instructions[389];
                return true;
            }
            case 0x34: // INC (IY+d).
            {
                instr = m_instructions[224];
                return true;
            }
            case 0x35: // DEC (IY+d).
            {
                instr = m_instructions[192];
                return true;
            }
            case 0x36: // LD (IY+d),n.
            {
                instr = m_instructions[437];
                return true;
            }
            case 0x39: // ADD IY,SP.
            {
                instr = m_instructions[45];
                return true;
            }
            case 0x44: // LD B,IYH.
            {
                instr = m_instructions[288];
                return true;
            }
            case 0x45: // LD B,IYL.
            {
                instr = m_instructions[289];
                return true;
            }
            case 0x46: // LD B,(IY+d).
            {
                instr = m_instructions[293];
                return true;
            }
            case 0x4C: // LD C,IYH.
            {
                instr = m_instructions[303];
                return true;
            }
            case 0x4D: // LD C,IYL.
            {
                instr = m_instructions[304];
                return true;
            }
            case 0x4E: // LD C,(IY+d).
            {
                instr = m_instructions[308];
                return true;
            }
            case 0x54: // LD D,IYH.
            {
                instr = m_instructions[320];
                return true;
            }
            case 0x55: // LD D,IYL.
            {
                instr = m_instructions[321];
                return true;
            }
            case 0x56: // LD D,(IY+d).
            {
                instr = m_instructions[325];
                return true;
            }
            case 0x5C: // LD E,IYH.
            {
                instr = m_instructions[335];
                return true;
            }
            case 0x5D: // LD E,IYL.
            {
                instr = m_instructions[336];
                return true;
            }
            case 0x5E: // LD E,(IY+d).
            {
                instr = m_instructions[340];
                return true;
            }
            case 0x60: // LD IYH,B.
            {
                instr = m_instructions[375];
                return true;
            }
            case 0x61: // LD IYH,C.
            {
                instr = m_instructions[376];
                return true;
            }
            case 0x62: // LD IYH,D.
            {
                instr = m_instructions[377];
                return true;
            }
            case 0x63: // LD IYH,E.
            {
                instr = m_instructions[378];
                return true;
            }
            case 0x64: // LD IYH,IYH.
            {
                instr = m_instructions[379];
                return true;
            }
            case 0x65: // LD IYH,IYL.
            {
                instr = m_instructions[380];
                return true;
            }
            case 0x66: // LD H,(IY+d).
            {
                instr = m_instructions[354];
                return true;
            }
            case 0x67: // LD IYH,A.
            {
                instr = m_instructions[374];
                return true;
            }
            case 0x68: // LD IYL,B.
            {
                instr = m_instructions[383];
                return true;
            }
            case 0x69: // LD IYL,C.
            {
                instr = m_instructions[384];
                return true;
            }
            case 0x6A: // LD IYL,D.
            {
                instr = m_instructions[385];
                return true;
            }
            case 0x6B: // LD IYL,E.
            {
                instr = m_instructions[386];
                return true;
            }
            case 0x6C: // LD IYL,IYH.
            {
                instr = m_instructions[387];
                return true;
            }
            case 0x6D: // LD IYL,IYL.
            {
                instr = m_instructions[388];
                return true;
            }
            case 0x6E: // LD L,(IY+d).
            {
                instr = m_instructions[402];
                return true;
            }
            case 0x6F: // LD IYL,A.
            {
                instr = m_instructions[382];
                return true;
            }
            case 0x70: // LD (IY+d),B.
            {
                instr = m_instructions[431];
                return true;
            }
            case 0x71: // LD (IY+d),C.
            {
                instr = m_instructions[432];
                return true;
            }
            case 0x72: // LD (IY+d),D.
            {
                instr = m_instructions[433];
                return true;
            }
            case 0x73: // LD (IY+d),E.
            {
                instr = m_instructions[434];
                return true;
            }
            case 0x74: // LD (IY+d),H.
            {
                instr = m_instructions[435];
                return true;
            }
            case 0x75: // LD (IY+d),L.
            {
                instr = m_instructions[436];
                return true;
            }
            case 0x77: // LD (IY+d),A.
            {
                instr = m_instructions[430];
                return true;
            }
            case 0x7C: // LD A,IYH.
            {
                instr = m_instructions[267];
                return true;
            }
            case 0x7D: // LD A,IYL.
            {
                instr = m_instructions[268];
                return true;
            }
            case 0x7E: // LD A,(IY+d).
            {
                instr = m_instructions[276];
                return true;
            }
            case 0x84: // ADD A,IYH.
            {
                instr = m_instructions[27];
                return true;
            }
            case 0x85: // ADD A,IYL.
            {
                instr = m_instructions[28];
                return true;
            }
            case 0x86: // ADD A,(IY+d).
            {
                instr = m_instructions[32];
                return true;
            }
            case 0x8C: // ADC A,IYH.
            {
                instr = m_instructions[8];
                return true;
            }
            case 0x8D: // ADC A,IYL.
            {
                instr = m_instructions[9];
                return true;
            }
            case 0x8E: // ADC A,(IY+d).
            {
                instr = m_instructions[13];
                return true;
            }
            case 0x94: // SUB IYH.
            {
                instr = m_instructions[777];
                return true;
            }
            case 0x95: // SUB IYL.
            {
                instr = m_instructions[778];
                return true;
            }
            case 0x96: // SUB (IY+d).
            {
                instr = m_instructions[782];
                return true;
            }
            case 0x9C: // SBC A,IYH.
            {
                instr = m_instructions[639];
                return true;
            }
            case 0x9D: // SBC A,IYL.
            {
                instr = m_instructions[640];
                return true;
            }
            case 0x9E: // SBC A,(IY+d).
            {
                instr = m_instructions[644];
                return true;
            }
            case 0xA4: // AND IYH.
            {
                instr = m_instructions[54];
                return true;
            }
            case 0xA5: // AND IYL.
            {
                instr = m_instructions[55];
                return true;
            }
            case 0xA6: // AND (IY+d).
            {
                instr = m_instructions[59];
                return true;
            }
            case 0xAC: // XOR IYH.
            {
                instr = m_instructions[792];
                return true;
            }
            case 0xAD: // XOR IYL.
            {
                instr = m_instructions[793];
                return true;
            }
            case 0xAE: // XOR (IY+d).
            {
                instr = m_instructions[797];
                return true;
            }
            case 0xB4: // OR IYH.
            {
                instr = m_instructions[455];
                return true;
            }
            case 0xB5: // OR IYL.
            {
                instr = m_instructions[456];
                return true;
            }
            case 0xB6: // OR (IY+d).
            {
                instr = m_instructions[460];
                return true;
            }
            case 0xBC: // CP IYH.
            {
                instr = m_instructions[165];
                return true;
            }
            case 0xBD: // CP IYL.
            {
                instr = m_instructions[166];
                return true;
            }
            case 0xBE: // CP (IY+d).
            {
                instr = m_instructions[170];
                return true;
            }
            case 0xE1: // POP IY.
            {
                instr = m_instructions[479];
                return true;
            }
            case 0xE3: // EX (SP),IY.
            {
                instr = m_instructions[200];
                return true;
            }
            case 0xE5: // PUSH IY.
            {
                instr = m_instructions[485];
                return true;
            }
            case 0xE9: // JP (IY).
            {
                instr = m_instructions[247];
                return true;
            }
            case 0xF9: // LD SP,IY.
            {
                instr = m_instructions[407];
                return true;
            }
        }

        instr = null;
        return false;
    }
    
    private bool HandleEDPrefix(Memory mainMemory, int addr, out Instruction instr)
    {
        switch (mainMemory.Peek((ushort)(addr + 1)))
        {
            case 0x40: // IN B,(C)
            {
                instr = m_instructions[231];
                return true;
            }
            case 0x41: // OUT B,(C)
            {
                instr = m_instructions[467];
                return true;
            }
            case 0x42: // SBC HL,BC
            {
                instr = m_instructions[646];
                return true;
            }
            case 0x43: // LD (nn),BC
            {
                instr = m_instructions[439];
                return true;
            }
            case 0x44: // NEG
            {
                instr = m_instructions[444];
                return true;
            }
            case 0x4C: // NEG - Duplicate
            {
                instr = m_instructions[799];
                return true;
            }
            case 0x54: // NEG - Duplicate
            {
                instr = m_instructions[800];
                return true;
            }
            case 0x5C: // NEG - Duplicate
            {
                instr = m_instructions[801];
                return true;
            }
            case 0x64: // NEG - Duplicate
            {
                instr = m_instructions[802];
                return true;
            }
            case 0x6C: // NEG - Duplicate
            {
                instr = m_instructions[803];
                return true;
            }
            case 0x74: // NEG - Duplicate
            {
                instr = m_instructions[804];
                return true;
            }
            case 0x7C: // NEG - Duplicate
            {
                instr = m_instructions[805];
                return true;
            }
            case 0x45: // RETN
            {
                instr = m_instructions[568];
                return true;
            }
            case 0x55: // RETN - Duplicate
            {
                instr = m_instructions[808];
                return true;
            }
            case 0x5D: // RETN - Duplicate
            {
                instr = m_instructions[809];
                return true;
            }
            case 0x65: // RETN - Duplicate
            {
                instr = m_instructions[810];
                return true;
            }
            case 0x6D: // RETN - Duplicate
            {
                instr = m_instructions[811];
                return true;
            }
            case 0x75: // RETN - Duplicate
            {
                instr = m_instructions[812];
                return true;
            }
            case 0x7D: // RETN - Duplicate
            {
                instr = m_instructions[813];
                return true;
            }
            case 0x46: // IM0
            {
                instr = m_instructions[202];
                return true;
            }
            case 0x4E: // IM0 - Duplicate
            {
                instr = m_instructions[806];
                return true;
            }
            case 0x66: // IM0 - Duplicate
            {
                instr = m_instructions[814];
                return true;
            }
            case 0x6E: // IM0 - Duplicate
            {
                instr = m_instructions[807];
                return true;
            }
            case 0x47: // LD I,A
            {
                instr = m_instructions[392];
                return true;
            }
            case 0x48: // IN C,(C)
            {
                instr = m_instructions[232];
                return true;
            }
            case 0x49: // OUT C,(C)
            {
                instr = m_instructions[468];
                return true;
            }
            case 0x4A: // ADC HL,BC
            {
                instr = m_instructions[15];
                return true;
            }
            case 0x4B: // LD BC,(nn)
            {
                instr = m_instructions[278];
                return true;
            }
            case 0x4D: // RETI
            {
                instr = m_instructions[567];
                return true;
            }
            case 0x4F: // LD R,A
            {
                instr = m_instructions[404];
                return true;
            }
            case 0x50: // IN D,(C)
            {
                instr = m_instructions[233];
                return true;
            }
            case 0x51: // OUT D,(C)
            {
                instr = m_instructions[469];
                return true;
            }
            case 0x52: // SBC HL,DE
            {
                instr = m_instructions[647];
                return true;
            }
            case 0x53: // LD (nn),DE
            {
                instr = m_instructions[440];
                return true;
            }
            case 0x56: // IM1
            {
                instr = m_instructions[203];
                return true;
            }
            case 0x76: // IM1 - Duplicate
            {
                instr = m_instructions[815];
                return true;
            }
            case 0x57: // LD A,I
            {
                instr = m_instructions[264];
                return true;
            }
            case 0x58: // IN E,(C)
            {
                instr = m_instructions[234];
                return true;
            }
            case 0x59: // OUT E,(C)
            {
                instr = m_instructions[470];
                return true;
            }
            case 0x5A: // ADC HL,DE
            {
                instr = m_instructions[16];
                return true;
            }
            case 0x5B: // LD DE,(nn)
            {
                instr = m_instructions[310];
                return true;
            }
            case 0x5E: // IM2
            {
                instr = m_instructions[204];
                return true;
            }
            case 0x7E: // IM2 - Duplicate
            {
                instr = m_instructions[816];
                return true;
            }
            case 0x5F: // LD A,R
            {
                instr = m_instructions[270];
                return true;
            }
            case 0x60: // IN H,(C)
            {
                instr = m_instructions[235];
                return true;
            }
            case 0x61: // OUT H,(C)
            {
                instr = m_instructions[471];
                return true;
            }
            case 0x62: // SBC HL,HL
            {
                instr = m_instructions[648];
                return true;
            }
            case 0x63: // LD (nn),HL
            {
                instr = m_instructions[413];
                return true;
            }
            case 0x67: // RRD
            {
                instr = m_instructions[612];
                return true;
            }
            case 0x68: // IN L,(C)
            {
                instr = m_instructions[236];
                return true;
            }
            case 0x69: // OUT L,(C)
            {
                instr = m_instructions[472];
                return true;
            }
            case 0x6A: // ADC HL,HL
            {
                instr = m_instructions[17];
                return true;
            }
            case 0x6B: // LD HL,(nn)
            {
                instr = m_instructions[343];
                return true;
            }
            case 0x6F: // RLD
            {
                instr = m_instructions[589];
                return true;
            }
            case 0x70: // IN (C)
            {
                instr = m_instructions[817];
                return true;
            }
            case 0x71: // OUT (C),0
            {
                instr = m_instructions[818];
                return true;
            }
            case 0x72: // SBC HL,SP
            {
                instr = m_instructions[649];
                return true;
            }
            case 0x73: // LD (nn),SP
            {
                instr = m_instructions[443];
                return true;
            }
            case 0x78: // IN A,(C)
            {
                instr = m_instructions[229];
                return true;
            }
            case 0x79: // OUT A,(C)
            {
                instr = m_instructions[466];
                return true;
            }
            case 0x7A: // ADC HL,SP
            {
                instr = m_instructions[18];
                return true;
            }
            case 0x7B: // LD SP,(nn)
            {
                instr = m_instructions[408];
                return true;
            }
            case 0xA0: // LDI
            {
                instr = m_instructions[256];
                return true;
            }
            case 0xA1: // CPI
            {
                instr = m_instructions[154];
                return true;
            }
            case 0xA2: // INI
            {
                instr = m_instructions[227];
                return true;
            }
            case 0xA3: // OUTI
            {
                instr = m_instructions[465];
                return true;
            }
            case 0xA8: // LDD
            {
                instr = m_instructions[254];
                return true;
            }
            case 0xA9: // CPD
            {
                instr = m_instructions[152];
                return true;
            }
            case 0xAA: // IND
            {
                instr = m_instructions[225];
                return true;
            }
            case 0xAB: // OUTD
            {
                instr = m_instructions[464];
                return true;
            }
            case 0xB0: // LDIR
            {
                instr = m_instructions[257];
                return true;
            }
            case 0xB1: // CPIR
            {
                instr = m_instructions[155];
                return true;
            }
            case 0xB2: // INIR
            {
                instr = m_instructions[228];
                return true;
            }
            case 0xB3: // OTIR
            {
                instr = m_instructions[463];
                return true;
            }
            case 0xB8: // LDDR
            {
                instr = m_instructions[255];
                return true;
            }
            case 0xB9: // CPDR
            {
                instr = m_instructions[153];
                return true;
            }
            case 0xBA: // INDR
            {
                instr = m_instructions[226];
                return true;
            }
            case 0xBB: // OTDR
            {
                instr = m_instructions[462];
                return true;
            }
        }
        
        instr = null;
        return false;
    }
    
    /// <summary>
    /// Treat HL in next instruction as IX.
    /// </summary>
    private bool HandleDDPrefix(Memory mainMemory, ushort addr, out Instruction instr)
    {
        var nextOpcode = mainMemory.Peek((ushort)(addr + 1));
        switch (nextOpcode)
        {
            case 0x09: // ADD IX,BC
            {
                instr = m_instructions[38];
                return true;
            }
            case 0x19: // ADD IX,DE
            {
                instr = m_instructions[39];
                return true;
            }
            case 0x21: // LD IX,nn
            {
                instr = m_instructions[373];
                return true;
            }
            case 0x22: // LD (nn),IX
            {
                instr = m_instructions[441];
                return true;
            }
            case 0x23: // INC IX
            {
                instr = m_instructions[214];
                return true;
            }
            case 0x24: // INC IXH
            {
                instr = m_instructions[215];
                return true;
            }
            case 0x25: // DEC IXH
            {
                instr = m_instructions[183];
                return true;
            }
            case 0x26: // LD IXH,n
            {
                instr = m_instructions[363];
                return true;
            }
            case 0x29: // ADD IX,IX
            {
                instr = m_instructions[40];
                return true;
            }
            case 0x2A: // LD IX,(nn)
            {
                instr = m_instructions[372];
                return true;
            }
            case 0x2B: // DEC IX
            {
                instr = m_instructions[182];
                return true;
            }
            case 0x2C: // INC IXL
            {
                instr = m_instructions[216];
                return true;
            }
            case 0x2D: // DEC IXL
            {
                instr = m_instructions[184];
                return true;
            }
            case 0x2E: // LD IXL,n
            {
                instr = m_instructions[371];
                return true;
            }
            case 0x34: // INC (IX+d)
            {
                instr = m_instructions[223];
                return true;
            }
            case 0x35: // DEC (IX+d)
            {
                instr = m_instructions[191];
                return true;
            }
            case 0x36: // LD (IX+d),n
            {
                instr = m_instructions[429];
                return true;
            }
            case 0x39: // ADD IX,SP
            {
                instr = m_instructions[41];
                return true;
            }
            case 0x44: // LD B,IXH
            {
                instr = m_instructions[286];
                return true;
            }
            case 0x45: // LD B,IXL
            {
                instr = m_instructions[287];
                return true;
            }
            case 0x46: // LD B,(IX+d)
            {
                instr = m_instructions[292];
                return true;
            }
            case 0x4C: // LD C,IXH
            {
                instr = m_instructions[301];
                return true;
            }
            case 0x4D: // LD C,IXL
            {
                instr = m_instructions[302];
                return true;
            }
            case 0x4E: // LD C,(IX+d)
            {
                instr = m_instructions[307];
                return true;
            }
            case 0x54: // LD D,IXH
            {
                instr = m_instructions[318];
                return true;
            }
            case 0x55: // LD D,IXL
            {
                instr = m_instructions[319];
                return true;
            }
            case 0x56: // LD D,(IX+d)
            {
                instr = m_instructions[324];
                return true;
            }
            case 0x5C: // LD E,IXH
            {
                instr = m_instructions[333];
                return true;
            }
            case 0x5D: // LD E,IXL
            {
                instr = m_instructions[334];
                return true;
            }
            case 0x5E: // LD E,(IX+d)
            {
                instr = m_instructions[339];
                return true;
            }
            case 0x60: // LD IXH,B
            {
                instr = m_instructions[357];
                return true;
            }
            case 0x61: // LD IXH,C
            {
                instr = m_instructions[358];
                return true;
            }
            case 0x62: // LD IXH,D
            {
                instr = m_instructions[359];
                return true;
            }
            case 0x63: // LD IXH,E
            {
                instr = m_instructions[360];
                return true;
            }
            case 0x64: // LD IXH,IXH
            {
                instr = m_instructions[361];
                return true;
            }
            case 0x65: // LD IXH,IXL
            {
                instr = m_instructions[362];
                return true;
            }
            case 0x66: // LD H,(IX+d)
            {
                instr = m_instructions[353];
                return true;
            }
            case 0x67: // LD IXH,A
            {
                instr = m_instructions[356];
                return true;
            }
            case 0x68: // LD IXL,B
            {
                instr = m_instructions[365];
                return true;
            }
            case 0x69: // LD IXL,C
            {
                instr = m_instructions[366];
                return true;
            }
            case 0x6A: // LD IXL,D
            {
                instr = m_instructions[367];
                return true;
            }
            case 0x6B: // LD IXL,E
            {
                instr = m_instructions[368];
                return true;
            }
            case 0x6C: // LD IXL,IXH
            {
                instr = m_instructions[369];
                return true;
            }
            case 0x6D: // LD IXL,IXL
            {
                instr = m_instructions[370];
                return true;
            }
            case 0x6E: // LD L,(IX+d)
            {
                instr = m_instructions[401];
                return true;
            }
            case 0x6F: // LD IXL,A
            {
                instr = m_instructions[364];
                return true;
            }
            case 0x70: // LD (IX+d),B
            {
                instr = m_instructions[423];
                return true;
            }
            case 0x71: // LD (IX+d),C
            {
                instr = m_instructions[424];
                return true;
            }
            case 0x72: // LD (IX+d),D
            {
                instr = m_instructions[425];
                return true;
            }
            case 0x73: // LD (IX+d),E
            {
                instr = m_instructions[426];
                return true;
            }
            case 0x74: // LD (IX+d),H
            {
                instr = m_instructions[427];
                return true;
            }
            case 0x75: // LD (IX+d),L
            {
                instr = m_instructions[428];
                return true;
            }
            case 0x77: // LD (IX+d),A
            {
                instr = m_instructions[422];
                return true;
            }
            case 0x7C: // LD A,IXH
            {
                instr = m_instructions[265];
                return true;
            }
            case 0x7D: // LD A,IXL
            {
                instr = m_instructions[266];
                return true;
            }
            case 0x7E: // LD A,(IX+d)
            {
                instr = m_instructions[275];
                return true;
            }
            case 0x84: // ADD A,IXH
            {
                instr = m_instructions[25];
                return true;
            }
            case 0x85: // ADD A,IXL
            {
                instr = m_instructions[26];
                return true;
            }
            case 0x86: // ADD A,(IX+d)
            {
                instr = m_instructions[31];
                return true;
            }
            case 0x8C: // ADC A,IXH
            {
                instr = m_instructions[6];
                return true;
            }
            case 0x8D: // ADC A,IXL
            {
                instr = m_instructions[7];
                return true;
            }
            case 0x8E: // ADC A,(IX+d)
            {
                instr = m_instructions[12];
                return true;
            }
            case 0x94: // SUB IXH
            {
                instr = m_instructions[775];
                return true;
            }
            case 0x95: // SUB IXL
            {
                instr = m_instructions[776];
                return true;
            }
            case 0x96: // SUB (IX+d)
            {
                instr = m_instructions[781];
                return true;
            }
            case 0x9C: // SBC A,IXH
            {
                instr = m_instructions[637];
                return true;
            }
            case 0x9D: // SBC A,IXL
            {
                instr = m_instructions[638];
                return true;
            }
            case 0x9E: // SBC A,(IX+d)
            {
                instr = m_instructions[643];
                return true;
            }
            case 0xA4: // AND IXH
            {
                instr = m_instructions[52];
                return true;
            }
            case 0xA5: // AND IXL
            {
                instr = m_instructions[53];
                return true;
            }
            case 0xA6: // AND (IX+d)
            {
                instr = m_instructions[58];
                return true;
            }
            case 0xAC: // XOR IXH
            {
                instr = m_instructions[790];
                return true;
            }
            case 0xAD: // XOR IXL
            {
                instr = m_instructions[791];
                return true;
            }
            case 0xAE: // XOR (IX+d)
            {
                instr = m_instructions[796];
                return true;
            }
            case 0xB4: // OR IXH
            {
                instr = m_instructions[453];
                return true;
            }
            case 0xB5: // OR IXL
            {
                instr = m_instructions[454];
                return true;
            }
            case 0xB6: // OR (IX+d)
            {
                instr = m_instructions[459];
                return true;
            }
            case 0xBC: // CP IXH
            {
                instr = m_instructions[163];
                return true;
            }
            case 0xBD: // CP IXL
            {
                instr = m_instructions[164];
                return true;
            }
            case 0xBE: // CP (IX+d)
            {
                instr = m_instructions[169];
                return true;
            }
            case 0xCB: // DDCB prefix
            {
                instr = null;
                for (var i = 0; i < InstructionSubSetDDCB.Length; i++)
                {
                    instr = InstructionSubSetDDCB[i];
                    if (instr.StartsWithOpcodeBytes(mainMemory, addr))
                        return true;
                }
                break;
            }
            case 0xE1: // POP IX
            {
                instr = m_instructions[478];
                return true;
            }
            case 0xE3: // EX (SP),IX
            {
                instr = m_instructions[199];
                return true;
            }
            case 0xE5: // PUSH IX
            {
                instr = m_instructions[484];
                return true;
            }
            case 0xE9: // JP (IX)
            {
                instr = m_instructions[246];
                return true;
            }
            case 0xF9: // LD SP,IX
            {
                instr = m_instructions[406];
                return true;
            }
        }

        instr = null;
        return false;
    }
    
    private Instruction HandleCBPrefix(Memory mainMemory, int addr)
    {
        return mainMemory.Peek((ushort)(addr + 1)) switch
        {
            0x00 => // RLC B
                m_instructions[580],
            0x01 => // RLC C
                m_instructions[581],
            0x02 => // RLC D
                m_instructions[582],
            0x03 => // RLC E
                m_instructions[583],
            0x04 => // RLC H
                m_instructions[584],
            0x05 => // RLC L
                m_instructions[585],
            0x06 => // RLC (HL)
                m_instructions[586],
            0x07 => // RLC A
                m_instructions[579],
            0x08 => // RRC B
                m_instructions[603],
            0x09 => // RRC C
                m_instructions[604],
            0x0A => // RRC D
                m_instructions[605],
            0x0B => // RRC E
                m_instructions[606],
            0x0C => // RRC H
                m_instructions[607],
            0x0D => // RRC L
                m_instructions[608],
            0x0E => // RRC (HL)
                m_instructions[609],
            0x0F => // RRC A
                m_instructions[602],
            0x10 => // RL B
                m_instructions[591],
            0x11 => // RL C
                m_instructions[592],
            0x12 => // RL D
                m_instructions[593],
            0x13 => // RL E
                m_instructions[594],
            0x14 => // RL H
                m_instructions[595],
            0x15 => // RL L
                m_instructions[596],
            0x16 => // RL (HL)
                m_instructions[597],
            0x17 => // RL A
                m_instructions[590],
            0x18 => // RR B
                m_instructions[614],
            0x19 => // RR C
                m_instructions[615],
            0x1A => // RR D
                m_instructions[616],
            0x1B => // RR E
                m_instructions[617],
            0x1C => // RR H
                m_instructions[618],
            0x1D => // RR L
                m_instructions[619],
            0x1E => // RR (HL)
                m_instructions[620],
            0x1F => // RR A
                m_instructions[613],
            0x20 => // SLA B
                m_instructions[732],
            0x21 => // SLA C
                m_instructions[733],
            0x22 => // SLA D
                m_instructions[734],
            0x23 => // SLA E
                m_instructions[735],
            0x24 => // SLA H
                m_instructions[736],
            0x25 => // SLA L
                m_instructions[737],
            0x26 => // SLA (HL)
                m_instructions[738],
            0x27 => // SLA A
                m_instructions[731],
            0x28 => // SRA B
                m_instructions[750],
            0x29 => // SRA C
                m_instructions[751],
            0x2A => // SRA D
                m_instructions[752],
            0x2B => // SRA E
                m_instructions[753],
            0x2C => // SRA H
                m_instructions[754],
            0x2D => // SRA L
                m_instructions[755],
            0x2E => // SRA (HL)
                m_instructions[756],
            0x2F => // SRA A
                m_instructions[749],
            0x30 => // SLL B
                m_instructions[742],
            0x31 => // SLL C
                m_instructions[743],
            0x32 => // SLL D
                m_instructions[744],
            0x33 => // SLL E
                m_instructions[745],
            0x34 => // SLL H
                m_instructions[746],
            0x35 => // SLL L
                m_instructions[747],
            0x36 => // SLL (HL)
                m_instructions[748],
            0x37 => // SLL A
                m_instructions[741],
            0x38 => // SRL B
                m_instructions[760],
            0x39 => // SRL C
                m_instructions[761],
            0x3A => // SRL D
                m_instructions[762],
            0x3B => // SRL E
                m_instructions[763],
            0x3C => // SRL H
                m_instructions[764],
            0x3D => // SRL L
                m_instructions[765],
            0x3E => // SRL (HL)
                m_instructions[766],
            0x3F => // SRL A
                m_instructions[759],
            0x40 => // BIT 0,B
                m_instructions[62],
            0x41 => // BIT 0,C
                m_instructions[63],
            0x42 => // BIT 0,D
                m_instructions[64],
            0x43 => // BIT 0,E
                m_instructions[65],
            0x44 => // BIT 0,H
                m_instructions[66],
            0x45 => // BIT 0,L
                m_instructions[67],
            0x46 => // BIT 0,(HL)
                m_instructions[68],
            0x47 => // BIT 0,A
                m_instructions[61],
            0x48 => // BIT 1,B
                m_instructions[72],
            0x49 => // BIT 1,C
                m_instructions[73],
            0x4A => // BIT 1,D
                m_instructions[74],
            0x4B => // BIT 1,E
                m_instructions[75],
            0x4C => // BIT 1,H
                m_instructions[76],
            0x4D => // BIT 1,L
                m_instructions[77],
            0x4E => // BIT 1,(HL)
                m_instructions[78],
            0x4F => // BIT 1,A
                m_instructions[71],
            0x50 => // BIT 2,B
                m_instructions[82],
            0x51 => // BIT 2,C
                m_instructions[83],
            0x52 => // BIT 2,D
                m_instructions[84],
            0x53 => // BIT 2,E
                m_instructions[85],
            0x54 => // BIT 2,H
                m_instructions[86],
            0x55 => // BIT 2,L
                m_instructions[87],
            0x56 => // BIT 2,(HL)
                m_instructions[88],
            0x57 => // BIT 2,A
                m_instructions[81],
            0x58 => // BIT 3,B
                m_instructions[92],
            0x59 => // BIT 3,C
                m_instructions[93],
            0x5A => // BIT 3,D
                m_instructions[94],
            0x5B => // BIT 3,E
                m_instructions[95],
            0x5C => // BIT 3,H
                m_instructions[96],
            0x5D => // BIT 3,L
                m_instructions[97],
            0x5E => // BIT 3,(HL)
                m_instructions[98],
            0x5F => // BIT 3,A
                m_instructions[91],
            0x60 => // BIT 4,B
                m_instructions[102],
            0x61 => // BIT 4,C
                m_instructions[103],
            0x62 => // BIT 4,D
                m_instructions[104],
            0x63 => // BIT 4,E
                m_instructions[105],
            0x64 => // BIT 4,H
                m_instructions[106],
            0x65 => // BIT 4,L
                m_instructions[107],
            0x66 => // BIT 4,(HL)
                m_instructions[108],
            0x67 => // BIT 4,A
                m_instructions[101],
            0x68 => // BIT 5,B
                m_instructions[112],
            0x69 => // BIT 5,C
                m_instructions[113],
            0x6A => // BIT 5,D
                m_instructions[114],
            0x6B => // BIT 5,E
                m_instructions[115],
            0x6C => // BIT 5,H
                m_instructions[116],
            0x6D => // BIT 5,L
                m_instructions[117],
            0x6E => // BIT 5,(HL)
                m_instructions[118],
            0x6F => // BIT 5,A
                m_instructions[111],
            0x70 => // BIT 6,B
                m_instructions[122],
            0x71 => // BIT 6,C
                m_instructions[123],
            0x72 => // BIT 6,D
                m_instructions[124],
            0x73 => // BIT 6,E
                m_instructions[125],
            0x74 => // BIT 6,H
                m_instructions[126],
            0x75 => // BIT 6,L
                m_instructions[127],
            0x76 => // BIT 6,(HL)
                m_instructions[128],
            0x77 => // BIT 6,A
                m_instructions[121],
            0x78 => // BIT 7,B
                m_instructions[132],
            0x79 => // BIT 7,C
                m_instructions[133],
            0x7A => // BIT 7,D
                m_instructions[134],
            0x7B => // BIT 7,E
                m_instructions[135],
            0x7C => // BIT 7,H
                m_instructions[136],
            0x7D => // BIT 7,L
                m_instructions[137],
            0x7E => // BIT 7,(HL)
                m_instructions[138],
            0x7F => // BIT 7,A
                m_instructions[131],
            0x80 => // RES 0,B
                m_instructions[487],
            0x81 => // RES 0,C
                m_instructions[488],
            0x82 => // RES 0,D
                m_instructions[489],
            0x83 => // RES 0,E
                m_instructions[490],
            0x84 => // RES 0,H
                m_instructions[491],
            0x85 => // RES 0,L
                m_instructions[492],
            0x86 => // RES 0,(HL)
                m_instructions[493],
            0x87 => // RES 0,A
                m_instructions[486],
            0x88 => // RES 1,B
                m_instructions[497],
            0x89 => // RES 1,C
                m_instructions[498],
            0x8A => // RES 1,D
                m_instructions[499],
            0x8B => // RES 1,E
                m_instructions[500],
            0x8C => // RES 1,H
                m_instructions[501],
            0x8D => // RES 1,L
                m_instructions[502],
            0x8E => // RES 1,(HL)
                m_instructions[503],
            0x8F => // RES 1,A
                m_instructions[496],
            0x90 => // RES 2,B
                m_instructions[507],
            0x91 => // RES 2,C
                m_instructions[508],
            0x92 => // RES 2,D
                m_instructions[509],
            0x93 => // RES 2,E
                m_instructions[510],
            0x94 => // RES 2,H
                m_instructions[511],
            0x95 => // RES 2,L
                m_instructions[512],
            0x96 => // RES 2,(HL)
                m_instructions[513],
            0x97 => // RES 2,A
                m_instructions[506],
            0x98 => // RES 3,B
                m_instructions[517],
            0x99 => // RES 3,C
                m_instructions[518],
            0x9A => // RES 3,D
                m_instructions[519],
            0x9B => // RES 3,E
                m_instructions[520],
            0x9C => // RES 3,H
                m_instructions[521],
            0x9D => // RES 3,L
                m_instructions[522],
            0x9E => // RES 3,(HL)
                m_instructions[523],
            0x9F => // RES 3,A
                m_instructions[516],
            0xA0 => // RES 4,B
                m_instructions[527],
            0xA1 => // RES 4,C
                m_instructions[528],
            0xA2 => // RES 4,D
                m_instructions[529],
            0xA3 => // RES 4,E
                m_instructions[530],
            0xA4 => // RES 4,H
                m_instructions[531],
            0xA5 => // RES 4,L
                m_instructions[532],
            0xA6 => // RES 4,(HL)
                m_instructions[533],
            0xA7 => // RES 4,A
                m_instructions[526],
            0xA8 => // RES 5,B
                m_instructions[537],
            0xA9 => // RES 5,C
                m_instructions[538],
            0xAA => // RES 5,D
                m_instructions[539],
            0xAB => // RES 5,E
                m_instructions[540],
            0xAC => // RES 5,H
                m_instructions[541],
            0xAD => // RES 5,L
                m_instructions[542],
            0xAE => // RES 5,(HL)
                m_instructions[543],
            0xAF => // RES 5,A
                m_instructions[536],
            0xB0 => // RES 6,B
                m_instructions[547],
            0xB1 => // RES 6,C
                m_instructions[548],
            0xB2 => // RES 6,D
                m_instructions[549],
            0xB3 => // RES 6,E
                m_instructions[550],
            0xB4 => // RES 6,H
                m_instructions[551],
            0xB5 => // RES 6,L
                m_instructions[552],
            0xB6 => // RES 6,(HL)
                m_instructions[553],
            0xB7 => // RES 6,A
                m_instructions[546],
            0xB8 => // RES 7,B
                m_instructions[557],
            0xB9 => // RES 7,C
                m_instructions[558],
            0xBA => // RES 7,D
                m_instructions[559],
            0xBB => // RES 7,E
                m_instructions[560],
            0xBC => // RES 7,H
                m_instructions[561],
            0xBD => // RES 7,L
                m_instructions[562],
            0xBE => // RES 7,(HL)
                m_instructions[563],
            0xBF => // RES 7,A
                m_instructions[556],
            0xC0 => // SET 0,B
                m_instructions[652],
            0xC1 => // SET 0,C
                m_instructions[653],
            0xC2 => // SET 0,D
                m_instructions[654],
            0xC3 => // SET 0,E
                m_instructions[655],
            0xC4 => // SET 0,H
                m_instructions[656],
            0xC5 => // SET 0,L
                m_instructions[657],
            0xC6 => // SET 0,(HL)
                m_instructions[658],
            0xC7 => // SET 0,A
                m_instructions[651],
            0xC8 => // SET 1,B
                m_instructions[662],
            0xC9 => // SET 1,C
                m_instructions[663],
            0xCA => // SET 1,D
                m_instructions[664],
            0xCB => // SET 1,E
                m_instructions[665],
            0xCC => // SET 1,H
                m_instructions[666],
            0xCD => // SET 1,L
                m_instructions[667],
            0xCE => // SET 1,(HL)
                m_instructions[668],
            0xCF => // SET 1,A
                m_instructions[661],
            0xD0 => // SET 2,B
                m_instructions[672],
            0xD1 => // SET 2,C
                m_instructions[673],
            0xD2 => // SET 2,D
                m_instructions[674],
            0xD3 => // SET 2,E
                m_instructions[675],
            0xD4 => // SET 2,H
                m_instructions[676],
            0xD5 => // SET 2,L
                m_instructions[677],
            0xD6 => // SET 2,(HL)
                m_instructions[678],
            0xD7 => // SET 2,A
                m_instructions[671],
            0xD8 => // SET 3,B
                m_instructions[682],
            0xD9 => // SET 3,C
                m_instructions[683],
            0xDA => // SET 3,D
                m_instructions[684],
            0xDB => // SET 3,E
                m_instructions[685],
            0xDC => // SET 3,H
                m_instructions[686],
            0xDD => // SET 3,L
                m_instructions[687],
            0xDE => // SET 3,(HL)
                m_instructions[688],
            0xDF => // SET 3,A
                m_instructions[681],
            0xE0 => // SET 4,B
                m_instructions[692],
            0xE1 => // SET 4,C
                m_instructions[693],
            0xE2 => // SET 4,D
                m_instructions[694],
            0xE3 => // SET 4,E
                m_instructions[695],
            0xE4 => // SET 4,H
                m_instructions[696],
            0xE5 => // SET 4,L
                m_instructions[697],
            0xE6 => // SET 4,(HL)
                m_instructions[698],
            0xE7 => // SET 4,A
                m_instructions[691],
            0xE8 => // SET 5,B
                m_instructions[702],
            0xE9 => // SET 5,C
                m_instructions[703],
            0xEA => // SET 5,D
                m_instructions[704],
            0xEB => // SET 5,E
                m_instructions[705],
            0xEC => // SET 5,H
                m_instructions[706],
            0xED => // SET 5,L
                m_instructions[707],
            0xEE => // SET 5,(HL)
                m_instructions[708],
            0xEF => // SET 5,A
                m_instructions[701],
            0xF0 => // SET 6,B
                m_instructions[712],
            0xF1 => // SET 6,C
                m_instructions[713],
            0xF2 => // SET 6,D
                m_instructions[714],
            0xF3 => // SET 6,E
                m_instructions[715],
            0xF4 => // SET 6,H
                m_instructions[716],
            0xF5 => // SET 6,L
                m_instructions[717],
            0xF6 => // SET 6,(HL)
                m_instructions[718],
            0xF7 => // SET 6,A
                m_instructions[711],
            0xF8 => // SET 7,B
                m_instructions[722],
            0xF9 => // SET 7,C
                m_instructions[723],
            0xFA => // SET 7,D
                m_instructions[724],
            0xFB => // SET 7,E
                m_instructions[725],
            0xFC => // SET 7,H
                m_instructions[726],
            0xFD => // SET 7,L
                m_instructions[727],
            0xFE => // SET 7,(HL)
                m_instructions[728],
            0xFF => // SET 7,A
                m_instructions[721]
        };
    }
}
