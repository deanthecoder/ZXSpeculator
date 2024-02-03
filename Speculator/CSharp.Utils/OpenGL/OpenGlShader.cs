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

using CSharp.Utils.Extensions;
using OpenTK.Graphics.OpenGL;

namespace CSharp.Utils.OpenGL;

/// <summary>
/// Represents a single pair of vertex and fragment shaders.
/// </summary>
public sealed class OpenGlShader : IDisposable
{
    private readonly int m_handle;
    private bool m_isDisposed;

    public OpenGlShader(FileInfo fragmentPath)
    {
        // Load vertex shader code.
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, "#version 330 core\nin vec3 aPosition;\nin vec2 aTexCoord;\n\nout vec2 texCoord;\n\nvoid main()\n{\n    texCoord = aTexCoord;\n    gl_Position = vec4(aPosition, 1);\n}");

        // Load fragment shader code.
        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentPath.ReadAllText());
			
        // Compile the shaders.
        CompileShader(vertexShader);
        CompileShader(fragmentShader);

        // Create a GL program.
        m_handle = GL.CreateProgram();
        GL.AttachShader(m_handle, vertexShader);
        GL.AttachShader(m_handle, fragmentShader);
        GL.LinkProgram(m_handle);
			
        // Done.
        GL.DetachShader(m_handle, vertexShader);
        GL.DetachShader(m_handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }
    
    private static void CompileShader(int shader)
    {
        GL.CompileShader(shader);
        var infoLog = GL.GetShaderInfoLog(shader);
        if (!string.IsNullOrEmpty(infoLog))
            Logger.Instance.Error($"Error compiling shader: {infoLog}");
    }

    public void Use() =>
        GL.UseProgram(m_handle);

    private int GetUniformLocation(string uniformName) => GL.GetUniformLocation(m_handle, uniformName);

    public void SetInt(string name, int value) => GL.Uniform1(GetUniformLocation(name), value);
    public void SetFloat(string name, double value) => GL.Uniform1(GetUniformLocation(name), (float)value);
    public void SetVec2(string name, double x, double y) => GL.Uniform2(GetUniformLocation(name), (float)x, (float)y);

    ~OpenGlShader()
    {
        GL.DeleteProgram(m_handle);
    }
		
    public void Dispose()
    {
        if (m_isDisposed)
            return;
        m_isDisposed = true;

        GL.DeleteProgram(m_handle);
        GC.SuppressFinalize(this);
    }
}