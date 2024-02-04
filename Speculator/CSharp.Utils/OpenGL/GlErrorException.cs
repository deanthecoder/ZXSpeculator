using Silk.NET.OpenGL;

namespace CSharp.Utils.OpenGL;

public class GlErrorException : Exception
{
    public GlErrorException(string message) : base(message) { }

    public static void ThrowIfError(GL gl)
    {
        var error = gl.GetError();
        if (error != GLEnum.NoError)
        {
            throw new GlErrorException(error.ToString());
        }
    }
}