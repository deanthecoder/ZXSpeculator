using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Speculator.Core;

namespace Speculator.ViewModels;

public class MemoryDumpViewModel
{
    public ObservableCollection<SingleItem> Content { get; }

    public MemoryDumpViewModel(Memory memory)
    {
        var lineCount = memory.Data.Length / 8;
        Content = new ObservableCollection<SingleItem>(Enumerable.Range(0, lineCount).Select(i => new SingleItem(memory, (ushort)(i * 8))));

        memory.DataLoaded += (sender, args) => Refresh();
    }
    
    public void Refresh()
    {
        for (ushort i = 0; i < Content.Count; i += 8)
        {
            var existingItem = Content[i];
            var newItem = new SingleItem(existingItem);
            
            if (existingItem.Values == newItem.Values)
                continue;
            Content.RemoveAt(i);
            Content.Insert(i, newItem);
        }
    }

    [DebuggerDisplay("{Addr} {Values}")]
    public class SingleItem
    {
        private readonly Memory m_memory;
        private readonly ushort m_addr;

        public string Addr => $"{m_addr:X04}:";
        public string Values { get; }
        public bool IsBios => m_memory.IsRom(m_addr);
        
        public SingleItem(Memory memory, ushort addr)
        {
            m_memory = memory;
            m_addr = addr;
            Values = memory.ReadAsHexString(addr, 8, true);
        }
        
        public SingleItem(SingleItem o) : this(o.m_memory, o.m_addr)
        {
        }
    }
}