namespace Underworld
{
    public partial class ConversationVM : UWClass
    {
        //The op codes for the vm.
        public const byte cnv_NOP = 0;
        public const byte cnv_OPADD = 1;
        public const byte cnv_OPMUL = 2;
        public const byte cnv_OPSUB = 3;
        public const byte cnv_OPDIV = 4;
        public const byte cnv_OPMOD = 5;
        public const byte cnv_OPOR = 6;
        public const byte cnv_OPAND = 7;
        public const byte cnv_OPNOT = 8;
        public const byte cnv_TSTGT = 9;
        public const byte cnv_TSTGE = 10;
        public const byte cnv_TSTLT = 11;
        public const byte cnv_TSTLE = 12;
        public const byte cnv_TSTEQ = 13;
        public const byte cnv_TSTNE = 14;
        public const byte cnv_JMP = 15;
        public const byte cnv_BEQ = 16;
        public const byte cnv_BNE = 17;
        public const byte cnv_BRA = 18;
        public const byte cnv_CALL = 19;
        public const byte cnv_CALLI = 20;
        public const byte cnv_RET = 21;
        public const byte cnv_PUSHI = 22;
        public const byte cnv_PUSHI_EFF = 23;
        public const byte cnv_POP = 24;
        public const byte cnv_SWAP = 25;
        public const byte cnv_PUSHBP = 26;
        public const byte cnv_POPBP = 27;
        public const byte cnv_SPTOBP = 28;
        public const byte cnv_BPTOSP = 29;
        public const byte cnv_ADDSP = 30;
        public const byte cnv_FETCHM = 31;
        public const byte cnv_STO = 32;
        public const byte cnv_OFFSET = 33;
        public const byte cnv_START = 34;
        public const byte cnv_SAVE_REG = 35;
        public const byte cnv_PUSH_REG = 36;
        public const byte cnv_STRCMP = 37;
        public const byte cnv_EXIT_OP = 38;
        public const byte cnv_SAY_OP = 39;
        public const byte cnv_RESPOND_OP = 40;
        public const byte cnv_OPNEG = 41;


        public const short import_function = 0x111;
        public const short import_variable = 0x10f;

        const short return_void = 0;
        const short return_int = 0x129;
        const short return_string = 0x12b;
    }
}