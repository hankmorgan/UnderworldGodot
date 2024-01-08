namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        //The op codes for the vm.
        const byte cnv_NOP = 0;
        const byte cnv_OPADD = 1;
        const byte cnv_OPMUL = 2;
        const byte cnv_OPSUB = 3;
        const byte cnv_OPDIV = 4;
        const byte cnv_OPMOD = 5;
        const byte cnv_OPOR = 6;
        const byte cnv_OPAND = 7;
        const byte cnv_OPNOT = 8;
        const byte cnv_TSTGT = 9;
        const byte cnv_TSTGE = 10;
        const byte cnv_TSTLT = 11;
        const byte cnv_TSTLE = 12;
        const byte cnv_TSTEQ = 13;
        const byte cnv_TSTNE = 14;
        const byte cnv_JMP = 15;
        const byte cnv_BEQ = 16;
        const byte cnv_BNE = 17;
        const byte cnv_BRA = 18;
        const byte cnv_CALL = 19;
        const byte cnv_CALLI = 20;
        const byte cnv_RET = 21;
        const byte cnv_PUSHI = 22;
        const byte cnv_PUSHI_EFF = 23;
        const byte cnv_POP = 24;
        const byte cnv_SWAP = 25;
        const byte cnv_PUSHBP = 26;
        const byte cnv_POPBP = 27;
        const byte cnv_SPTOBP = 28;
        const byte cnv_BPTOSP = 29;
        const byte cnv_ADDSP = 30;
        const byte cnv_FETCHM = 31;
        const byte cnv_STO = 32;
        const byte cnv_OFFSET = 33;
        const byte cnv_START = 34;
        const byte cnv_SAVE_REG = 35;
        const byte cnv_PUSH_REG = 36;
        const byte cnv_STRCMP = 37;
        const byte cnv_EXIT_OP = 38;
        const byte cnv_SAY_OP = 39;
        const byte cnv_RESPOND_OP = 40;
        const byte cnv_OPNEG = 41;


        const short import_function = 0x111;
        const short import_variable = 0x10f;

        const short return_void = 0;
        const short return_int = 0x129;
        const short return_string = 0x12b;
    }
}