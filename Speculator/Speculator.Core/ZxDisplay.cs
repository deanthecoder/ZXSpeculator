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

using System.Collections;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CSharp.Core;
using CSharp.Core.Extensions;
using CSharp.Core.ViewModels;
using OpenTK.Mathematics;
using Vector = Avalonia.Vector;
using Vector3 = System.Numerics.Vector3;

namespace Speculator.Core;

public class ZxDisplay : ViewModelBase
{
    public const int ScreenBase = 0x4000;

    private const int ColorMapBase = 0x5800;
    private const int LeftMargin = 32;
    private const int RightMargin = 32;
    private const int TopMargin = 24;
    private const int BottomMargin = 24;
    private const int WriteableWidth = 256;
    private const int WritableHeight = 192;
    private const int FramesPerFlash = 16;
    private bool m_isCrt = true;
    private Vector3 m_scanlineMultiplier;
    private float m_phosphorShrink;
    private float m_brightness;
    private readonly Vector3 m_crtSaturation = new Vector3(1.1f, 1.0f, 1.1f);
    private int m_flashFrameCount;
    private bool m_isFlashing;
    private DateTime m_lastFlashTime = DateTime.Now;
    private double m_emulationSpeed;
    private bool m_isPaused;
    private readonly Random m_random = new Random(0);
    private readonly BitArray m_pauseBitmap = new BitArray(new byte[] { 0x1F, 0x1E, 0x21, 0x1E, 0xFF, 0x87, 0x47, 0x88, 0xC7, 0x1F, 0x12, 0x12, 0x12, 0x10, 0x84, 0x84, 0x84, 0x04, 0x04, 0x21, 0x21, 0x21, 0x1E, 0x5F, 0x48, 0x48, 0x88, 0xC7, 0xF7, 0xF1, 0x13, 0x02, 0x12, 0x7C, 0xFC, 0x84, 0x80, 0x04, 0x01, 0x21, 0x21, 0x21, 0x41, 0x40, 0x48, 0x48, 0x48, 0x10, 0x10, 0xE2, 0xE1, 0xF1, 0x07, 0x84, 0x78, 0x78, 0xFC});

    /// <summary>
    /// A timer active when the machine is paused, allowing the pause animation to occur.
    /// </summary>
    private PeriodicAction m_updateTimer;

    /// <summary>
    /// 'Grain' overlay applied to the CRT screen.
    /// </summary>
    private readonly Vector3[][] m_grain;

    /// <summary>
    /// 320x240 Buffer of pixels, each byte a palette index.
    /// </summary>
    private readonly byte[][] m_screenBuffer = CreateScreenBuffer();

    /// <summary>
    /// Bitmap used to contain the RGB pixel data blitted to the screen.
    /// </summary>
    public WriteableBitmap Bitmap { get; } = CreateWriteableBitmap(true); // Expand height for scanlines.

    /// <summary>
    /// Used to prevent unnecessary UI screen refreshes.
    /// </summary>
    private bool m_didPixelsChange;
    
    private static readonly Vector3[] Colors =
    {
        new Vector3(0x00, 0x00, 0x00), // Black
        new Vector3(0x00, 0x00, 0xCD), // Blue
        new Vector3(0xCD, 0x00, 0x00), // Red
        new Vector3(0xCD, 0x00, 0xCD), // Magenta
        new Vector3(0x00, 0xCD, 0x00), // Green
        new Vector3(0x00, 0xCD, 0xCD), // Cyan
        new Vector3(0xCD, 0xCD, 0x00), // Yellow
        new Vector3(0xCD, 0xCD, 0xCD), // White (/Gray)
        new Vector3(0x00, 0x00, 0x00), // (Bright) Black
        new Vector3(0x00, 0x00, 0xFF), // Bright Blue
        new Vector3(0xFF, 0x00, 0x00), // Bright Red
        new Vector3(0xFF, 0x00, 0xFF), // Bright Magenta
        new Vector3(0x00, 0xFF, 0x00), // Bright Green
        new Vector3(0x00, 0xFF, 0xFF), // Bright Cyan
        new Vector3(0xFF, 0xFF, 0x00), // Bright Yellow
        new Vector3(0xFF, 0xFF, 0xFF)  // Bright White
    };
    
    public byte BorderAttr { get; set; }

    public bool IsCrt
    {
        get => m_isCrt;
        set
        {
            m_isCrt = value;
            m_didPixelsChange = true;

            if (m_isCrt)
            {
                m_scanlineMultiplier = new Vector3(0.7f);
                m_phosphorShrink = 0.5f;
            }
            else
            {
                m_scanlineMultiplier = Vector3.One;
                m_phosphorShrink = 1.0f;
            }

            m_brightness = 3.0f / (1.0f + 2.0f * m_phosphorShrink);

            for (var i = 0; i < m_screenBuffer.Length; i++)
            {
                for (var j = 0; j < m_screenBuffer[0].Length; j++)
                    m_grain[i][j] = m_isCrt ? new Vector3((float)(m_random.NextDouble() * 12.0)) : Vector3.Zero;
            }

            m_didPixelsChange = true;
        }
    }

    public bool IsPaused
    {
        get => m_isPaused;
        set
        {
            if (m_isPaused == value)
                return;
            m_isPaused = value;

            if (m_isPaused)
            {
                // Keep updating the UI whilst paused.
                m_updateTimer = PeriodicAction.Start(TimeSpan.FromSeconds(1.0 / 30.0), UpdateScreen);

                EmulationSpeed = 0.0;
            }
            else
            {
                m_updateTimer?.Dispose();
                m_updateTimer = null;
            }

            m_didPixelsChange = true;
        }
    }

    /// <summary>
    /// The emulation speed (where 1.0 => 100% Speccy).
    /// </summary>
    public double EmulationSpeed
    {
        get => m_emulationSpeed;
        private set => SetField(ref m_emulationSpeed, value);
    }

    public event EventHandler Refreshed;

    public ZxDisplay()
    {
        m_grain = new Vector3[m_screenBuffer.Length][];
        for (var i = 0; i < m_screenBuffer.Length; i++)
            m_grain[i] = new Vector3[m_screenBuffer[0].Length];
    }

    private static (byte, byte) GetColorIndices(byte attr, bool invert = false)
    {
        var paperIndex = (byte)(attr >> 3 & 0x07);
        var penIndex = (byte)(attr & 0x07);
        
        var isBright = attr & 0x40;
        if (isBright != 0)
        {
            paperIndex += 8;
            penIndex += 8;
        }

        return invert ? (paperIndex, penIndex) : (penIndex, paperIndex);
    }
    
    public void OnRenderScanline(object sender, (Memory memory, int scanline) args)
    {
        var didReachScreenBottom = RenderScanlineIntoBuffer(args.memory, args.scanline, m_screenBuffer, BorderAttr, m_isFlashing, ref m_didPixelsChange);

        // If scanline reached the bottom of the screen, update the UI.
        if (!didReachScreenBottom)
            return;
        
        // Update the flash.
        if (m_flashFrameCount++ == FramesPerFlash)
        {
            m_isFlashing = !m_isFlashing;
            m_flashFrameCount = 0;
            
            // Also update the EmulationSpeed.
            var now = DateTime.Now;
            if (IsPaused)
            {
                EmulationSpeed = 0.0;
            }
            else
            {
                var elapsedSecs = (now - m_lastFlashTime).TotalSeconds;
                EmulationSpeed = Math.Round(FramesPerFlash / (50.0 * elapsedSecs) / 0.25) * 0.25;
            }

            m_lastFlashTime = now;
        }

        if (m_didPixelsChange)
            UpdateScreen();
        m_didPixelsChange = false;
    }

    /// <summary>
    /// Returns true if the scanline has reached the bottom of the screen.
    /// </summary>
    private static bool RenderScanlineIntoBuffer(Memory memory, int scanlineIndex, byte[][] screenBuffer, byte borderAttr, bool isFlashing, ref bool didPixelsChange)
    {
        var y = scanlineIndex - (48 - TopMargin);
        if (y < 0 || y >= screenBuffer.Length)
        {
            // Off-screen(/vsync) area.
            return false;
        }
        
        // Fill entire line with border color.
        var border = GetColorIndices(borderAttr).Item1;
        if (screenBuffer[y][0] != border)
        {
            didPixelsChange = true;
            Array.Fill(screenBuffer[y], border);
        }

        // Set Y to the drawable screen coordinate.
        y -= TopMargin;
        if (y < 0 || y >= WritableHeight)
        {
            // In top or bottom border area - No screen content needed.
            return false;
        }
        
        // Draw screen pixel content.
        var y76 = (byte)(y >> 6);
        var y210 = (byte)(y & 0x07);
        var y543 = (byte)((y >> 3) & 0x07);
        var srcRowStart = (ushort)(0x4000 | (y76 << 11) | (y210 << 8) | (y543 << 5));

        var characterRow = y / 8;
        for (var characterColumn = 0; characterColumn < 32; characterColumn++)
        {
            // Get block of 8 horizontal pixels.
            var screenByte = memory.Data[srcRowStart + characterColumn]; 

            // Get the pen/paper color for this block.
            var a = characterRow * 32 + characterColumn;
            var attr = memory.Data[ColorMapBase + a];
            var isFlashSet = (attr & 0x80) != 0;
            var penAndPaper = GetColorIndices(attr, isFlashing && isFlashSet);

            // Write indexed colors for the 8 pixel block.
            var off = LeftMargin + characterColumn * 8;
            for (var i = 7; i >= 0; i--, off++)
            {
                var isSet = (screenByte & (1 << i)) != 0;
                var index = isSet ? penAndPaper.Item1 : penAndPaper.Item2;
                if (screenBuffer[y + TopMargin][off] == index)
                    continue;
                didPixelsChange = true;
                screenBuffer[y + TopMargin][off] = index;
            }
        }

        return y == WritableHeight - 1;
    }

    /// <summary>
    /// Create a full screen image from the current state of memory.
    /// </summary>
    public static Bitmap CaptureScreenFromMemory(Memory theMemory, byte borderAttr)
    {
        // Render the screen into a buffer of indexed palette values.
        const int scanlineCount = 312;
        var screenBuffer = CreateScreenBuffer();
        for (var i = 0; i < scanlineCount; i++)
        {
            var unused = false;
            RenderScanlineIntoBuffer(theMemory, i, screenBuffer, borderAttr, false, ref unused);
        }

        // Convert buffer to an image.
        var bitmap = CreateWriteableBitmap(false);
        using var frameBuffer = bitmap.Lock();
        unsafe
        {
            var framePtr = (byte*)frameBuffer.Address;
            var ptr = new Span<byte>(framePtr, frameBuffer.RowBytes * frameBuffer.Size.Height);
            for (var y = 0; y < screenBuffer.Length; y++)
            {
                var row = screenBuffer[y];
                for (var x = 0; x < screenBuffer[0].Length; x++)
                    FrameBuffer.SetPixel(ptr, frameBuffer.RowBytes, x, y, Colors[row[x]]);
            }
        }

        return bitmap;
    }

    /// <summary>
    /// Render the Speccy screen memory into a bitmap for display.
    /// </summary>
    private unsafe void UpdateScreen()
    {
        lock (Bitmap)
        {
            using (var frameBuffer = Bitmap.Lock())
            {
                var framerBufferStride = frameBuffer.RowBytes;
                var w = m_screenBuffer[0].Length;
                var h = m_screenBuffer.Length;
                var ptr = new Span<byte>((byte*)frameBuffer.Address, frameBuffer.RowBytes * frameBuffer.Size.Height);

                var phosphorR = new Vector3(1.0f, m_phosphorShrink, m_phosphorShrink);
                var phosphorG = new Vector3(m_phosphorShrink, 1.0f, m_phosphorShrink);
                var phosphorB = new Vector3(m_phosphorShrink, m_phosphorShrink, 1.0f);

                // Software pixel shader.
                var iTime = DateTime.Now.TimeOfDay.TotalSeconds;
                var dy = 0;
                if (IsPaused)
                {
                    // Vertical screen wobble.
                    dy = (int)(m_random.NextDouble() * 1.5);
                }

                for (var y = 0; y < h; y++)
                {
                    var row = m_screenBuffer[y];
                    var uvY = (double)y / h;

                    var dx = 0;
                    var distFromPulse = 0.0;
                    if (IsPaused)
                    {
                        // White pulse moving down the screen.
                        var pulseY = ((iTime % 8.0) / 8.0) * h * 2.5;
                        var pulseHeight = 20.0;
                        distFromPulse = (Math.Abs(y - pulseY) / pulseHeight).Clamp(0.0, 1.0);
                        dx = (int)(-12.0 * m_random.NextDouble() * Math.Cos(distFromPulse * Math.PI / 2.0));
                        dx = (int)(dx + (m_random.NextDouble() * 2.2 - 1.1));
                    }

                    for (var x = 0; x < w; x++)
                    {
                        var origColor = Colors[row[x]];

                        if (IsCrt)
                        {
                            var uvX = (double)x / w;
                            var vignette = (float)MathHelper.Lerp(0.7, 1.0, Math.Sqrt(64.0 * uvX * uvY * (1.0 - uvX) * (1.0 - uvY)));

                            if (IsPaused)
                            {
                                // Screen displacement.
                                var lx = Math.Max(0, x + dx) % w;
                                var ly = Math.Max(0, y + dy) % h;
                                origColor = Colors[m_screenBuffer[ly][lx]];

                                // Desaturate color.
                                var lumin = Vector3.Dot(origColor, new Vector3(0.2f, 0.7f, 0.1f));
                                origColor = Vector3.Lerp(origColor, new Vector3(lumin), 0.9f);

                                // Add noise.
                                origColor += new Vector3((float)((m_random.NextDouble() - 0.5) * 50.0));

                                // Add extra noise to the white pulse.
                                if (m_random.NextDouble() * (1.0 - distFromPulse) > 0.4)
                                    origColor += new Vector3((float)(92.0 * m_random.NextDouble()));

                                // PAUSE.
                                var px = x - 20;
                                var py = ly - 20;
                                if (px >= 0 && px < 38 && py >= 0 && py < 12)
                                {
                                    if (m_pauseBitmap[py * 38 + px])
                                        origColor += new Vector3(200);
                                }
                            }

                            origColor += m_grain[y][x];
                            origColor *= m_brightness * vignette * m_crtSaturation;
                        }

                        var xx = x * 3;
                        var yy = y * 4;
                        FrameBuffer.SetPixelV4(ptr, framerBufferStride, xx, yy, origColor * phosphorR, m_scanlineMultiplier);
                        FrameBuffer.SetPixelV4(ptr, framerBufferStride, xx + 1, yy, origColor * phosphorG, m_scanlineMultiplier);
                        FrameBuffer.SetPixelV4(ptr, framerBufferStride, xx + 2, yy, origColor * phosphorB, m_scanlineMultiplier);
                    }
                }
            }

            Refreshed?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Buffer of pixels, each byte a palette index.
    /// </summary>
    private static byte[][] CreateScreenBuffer() =>
        Enumerable.Range(0, TopMargin + WritableHeight + BottomMargin).Select(_ => new byte[LeftMargin + WriteableWidth + RightMargin]).ToArray();

    /// <summary>
    /// Create a bitmap suitable for use in the UI.
    /// </summary>
    /// <remarks>
    /// The expanded bitmap allows for scanlines and RGB phosphor dots to be added.
    /// </remarks>
    private static WriteableBitmap CreateWriteableBitmap(bool expandForFx) =>
        new WriteableBitmap(new PixelSize((LeftMargin + WriteableWidth + RightMargin) * (expandForFx ? 3 : 1), (TopMargin + WritableHeight + BottomMargin) * (expandForFx ? 4 : 1)), new Vector(96, 96), PixelFormat.Rgba8888);
}
