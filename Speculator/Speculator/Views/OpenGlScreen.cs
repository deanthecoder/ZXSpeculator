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

using System.Reflection;
using Avalonia;
using Avalonia.Media.Imaging;
using CSharp.Utils.Extensions;
using CSharp.Utils.OpenGL;
using OpenTK.Graphics.OpenGL;

namespace Speculator.Views;

/// <summary>
/// The Speccy's screen control, with GLSL CRT effect.
/// </summary>
public class OpenGlScreen : OpenGlControl
{
    private OpenGlShader m_shader;
    private int m_vertexBufferObject;
    private int m_vertexArrayObject;
    private OpenGlTexture m_bitmapTexture;
    private WriteableBitmap m_source;
    private bool m_isCrt;
    private bool m_isAmbientBlur;

    private readonly float[] m_vertices = {
        // Positions, Texture Coords
        0.0f, 0.0f,   -1.0f, 1.0f, // Bottom-left
        1.0f, 0.0f,   1.0f, 1.0f,  // Bottom-right
        1.0f, 1.0f,   1.0f, -1.0f, // Top-right
        0.0f, 1.0f,   -1.0f, -1.0f // Top-left
    };

    private readonly uint[] m_indices = {
        0, 1, 2, // First Triangle
        0, 2, 3  // Second Triangle
    };

    public static readonly DirectProperty<OpenGlScreen, WriteableBitmap> SourceProperty = AvaloniaProperty.RegisterDirect<OpenGlScreen, WriteableBitmap>(nameof(Source), o => o.Source, (o, v) => o.Source = v);
    public static readonly DirectProperty<OpenGlScreen, bool> IsCrtProperty = AvaloniaProperty.RegisterDirect<OpenGlScreen, bool>(nameof(IsCrt), o => o.IsCrt, (o, v) => o.IsCrt = v);
    public static readonly DirectProperty<OpenGlScreen, bool> IsAmbientBlurProperty = AvaloniaProperty.RegisterDirect<OpenGlScreen, bool>(nameof(IsAmbientBlur), o => o.IsAmbientBlur, (o, v) => o.IsAmbientBlur = v);

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

    override protected void Init()
    {
        m_shader = new OpenGlShader(Assembly.GetExecutingAssembly().GetDirectory().GetFile("Shaders/shader.frag"));

        m_bitmapTexture = new OpenGlTexture();
        m_bitmapTexture.Use();
        
        m_shader.Use();
        m_shader.SetInt("texture0", 0);

        m_vertexArrayObject = GL.GenVertexArray();
        m_vertexBufferObject = GL.GenBuffer();
        GL.BindVertexArray(m_vertexArrayObject);

        GL.BindBuffer(BufferTarget.ArrayBuffer, m_vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, m_vertices.Length * sizeof(float), m_vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        var ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, m_indices.Length * sizeof(uint), m_indices, BufferUsageHint.StaticDraw);
    }

    override protected void Render()
    {
        using var frameBuffer = Source.Lock();
        m_bitmapTexture.LoadFrom((int)Source.Size.Width, (int)Source.Size.Height, frameBuffer.Address);
        m_bitmapTexture.Use();

        m_shader.SetVec2("resolution", Bounds.Width, Bounds.Height);
        m_shader.SetFloat("crt", IsCrt ? 1.0 : 0.0);
        m_shader.SetFloat("ambientBlur", IsAmbientBlur ? 1.0 : 0.0);
        m_shader.SetFloat("xAspect", Bounds.Width * 240.0 / (Bounds.Height * 320.0));

        GL.BindVertexArray(m_vertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, m_indices.Length, DrawElementsType.UnsignedInt, 0);
    }

    override protected void UnInit()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.DeleteBuffer(m_vertexBufferObject);
        GL.DeleteVertexArray(m_vertexArrayObject);

        m_shader.Dispose();
        GL.UseProgram(0);
        m_bitmapTexture.Dispose();
    }
}