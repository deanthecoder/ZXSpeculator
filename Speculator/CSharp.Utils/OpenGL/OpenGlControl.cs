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

using System.Drawing;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using Silk.NET.OpenGL;

namespace CSharp.Utils.OpenGL;

public abstract class OpenGlControl : OpenGlControlBase
{
    private GL m_gl;
    private BufferObject<uint> m_ebo;
    private BufferObject<float> m_vbo;
    private VertexArrayObject<float,uint> m_vao;

        private static readonly float[] m_vertices =
        {
            //X    Y      tx ty
            -1.0f, -1.0f,  0.0f, 1.0f,
             1.0f, -1.0f,   1.0f, 1.0f,
             1.0f, 1.0f,   1.0f, 0.0f,
            -1.0f, 1.0f,  0.0f, 0.0f
        };

        private static readonly uint[] m_indices =
        {
            0, 1, 3,
            1, 2, 3
        };

    /// <summary>
    /// Called when control is created - Load shaders, etc.
    /// </summary>
    /// <param name="gl"></param>
    abstract protected void Init(GL gl);

    /// <summary>
    /// Called once per frame.
    /// </summary>
    /// <param name="context"></param>
    abstract protected void Render(GL context);

    /// <summary>
    /// Called when control is destroyed - Clean up OpenGL resources.
    /// </summary>
    /// <param name="gl"></param>
    abstract protected void UnInit(GL gl);

    unsafe override protected void OnOpenGlRender(GlInterface gl, int unused)
    {
        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        gl.Viewport(0, 0, (int)(Bounds.Width * scaling), (int)(Bounds.Height * scaling));
        m_gl.ClearColor(Color.Firebrick);
        m_gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
        //m_gl.Enable(EnableCap.DepthTest);
        //var pTl = this.PointToScreen(Bounds.TopLeft);
        //var pBr = this.PointToScreen(Bounds.BottomRight);
        //var p = pBr - pTl;
        //gl.Viewport(0, 0, p.X, p.Y);

        m_ebo.Bind();
        m_vbo.Bind();
        m_vao.Bind();
        
        //if (IsVisible && Bounds.Width != 0 && Bounds.Height != 0)
            Render(m_gl);

        m_gl.DrawElements(PrimitiveType.Triangles, (uint)m_indices.Length, DrawElementsType.UnsignedInt, null);
        //Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
        Dispatcher.UIThread.Post(InvalidateVisual, DispatcherPriority.Background);
    }

    override protected void OnOpenGlInit(GlInterface gl)
    {
        m_gl = GL.GetApi(gl.GetProcAddress);

        //Instantiating our new abstractions
        m_ebo = new BufferObject<uint>(m_gl, m_indices, BufferTargetARB.ElementArrayBuffer);
        m_vbo = new BufferObject<float>(m_gl, m_vertices, BufferTargetARB.ArrayBuffer);
        m_vao = new VertexArrayObject<float, uint>(m_gl, m_vbo, m_ebo);

        //Telling the VAO object how to lay out the attribute pointers
        m_vao.VertexAttributePointer(0, 2, VertexAttribPointerType.Float, 4, 0);
        m_vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 4, 2);

        Init(m_gl);
    }

    override protected void OnOpenGlDeinit(GlInterface gl)
    {
        m_vbo.Dispose();
        m_ebo.Dispose();
        m_vao.Dispose();
        
        UnInit(m_gl);
    }
}