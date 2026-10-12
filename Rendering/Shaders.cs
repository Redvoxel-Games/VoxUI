using System.Reflection;
using System.Resources;
using System.Text;

namespace VoxUI.Rendering;

public static class Shaders
{
    public static string ReadStreamAsString(Stream stream)
    {
        byte[] data = new byte[stream.Length];
        stream.ReadExactly(data, 0, data.Length);
        return Encoding.UTF8.GetString(data, 0, data.Length);
    }
    
    public static void Init()
    {
        // Load RectVertex.vert
        var rectVertexStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VoxUI.Assets.Shaders.RectVertex.vert");
        if (rectVertexStream == null)
            throw new MissingManifestResourceException("Rectangle vertex shader is missing or could not be loaded!");
        RectVertex = ReadStreamAsString(rectVertexStream);
        
        // Load RectFragment.frag
        var rectFragmentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VoxUI.Assets.Shaders.RectFragment.frag");
        if (rectFragmentStream == null)
            throw new MissingManifestResourceException("Rectangle fragment shader is missing or could not be loaded!");
        RectFragment = ReadStreamAsString(rectFragmentStream);
        
        // Load TextFragment.frag
        var textFragmentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VoxUI.Assets.Shaders.TextFragment.frag");
        if (textFragmentStream == null)
            throw new MissingManifestResourceException("Text fragment shader is missing or could not be loaded!");
        TextFragment = ReadStreamAsString(textFragmentStream);
        
        // Load ImageFragment.frag
        var imageFragmentStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VoxUI.Assets.Shaders.ImageFragment.frag");
        if (imageFragmentStream == null)
            throw new MissingManifestResourceException("Image fragment shader is missing or could not be loaded!");
        ImageFragment = ReadStreamAsString(imageFragmentStream);
    }
    
    public static string RectVertex;
    public static string RectFragment;
    public static string TextFragment;
    public static string ImageFragment;
}