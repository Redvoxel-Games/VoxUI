using System.Reflection;
using Silk.NET.OpenGL;
using StbTrueTypeSharp;
using VoxUI.Math;

namespace VoxUI.Rendering;

public static class FontHandler
{

    public static Font LoadFont(
        GL gl,
        string resourceName,
        float pixelHeight,
        int atlasWidth = 1024,
        int atlasHeight = 1024)
    {

        var assembly = Assembly.GetExecutingAssembly();

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Could not find embedded font resource '{resourceName}'.");

        var fontData = new byte[stream.Length];
        stream.ReadExactly(fontData);

        const int firstCharacter = 32;
        const int characterCount = 95;
        
        var atlas = new byte[atlasWidth * atlasHeight];

        var glyphs = new StbTrueType.stbtt_bakedchar[characterCount];

        bool result = StbTrueType.stbtt_BakeFontBitmap(
            fontData,
            0,
            pixelHeight,
            atlas,
            atlasWidth,
            atlasHeight,
            firstCharacter,
            characterCount,
            glyphs
        );

        if (!result)
        {
            throw new InvalidOperationException(
                $"Failed to rasterize font '{resourceName}'. " +
                $"The atlas may be too small for {characterCount} glyphs.");
        }

        uint texture = gl.GenTexture();

        gl.BindTexture(TextureTarget.Texture2D, texture);

        unsafe
        {
            fixed (byte* pixels = atlas)
            {
                gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.R8,
                    (uint)atlasWidth,
                    (uint)atlasHeight,
                    0,
                    PixelFormat.Red,
                    PixelType.UnsignedByte,
                    pixels
                );
            }
        }

        // Linear filtering gives us antialiased edges when scaling.
        gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Nearest);

        gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Nearest);

        gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)GLEnum.ClampToEdge);

        gl.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)GLEnum.ClampToEdge);

        gl.BindTexture(TextureTarget.Texture2D, 0);

        return new Font
        {
            Texture = texture,

            AtlasWidth = atlasWidth,
            AtlasHeight = atlasHeight,

            PixelHeight = pixelHeight,

            FirstCharacter = firstCharacter,
            CharacterCount = characterCount,

            Glyphs = glyphs
        };
    }
}

public class FontSet(string resourceName)
{
    public Dictionary<int, Font> Fonts { get; } = [];
    public readonly string ResourceName = resourceName;

    public Font GetFontForSize(int size)
    {
        if (Fonts.TryGetValue(size, out var font))
        {
            return font;
        }
        
        font = FontHandler.LoadFont(Renderer.Gl, ResourceName, size);
        Fonts.Add(size, font);
        return font;
    }
}

public sealed class Font
{
    public uint Texture { get; init; }

    public int AtlasWidth { get; init; }
    public int AtlasHeight { get; init; }

    public float PixelHeight { get; init; }

    public int FirstCharacter { get; init; }
    public int CharacterCount { get; init; }

    public required StbTrueType.stbtt_bakedchar[] Glyphs { get; init; }

    public StbTrueType.stbtt_bakedchar GetGlyph(char c)
    {
        return Glyphs[c - FirstCharacter];
    }

    public Vector2 GetTextSize(string text)
    {
        float xSize = 0;
        foreach (var character in text)
        {
            var glyph = GetGlyph(character);

            if (character == ' ')
            {
                xSize += PixelHeight / 5f;
            }
            else
            {
                xSize += glyph.x1 - glyph.x0 + 1;
            }
        }
        return new Vector2(xSize, PixelHeight);
    }
}