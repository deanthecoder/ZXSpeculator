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
    public enum InstructionID
    {
        NOP,
        NOPNOP,

        // 8-bit load group.
        LD_A_n,
        LD_B_n,
        LD_C_n,
        LD_D_n,
        LD_E_n,
        LD_H_n,
        LD_L_n,
        LD_A_addr,
        LD_A_A,
        LD_A_B,
        LD_A_C,
        LD_A_D,
        LD_A_E,
        LD_A_H,
        LD_A_L,
        LD_B_A,
        LD_B_B,
        LD_B_C,
        LD_B_D,
        LD_B_E,
        LD_B_H,
        LD_B_L,
        LD_C_A,
        LD_C_B,
        LD_C_C,
        LD_C_D,
        LD_C_E,
        LD_C_H,
        LD_C_L,
        LD_D_A,
        LD_D_B,
        LD_D_C,
        LD_D_D,
        LD_D_E,
        LD_D_H,
        LD_D_L,
        LD_E_A,
        LD_E_B,
        LD_E_C,
        LD_E_D,
        LD_E_E,
        LD_E_H,
        LD_E_L,
        LD_H_A,
        LD_H_B,
        LD_H_C,
        LD_H_D,
        LD_H_E,
        LD_H_H,
        LD_H_L,
        LD_L_A,
        LD_L_B,
        LD_L_C,
        LD_L_D,
        LD_L_E,
        LD_L_H,
        LD_L_L,
        LD_A_addrHL,
        LD_B_addrHL,
        LD_C_addrHL,
        LD_D_addrHL,
        LD_E_addrHL,
        LD_H_addrHL,
        LD_L_addrHL,
        LD_A_addrIXplus_d,
        LD_A_addrIYplus_d,
        LD_B_addrIXplus_d,
        LD_B_addrIYplus_d,
        LD_C_addrIXplus_d,
        LD_C_addrIYplus_d,
        LD_D_addrIXplus_d,
        LD_D_addrIYplus_d,
        LD_E_addrIXplus_d,
        LD_E_addrIYplus_d,
        LD_H_addrIXplus_d,
        LD_H_addrIYplus_d,
        LD_L_addrIXplus_d,
        LD_L_addrIYplus_d,
        LD_addrHL_n,
        LD_addrHL_A,
        LD_addrHL_B,
        LD_addrHL_C,
        LD_addrHL_D,
        LD_addrHL_E,
        LD_addrHL_H,
        LD_addrHL_L,
        LD_addrIXplus_d_n,
        LD_addrIYplus_d_n,
        LD_addrIXplus_d_A,
        LD_addrIXplus_d_B,
        LD_addrIXplus_d_C,
        LD_addrIXplus_d_D,
        LD_addrIXplus_d_E,
        LD_addrIXplus_d_H,
        LD_addrIXplus_d_L,
        LD_addrIYplus_d_A,
        LD_addrIYplus_d_B,
        LD_addrIYplus_d_C,
        LD_addrIYplus_d_D,
        LD_addrIYplus_d_E,
        LD_addrIYplus_d_H,
        LD_addrIYplus_d_L,
        LD_A_addrBC,
        LD_A_addrDE,
        LD_addrBC_A,
        LD_addrDE_A,
        LD_addr_A,
        LD_A_I,
        LD_A_R,
        LD_I_A,
        LD_R_A,

        // 16-bit load group.
        LD_BC_nn,
        LD_DE_nn,
        LD_HL_nn,
        LD_SP_nn,
        LD_IX_nn,
        LD_IY_nn,
        LD_BC_addr,
        LD_DE_addr,
        LD_HL_addr,
        LD_SP_addr,
        LD_IX_addr,
        LD_IY_addr,
        LD_addr_BC,
        LD_addr_DE,
        LD_addrHL,
        LD_addr_SP,
        LD_addr_IX,
        LD_addr_IY,
        LD_SP_HL,
        LD_SP_IX,
        LD_SP_IY,
        PUSH_AF,
        PUSH_BC,
        PUSH_DE,
        PUSH_HL,
        PUSH_IX,
        PUSH_IY,
        POP_AF,
        POP_BC,
        POP_DE,
        POP_HL,
        POP_IX,
        POP_IY,

        // Exchange, block transfer and search group.
        EX_DE_HL,
        EX_AF_altAF,
        EXX,
        EX_addrSP_HL,
        EX_addrSP_IX,
        EX_addrSP_IY,
        LDD,
        LDDR,
        LDI,
        LDIR,
        CPD,
        CPDR,
        CPI,
        CPIR,

        // 8-bit arithmetic group.
        ADC_A_n,
        ADC_A_A,
        ADC_A_B,
        ADC_A_C,
        ADC_A_D,
        ADC_A_E,
        ADC_A_H,
        ADC_A_L,
        ADC_A_addrHL,
        ADC_A_addrIXplus_d,
        ADC_A_addrIYplus_d,
        ADD_A_n,
        ADD_A_A,
        ADD_A_B,
        ADD_A_C,
        ADD_A_D,
        ADD_A_E,
        ADD_A_H,
        ADD_A_L,
        ADD_A_addrHL,
        ADD_A_addrIXplus_d,
        ADD_A_addrIYplus_d,
        DEC_A,
        DEC_B,
        DEC_C,
        DEC_D,
        DEC_E,
        DEC_H,
        DEC_L,
        DEC_addrHL,
        DEC_addrIXplus_d,
        DEC_addrIYplus_d,
        INC_A,
        INC_B,
        INC_C,
        INC_D,
        INC_E,
        INC_H,
        INC_L,
        INC_addrHL,
        INC_addrIXplus_d,
        INC_addrIYplus_d,
        SBC_A_n,
        SBC_A_A,
        SBC_A_B,
        SBC_A_C,
        SBC_A_D,
        SBC_A_E,
        SBC_A_H,
        SBC_A_L,
        SBC_A_addrHL,
        SBC_A_addrIXplus_d,
        SBC_A_addrIYplus_d,
        SUB_n,
        SUB_A,
        SUB_B,
        SUB_C,
        SUB_D,
        SUB_E,
        SUB_H,
        SUB_L,
        SUB_addrHL,
        SUB_addrIXplus_d,
        SUB_addrIYplus_d,

        // 16-bit arithmetic group.
        ADC_HL_BC,
        ADC_HL_DE,
        ADC_HL_HL,
        ADC_HL_SP,
        SBC_HL_BC,
        SBC_HL_DE,
        SBC_HL_HL,
        SBC_HL_SP,
        ADD_HL_BC,
        ADD_HL_DE,
        ADD_HL_HL,
        ADD_HL_SP,
        ADD_IX_BC,
        ADD_IX_DE,
        ADD_IX_IX,
        ADD_IX_SP,
        ADD_IY_BC,
        ADD_IY_DE,
        ADD_IY_IY,
        ADD_IY_SP,
        DEC_BC,
        DEC_DE,
        DEC_HL,
        DEC_SP,
        DEC_IX,
        DEC_IY,
        INC_BC,
        INC_DE,
        INC_HL,
        INC_SP,
        INC_IX,
        INC_IY,

        // Logic group.
        AND_n,
        AND_A,
        AND_B,
        AND_C,
        AND_D,
        AND_E,
        AND_H,
        AND_L,
        AND_addrHL,
        AND_addrIX_plus_d,
        AND_addrIY_plus_d,
        CP_n,
        CP_A,
        CP_B,
        CP_C,
        CP_D,
        CP_E,
        CP_H,
        CP_L,
        CP_addrHL,
        CP_addrIX_plus_d,
        CP_addrIY_plus_d,
        OR_n,
        OR_A,
        OR_B,
        OR_C,
        OR_D,
        OR_E,
        OR_H,
        OR_L,
        OR_addrHL,
        OR_addrIX_plus_d,
        OR_addrIY_plus_d,
        XOR_n,
        XOR_A,
        XOR_B,
        XOR_C,
        XOR_D,
        XOR_E,
        XOR_H,
        XOR_L,
        XOR_addrHL,
        XOR_addrIX_plus_d,
        XOR_addrIY_plus_d,

        // General purpose arithmetic and CPU control group.
        CCF,
        CPL,
        DAA,
        DI,
        EI,
        HALT,
        IM0,
        IM1,
        IM2,
        NEG,
        SCF,

        // Rotate and Shift group.
        RL_A,
        RL_B,
        RL_C,
        RL_D,
        RL_E,
        RL_H,
        RL_L,
        RLA,
        RL_addrHL,
        RL_addrIX_plus_d,
        RL_addrIY_plus_d,
        RLC_A,
        RLC_B,
        RLC_C,
        RLC_D,
        RLC_E,
        RLC_H,
        RLC_L,
        RLCA,
        RLC_addrHL,
        RLC_addrIX_plus_d,
        RLC_addrIY_plus_d,
        RLD,
        RR_A,
        RR_B,
        RR_C,
        RR_D,
        RR_E,
        RR_H,
        RR_L,
        RRA,
        RR_addrHL,
        RR_addrIX_plus_d,
        RR_addrIY_plus_d,
        RRC_A,
        RRC_B,
        RRC_C,
        RRC_D,
        RRC_E,
        RRC_H,
        RRC_L,
        RRCA,
        RRC_addrHL,
        RRC_addrIX_plus_d,
        RRC_addrIY_plus_d,
        RRD,
        SLA_A,
        SLA_B,
        SLA_C,
        SLA_D,
        SLA_E,
        SLA_H,
        SLA_L,
        SLA_addrHL,
        SLA_addrIX_plus_d,
        SLA_addrIY_plus_d,
        SRA_A,
        SRA_B,
        SRA_C,
        SRA_D,
        SRA_E,
        SRA_H,
        SRA_L,
        SRA_addrHL,
        SRA_addrIX_plus_d,
        SRA_addrIY_plus_d,
        SRL_A,
        SRL_B,
        SRL_C,
        SRL_D,
        SRL_E,
        SRL_H,
        SRL_L,
        SRL_addrHL,
        SRL_addrIX_plus_d,
        SRL_addrIY_plus_d,
        SLL_A,
        SLL_B,
        SLL_C,
        SLL_D,
        SLL_E,
        SLL_H,
        SLL_L,
        SLL_addrHL,

        // Bit set, reset, and test group.
        BIT_0_A,
        BIT_1_A,
        BIT_2_A,
        BIT_3_A,
        BIT_4_A,
        BIT_5_A,
        BIT_6_A,
        BIT_7_A,
        BIT_0_B,
        BIT_1_B,
        BIT_2_B,
        BIT_3_B,
        BIT_4_B,
        BIT_5_B,
        BIT_6_B,
        BIT_7_B,
        BIT_0_C,
        BIT_1_C,
        BIT_2_C,
        BIT_3_C,
        BIT_4_C,
        BIT_5_C,
        BIT_6_C,
        BIT_7_C,
        BIT_0_D,
        BIT_1_D,
        BIT_2_D,
        BIT_3_D,
        BIT_4_D,
        BIT_5_D,
        BIT_6_D,
        BIT_7_D,
        BIT_0_E,
        BIT_1_E,
        BIT_2_E,
        BIT_3_E,
        BIT_4_E,
        BIT_5_E,
        BIT_6_E,
        BIT_7_E,
        BIT_0_H,
        BIT_1_H,
        BIT_2_H,
        BIT_3_H,
        BIT_4_H,
        BIT_5_H,
        BIT_6_H,
        BIT_7_H,
        BIT_0_L,
        BIT_1_L,
        BIT_2_L,
        BIT_3_L,
        BIT_4_L,
        BIT_5_L,
        BIT_6_L,
        BIT_7_L,
        BIT_0_addrHL,
        BIT_1_addrHL,
        BIT_2_addrHL,
        BIT_3_addrHL,
        BIT_4_addrHL,
        BIT_5_addrHL,
        BIT_6_addrHL,
        BIT_7_addrHL,
        BIT_0_addrIX_plus_d,
        BIT_1_addrIX_plus_d,
        BIT_2_addrIX_plus_d,
        BIT_3_addrIX_plus_d,
        BIT_4_addrIX_plus_d,
        BIT_5_addrIX_plus_d,
        BIT_6_addrIX_plus_d,
        BIT_7_addrIX_plus_d,
        BIT_0_addrIY_plus_d,
        BIT_1_addrIY_plus_d,
        BIT_2_addrIY_plus_d,
        BIT_3_addrIY_plus_d,
        BIT_4_addrIY_plus_d,
        BIT_5_addrIY_plus_d,
        BIT_6_addrIY_plus_d,
        BIT_7_addrIY_plus_d,
        RES_0_A,
        RES_1_A,
        RES_2_A,
        RES_3_A,
        RES_4_A,
        RES_5_A,
        RES_6_A,
        RES_7_A,
        RES_0_B,
        RES_1_B,
        RES_2_B,
        RES_3_B,
        RES_4_B,
        RES_5_B,
        RES_6_B,
        RES_7_B,
        RES_0_C,
        RES_1_C,
        RES_2_C,
        RES_3_C,
        RES_4_C,
        RES_5_C,
        RES_6_C,
        RES_7_C,
        RES_0_D,
        RES_1_D,
        RES_2_D,
        RES_3_D,
        RES_4_D,
        RES_5_D,
        RES_6_D,
        RES_7_D,
        RES_0_E,
        RES_1_E,
        RES_2_E,
        RES_3_E,
        RES_4_E,
        RES_5_E,
        RES_6_E,
        RES_7_E,
        RES_0_H,
        RES_1_H,
        RES_2_H,
        RES_3_H,
        RES_4_H,
        RES_5_H,
        RES_6_H,
        RES_7_H,
        RES_0_L,
        RES_1_L,
        RES_2_L,
        RES_3_L,
        RES_4_L,
        RES_5_L,
        RES_6_L,
        RES_7_L,
        RES_0_addrHL,
        RES_1_addrHL,
        RES_2_addrHL,
        RES_3_addrHL,
        RES_4_addrHL,
        RES_5_addrHL,
        RES_6_addrHL,
        RES_7_addrHL,
        RES_0_addrIX_plus_d,
        RES_1_addrIX_plus_d,
        RES_2_addrIX_plus_d,
        RES_3_addrIX_plus_d,
        RES_4_addrIX_plus_d,
        RES_5_addrIX_plus_d,
        RES_6_addrIX_plus_d,
        RES_7_addrIX_plus_d,
        RES_0_addrIY_plus_d,
        RES_1_addrIY_plus_d,
        RES_2_addrIY_plus_d,
        RES_3_addrIY_plus_d,
        RES_4_addrIY_plus_d,
        RES_5_addrIY_plus_d,
        RES_6_addrIY_plus_d,
        RES_7_addrIY_plus_d,
        SET_0_A,
        SET_1_A,
        SET_2_A,
        SET_3_A,
        SET_4_A,
        SET_5_A,
        SET_6_A,
        SET_7_A,
        SET_0_B,
        SET_1_B,
        SET_2_B,
        SET_3_B,
        SET_4_B,
        SET_5_B,
        SET_6_B,
        SET_7_B,
        SET_0_C,
        SET_1_C,
        SET_2_C,
        SET_3_C,
        SET_4_C,
        SET_5_C,
        SET_6_C,
        SET_7_C,
        SET_0_D,
        SET_1_D,
        SET_2_D,
        SET_3_D,
        SET_4_D,
        SET_5_D,
        SET_6_D,
        SET_7_D,
        SET_0_E,
        SET_1_E,
        SET_2_E,
        SET_3_E,
        SET_4_E,
        SET_5_E,
        SET_6_E,
        SET_7_E,
        SET_0_H,
        SET_1_H,
        SET_2_H,
        SET_3_H,
        SET_4_H,
        SET_5_H,
        SET_6_H,
        SET_7_H,
        SET_0_L,
        SET_1_L,
        SET_2_L,
        SET_3_L,
        SET_4_L,
        SET_5_L,
        SET_6_L,
        SET_7_L,
        SET_0_addrHL,
        SET_1_addrHL,
        SET_2_addrHL,
        SET_3_addrHL,
        SET_4_addrHL,
        SET_5_addrHL,
        SET_6_addrHL,
        SET_7_addrHL,
        SET_0_addrIX_plus_d,
        SET_1_addrIX_plus_d,
        SET_2_addrIX_plus_d,
        SET_3_addrIX_plus_d,
        SET_4_addrIX_plus_d,
        SET_5_addrIX_plus_d,
        SET_6_addrIX_plus_d,
        SET_7_addrIX_plus_d,
        SET_0_addrIY_plus_d,
        SET_1_addrIY_plus_d,
        SET_2_addrIY_plus_d,
        SET_3_addrIY_plus_d,
        SET_4_addrIY_plus_d,
        SET_5_addrIY_plus_d,
        SET_6_addrIY_plus_d,
        SET_7_addrIY_plus_d,

        // Jump, call, and return group.
        CALL_nn,
        CALL_NZ_nn,
        CALL_Z_nn,
        CALL_NC_nn,
        CALL_C_nn,
        CALL_PO_nn,
        CALL_PE_nn,
        CALL_P_nn,
        CALL_M_nn,
        CALL_DJNZ_n,
        JP_nn,
        JP_NZ_nn,
        JP_Z_nn,
        JP_NC_nn,
        JP_C_nn,
        JP_PO_nn,
        JP_PE_nn,
        JP_P_nn,
        JP_M_nn,
        JP_addrHL,
        JP_addr_IX,
        JP_addr_IY,
        JR_n,
        JR_C_n,
        JR_NC_n,
        JR_NZ_n,
        JR_Z_n,
        RET,
        RET_NZ,
        RET_Z,
        RET_NC,
        RET_C,
        RET_PO,
        RET_PE,
        RET_P,
        RET_M,
        RETI,
        RETN,
        RST_00,
        RST_08,
        RST_10,
        RST_18,
        RST_20,
        RST_28,
        RST_30,
        RST_38,

        // Input and output group table.
        IN_A_addr_n,
        IN_A_addr_C,
        IN_B_addr_C,
        IN_C_addr_C,
        IN_D_addr_C,
        IN_E_addr_C,
        IN_H_addr_C,
        IN_L_addr_C,
        IND,
        INI,
        INIR,
        INDR,
        OTDR,
        OTIR,
        OUT_addr_n_A,
        OUT_A_addr_C,
        OUT_B_addr_C,
        OUT_C_addr_C,
        OUT_D_addr_C,
        OUT_E_addr_C,
        OUT_H_addr_C,
        OUT_L_addr_C,
        OUTD,
        OUTI,

        // Undocumented IX(hl), IY(hl) group.
        ADC_A_IXH,
        ADC_A_IXL,
        ADC_A_IYH,
        ADC_A_IYL,
        ADD_A_IXH,
        ADD_A_IXL,
        ADD_A_IYH,
        ADD_A_IYL,
        AND_IXH,
        AND_IXL,
        AND_IYH,
        AND_IYL,
        CP_IXH,
        CP_IXL,
        CP_IYH,
        CP_IYL,
        DEC_IXH,
        DEC_IXL,
        DEC_IYH,
        DEC_IYL,
        INC_IXH,
        INC_IXL,
        INC_IYH,
        INC_IYL,
        LD_A_IXH,
        LD_A_IXL,
        LD_A_IYH,
        LD_A_IYL,
        LD_B_IXH,
        LD_B_IXL,
        LD_B_IYH,
        LD_B_IYL,
        LD_C_IXH,
        LD_C_IXL,
        LD_C_IYH,
        LD_C_IYL,
        LD_D_IXH,
        LD_D_IXL,
        LD_D_IYH,
        LD_D_IYL,
        LD_E_IXH,
        LD_E_IXL,
        LD_E_IYH,
        LD_E_IYL,
        LD_IXH_A,
        LD_IXH_B,
        LD_IXH_C,
        LD_IXH_D,
        LD_IXH_E,
        LD_IXH_IXH,
        LD_IXH_IXL,
        LD_IXH_n,
        LD_IXL_A,
        LD_IXL_B,
        LD_IXL_C,
        LD_IXL_D,
        LD_IXL_E,
        LD_IXL_IXH,
        LD_IXL_IXL,
        LD_IXL_n,
        LD_IYH_A,
        LD_IYH_B,
        LD_IYH_C,
        LD_IYH_D,
        LD_IYH_E,
        LD_IYH_IYH,
        LD_IYH_IYL,
        LD_IYH_n,
        LD_IYL_A,
        LD_IYL_B,
        LD_IYL_C,
        LD_IYL_D,
        LD_IYL_E,
        LD_IYL_IYH,
        LD_IYL_IYL,
        LD_IYL_n,
        OR_IXH,
        OR_IXL,
        OR_IYH,
        OR_IYL,
        SBC_A_IXH,
        SBC_A_IXL,
        SBC_A_IYH,
        SBC_A_IYL,
        SUB_IXH,
        SUB_IXL,
        SUB_IYH,
        SUB_IYL,
        XOR_IXH,
        XOR_IXL,
        XOR_IYH,
        XOR_IYL,
    }
}
