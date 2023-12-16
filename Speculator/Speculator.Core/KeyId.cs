using Avalonia.Input;

namespace Speculator.Core;

/// <summary>
/// Encapsulates the state of one host keyboard key.
/// </summary>
public class KeyId
{
    private KeyModifiers KeyModifiers { get; }
    public Key Key { get; }

    public static implicit operator KeyId(Key key) => new KeyId(key);

    public KeyId(Key key, KeyModifiers keyModifiers = KeyModifiers.None)
    {
        Key = key;
        KeyModifiers = keyModifiers;
    }

    private bool Equals(KeyId other) =>
        Key == other.Key && KeyModifiers == other.KeyModifiers;
    
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        return obj.GetType() == GetType() && Equals((KeyId)obj);
    }
    
    public override int GetHashCode()
    {
        unchecked
        {
            return ((int)Key * 397) ^ (int)KeyModifiers;
        }
    }
}