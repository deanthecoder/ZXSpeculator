namespace Speculator.Core;

public interface IPortHandler
{
    byte In(ushort portAddress);
    void Out(byte port, byte b);
}