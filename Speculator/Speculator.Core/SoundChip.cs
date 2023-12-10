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

namespace Speculator.Core;

public class SoundChip : IDisposable
{
    private bool m_speakerState;

    public void SetSpeakerState(bool soundBit, bool force)
    {
        if (!force && m_speakerState == soundBit)
            return;
        m_speakerState = soundBit;
    }

    public void Dispose()
    {
    }

    public void OnCpuInterrupt()
    {
    }
}