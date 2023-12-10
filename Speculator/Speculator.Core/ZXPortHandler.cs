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

public interface IPortHandler
{
    byte In(int portAddress);
    void Out(byte port, byte b);
}

public class ZXPortHandler : IPortHandler
{
    private readonly SoundChip m_soundChip;

    public ZXPortHandler(SoundChip soundChip)
    {
        m_soundChip = soundChip;
        m_PCToSpectrumKeyMap[Key.Back] = new List<Key> { Key.LeftShift, Key.D0 };
        m_PCToSpectrumKeyMap[Key.OemComma] = new List<Key> { Key.RightShift, Key.N };
        m_PCToSpectrumKeyMap[Key.OemPeriod] = new List<Key> { Key.RightShift, Key.M };
        m_PCToSpectrumKeyMap[Key.OemPlus] = new List<Key> { Key.RightShift, Key.K };
        m_PCToSpectrumKeyMap[Key.OemMinus] = new List<Key> { Key.RightShift, Key.J };
        m_PCToSpectrumKeyMap[Key.OemQuestion] = new List<Key> { Key.RightShift, Key.C };
    }

    public byte In(int portAddress)
    {
        if ((portAddress & 0x00FF) != 0xFE)
            return 0xFF;

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

        return (byte)(~result);
    }

    public void Out(byte port, byte b)
    {
        if (port != 0xFE)
            return;

        var bit4 = (b & (1 << 4)) != 0;
        m_soundChip.SetSpeakerState(bit4, false);
    }

    private bool IsKeyPressed(Key key)
    {
        return m_pressedKeys.Contains(key);
    }

    private readonly List<Key> m_pressedKeys = new List<Key>();
    private readonly Dictionary<Key, List<Key>> m_PCToSpectrumKeyMap = new Dictionary<Key, List<Key>>();

    public void SetKeyDown(Key key)
    {
        if (m_PCToSpectrumKeyMap.ContainsKey(key))
        {
            foreach (Key altKey in m_PCToSpectrumKeyMap[key])
                SetKeyDown(altKey);
            return;
        }

        if (! m_pressedKeys.Contains(key))
            m_pressedKeys.Add(key);
    }

    public void SetKeyUp(Key key)
    {
        if (m_PCToSpectrumKeyMap.ContainsKey(key))
        {
            foreach (Key altKey in m_PCToSpectrumKeyMap[key])
                SetKeyUp(altKey);
            return;
        }

        m_pressedKeys.Remove(key);
    }

    public void ClearKeys()
    {
        m_pressedKeys.Clear();
    }
}