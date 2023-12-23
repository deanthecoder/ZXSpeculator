namespace Speculator.Core;

public interface IPortHandler
{
    void StartKeyboardHook();
    
    byte In(ushort portAddress);
    void Out(byte port, byte b);
}