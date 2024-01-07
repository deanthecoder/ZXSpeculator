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
    private Instruction[] m_instructionSubSetDDCB;
    private Instruction[] InstructionSubSetDDCB =>
        m_instructionSubSetDDCB ??= Instructions.Where(o => o.HexTemplate.Replace(" ", string.Empty).StartsWith("DDCB")).ToArray();
    private Instruction[] m_instructionSubSetFDCB;
    private Instruction m_nop;
    private Instruction m_nopnop;

    private Instruction[] InstructionSubSetFDCB =>
        m_instructionSubSetFDCB ??= Instructions.Where(o => o.HexTemplate.Replace(" ", string.Empty).StartsWith("FDCB")).ToArray();

    public Instruction Nop => m_nop ??= Instructions.First(t => t.Id == InstructionID.NOP);
    public Instruction NopNop => m_nopnop ??= Instructions.First(t => t.Id == InstructionID.NOPNOP);
    
    public Z80Instructions()
    {
        InitList();
    }

    private List<Instruction> Instructions { get; set; }

    public Instruction FindInstructionAtMemoryLocation(Memory mainMemory, ushort addr)
    {
        switch (mainMemory.Peek(addr))
        {
            case 0x00: // NOP
                return Instructions[445];
            case 0x01: // LD BC,nn
                return Instructions[279];
            case 0x02: // LD (BC),A
                return Instructions[410];
            case 0x03: // INC BC
                return Instructions[207];
            case 0x04: // INC B
                return Instructions[206];
            case 0x05: // DEC B
                return Instructions[174];
            case 0x06: // LD B,n
                return Instructions[294];
            case 0x07: // RLCA
                return Instructions[578];
            case 0x08: // EX AF,AF'
                return Instructions[196];
            case 0x09: // ADD HL,BC
                return Instructions[34];
            case 0x0A: // LD A,(BC)
                return Instructions[272];
            case 0x0B: // DEC BC
                return Instructions[175];
            case 0x0C: // INC C
                return Instructions[208];
            case 0x0D: // DEC C
                return Instructions[176];
            case 0x0E: // LD C,n
                return Instructions[309];
            case 0x0F: // RRCA
                return Instructions[601];
            case 0x10: // DJNZ n
                return Instructions[142];
            case 0x11: // LD DE,nn
                return Instructions[311];
            case 0x12: // LD (DE),A
                return Instructions[411];
            case 0x13: // INC DE
                return Instructions[210];
            case 0x14: // INC D
                return Instructions[209];
            case 0x15: // DEC D
                return Instructions[177];
            case 0x16: // LD D,n
                return Instructions[326];
            case 0x17: // RLA
                return Instructions[577];
            case 0x18: // JR d
                return Instructions[253];
            case 0x19: // ADD HL,DE
                return Instructions[35];
            case 0x1A: // LD A,(DE)
                return Instructions[273];
            case 0x1B: // DEC DE
                return Instructions[178];
            case 0x1C: // INC E
                return Instructions[211];
            case 0x1D: // DEC E
                return Instructions[179];
            case 0x1E: // LD E,n
                return Instructions[341];
            case 0x1F: // RRA
                return Instructions[600];
            case 0x20: // JR NZ,d
                return Instructions[251];
            case 0x21: // LD HL,nn
                return Instructions[344];
            case 0x22: // LD (nn),HL
                return Instructions[412];
            case 0x23: // INC HL
                return Instructions[213];
            case 0x24: // INC H
                return Instructions[212];
            case 0x25: // DEC H
                return Instructions[180];
            case 0x26: // LD H,n
                return Instructions[355];
            case 0x27: // DAA
                return Instructions[172];
            case 0x28: // JR Z,d
                return Instructions[252];
            case 0x29: // ADD HL,HL
                return Instructions[36];
            case 0x2A: // LD HL,(nn)
                return Instructions[342];
            case 0x2B: // DEC HL
                return Instructions[181];
            case 0x2C: // INC L
                return Instructions[220];
            case 0x2D: // DEC L
                return Instructions[188];
            case 0x2E: // LD L,n
                return Instructions[403];
            case 0x2F: // CPL
                return Instructions[156];
            case 0x30: // JR NC,d
                return Instructions[250];
            case 0x31: // LD SP,nn
                return Instructions[409];
            case 0x32: // LD (nn),A
                return Instructions[438];
            case 0x33: // INC SP
                return Instructions[221];
            case 0x34: // INC (HL)
                return Instructions[222];
            case 0x35: // DEC (HL)
                return Instructions[190];
            case 0x36: // LD (HL),n
                return Instructions[421];
            case 0x37: // SCF
                return Instructions[650];
            case 0x38: // JR C,d
                return Instructions[249];
            case 0x39: // ADD HL,SP
                return Instructions[37];
            case 0x3A: // LD A,(nn)
                return Instructions[271];
            case 0x3B: // DEC SP
                return Instructions[189];
            case 0x3C: // INC A
                return Instructions[205];
            case 0x3D: // DEC A
                return Instructions[173];
            case 0x3E: // LD A,n
                return Instructions[277];
            case 0x3F: // CCF
                return Instructions[151];
            case 0x40: // LD B,B
                return Instructions[281];
            case 0x41: // LD B,C
                return Instructions[282];
            case 0x42: // LD B,D
                return Instructions[283];
            case 0x43: // LD B,E
                return Instructions[284];
            case 0x44: // LD B,H
                return Instructions[285];
            case 0x45: // LD B,L
                return Instructions[290];
            case 0x46: // LD B,(HL)
                return Instructions[291];
            case 0x47: // LD B,A
                return Instructions[280];
            case 0x48: // LD C,B
                return Instructions[296];
            case 0x49: // LD C,C
                return Instructions[297];
            case 0x4A: // LD C,D
                return Instructions[298];
            case 0x4B: // LD C,E
                return Instructions[299];
            case 0x4C: // LD C,H
                return Instructions[300];
            case 0x4D: // LD C,L
                return Instructions[305];
            case 0x4E: // LD C,(HL)
                return Instructions[306];
            case 0x4F: // LD C,A
                return Instructions[295];
            case 0x50: // LD D,B
                return Instructions[313];
            case 0x51: // LD D,C
                return Instructions[314];
            case 0x52: // LD D,D
                return Instructions[315];
            case 0x53: // LD D,E
                return Instructions[316];
            case 0x54: // LD D,H
                return Instructions[317];
            case 0x55: // LD D,L
                return Instructions[322];
            case 0x56: // LD D,(HL)
                return Instructions[323];
            case 0x57: // LD D,A
                return Instructions[312];
            case 0x58: // LD E,B
                return Instructions[328];
            case 0x59: // LD E,C
                return Instructions[329];
            case 0x5A: // LD E,D
                return Instructions[330];
            case 0x5B: // LD E,E
                return Instructions[331];
            case 0x5C: // LD E,H
                return Instructions[332];
            case 0x5D: // LD E,L
                return Instructions[337];
            case 0x5E: // LD E,(HL)
                return Instructions[338];
            case 0x5F: // LD E,A
                return Instructions[327];
            case 0x60: // LD H,B
                return Instructions[346];
            case 0x61: // LD H,C
                return Instructions[347];
            case 0x62: // LD H,D
                return Instructions[348];
            case 0x63: // LD H,E
                return Instructions[349];
            case 0x64: // LD H,H
                return Instructions[350];
            case 0x65: // LD H,L
                return Instructions[351];
            case 0x66: // LD H,(HL)
                return Instructions[352];
            case 0x67: // LD H,A
                return Instructions[345];
            case 0x68: // LD L,B
                return Instructions[394];
            case 0x69: // LD L,C
                return Instructions[395];
            case 0x6A: // LD L,D
                return Instructions[396];
            case 0x6B: // LD L,E
                return Instructions[397];
            case 0x6C: // LD L,H
                return Instructions[398];
            case 0x6D: // LD L,L
                return Instructions[399];
            case 0x6E: // LD L,(HL)
                return Instructions[400];
            case 0x6F: // LD L,A
                return Instructions[393];
            case 0x70: // LD (HL),B
                return Instructions[415];
            case 0x71: // LD (HL),C
                return Instructions[416];
            case 0x72: // LD (HL),D
                return Instructions[417];
            case 0x73: // LD (HL),E
                return Instructions[418];
            case 0x74: // LD (HL),H
                return Instructions[419];
            case 0x75: // LD (HL),L
                return Instructions[420];
            case 0x76: // HALT
                return Instructions[201];
            case 0x77: // LD (HL),A
                return Instructions[414];
            case 0x78: // LD A,B
                return Instructions[259];
            case 0x79: // LD A,C
                return Instructions[260];
            case 0x7A: // LD A,D
                return Instructions[261];
            case 0x7B: // LD A,E
                return Instructions[262];
            case 0x7C: // LD A,H
                return Instructions[263];
            case 0x7D: // LD A,L
                return Instructions[269];
            case 0x7E: // LD A,(HL)
                return Instructions[274];
            case 0x7F: // LD A,A
                return Instructions[258];
            case 0x80: // ADD A,B
                return Instructions[20];
            case 0x81: // ADD A,C
                return Instructions[21];
            case 0x82: // ADD A,D
                return Instructions[22];
            case 0x83: // ADD A,E
                return Instructions[23];
            case 0x84: // ADD A,H
                return Instructions[24];
            case 0x85: // ADD A,L
                return Instructions[29];
            case 0x86: // ADD A,(HL)
                return Instructions[30];
            case 0x87: // ADD A,A
                return Instructions[19];
            case 0x88: // ADC A,B
                return Instructions[1];
            case 0x89: // ADC A,C
                return Instructions[2];
            case 0x8A: // ADC A,D
                return Instructions[3];
            case 0x8B: // ADC A,E
                return Instructions[4];
            case 0x8C: // ADC A,H
                return Instructions[5];
            case 0x8D: // ADC A,L
                return Instructions[10];
            case 0x8E: // ADC A,(HL)
                return Instructions[11];
            case 0x8F: // ADC A,A
                return Instructions[0];
            case 0x90: // SUB B
                return Instructions[770];
            case 0x91: // SUB C
                return Instructions[771];
            case 0x92: // SUB D
                return Instructions[772];
            case 0x93: // SUB E
                return Instructions[773];
            case 0x94: // SUB H
                return Instructions[774];
            case 0x95: // SUB L
                return Instructions[779];
            case 0x96: // SUB (HL)
                return Instructions[780];
            case 0x97: // SUB A
                return Instructions[769];
            case 0x98: // SBC A,B
                return Instructions[632];
            case 0x99: // SBC A,C
                return Instructions[633];
            case 0x9A: // SBC A,D
                return Instructions[634];
            case 0x9B: // SBC A,E
                return Instructions[635];
            case 0x9C: // SBC A,H
                return Instructions[636];
            case 0x9D: // SBC A,L
                return Instructions[641];
            case 0x9E: // SBC A,(HL)
                return Instructions[642];
            case 0x9F: // SBC A,A
                return Instructions[631];
            case 0xA0: // AND B
                return Instructions[47];
            case 0xA1: // AND C
                return Instructions[48];
            case 0xA2: // AND D
                return Instructions[49];
            case 0xA3: // AND E
                return Instructions[50];
            case 0xA4: // AND H
                return Instructions[51];
            case 0xA5: // AND L
                return Instructions[56];
            case 0xA6: // AND (HL)
                return Instructions[57];
            case 0xA7: // AND A
                return Instructions[46];
            case 0xA8: // XOR B
                return Instructions[785];
            case 0xA9: // XOR C
                return Instructions[786];
            case 0xAA: // XOR D
                return Instructions[787];
            case 0xAB: // XOR E
                return Instructions[788];
            case 0xAC: // XOR H
                return Instructions[789];
            case 0xAD: // XOR L
                return Instructions[794];
            case 0xAE: // XOR (HL)
                return Instructions[795];
            case 0xAF: // XOR A
                return Instructions[784];
            case 0xB0: // OR B
                return Instructions[448];
            case 0xB1: // OR C
                return Instructions[449];
            case 0xB2: // OR D
                return Instructions[450];
            case 0xB3: // OR E
                return Instructions[451];
            case 0xB4: // OR H
                return Instructions[452];
            case 0xB5: // OR L
                return Instructions[457];
            case 0xB6: // OR (HL)
                return Instructions[458];
            case 0xB7: // OR A
                return Instructions[447];
            case 0xB8: // CP B
                return Instructions[158];
            case 0xB9: // CP C
                return Instructions[159];
            case 0xBA: // CP D
                return Instructions[160];
            case 0xBB: // CP E
                return Instructions[161];
            case 0xBC: // CP H
                return Instructions[162];
            case 0xBD: // CP L
                return Instructions[167];
            case 0xBE: // CP (HL)
                return Instructions[168];
            case 0xBF: // CP A
                return Instructions[157];
            case 0xC0: // RET NZ
                return Instructions[572];
            case 0xC1: // POP BC
                return Instructions[475];
            case 0xC2: // JP NZ,nn
                return Instructions[240];
            case 0xC3: // JP nn
                return Instructions[248];
            case 0xC4: // CALL NZ,nn
                return Instructions[145];
            case 0xC5: // PUSH BC
                return Instructions[481];
            case 0xC6: // ADD A,n
                return Instructions[33];
            case 0xC7: // RST 00
                return Instructions[623];
            case 0xC8: // RET Z
                return Instructions[576];
            case 0xC9: // RET
                return Instructions[566];
            case 0xCA: // JP Z,nn
                return Instructions[244];
            case 0xCB: // CB prefix
                return HandleCBPrefix(mainMemory, addr);
            case 0xCC: // CALL Z,nn
                return Instructions[149];
            case 0xCD: // CALL nn
                return Instructions[150];
            case 0xCE: // ADC A,n
                return Instructions[14];
            case 0xCF: // RST 08
                return Instructions[624];
            case 0xD0: // RET NC
                return Instructions[571];
            case 0xD1: // POP DE
                return Instructions[476];
            case 0xD2: // JP NC,nn
                return Instructions[239];
            case 0xD3: // OUT (n),A
                return Instructions[473];
            case 0xD4: // CALL NC,nn
                return Instructions[144];
            case 0xD5: // PUSH DE
                return Instructions[482];
            case 0xD6: // SUB n
                return Instructions[783];
            case 0xD7: // RST 10
                return Instructions[625];
            case 0xD8: // RET C
                return Instructions[569];
            case 0xD9: // EXX
                return Instructions[195];
            case 0xDA: // JP C,nn
                return Instructions[237];
            case 0xDB: // IN A,(n)
                return Instructions[230];
            case 0xDC: // CALL C,nn
                return Instructions[141];
            case 0xDD: // DD prefix
                if (HandleDDPrefix(mainMemory, addr, out var instr2))
                    return instr2;
                break;
            case 0xDE: // SBC A,n
                return Instructions[645];
            case 0xDF: // RST 18
                return Instructions[626];
            case 0xE0: // RET PO
                return Instructions[574];
            case 0xE1: // POP HL
                return Instructions[477];
            case 0xE2: // JP PO,nn
                return Instructions[242];
            case 0xE3: // EX (SP),HL
                return Instructions[198];
            case 0xE4: // CALL PO,nn
                return Instructions[147];
            case 0xE5: // PUSH HL
                return Instructions[483];
            case 0xE6: // AND n
                return Instructions[60];
            case 0xE7: // RST 20
                return Instructions[627];
            case 0xE8: // RET PE
                return Instructions[573];
            case 0xE9: // JP (HL)
                return Instructions[245];
            case 0xEA: // JP PE,nn
                return Instructions[241];
            case 0xEB: // EX DE,HL
                return Instructions[197];
            case 0xEC: // CALL PE,nn
                return Instructions[146];
            case 0xED: // ED prefix
                if (HandleEDPrefix(mainMemory, addr, out var instr1))
                    return instr1;
                break;
            case 0xEE: // XOR n
                return Instructions[798];
            case 0xEF: // RST 28
                return Instructions[628];
            case 0xF0: // RET P
                return Instructions[575];
            case 0xF1: // POP AF
                return Instructions[474];
            case 0xF2: // JP P,nn
                return Instructions[243];
            case 0xF3: // DI
                return Instructions[193];
            case 0xF4: // CALL P,nn
                return Instructions[148];
            case 0xF5: // PUSH AF
                return Instructions[480];
            case 0xF6: // OR n
                return Instructions[461];
            case 0xF7: // RST 30
                return Instructions[629];
            case 0xF8: // RET M
                return Instructions[570];
            case 0xF9: // LD SP,HL
                return Instructions[405];
            case 0xFA: // JP M,nn
                return Instructions[238];
            case 0xFB: // EI
                return Instructions[194];
            case 0xFC: // CALL M,nn
                return Instructions[143];
            case 0xFD: // FD prefix
                if (HandleFDPrefix(mainMemory, addr, out var instr))
                    return instr;
                break;
            case 0xFE: // CP n
                return Instructions[171];
            case 0xFF: // RST 38
                return Instructions[630];
        }
        
        return null;

    }
    
    private bool HandleFDPrefix(Memory mainMemory, ushort addr, out Instruction instr)
    {
        switch (mainMemory.Peek((ushort)(addr + 1)))
        {
            case 0x09: // ADD IY,BC
            {
                instr = Instructions[42];
                return true;
            }
            case 0x19: // ADD IY,DE
            {
                instr = Instructions[43];
                return true;
            }
            case 0x21: // LD IY,nn
            {
                instr = Instructions[391];
                return true;
            }
            case 0x22: // LD (nn),IY
            {
                instr = Instructions[442];
                return true;
            }
            case 0x23: // INC IY
            {
                instr = Instructions[217];
                return true;
            }
            case 0x24: // INC IYH
            {
                instr = Instructions[218];
                return true;
            }
            case 0x25: // DEC IYH
            {
                instr = Instructions[186];
                return true;
            }
            case 0x26: // LD IYH,n
            {
                instr = Instructions[381];
                return true;
            }
            case 0x29: // ADD IY,IY
            {
                instr = Instructions[44];
                return true;
            }
            case 0x2A: // LD IY,(nn)
            {
                instr = Instructions[390];
                return true;
            }
            case 0x2B: // DEC IY
            {
                instr = Instructions[185];
                return true;
            }
            case 0x2C: // INC IYL
            {
                instr = Instructions[219];
                return true;
            }
            case 0x2D: // DEC IYL
            {
                instr = Instructions[187];
                return true;
            }
            case 0x2E: // LD IYL,n
            {
                instr = Instructions[389];
                return true;
            }
            case 0x34: // INC (IY+d)
            {
                instr = Instructions[224];
                return true;
            }
            case 0x35: // DEC (IY+d)
            {
                instr = Instructions[192];
                return true;
            }
            case 0x36: // LD (IY+d),n
            {
                instr = Instructions[437];
                return true;
            }
            case 0x39: // ADD IY,SP
            {
                instr = Instructions[45];
                return true;
            }
            case 0x44: // LD B,IYH
            {
                instr = Instructions[288];
                return true;
            }
            case 0x45: // LD B,IYL
            {
                instr = Instructions[289];
                return true;
            }
            case 0x46: // LD B,(IY+d)
            {
                instr = Instructions[293];
                return true;
            }
            case 0x4C: // LD C,IYH
            {
                instr = Instructions[303];
                return true;
            }
            case 0x4D: // LD C,IYL
            {
                instr = Instructions[304];
                return true;
            }
            case 0x4E: // LD C,(IY+d)
            {
                instr = Instructions[308];
                return true;
            }
            case 0x54: // LD D,IYH
            {
                instr = Instructions[320];
                return true;
            }
            case 0x55: // LD D,IYL
            {
                instr = Instructions[321];
                return true;
            }
            case 0x56: // LD D,(IY+d)
            {
                instr = Instructions[325];
                return true;
            }
            case 0x5C: // LD E,IYH
            {
                instr = Instructions[335];
                return true;
            }
            case 0x5D: // LD E,IYL
            {
                instr = Instructions[336];
                return true;
            }
            case 0x5E: // LD E,(IY+d)
            {
                instr = Instructions[340];
                return true;
            }
            case 0x60: // LD IYH,B
            {
                instr = Instructions[375];
                return true;
            }
            case 0x61: // LD IYH,C
            {
                instr = Instructions[376];
                return true;
            }
            case 0x62: // LD IYH,D
            {
                instr = Instructions[377];
                return true;
            }
            case 0x63: // LD IYH,E
            {
                instr = Instructions[378];
                return true;
            }
            case 0x64: // LD IYH,IYH
            {
                instr = Instructions[379];
                return true;
            }
            case 0x65: // LD IYH,IYL
            {
                instr = Instructions[380];
                return true;
            }
            case 0x66: // LD H,(IY+d)
            {
                instr = Instructions[354];
                return true;
            }
            case 0x67: // LD IYH,A
            {
                instr = Instructions[374];
                return true;
            }
            case 0x68: // LD IYL,B
            {
                instr = Instructions[383];
                return true;
            }
            case 0x69: // LD IYL,C
            {
                instr = Instructions[384];
                return true;
            }
            case 0x6A: // LD IYL,D
            {
                instr = Instructions[385];
                return true;
            }
            case 0x6B: // LD IYL,E
            {
                instr = Instructions[386];
                return true;
            }
            case 0x6C: // LD IYL,IYH
            {
                instr = Instructions[387];
                return true;
            }
            case 0x6D: // LD IYL,IYL
            {
                instr = Instructions[388];
                return true;
            }
            case 0x6E: // LD L,(IY+d)
            {
                instr = Instructions[402];
                return true;
            }
            case 0x6F: // LD IYL,A
            {
                instr = Instructions[382];
                return true;
            }
            case 0x70: // LD (IY+d),B
            {
                instr = Instructions[431];
                return true;
            }
            case 0x71: // LD (IY+d),C
            {
                instr = Instructions[432];
                return true;
            }
            case 0x72: // LD (IY+d),D
            {
                instr = Instructions[433];
                return true;
            }
            case 0x73: // LD (IY+d),E
            {
                instr = Instructions[434];
                return true;
            }
            case 0x74: // LD (IY+d),H
            {
                instr = Instructions[435];
                return true;
            }
            case 0x75: // LD (IY+d),L
            {
                instr = Instructions[436];
                return true;
            }
            case 0x77: // LD (IY+d),A
            {
                instr = Instructions[430];
                return true;
            }
            case 0x7C: // LD A,IYH
            {
                instr = Instructions[267];
                return true;
            }
            case 0x7D: // LD A,IYL
            {
                instr = Instructions[268];
                return true;
            }
            case 0x7E: // LD A,(IY+d)
            {
                instr = Instructions[276];
                return true;
            }
            case 0x84: // AFD A,IYH
            {
                instr = Instructions[27];
                return true;
            }
            case 0x85: // AFD A,IYL
            {
                instr = Instructions[28];
                return true;
            }
            case 0x86: // ADD A,(IY+d)
            {
                instr = Instructions[32];
                return true;
            }
            case 0x8C: // ADC A,IYH
            {
                instr = Instructions[8];
                return true;
            }
            case 0x8D: // ADC A,IYL
            {
                instr = Instructions[9];
                return true;
            }
            case 0x8E: // ADC A,(IY+d)
            {
                instr = Instructions[13];
                return true;
            }
            case 0x94: // SUB IYH
            {
                instr = Instructions[777];
                return true;
            }
            case 0x95: // SUB IYL
            {
                instr = Instructions[778];
                return true;
            }
            case 0x96: // SUB (IY+d)
            {
                instr = Instructions[782];
                return true;
            }
            case 0x9C: // SBC A,IYH
            {
                instr = Instructions[639];
                return true;
            }
            case 0x9D: // SBC A,IYL
            {
                instr = Instructions[640];
                return true;
            }
            case 0x9E: // SBC A,(IY+d)
            {
                instr = Instructions[644];
                return true;
            }
            case 0xA4: // AND IYH
            {
                instr = Instructions[54];
                return true;
            }
            case 0xA5: // AND IYL
            {
                instr = Instructions[55];
                return true;
            }
            case 0xA6: // AND (IY+d)
            {
                instr = Instructions[59];
                return true;
            }
            case 0xAC: // XOR IYH
            {
                instr = Instructions[792];
                return true;
            }
            case 0xAD: // XOR IYL
            {
                instr = Instructions[793];
                return true;
            }
            case 0xAE: // XOR (IY+d)
            {
                instr = Instructions[797];
                return true;
            }
            case 0xB4: // OR IYH
            {
                instr = Instructions[455];
                return true;
            }
            case 0xB5: // OR IYL
            {
                instr = Instructions[456];
                return true;
            }
            case 0xB6: // OR (IY+d)
            {
                instr = Instructions[460];
                return true;
            }
            case 0xBC: // CP IYH
            {
                instr = Instructions[165];
                return true;
            }
            case 0xBD: // CP IYL
            {
                instr = Instructions[166];
                return true;
            }
            case 0xBE: // CP (IY+d)
            {
                instr = Instructions[170];
                return true;
            }
            case 0xCB: // FDCB prefix
            {
                instr = null;
                for (var i = 0; i < InstructionSubSetFDCB.Length; i++)
                {
                    var o = InstructionSubSetFDCB[i];
                    if (!o.StartsWithOpcodeBytes(mainMemory, addr))
                        continue;
                    instr = o;
                    break;
                }
                return true;
            }
            case 0xE1: // POP IY
            {
                instr = Instructions[479];
                return true;
            }
            case 0xE3: // EX (SP),IY
            {
                instr = Instructions[200];
                return true;
            }
            case 0xE5: // PUSH IY
            {
                instr = Instructions[485];
                return true;
            }
            case 0xE9: // JP (IY)
            {
                instr = Instructions[247];
                return true;
            }
            case 0xF9: // LD SP,IY
            {
                instr = Instructions[407];
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
                instr = Instructions[231];
                return true;
            }
            case 0x41: // OUT B,(C)
            {
                instr = Instructions[467];
                return true;
            }
            case 0x42: // SBC HL,BC
            {
                instr = Instructions[646];
                return true;
            }
            case 0x43: // LD (nn),BC
            {
                instr = Instructions[439];
                return true;
            }
            case 0x44: // NEG
            {
                instr = Instructions[444];
                return true;
            }
            case 0x4C: // NEG - Duplicate
            {
                instr = Instructions[799];
                return true;
            }
            case 0x54: // NEG - Duplicate
            {
                instr = Instructions[800];
                return true;
            }
            case 0x5C: // NEG - Duplicate
            {
                instr = Instructions[801];
                return true;
            }
            case 0x64: // NEG - Duplicate
            {
                instr = Instructions[802];
                return true;
            }
            case 0x6C: // NEG - Duplicate
            {
                instr = Instructions[803];
                return true;
            }
            case 0x74: // NEG - Duplicate
            {
                instr = Instructions[804];
                return true;
            }
            case 0x7C: // NEG - Duplicate
            {
                instr = Instructions[805];
                return true;
            }
            case 0x45: // RETN
            {
                instr = Instructions[568];
                return true;
            }
            case 0x55: // RETN - Duplicate
            {
                instr = Instructions[808];
                return true;
            }
            case 0x5D: // RETN - Duplicate
            {
                instr = Instructions[809];
                return true;
            }
            case 0x65: // RETN - Duplicate
            {
                instr = Instructions[810];
                return true;
            }
            case 0x6D: // RETN - Duplicate
            {
                instr = Instructions[811];
                return true;
            }
            case 0x75: // RETN - Duplicate
            {
                instr = Instructions[812];
                return true;
            }
            case 0x7D: // RETN - Duplicate
            {
                instr = Instructions[813];
                return true;
            }
            case 0x46: // IM0
            {
                instr = Instructions[202];
                return true;
            }
            case 0x4E: // IM0 - Duplicate
            {
                instr = Instructions[806];
                return true;
            }
            case 0x66: // IM0 - Duplicate
            {
                instr = Instructions[814];
                return true;
            }
            case 0x6E: // IM0 - Duplicate
            {
                instr = Instructions[807];
                return true;
            }
            case 0x47: // LD I,A
            {
                instr = Instructions[392];
                return true;
            }
            case 0x48: // IN C,(C)
            {
                instr = Instructions[232];
                return true;
            }
            case 0x49: // OUT C,(C)
            {
                instr = Instructions[468];
                return true;
            }
            case 0x4A: // ADC HL,BC
            {
                instr = Instructions[15];
                return true;
            }
            case 0x4B: // LD BC,(nn)
            {
                instr = Instructions[278];
                return true;
            }
            case 0x4D: // RETI
            {
                instr = Instructions[567];
                return true;
            }
            case 0x4F: // LD R,A
            {
                instr = Instructions[404];
                return true;
            }
            case 0x50: // IN D,(C)
            {
                instr = Instructions[233];
                return true;
            }
            case 0x51: // OUT D,(C)
            {
                instr = Instructions[469];
                return true;
            }
            case 0x52: // SBC HL,DE
            {
                instr = Instructions[647];
                return true;
            }
            case 0x53: // LD (nn),DE
            {
                instr = Instructions[440];
                return true;
            }
            case 0x56: // IM1
            {
                instr = Instructions[203];
                return true;
            }
            case 0x76: // IM1 - Duplicate
            {
                instr = Instructions[815];
                return true;
            }
            case 0x57: // LD A,I
            {
                instr = Instructions[264];
                return true;
            }
            case 0x58: // IN E,(C)
            {
                instr = Instructions[234];
                return true;
            }
            case 0x59: // OUT E,(C)
            {
                instr = Instructions[470];
                return true;
            }
            case 0x5A: // ADC HL,DE
            {
                instr = Instructions[16];
                return true;
            }
            case 0x5B: // LD DE,(nn)
            {
                instr = Instructions[310];
                return true;
            }
            case 0x5E: // IM2
            {
                instr = Instructions[204];
                return true;
            }
            case 0x5F: // LD A,R
            {
                instr = Instructions[270];
                return true;
            }
            case 0x60: // IN H,(C)
            {
                instr = Instructions[235];
                return true;
            }
            case 0x61: // OUT H,(C)
            {
                instr = Instructions[471];
                return true;
            }
            case 0x62: // SBC HL,HL
            {
                instr = Instructions[648];
                return true;
            }
            case 0x63: // LD (nn),HL
            {
                instr = Instructions[413];
                return true;
            }
            case 0x67: // RRD
            {
                instr = Instructions[612];
                return true;
            }
            case 0x68: // IN L,(C)
            {
                instr = Instructions[236];
                return true;
            }
            case 0x69: // OUT L,(C)
            {
                instr = Instructions[472];
                return true;
            }
            case 0x6A: // ADC HL,HL
            {
                instr = Instructions[17];
                return true;
            }
            case 0x6B: // LD HL,(nn)
            {
                instr = Instructions[343];
                return true;
            }
            case 0x6F: // RLD
            {
                instr = Instructions[589];
                return true;
            }
            case 0x72: // SBC HL,SP
            {
                instr = Instructions[649];
                return true;
            }
            case 0x73: // LD (nn),SP
            {
                instr = Instructions[443];
                return true;
            }
            case 0x78: // IN A,(C)
            {
                instr = Instructions[229];
                return true;
            }
            case 0x79: // OUT A,(C)
            {
                instr = Instructions[466];
                return true;
            }
            case 0x7A: // ADC HL,SP
            {
                instr = Instructions[18];
                return true;
            }
            case 0x7B: // LD SP,(nn)
            {
                instr = Instructions[408];
                return true;
            }
            case 0xA0: // LDI
            {
                instr = Instructions[256];
                return true;
            }
            case 0xA1: // CPI
            {
                instr = Instructions[154];
                return true;
            }
            case 0xA2: // INI
            {
                instr = Instructions[227];
                return true;
            }
            case 0xA3: // OUTI
            {
                instr = Instructions[465];
                return true;
            }
            case 0xA8: // LDD
            {
                instr = Instructions[254];
                return true;
            }
            case 0xA9: // CPD
            {
                instr = Instructions[152];
                return true;
            }
            case 0xAA: // IND
            {
                instr = Instructions[225];
                return true;
            }
            case 0xAB: // OUTD
            {
                instr = Instructions[464];
                return true;
            }
            case 0xB0: // LDIR
            {
                instr = Instructions[257];
                return true;
            }
            case 0xB1: // CPIR
            {
                instr = Instructions[155];
                return true;
            }
            case 0xB2: // INIR
            {
                instr = Instructions[228];
                return true;
            }
            case 0xB3: // OTIR
            {
                instr = Instructions[463];
                return true;
            }
            case 0xB8: // LDDR
            {
                instr = Instructions[255];
                return true;
            }
            case 0xB9: // CPDR
            {
                instr = Instructions[153];
                return true;
            }
            case 0xBA: // INDR
            {
                instr = Instructions[226];
                return true;
            }
            case 0xBB: // OTDR
            {
                instr = Instructions[462];
                return true;
            }
        }
        
        instr = null;
        return false;
    }
    
    private bool HandleDDPrefix(Memory mainMemory, ushort addr, out Instruction instr)
    {
        switch (mainMemory.Peek((ushort)(addr + 1)))
        {
            case 0x09: // ADD IX,BC
            {
                instr = Instructions[38];
                return true;
            }
            case 0x19: // ADD IX,DE
            {
                instr = Instructions[39];
                return true;
            }
            case 0x21: // LD IX,nn
            {
                instr = Instructions[373];
                return true;
            }
            case 0x22: // LD (nn),IX
            {
                instr = Instructions[441];
                return true;
            }
            case 0x23: // INC IX
            {
                instr = Instructions[214];
                return true;
            }
            case 0x24: // INC IXH
            {
                instr = Instructions[215];
                return true;
            }
            case 0x25: // DEC IXH
            {
                instr = Instructions[183];
                return true;
            }
            case 0x26: // LD IXH,n
            {
                instr = Instructions[363];
                return true;
            }
            case 0x29: // ADD IX,IX
            {
                instr = Instructions[40];
                return true;
            }
            case 0x2A: // LD IX,(nn)
            {
                instr = Instructions[372];
                return true;
            }
            case 0x2B: // DEC IX
            {
                instr = Instructions[182];
                return true;
            }
            case 0x2C: // INC IXL
            {
                instr = Instructions[216];
                return true;
            }
            case 0x2D: // DEC IXL
            {
                instr = Instructions[184];
                return true;
            }
            case 0x2E: // LD IXL,n
            {
                instr = Instructions[371];
                return true;
            }
            case 0x34: // INC (IX+d)
            {
                instr = Instructions[223];
                return true;
            }
            case 0x35: // DEC (IX+d)
            {
                instr = Instructions[191];
                return true;
            }
            case 0x36: // LD (IX+d),n
            {
                instr = Instructions[429];
                return true;
            }
            case 0x39: // ADD IX,SP
            {
                instr = Instructions[41];
                return true;
            }
            case 0x44: // LD B,IXH
            {
                instr = Instructions[286];
                return true;
            }
            case 0x45: // LD B,IXL
            {
                instr = Instructions[287];
                return true;
            }
            case 0x46: // LD B,(IX+d)
            {
                instr = Instructions[292];
                return true;
            }
            case 0x4C: // LD C,IXH
            {
                instr = Instructions[301];
                return true;
            }
            case 0x4D: // LD C,IXL
            {
                instr = Instructions[302];
                return true;
            }
            case 0x4E: // LD C,(IX+d)
            {
                instr = Instructions[307];
                return true;
            }
            case 0x54: // LD D,IXH
            {
                instr = Instructions[318];
                return true;
            }
            case 0x55: // LD D,IXL
            {
                instr = Instructions[319];
                return true;
            }
            case 0x56: // LD D,(IX+d)
            {
                instr = Instructions[324];
                return true;
            }
            case 0x5C: // LD E,IXH
            {
                instr = Instructions[333];
                return true;
            }
            case 0x5D: // LD E,IXL
            {
                instr = Instructions[334];
                return true;
            }
            case 0x5E: // LD E,(IX+d)
            {
                instr = Instructions[339];
                return true;
            }
            case 0x60: // LD IXH,B
            {
                instr = Instructions[357];
                return true;
            }
            case 0x61: // LD IXH,C
            {
                instr = Instructions[358];
                return true;
            }
            case 0x62: // LD IXH,D
            {
                instr = Instructions[359];
                return true;
            }
            case 0x63: // LD IXH,E
            {
                instr = Instructions[360];
                return true;
            }
            case 0x64: // LD IXH,IXH
            {
                instr = Instructions[361];
                return true;
            }
            case 0x65: // LD IXH,IXL
            {
                instr = Instructions[362];
                return true;
            }
            case 0x66: // LD H,(IX+d)
            {
                instr = Instructions[353];
                return true;
            }
            case 0x67: // LD IXH,A
            {
                instr = Instructions[356];
                return true;
            }
            case 0x68: // LD IXL,B
            {
                instr = Instructions[365];
                return true;
            }
            case 0x69: // LD IXL,C
            {
                instr = Instructions[366];
                return true;
            }
            case 0x6A: // LD IXL,D
            {
                instr = Instructions[367];
                return true;
            }
            case 0x6B: // LD IXL,E
            {
                instr = Instructions[368];
                return true;
            }
            case 0x6C: // LD IXL,IXH
            {
                instr = Instructions[369];
                return true;
            }
            case 0x6D: // LD IXL,IXL
            {
                instr = Instructions[370];
                return true;
            }
            case 0x6E: // LD L,(IX+d)
            {
                instr = Instructions[401];
                return true;
            }
            case 0x6F: // LD IXL,A
            {
                instr = Instructions[364];
                return true;
            }
            case 0x70: // LD (IX+d),B
            {
                instr = Instructions[423];
                return true;
            }
            case 0x71: // LD (IX+d),C
            {
                instr = Instructions[424];
                return true;
            }
            case 0x72: // LD (IX+d),D
            {
                instr = Instructions[425];
                return true;
            }
            case 0x73: // LD (IX+d),E
            {
                instr = Instructions[426];
                return true;
            }
            case 0x74: // LD (IX+d),H
            {
                instr = Instructions[427];
                return true;
            }
            case 0x75: // LD (IX+d),L
            {
                instr = Instructions[428];
                return true;
            }
            case 0x77: // LD (IX+d),A
            {
                instr = Instructions[422];
                return true;
            }
            case 0x7C: // LD A,IXH
            {
                instr = Instructions[265];
                return true;
            }
            case 0x7D: // LD A,IXL
            {
                instr = Instructions[266];
                return true;
            }
            case 0x7E: // LD A,(IX+d)
            {
                instr = Instructions[275];
                return true;
            }
            case 0x84: // ADD A,IXH
            {
                instr = Instructions[25];
                return true;
            }
            case 0x85: // ADD A,IXL
            {
                instr = Instructions[26];
                return true;
            }
            case 0x86: // ADD A,(IX+d)
            {
                instr = Instructions[31];
                return true;
            }
            case 0x8C: // ADC A,IXH
            {
                instr = Instructions[6];
                return true;
            }
            case 0x8D: // ADC A,IXL
            {
                instr = Instructions[7];
                return true;
            }
            case 0x8E: // ADC A,(IX+d)
            {
                instr = Instructions[12];
                return true;
            }
            case 0x94: // SUB IXH
            {
                instr = Instructions[775];
                return true;
            }
            case 0x95: // SUB IXL
            {
                instr = Instructions[776];
                return true;
            }
            case 0x96: // SUB (IX+d)
            {
                instr = Instructions[781];
                return true;
            }
            case 0x9C: // SBC A,IXH
            {
                instr = Instructions[637];
                return true;
            }
            case 0x9D: // SBC A,IXL
            {
                instr = Instructions[638];
                return true;
            }
            case 0x9E: // SBC A,(IX+d)
            {
                instr = Instructions[643];
                return true;
            }
            case 0xA4: // AND IXH
            {
                instr = Instructions[52];
                return true;
            }
            case 0xA5: // AND IXL
            {
                instr = Instructions[53];
                return true;
            }
            case 0xA6: // AND (IX+d)
            {
                instr = Instructions[58];
                return true;
            }
            case 0xAC: // XOR IXH
            {
                instr = Instructions[790];
                return true;
            }
            case 0xAD: // XOR IXL
            {
                instr = Instructions[791];
                return true;
            }
            case 0xAE: // XOR (IX+d)
            {
                instr = Instructions[796];
                return true;
            }
            case 0xB4: // OR IXH
            {
                instr = Instructions[453];
                return true;
            }
            case 0xB5: // OR IXL
            {
                instr = Instructions[454];
                return true;
            }
            case 0xB6: // OR (IX+d)
            {
                instr = Instructions[459];
                return true;
            }
            case 0xBC: // CP IXH
            {
                instr = Instructions[163];
                return true;
            }
            case 0xBD: // CP IXL
            {
                instr = Instructions[164];
                return true;
            }
            case 0xBE: // CP (IX+d)
            {
                instr = Instructions[169];
                return true;
            }
            case 0xCB: // DDCB prefix
            {
                instr = null;
                for (var i = 0; i < InstructionSubSetDDCB.Length; i++)
                {
                    var o = InstructionSubSetDDCB[i];
                    if (!o.StartsWithOpcodeBytes(mainMemory, addr))
                        continue;
                    instr = o;
                    break;
                }
                return true;
            }
            case 0xE1: // POP IX
            {
                instr = Instructions[478];
                return true;
            }
            case 0xE3: // EX (SP),IX
            {
                instr = Instructions[199];
                return true;
            }
            case 0xE5: // PUSH IX
            {
                instr = Instructions[484];
                return true;
            }
            case 0xE9: // JP (IX)
            {
                instr = Instructions[246];
                return true;
            }
            case 0xF9: // LD SP,IX
            {
                instr = Instructions[406];
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
                Instructions[580],
            0x01 => // RLC C
                Instructions[581],
            0x02 => // RLC D
                Instructions[582],
            0x03 => // RLC E
                Instructions[583],
            0x04 => // RLC H
                Instructions[584],
            0x05 => // RLC L
                Instructions[585],
            0x06 => // RLC (HL)
                Instructions[586],
            0x07 => // RLC A
                Instructions[579],
            0x08 => // RRC B
                Instructions[603],
            0x09 => // RRC C
                Instructions[604],
            0x0A => // RRC D
                Instructions[605],
            0x0B => // RRC E
                Instructions[606],
            0x0C => // RRC H
                Instructions[607],
            0x0D => // RRC L
                Instructions[608],
            0x0E => // RRC (HL)
                Instructions[609],
            0x0F => // RRC A
                Instructions[602],
            0x10 => // RL B
                Instructions[591],
            0x11 => // RL C
                Instructions[592],
            0x12 => // RL D
                Instructions[593],
            0x13 => // RL E
                Instructions[594],
            0x14 => // RL H
                Instructions[595],
            0x15 => // RL L
                Instructions[596],
            0x16 => // RL (HL)
                Instructions[597],
            0x17 => // RL A
                Instructions[590],
            0x18 => // RR B
                Instructions[614],
            0x19 => // RR C
                Instructions[615],
            0x1A => // RR D
                Instructions[616],
            0x1B => // RR E
                Instructions[617],
            0x1C => // RR H
                Instructions[618],
            0x1D => // RR L
                Instructions[619],
            0x1E => // RR (HL)
                Instructions[620],
            0x1F => // RR A
                Instructions[613],
            0x20 => // SLA B
                Instructions[732],
            0x21 => // SLA C
                Instructions[733],
            0x22 => // SLA D
                Instructions[734],
            0x23 => // SLA E
                Instructions[735],
            0x24 => // SLA H
                Instructions[736],
            0x25 => // SLA L
                Instructions[737],
            0x26 => // SLA (HL)
                Instructions[738],
            0x27 => // SLA A
                Instructions[731],
            0x28 => // SRA B
                Instructions[750],
            0x29 => // SRA C
                Instructions[751],
            0x2A => // SRA D
                Instructions[752],
            0x2B => // SRA E
                Instructions[753],
            0x2C => // SRA H
                Instructions[754],
            0x2D => // SRA L
                Instructions[755],
            0x2E => // SRA (HL)
                Instructions[756],
            0x2F => // SRA A
                Instructions[749],
            0x30 => // SLL B
                Instructions[742],
            0x31 => // SLL C
                Instructions[743],
            0x32 => // SLL D
                Instructions[744],
            0x33 => // SLL E
                Instructions[745],
            0x34 => // SLL H
                Instructions[746],
            0x35 => // SLL L
                Instructions[747],
            0x36 => // SLL (HL)
                Instructions[748],
            0x37 => // SLL A
                Instructions[741],
            0x38 => // SRL B
                Instructions[760],
            0x39 => // SRL C
                Instructions[761],
            0x3A => // SRL D
                Instructions[762],
            0x3B => // SRL E
                Instructions[763],
            0x3C => // SRL H
                Instructions[764],
            0x3D => // SRL L
                Instructions[765],
            0x3E => // SRL (HL)
                Instructions[766],
            0x3F => // SRL A
                Instructions[759],
            0x40 => // BIT 0,B
                Instructions[62],
            0x41 => // BIT 0,C
                Instructions[63],
            0x42 => // BIT 0,D
                Instructions[64],
            0x43 => // BIT 0,E
                Instructions[65],
            0x44 => // BIT 0,H
                Instructions[66],
            0x45 => // BIT 0,L
                Instructions[67],
            0x46 => // BIT 0,(HL)
                Instructions[68],
            0x47 => // BIT 0,A
                Instructions[61],
            0x48 => // BIT 1,B
                Instructions[72],
            0x49 => // BIT 1,C
                Instructions[73],
            0x4A => // BIT 1,D
                Instructions[74],
            0x4B => // BIT 1,E
                Instructions[75],
            0x4C => // BIT 1,H
                Instructions[76],
            0x4D => // BIT 1,L
                Instructions[77],
            0x4E => // BIT 1,(HL)
                Instructions[78],
            0x4F => // BIT 1,A
                Instructions[71],
            0x50 => // BIT 2,B
                Instructions[82],
            0x51 => // BIT 2,C
                Instructions[83],
            0x52 => // BIT 2,D
                Instructions[84],
            0x53 => // BIT 2,E
                Instructions[85],
            0x54 => // BIT 2,H
                Instructions[86],
            0x55 => // BIT 2,L
                Instructions[87],
            0x56 => // BIT 2,(HL)
                Instructions[88],
            0x57 => // BIT 2,A
                Instructions[81],
            0x58 => // BIT 3,B
                Instructions[92],
            0x59 => // BIT 3,C
                Instructions[93],
            0x5A => // BIT 3,D
                Instructions[94],
            0x5B => // BIT 3,E
                Instructions[95],
            0x5C => // BIT 3,H
                Instructions[96],
            0x5D => // BIT 3,L
                Instructions[97],
            0x5E => // BIT 3,(HL)
                Instructions[98],
            0x5F => // BIT 3,A
                Instructions[91],
            0x60 => // BIT 4,B
                Instructions[102],
            0x61 => // BIT 4,C
                Instructions[103],
            0x62 => // BIT 4,D
                Instructions[104],
            0x63 => // BIT 4,E
                Instructions[105],
            0x64 => // BIT 4,H
                Instructions[106],
            0x65 => // BIT 4,L
                Instructions[107],
            0x66 => // BIT 4,(HL)
                Instructions[108],
            0x67 => // BIT 4,A
                Instructions[101],
            0x68 => // BIT 5,B
                Instructions[112],
            0x69 => // BIT 5,C
                Instructions[113],
            0x6A => // BIT 5,D
                Instructions[114],
            0x6B => // BIT 5,E
                Instructions[115],
            0x6C => // BIT 5,H
                Instructions[116],
            0x6D => // BIT 5,L
                Instructions[117],
            0x6E => // BIT 5,(HL)
                Instructions[118],
            0x6F => // BIT 5,A
                Instructions[111],
            0x70 => // BIT 6,B
                Instructions[122],
            0x71 => // BIT 6,C
                Instructions[123],
            0x72 => // BIT 6,D
                Instructions[124],
            0x73 => // BIT 6,E
                Instructions[125],
            0x74 => // BIT 6,H
                Instructions[126],
            0x75 => // BIT 6,L
                Instructions[127],
            0x76 => // BIT 6,(HL)
                Instructions[128],
            0x77 => // BIT 6,A
                Instructions[121],
            0x78 => // BIT 7,B
                Instructions[132],
            0x79 => // BIT 7,C
                Instructions[133],
            0x7A => // BIT 7,D
                Instructions[134],
            0x7B => // BIT 7,E
                Instructions[135],
            0x7C => // BIT 7,H
                Instructions[136],
            0x7D => // BIT 7,L
                Instructions[137],
            0x7E => // BIT 7,(HL)
                Instructions[138],
            0x7F => // BIT 7,A
                Instructions[131],
            0x80 => // RES 0,B
                Instructions[487],
            0x81 => // RES 0,C
                Instructions[488],
            0x82 => // RES 0,D
                Instructions[489],
            0x83 => // RES 0,E
                Instructions[490],
            0x84 => // RES 0,H
                Instructions[491],
            0x85 => // RES 0,L
                Instructions[492],
            0x86 => // RES 0,(HL)
                Instructions[493],
            0x87 => // RES 0,A
                Instructions[486],
            0x88 => // RES 1,B
                Instructions[497],
            0x89 => // RES 1,C
                Instructions[498],
            0x8A => // RES 1,D
                Instructions[499],
            0x8B => // RES 1,E
                Instructions[500],
            0x8C => // RES 1,H
                Instructions[501],
            0x8D => // RES 1,L
                Instructions[502],
            0x8E => // RES 1,(HL)
                Instructions[503],
            0x8F => // RES 1,A
                Instructions[496],
            0x90 => // RES 2,B
                Instructions[507],
            0x91 => // RES 2,C
                Instructions[508],
            0x92 => // RES 2,D
                Instructions[509],
            0x93 => // RES 2,E
                Instructions[510],
            0x94 => // RES 2,H
                Instructions[511],
            0x95 => // RES 2,L
                Instructions[512],
            0x96 => // RES 2,(HL)
                Instructions[513],
            0x97 => // RES 2,A
                Instructions[506],
            0x98 => // RES 3,B
                Instructions[517],
            0x99 => // RES 3,C
                Instructions[518],
            0x9A => // RES 3,D
                Instructions[519],
            0x9B => // RES 3,E
                Instructions[520],
            0x9C => // RES 3,H
                Instructions[521],
            0x9D => // RES 3,L
                Instructions[522],
            0x9E => // RES 3,(HL)
                Instructions[523],
            0x9F => // RES 3,A
                Instructions[516],
            0xA0 => // RES 4,B
                Instructions[527],
            0xA1 => // RES 4,C
                Instructions[528],
            0xA2 => // RES 4,D
                Instructions[529],
            0xA3 => // RES 4,E
                Instructions[530],
            0xA4 => // RES 4,H
                Instructions[531],
            0xA5 => // RES 4,L
                Instructions[532],
            0xA6 => // RES 4,(HL)
                Instructions[533],
            0xA7 => // RES 4,A
                Instructions[526],
            0xA8 => // RES 5,B
                Instructions[537],
            0xA9 => // RES 5,C
                Instructions[538],
            0xAA => // RES 5,D
                Instructions[539],
            0xAB => // RES 5,E
                Instructions[540],
            0xAC => // RES 5,H
                Instructions[541],
            0xAD => // RES 5,L
                Instructions[542],
            0xAE => // RES 5,(HL)
                Instructions[543],
            0xAF => // RES 5,A
                Instructions[536],
            0xB0 => // RES 6,B
                Instructions[547],
            0xB1 => // RES 6,C
                Instructions[548],
            0xB2 => // RES 6,D
                Instructions[549],
            0xB3 => // RES 6,E
                Instructions[550],
            0xB4 => // RES 6,H
                Instructions[551],
            0xB5 => // RES 6,L
                Instructions[552],
            0xB6 => // RES 6,(HL)
                Instructions[553],
            0xB7 => // RES 6,A
                Instructions[546],
            0xB8 => // RES 7,B
                Instructions[557],
            0xB9 => // RES 7,C
                Instructions[558],
            0xBA => // RES 7,D
                Instructions[559],
            0xBB => // RES 7,E
                Instructions[560],
            0xBC => // RES 7,H
                Instructions[561],
            0xBD => // RES 7,L
                Instructions[562],
            0xBE => // RES 7,(HL)
                Instructions[563],
            0xBF => // RES 7,A
                Instructions[556],
            0xC0 => // SET 0,B
                Instructions[652],
            0xC1 => // SET 0,C
                Instructions[653],
            0xC2 => // SET 0,D
                Instructions[654],
            0xC3 => // SET 0,E
                Instructions[655],
            0xC4 => // SET 0,H
                Instructions[656],
            0xC5 => // SET 0,L
                Instructions[657],
            0xC6 => // SET 0,(HL)
                Instructions[658],
            0xC7 => // SET 0,A
                Instructions[651],
            0xC8 => // SET 1,B
                Instructions[662],
            0xC9 => // SET 1,C
                Instructions[663],
            0xCA => // SET 1,D
                Instructions[664],
            0xCB => // SET 1,E
                Instructions[665],
            0xCC => // SET 1,H
                Instructions[666],
            0xCD => // SET 1,L
                Instructions[667],
            0xCE => // SET 1,(HL)
                Instructions[668],
            0xCF => // SET 1,A
                Instructions[661],
            0xD0 => // SET 2,B
                Instructions[672],
            0xD1 => // SET 2,C
                Instructions[673],
            0xD2 => // SET 2,D
                Instructions[674],
            0xD3 => // SET 2,E
                Instructions[675],
            0xD4 => // SET 2,H
                Instructions[676],
            0xD5 => // SET 2,L
                Instructions[677],
            0xD6 => // SET 2,(HL)
                Instructions[678],
            0xD7 => // SET 2,A
                Instructions[671],
            0xD8 => // SET 3,B
                Instructions[682],
            0xD9 => // SET 3,C
                Instructions[683],
            0xDA => // SET 3,D
                Instructions[684],
            0xDB => // SET 3,E
                Instructions[685],
            0xDC => // SET 3,H
                Instructions[686],
            0xDD => // SET 3,L
                Instructions[687],
            0xDE => // SET 3,(HL)
                Instructions[688],
            0xDF => // SET 3,A
                Instructions[681],
            0xE0 => // SET 4,B
                Instructions[692],
            0xE1 => // SET 4,C
                Instructions[693],
            0xE2 => // SET 4,D
                Instructions[694],
            0xE3 => // SET 4,E
                Instructions[695],
            0xE4 => // SET 4,H
                Instructions[696],
            0xE5 => // SET 4,L
                Instructions[697],
            0xE6 => // SET 4,(HL)
                Instructions[698],
            0xE7 => // SET 4,A
                Instructions[691],
            0xE8 => // SET 5,B
                Instructions[702],
            0xE9 => // SET 5,C
                Instructions[703],
            0xEA => // SET 5,D
                Instructions[704],
            0xEB => // SET 5,E
                Instructions[705],
            0xEC => // SET 5,H
                Instructions[706],
            0xED => // SET 5,L
                Instructions[707],
            0xEE => // SET 5,(HL)
                Instructions[708],
            0xEF => // SET 5,A
                Instructions[701],
            0xF0 => // SET 6,B
                Instructions[712],
            0xF1 => // SET 6,C
                Instructions[713],
            0xF2 => // SET 6,D
                Instructions[714],
            0xF3 => // SET 6,E
                Instructions[715],
            0xF4 => // SET 6,H
                Instructions[716],
            0xF5 => // SET 6,L
                Instructions[717],
            0xF6 => // SET 6,(HL)
                Instructions[718],
            0xF7 => // SET 6,A
                Instructions[711],
            0xF8 => // SET 7,B
                Instructions[722],
            0xF9 => // SET 7,C
                Instructions[723],
            0xFA => // SET 7,D
                Instructions[724],
            0xFB => // SET 7,E
                Instructions[725],
            0xFC => // SET 7,H
                Instructions[726],
            0xFD => // SET 7,L
                Instructions[727],
            0xFE => // SET 7,(HL)
                Instructions[728],
            0xFF => // SET 7,A
                Instructions[721]
        };
    }
}
