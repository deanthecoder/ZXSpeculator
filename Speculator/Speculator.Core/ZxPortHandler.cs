// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If modified, please retain this copyright header, and consider telling us
// about your changes.  We're always glad to see how people use our code!
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND.
// We do not accept any liability for damage caused by executing
// or modifying this code.

using Avalonia.Input;

namespace Speculator.Core;

/// <summary>
/// Detects key presses and speaker state changes, and feeds port info back to the emulator.
/// </summary>
public class ZxPortHandler : IPortHandler
{
    private readonly SoundHandler m_soundHandler;
    private readonly ZxDisplay m_theDisplay;
    private readonly Dictionary<Key, bool> m_pressedKeys = new Dictionary<Key, bool>();
    private readonly Dictionary<KeyId, List<Key>> m_pcToSpectrumKeyMap;

    public Memory MainMemory { get; set; }

    public ZxPortHandler(SoundHandler soundHandler, ZxDisplay theDisplay)
    {
        m_soundHandler = soundHandler;
        m_theDisplay = theDisplay;

        m_pcToSpectrumKeyMap = new Dictionary<KeyId, List<Key>>
        {
            // Map PC key to a sequence of emulated Speccy keys.
            [Key.Back] = new List<Key> { Key.LeftShift, Key.D0 },
            [Key.OemComma] = new List<Key> { Key.RightShift, Key.N },
            [Key.OemPeriod] = new List<Key> { Key.RightShift, Key.M },
            [Key.OemPlus] = new List<Key> { Key.RightShift, Key.L },
            [new KeyId(Key.OemPlus, KeyModifiers.Shift)] = new List<Key> { Key.RightShift, Key.K },
            [Key.OemMinus] = new List<Key> { Key.RightShift, Key.J },
            [Key.OemQuestion] = new List<Key> { Key.RightShift, Key.C },
            [Key.OemQuotes] = new List<Key> { Key.RightShift, Key.D7 },
            [new KeyId(Key.OemQuotes, KeyModifiers.Shift)] = new List<Key> { Key.RightShift, Key.P },
            [Key.OemSemicolon] = new List<Key> { Key.RightShift, Key.O },
            [new KeyId(Key.OemSemicolon, KeyModifiers.Shift)] = new List<Key> { Key.RightShift, Key.Z },
            [new KeyId(Key.LeftCtrl, KeyModifiers.Control)] = new List<Key> { Key.LeftShift, Key.RightShift },
        };
    }

    public byte In(int portAddress)
    {
        // We only support reading from the keyboard.
        if ((portAddress & 0x00FF) != 0xFE)
            return 0xFF; // Floating bus value.

        byte result = 0x00;

        var hi = (byte)(portAddress >> 8);

        if ((hi & 0x80) == 0)
        {
            if (IsKeyPressed(Key.B)) result |= 1 << 4;
            if (IsKeyPressed(Key.N)) result |= 1 << 3;
            if (IsKeyPressed(Key.M)) result |= 1 << 2;
            if (IsKeyPressed(Key.RightShift)) result |= 1 << 1;
            if (IsKeyPressed(Key.Space)) result |= 1 << 0;
        } 

        if ((hi & 0x08) == 0)
        {
            if (IsKeyPressed(Key.D1)) result |= 1 << 0;
            if (IsKeyPressed(Key.D2)) result |= 1 << 1;
            if (IsKeyPressed(Key.D3)) result |= 1 << 2;
            if (IsKeyPressed(Key.D4)) result |= 1 << 3;
            if (IsKeyPressed(Key.D5)) result |= 1 << 4;
        }

        if ((hi & 0x10) == 0)
        {
            if (IsKeyPressed(Key.D6)) result |= 1 << 4;
            if (IsKeyPressed(Key.D7)) result |= 1 << 3;
            if (IsKeyPressed(Key.D8)) result |= 1 << 2;
            if (IsKeyPressed(Key.D9)) result |= 1 << 1;
            if (IsKeyPressed(Key.D0)) result |= 1 << 0;
        }

        if ((hi & 0x04) == 0)
        {
            if (IsKeyPressed(Key.Q)) result |= 1 << 0;
            if (IsKeyPressed(Key.W)) result |= 1 << 1;
            if (IsKeyPressed(Key.E)) result |= 1 << 2;
            if (IsKeyPressed(Key.R)) result |= 1 << 3;
            if (IsKeyPressed(Key.T)) result |= 1 << 4;
        }

        if ((hi & 0x20) == 0)
        {
            if (IsKeyPressed(Key.Y)) result |= 1 << 4;
            if (IsKeyPressed(Key.U)) result |= 1 << 3;
            if (IsKeyPressed(Key.I)) result |= 1 << 2;
            if (IsKeyPressed(Key.O)) result |= 1 << 1;
            if (IsKeyPressed(Key.P)) result |= 1 << 0;
        }

        if ((hi & 0x02) == 0)
        {
            if (IsKeyPressed(Key.A)) result |= 1 << 0;
            if (IsKeyPressed(Key.S)) result |= 1 << 1;
            if (IsKeyPressed(Key.D)) result |= 1 << 2;
            if (IsKeyPressed(Key.F)) result |= 1 << 3;
            if (IsKeyPressed(Key.G)) result |= 1 << 4;
        }

        if ((hi & 0x40) == 0)
        {
            if (IsKeyPressed(Key.H)) result |= 1 << 4;
            if (IsKeyPressed(Key.J)) result |= 1 << 3;
            if (IsKeyPressed(Key.K)) result |= 1 << 2;
            if (IsKeyPressed(Key.L)) result |= 1 << 1;
            if (IsKeyPressed(Key.Return)) result |= 1 << 0;
        }

        if ((hi & 0x01) == 0)
        {
            if (IsKeyPressed(Key.LeftShift)) result |= 1 << 0;
            if (IsKeyPressed(Key.Z)) result |= 1 << 1;
            if (IsKeyPressed(Key.X)) result |= 1 << 2;
            if (IsKeyPressed(Key.C)) result |= 1 << 3;
            if (IsKeyPressed(Key.V)) result |= 1 << 4;
        }

        return (byte)~result;
    }

    public void Out(byte port, byte b)
    {
        // We only care about writes to port 0xFE.
        if (port != 0xFE)
            return;
        
        // Bit 4 is the speaker on/off bit.
        var bit4 = (b & (1 << 4)) != 0;
        m_soundHandler?.SetSpeakerState(bit4);
        
        // Lower 3 bits will set the border color.
        m_theDisplay.BorderAttr = b;
    }

    private bool IsKeyPressed(Key key) => m_pressedKeys.ContainsKey(key);

    public void SetKeyDown(Key key, KeyModifiers modifiers)
    {
        var keyId = new KeyId(key, modifiers);
        if (m_pcToSpectrumKeyMap.TryGetValue(keyId, out var speccyKeys))
        {
            foreach (var speccyKey in speccyKeys)
                m_pressedKeys[speccyKey] = true;
        }
        
        m_pressedKeys[key] = true;
    }

    public void SetKeyUp(Key key)
    {
        m_pressedKeys.Remove(key);

        foreach (var keyId in m_pcToSpectrumKeyMap.Keys.Where(o => o.Key == key))
        {
            foreach (var speccyKey in m_pcToSpectrumKeyMap[keyId])
                m_pressedKeys.Remove(speccyKey);
        }
    }

    public void ClearKeys()
    {
        m_pressedKeys.Clear();
    }
}