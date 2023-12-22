namespace UnitTests;

public class MemoryDelta
{
    public ushort Addr { get; }
    public byte Value { get; }

    public MemoryDelta(ushort addr, byte value)
    {
        Addr = addr;
        Value = value;
    }
}