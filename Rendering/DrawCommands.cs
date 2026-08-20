using Silk.NET.OpenGL;
using VoxUI.Math;

namespace VoxUI.Rendering;

public class DrawRect(float x, float y, float width, float height, Color4 color, CornerRadii cornerRadii, float lineWidth = -1, int zIndex = 0) : DrawCommand(zIndex)
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public float Width { get; } = width;
    public float Height { get; } = height;
    public Color4 Color { get; } = color;
    public CornerRadii CornerRadii { get; } = cornerRadii;
    public float LineWidth { get; } = lineWidth;

    public override void Draw(GL gl, Vector2 offset)
    {
        var shader = Renderer.Shader;
        var windowSize = Renderer.Window.Size;
        Vector2 size = new Vector2(windowSize.X, windowSize.Y);
        
        shader.Use();
        shader.SetVector2("screenSize", size);
        shader.SetVector2("rectPos", new Vector2(X, Y) + offset);
        shader.SetVector2("rectSize", new Vector2(Width, Height));
        shader.SetVector4("rectColor", Color);
        shader.SetVector4("cornerRadii", CornerRadii);
        shader.SetFloat("rectLineWidth", LineWidth);
        shader.SetVector2("UVMin", new Vector2(0));
        shader.SetVector2("UVMax", new Vector2(1,1));
        
        gl.BindVertexArray(Renderer.QuadVao);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        gl.BindVertexArray(0);
    }
}

public class DrawImage(float x, float y, float width, float height, uint image, int zIndex) : DrawCommand(zIndex)
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public float Width { get; } = width;
    public float Height { get; } = height;
    public uint Image { get; } = image;
    
    public override void Draw(GL gl, Vector2 offset)
    {
        var shader = Renderer.ImageShader;
        var windowSize = Renderer.Window.Size;
        Vector2 wSize = new Vector2(windowSize.X, windowSize.Y);
        
        shader.Use();
        shader.SetVector2("screenSize", wSize);
        shader.SetVector2("rectPos", new Vector2(X, Y) + offset);
        shader.SetVector2("rectSize", new Vector2(Width, Height));
        shader.SetVector2("UVMin", new Vector2(0));
        shader.SetVector2("UVMax", new Vector2(1,1));
        
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, Image);
        shader.SetUint("image", 0);
        
        gl.BindVertexArray(Renderer.QuadVao);
        gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
        gl.BindVertexArray(0);
    }
}

public class DrawText(float x, float y, string text, int size, Color4 color, int zIndex, int parentX, int parentY, uint parentW, uint parentH) : DrawCommand(zIndex)
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public string Text { get; } = text;
    public Color4 Color { get; } = color;
    public int Size { get; } = size;
    
    public int ParentX { get; } = parentX;
    public int ParentY { get; } = parentY;
    public uint ParentW { get; } = parentW;
    public uint ParentH { get; } = parentH;

    public FontSet FontSet = Renderer.OpenSans;

    public override void Draw(GL gl, Vector2 offset)
    {
        var shader = Renderer.TextShader;
        var windowSize = Renderer.Window.Size;
        Vector2 wSize = new Vector2(windowSize.X, windowSize.Y);
        
        var font = FontSet.GetFontForSize(Size);
        
        shader.Use();
        shader.SetVector2("screenSize", wSize);
        shader.SetVector4("textColor", Color);
        
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindTexture(TextureTarget.Texture2D, font.Texture);
        shader.SetUint("fontTexture", 0);
        
        gl.ScissorIndexed(1, ParentX + (int)offset.X, (int)(Renderer.Window.Size.Y - ParentY - ParentH + offset.Y), ParentW, ParentH);
        gl.Enable(EnableCap.ScissorTest, 1);
        
        gl.BindVertexArray(Renderer.QuadVao);

        float charX = X + offset.X;
        float charY = Y + offset.Y + Size - 4;

        foreach (var character in Text)
        {
            var glyph = font.GetGlyph(character);
            
            
            float uMin = glyph.x0 / (float)font.AtlasWidth;
            float vMin = glyph.y0 / (float)font.AtlasHeight;

            float uMax = glyph.x1 / (float)font.AtlasWidth;
            float vMax = glyph.y1 / (float)font.AtlasHeight;
            
            shader.SetVector2("UVMin", new Vector2(uMin, vMin));
            shader.SetVector2("UVMax", new Vector2(uMax, vMax));
            
            float width = glyph.x1 - glyph.x0;
            float height = glyph.y1 - glyph.y0;

            if (character == ' ')
                width = Size / 5f;

            float cX = charX + glyph.xoff;
            float cY = charY + glyph.yoff;
            
            shader.SetVector2("rectPos", new Vector2(cX, cY));
            shader.SetVector2("rectSize", new Vector2(width, height));
            
            gl.DrawArrays(PrimitiveType.Triangles, 0, 6);

            charX += width + 1;
        }
        
        gl.Disable(EnableCap.ScissorTest, 1);
        
        gl.BindVertexArray(0);
        gl.BindTexture(TextureTarget.Texture2D, 0);
    }
}