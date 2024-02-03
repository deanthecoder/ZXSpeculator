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

using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace CSharp.Utils.OpenGL;

public abstract class OpenGlControl : OpenGlControlBase
{
    private Bindings m_bindings;

    /// <summary>
    /// Called when control is created - Load shaders, etc.
    /// </summary>
    abstract protected void Init();

    /// <summary>
    /// Called once per frame.
    /// </summary>
    abstract protected void Render();

    /// <summary>
    /// Called when control is destroyed - Clean up OpenGL resources.
    /// </summary>
    abstract protected void UnInit();

    override protected void OnOpenGlRender(GlInterface gl, int unused)
    {
        var scaling = VisualRoot?.RenderScaling ?? 1.0;
        GL.Viewport(0, 0, (int)(Bounds.Width * scaling), (int)(Bounds.Height * scaling));

        if (IsVisible && Bounds.Width != 0 && Bounds.Height != 0)
            Render();

        Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Background);
    }

    override protected void OnOpenGlInit(GlInterface gl)
    {
        m_bindings = new Bindings(gl);
        GL.LoadBindings(m_bindings);

        Init();
    }

    override protected void OnOpenGlDeinit(GlInterface gl) =>
        UnInit();

    private class Bindings : IBindingsContext
    {
        private readonly GlInterface m_glInterface;

        public Bindings(GlInterface glInterface)
        {
            m_glInterface = glInterface;
        }

        public IntPtr GetProcAddress(string procName) =>
            m_glInterface.GetProcAddress(procName);
    }
}