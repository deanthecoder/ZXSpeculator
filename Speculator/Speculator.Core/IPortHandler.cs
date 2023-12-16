namespace Speculator.Core;

public interface IPortHandler
{
    byte In(int portAddress);
    void Out(byte port, byte b);
}