namespace Underworld.Sfx;

public interface IOplRegisterSink
{
    void WriteReg(int addr, byte val);
}
