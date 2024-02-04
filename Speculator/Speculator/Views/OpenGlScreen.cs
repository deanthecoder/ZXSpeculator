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

using System;
using System.Reflection;
using Avalonia;
using Avalonia.Media.Imaging;
using CSharp.Utils.Extensions;
using CSharp.Utils.OpenGL;
using Silk.NET.OpenGL;
using Shader = CSharp.Utils.OpenGL.Shader;
using Texture = CSharp.Utils.OpenGL.Texture;

namespace Speculator.Views;

/// <summary>
/// The Speccy's screen control, with GLSL CRT effect.
/// </summary>
public class OpenGlScreen : OpenGlControl
{
    private Shader m_shader;
    private WriteableBitmap m_source;
    private bool m_isCrt;
    private bool m_isAmbientBlur;

    public static readonly DirectProperty<OpenGlScreen, WriteableBitmap> SourceProperty = AvaloniaProperty.RegisterDirect<OpenGlScreen, WriteableBitmap>(nameof(Source), o => o.Source, (o, v) => o.Source = v);
    public static readonly DirectProperty<OpenGlScreen, bool> IsCrtProperty = AvaloniaProperty.RegisterDirect<OpenGlScreen, bool>(nameof(IsCrt), o => o.IsCrt, (o, v) => o.IsCrt = v);
    public static readonly DirectProperty<OpenGlScreen, bool> IsAmbientBlurProperty = AvaloniaProperty.RegisterDirect<OpenGlScreen, bool>(nameof(IsAmbientBlur), o => o.IsAmbientBlur, (o, v) => o.IsAmbientBlur = v);
    private Texture m_bitmapTexture;

    public WriteableBitmap Source
    {
        get => m_source;
        set => SetAndRaise(SourceProperty, ref m_source, value);
    }
    
    public bool IsCrt
    {
        get => m_isCrt;
        set => SetAndRaise(IsCrtProperty, ref m_isCrt, value);
    }

    public bool IsAmbientBlur
    {
        get => m_isAmbientBlur;
        set => SetAndRaise(IsAmbientBlurProperty, ref m_isAmbientBlur, value);
    }

    override protected void Init(GL gl)
    {
        m_shader = new Shader(gl,
            "#version 330 core\nlayout (location = 0) in vec2 vPos;\nlayout (location = 1) in vec2 aTexCoord;\nout vec2 texCoord;\nvoid main()\n{\ntexCoord = aTexCoord;\ngl_Position = vec4(vPos,0.0, 1.0);\n}",
            Assembly.GetExecutingAssembly().GetDirectory().GetFile("Shaders/shader.frag").ReadAllText());
    }

    unsafe override protected void Render(GL context)
    {
        using var frameBuffer = Source.Lock();
        var array = new byte[(int)(Source.Size.Width * Source.Size.Height * 4)];
        var bitmapData = new Span<byte>((void*)frameBuffer.Address, array.Length);

        m_bitmapTexture?.Dispose(); // todo - Only assign once.
        m_bitmapTexture = new Texture(context, bitmapData, (uint)Source.Size.Width, (uint)Source.Size.Height);

        m_shader.Use();

        m_bitmapTexture.Bind();
        m_shader.SetUniform("texture0", 0);
        m_shader.SetUniform("crt", IsCrt ? 1.0 : 0.0);
        m_shader.SetUniform("ambientBlur", IsAmbientBlur ? 1.0 : 0.0);
        m_shader.SetUniform("xAspect", Bounds.Width * 240.0 / (Bounds.Height * 320.0));
    }

    override protected void UnInit(GL gl)
    {
        m_shader.Dispose();
        m_bitmapTexture.Dispose();
    }
}