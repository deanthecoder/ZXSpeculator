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

using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using CSharp.Core.Commands;
using CSharp.Core.Extensions;
using Newtonsoft.Json;

namespace CSharp.Core.ViewModels;

public class MruFiles : ViewModelBase
{
    private const int MaxCount = 8;
    private readonly List<SingleItem> m_items = Enumerable.Range(0, MaxCount).Select(_ => SingleItem.Empty).ToList();

    public event EventHandler<FileInfo> OpenRequested;

    public bool IsEmpty => m_items.All(o => !o.IsUsed);
    public SingleItem Item1 => m_items[0];
    public SingleItem Item2 => m_items[1];
    public SingleItem Item3 => m_items[2];
    public SingleItem Item4 => m_items[3];
    public SingleItem Item5 => m_items[4];
    public SingleItem Item6 => m_items[5];
    public SingleItem Item7 => m_items[6];
    public SingleItem Item8 => m_items[7];

    /// <summary>
    /// Command that is bound to from the view, triggering a load from the MRU list.
    /// Command parameter is the file number (1+).
    /// Triggers an OpenRequested event.
    /// </summary>
    public ICommand Open { get; }

    public MruFiles()
    {
        Open = new RelayCommand(o =>
            {
                var file = m_items[int.Parse((string)o) - 1];
                OpenRequested?.Invoke(this, file.File);
                Add(file.File);
            },
            o =>
            {
                var file = m_items[int.Parse((string)o) - 1];
                return file?.File?.Exists() == true;
            }
        );
    }

    public void Add(FileInfo file)
    {
        m_items.RemoveAll(o => file.FullName == o?.FullName);
        m_items.Insert(0, new SingleItem(file));
        while (m_items.Count > MaxCount)
            m_items.Remove(m_items.Last());
        
        RaisePropertyChanges();
    }
    
    private void RaisePropertyChanges()
    {

        GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(o => o.PropertyType == typeof(SingleItem))
            .ToList()
            .ForEach(o => OnPropertyChanged(o.Name));
        OnPropertyChanged(nameof(IsEmpty));
    }

    public void Clear()
    {
        for (var i = 0; i < m_items.Count; i++)
            m_items[i] = SingleItem.Empty;
        RaisePropertyChanges();
    }

    public MruFiles InitFromString(string serialized)
    {
        if (!string.IsNullOrEmpty(serialized))
        {
            m_items.Clear();
            JsonConvert.PopulateObject(serialized, m_items);
        }
        else
        {
            Clear();
        }
        
        return this;
    }

    public string AsString() =>
        JsonConvert.SerializeObject(m_items);

    [DebuggerDisplay("{FullName}")]
    [JsonObject(MemberSerialization.OptIn)]
    public class SingleItem
    {
        [JsonProperty]
        public string FullName { get; set; }

        public FileInfo File => FullName != null ? new FileInfo(FullName) : null;

        public static SingleItem Empty { get; } = new SingleItem(null);

        public SingleItem(FileInfo info)
        {
            FullName = info?.FullName;
        }
        
        public bool IsUsed => !string.IsNullOrEmpty(FullName);

        public override string ToString() =>
            File?.LeafName();
    }
}