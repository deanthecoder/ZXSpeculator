using Newtonsoft.Json;
using Speculator.Core;

namespace UnitTests;

/// <summary>
/// Saves/Restores a complete machine snapshot of a CPU and memory.
/// </summary>
public class CpuSnapshot
{
    public string Name { get; set; }
    public MemoryDelta[] Deltas { get; set; }
    public Registers Registers { get; set; }

    [JsonConstructor]
    private CpuSnapshot()
    {
    }
    
    private CpuSnapshot(Registers registers, MemoryDelta[] snapshotDelta): this()
    {
        Registers = registers;
        Deltas = snapshotDelta;
    }

    public static CpuSnapshot From(string name, Registers registers, byte[] baseMemory, byte[] newMemory)
    {
        var snapshotDelta = GetMemoryDeltas(baseMemory, newMemory);
        return new CpuSnapshot(registers, snapshotDelta) { Name = name };
    }

    public static CpuSnapshot FromString(string serialized)
    {
        return JsonConvert.DeserializeObject<CpuSnapshot>(serialized);
    }

    public string AsString()
    {
        return JsonConvert.SerializeObject(this);
    }

    private static MemoryDelta[] GetMemoryDeltas(byte[] baseMemory, byte[] newMemory)
    {
        var deltas = new List<MemoryDelta>();
        for (var i = 0; i < baseMemory.Length; i++)
        {
            if (baseMemory[i] != newMemory[i])
                deltas.Add(new MemoryDelta((ushort)i, newMemory[i]));
        }

        return deltas.ToArray();
    }
    
    public void ApplyTo(CPU cpu)
    {
        JsonConvert.PopulateObject(JsonConvert.SerializeObject(Registers), cpu.TheRegisters);
        foreach (var delta in Deltas)
            cpu.MainMemory.Data[delta.Addr] = delta.Value;
    }
}