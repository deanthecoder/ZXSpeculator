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
using CSharp.Utils.Extensions;

namespace CSharp.Utils;

/// <summary>
/// A container for arbitrary data, which will be stored compressed.
/// </summary>
[DebuggerDisplay("{Count} items in {UsedKb.ToString(\"F1\")}Kb")]
public class CompressedDataStore<TKey>
{
    private readonly bool m_allowDuplicateKeys;
    private readonly List<(TKey Key, byte[] Value)> m_store = new List<(TKey Key, byte[] Value)>();

    public int Count => m_store.Count;
    public double UsedKb => m_store.Any() ? m_store.Sum(o => o.Value.Length) / 1024.0 : 0;
    
    public IEnumerable<TKey> Keys => m_store.Select(o => o.Key);

    public CompressedDataStore(bool allowDuplicateKeys = false)
    {
        m_allowDuplicateKeys = allowDuplicateKeys;
    }

    public void Add(byte[] data) =>
        Add(default, data);

    public void Add(TKey key, byte[] data)
    {
        if (!m_allowDuplicateKeys)
            Remove(key);
        m_store.Add((key, data.Compress()));
    }

    public void Remove(TKey key) =>
        m_store.RemoveAll(o => o.Key.Equals(key));

    public void RemoveAt(int index) =>
        m_store.RemoveAt(index);

    public bool ContainsKey(TKey key) =>
        m_store.Any(o => o.Key.Equals(key));

    public byte[] At(int index) =>
        m_store[index].Value.Decompress();

    public byte[] Get(TKey key) =>
        m_store.First(o => o.Key.Equals(key)).Value.Decompress();
}