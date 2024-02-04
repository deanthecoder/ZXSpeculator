using Silk.NET.OpenGL;

namespace CSharp.Utils.OpenGL;

public class Shader : IDisposable
{
    private uint m_handle;
    private GL m_gl;

    public Shader(GL gl, string vertexGlsl, string fragmentGlsl)
    {
        m_gl = gl;

        var vertex = LoadShader(ShaderType.VertexShader, vertexGlsl);
        var fragment = LoadShader(ShaderType.FragmentShader, fragmentGlsl);
        m_handle = m_gl.CreateProgram();
        m_gl.AttachShader(m_handle, vertex);
        m_gl.AttachShader(m_handle, fragment);
        m_gl.LinkProgram(m_handle);
        m_gl.GetProgram(m_handle, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            throw new Exception($"Program failed to link with error: {m_gl.GetProgramInfoLog(m_handle)}");
        }
        m_gl.DetachShader(m_handle, vertex);
        m_gl.DetachShader(m_handle, fragment);
        m_gl.DeleteShader(vertex);
        m_gl.DeleteShader(fragment);
    }

    public void Use()
    {
        m_gl.UseProgram(m_handle);
    }

    public void SetUniform(string name, int value)
    {
        var location = m_gl.GetUniformLocation(m_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        m_gl.Uniform1(location, value);
    }

    public void SetUniform(string name, double value)
    {
        var location = m_gl.GetUniformLocation(m_handle, name);
        if (location == -1)
        {
            throw new Exception($"{name} uniform not found on shader.");
        }
        m_gl.Uniform1(location, (float)value);
    }

    public void Dispose()
    {
        m_gl.DeleteProgram(m_handle);
    }

    private uint LoadShader(ShaderType type, string glsl)
    {
        var handle = m_gl.CreateShader(type);
        m_gl.ShaderSource(handle, glsl);
        m_gl.CompileShader(handle);
        var infoLog = m_gl.GetShaderInfoLog(handle);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling shader of type {type}, failed with error {infoLog}");
        }

        return handle;
    }
}