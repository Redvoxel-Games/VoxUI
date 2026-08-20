using System.Reflection;
using Silk.NET.Core;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VoxUI.Math;
using VoxUI.Rendering;

namespace VoxUI.Core;

public static class Images
{
    private static GL Gl => Renderer.Gl;

    public static unsafe uint LoadImageFromStream(Stream stream)
    {
        using Image<Rgba32> image = Image.Load<Rgba32>(stream);
        
        byte[] pixels = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(pixels);

        var rawImg = new RawImage(image.Width, image.Height, pixels);
        
        var texture = Gl.GenTexture();
        Gl.ActiveTexture(TextureUnit.Texture0);
        Gl.BindTexture(TextureTarget.Texture2D, texture);
        
        // Define a pointer to the image data
        fixed (byte* ptr = pixels)
            // Here we use "result.Width" and "result.Height" to tell OpenGL about how big our texture is.
            Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)rawImg.Width,
                (uint)rawImg.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
        
        Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)GLEnum.Linear);

        Gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)GLEnum.Linear);
        
        Gl.BindTexture(TextureTarget.Texture2D, 0);
        
        return texture;
    }
    
    public static uint LoadImageFromResourceName(string resourceName, Assembly assembly)
    {
        using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException("Failed to get image stream!");
        return LoadImageFromStream(stream);
    }

    public static uint Checkmark;
    public static uint Xmark;
    public static uint RightArrow;
    public static uint DownArrow;

    public static void LoadBuiltIn()
    {
        Checkmark = LoadImageFromResourceName("VoxUI.Assets.checkmark.png", Assembly.GetExecutingAssembly());
        Xmark = LoadImageFromResourceName("VoxUI.Assets.xmark.png", Assembly.GetExecutingAssembly());
        RightArrow = LoadImageFromResourceName("VoxUI.Assets.arrow_right.png", Assembly.GetExecutingAssembly());
        DownArrow = LoadImageFromResourceName("VoxUI.Assets.arrow_down.png", Assembly.GetExecutingAssembly());
    }
}

// ReSharper disable once InconsistentNaming
public static partial class VoxUIR
{
    public static void Image(Vector2 size, uint image)
    {
        var pos = GetDrawPositionInScreenSpace();
        Renderer.AddImage(pos, size, image);
        
        _currentDrawY += size.Y + UIStyle.ItemSpacing;
        _currentMaxSizeX = MathF.Max(_currentMaxSizeX, size.X);
        _currentMaxSizeY += size.Y + UIStyle.ItemSpacing;
    }
}