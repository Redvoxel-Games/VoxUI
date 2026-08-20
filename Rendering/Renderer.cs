using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using VoxUI.Core;
using VoxUI.Math;
using Vector2 = VoxUI.Math.Vector2;

namespace VoxUI.Rendering;

public abstract class DrawCommand(int zIndex)
{
    public uint FocusOrder = VoxUIR.CurrentWindow != null ? VoxUIR.CurrentWindow.FocusOrder : 0;
    public readonly int ZIndex = zIndex;
    public bool NoClipping = false;
    public abstract void Draw(GL gl, Vector2 offset);
}

public struct CornerRadii(float topLeft = 0, float topRight = 0, float bottomLeft = 0, float bottomRight = 0)
{
    public float TL = topLeft;
    public float TR = topRight;
    public float BL = bottomLeft;
    public float BR = bottomRight;

    public static CornerRadii All(float radius)
    {
        return new CornerRadii(radius,radius,radius,radius);
    }

    public static CornerRadii TopBottom(float topRadius, float bottomRadius)
    {
        return new  CornerRadii(topRadius,topRadius,bottomRadius,bottomRadius);
    }

    public static CornerRadii LeftRight(float leftRadius, float rightRadius)
    {
        return new  CornerRadii(leftRadius,rightRadius,leftRadius,rightRadius);
    }
    
    public static implicit operator Vector4(CornerRadii radii)
    {
        return new Vector4(radii.TL, radii.TR, radii.BL, radii.BR);
}
    }
public struct Color4(float r = 0, float g = 0, float b = 0, float a = 1)
{
    public float R = r;
    public float G = g;
    public float B = b;
    public float A = a;

    public static implicit operator Vector4(Color4 color)
    {
        return new Vector4(color.R, color.G, color.B, color.A);
    }

    public Color4 Lerp(Color4 target, float t)
    {
        return new Color4(
            R + (target.R - R) * t,
            G + (target.G - G) * t,
            B + (target.B - B) * t,
            A + (target.A - A) * t
        );
    }

    public static Color4 operator *(Color4 a, Color4 b)
    {
        return new Color4(a.R*b.R, a.G*b.G, a.B*b.B, a.A*b.A);
    }

    public static Color4 White => new(1, 1, 1);
    public static Color4 Black => new();
    public static Color4 Red => new(1);
    public static Color4 Green => new(0,0.5f);
    public static Color4 Lime => new(0,1);
    public static Color4 Blue => new(0, 0, 1);

    public static Color4 FromRgb(int r, int g, int b)
    {
        return new Color4(r / 255f, g / 255f, b / 255f);
    }
}
public static class Renderer
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    
    public static GL Gl;
    public static readonly List<DrawCommand> DrawList = [];
    public static readonly List<DrawCommand> OverlayList = [];
    
    public static IWindow Window;
    public static IInputContext InputContext;

    public static RootDock? RootDock;
    
    public static Shader Shader;
    public static Shader TextShader;
    public static Shader ImageShader;
    public static double DeltaTime;
    
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static void Update(double deltaTime)
    {
        DeltaTime = deltaTime;
        Input.Update(deltaTime);
    }
    
    public static void DoDraw()
    {
        Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        Gl.Clear(ClearBufferMask.ColorBufferBit);
        
        Gl.Enable(EnableCap.Blend);
        Gl.BlendFunc(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha
        );
        
        RootDock?.Handle();

        Dictionary<uint, List<DrawCommand>> focusOrdersDocked = [];
        Dictionary<uint, List<DrawCommand>> focusOrdersFree = [];
        
        foreach (var command in DrawList)
        {
            var window = WindowDefinition.GetWindowFromFocus(command.FocusOrder);
            if (window == null) continue;
            
            var dict = window.DockOverride ? focusOrdersDocked : focusOrdersFree;
            
            if (dict.ContainsKey(command.FocusOrder))
            {
                dict[command.FocusOrder].Add(command);
            }
            else
            {
                dict.Add(command.FocusOrder, [command]);
            }
        }
        
        Gl.Enable(EnableCap.ScissorTest);

        foreach (var order in focusOrdersDocked.OrderByDescending(x => x.Key))
        {
            var window = WindowDefinition.GetWindowFromFocus(order.Key);
            if (window == null) continue;
            
            Gl.ScissorIndexed(0, (int)window.Position.X, (int)(Window.Size.Y - window.Position.Y - window.Size.Y), (uint)window.Size.X, (uint)window.Size.Y);
            Gl.Enable(EnableCap.ScissorTest, 0);
            foreach (var draw in order.Value.OrderBy(x => x.ZIndex))
            {
                if (draw.NoClipping)
                {
                    Gl.Disable(EnableCap.ScissorTest, 0);
                }
                draw.Draw(Gl, new());
                if (draw.NoClipping)
                {
                    Gl.Enable(EnableCap.ScissorTest, 0);
                }
            }
        }
        
        foreach (var order in focusOrdersFree.OrderByDescending(x => x.Key))
        {
            var window = WindowDefinition.GetWindowFromFocus(order.Key);
            if (window == null) continue;
            
            Gl.ScissorIndexed(0, (int)window.Position.X, (int)(Window.Size.Y - window.Position.Y - window.Size.Y), (uint)window.Size.X, (uint)window.Size.Y);
            Gl.Enable(EnableCap.ScissorTest, 0);
            foreach (var draw in order.Value.OrderBy(x => x.ZIndex))
            {
                if (draw.NoClipping)
                {
                    Gl.Disable(EnableCap.ScissorTest, 0);
                }
                draw.Draw(Gl, new());
                if (draw.NoClipping)
                {
                    Gl.Enable(EnableCap.ScissorTest, 0);
                }
            }
        }
        DrawList.Clear();
        
        Gl.Disable(EnableCap.ScissorTest);

        foreach (var draw in OverlayList.OrderBy(x => x.ZIndex))
        {
            draw.Draw(Gl, new());
        }
        OverlayList.Clear();
        
        // Look for window to focus
        if (VoxUIR.IsMouseButtonJustPressed(MouseButton.Left))
            GetHoveredWindow()?.Focus();

        if (!VoxUIR.IsMouseButtonDown(MouseButton.Left))
            WindowDefinition.WindowToDock = null;
        
        Input.Clear();
    }

    public static WindowDefinition? GetHoveredWindow()
    {
        var mousePos = Input.MousePosition;
        
        
        List<WindowDefinition> freeWindows = [];
        List<WindowDefinition> dockedWindows = [];

        foreach (var windowDef in WindowDefinition.Windows.Values)
        {
            if (windowDef.DockOverride)
            {
                dockedWindows.Add(windowDef);
            }
            else
            {
                freeWindows.Add(windowDef);
            }
        }
        
        foreach (var windowDef in freeWindows.OrderBy(x => x.FocusOrder))
        {
            var mouseInside = mousePos.X >= windowDef.Position.X
                              && mousePos.X <= windowDef.Position.X + windowDef.Size.X 
                              && mousePos.Y >= windowDef.Position.Y
                              && mousePos.Y <= windowDef.Position.Y + windowDef.Size.Y;

            if (mouseInside && windowDef.IsBeingDrawn)
            {
                return windowDef;
            }
        }
        foreach (var windowDef in dockedWindows.OrderBy(x => x.FocusOrder))
        {
            var mouseInside = mousePos.X >= windowDef.Position.X
                              && mousePos.X <= windowDef.Position.X + windowDef.Size.X 
                              && mousePos.Y >= windowDef.Position.Y
                              && mousePos.Y <= windowDef.Position.Y + windowDef.Size.Y;

            if (mouseInside && windowDef.IsBeingDrawn)
            {
                return windowDef;
            }
        }

        return null;
    }

    public static void AddRect(float x, float y, float width, float height, Color4? color = null, CornerRadii? cornerRadii = null, float lineWidth = -1, int zIndex = 0, bool overlay = false, uint? focusOrder = null, bool noClipping = false)
    {
        var rect = new DrawRect(
            x, y, width, height,
            color ?? Color4.White,
            cornerRadii ?? new CornerRadii(),
            lineWidth,
            zIndex
        )
        {
            NoClipping = noClipping
        };
        if (focusOrder.HasValue)
            rect.FocusOrder = focusOrder.Value;
        (overlay ? OverlayList : DrawList).Add(rect);
    }
    public static void AddRect(Vector2 position, Vector2 size, Color4? color = null, CornerRadii? cornerRadii = null,
        float lineWidth = -1, int zIndex = 0, bool overlay = false, uint? focusOrder = null, bool noClipping = false)
    {
        AddRect(position.X, position.Y, size.X, size.Y, color, cornerRadii, lineWidth, zIndex, overlay, focusOrder, noClipping);
    }

    public static void AddText(float x, float y, string text, int size, Color4? color = null, int zIndex = 0, float px=0, float py=0, float pw=99999, float ph=99999, bool noClipping = false)
    {
        DrawList.Add(new DrawText(x, y, text, size, color ?? Color4.White, zIndex, (int)px, (int)py, (uint)pw, (uint)ph) {NoClipping = noClipping});
    }
    public static void AddText(Vector2 position, string text, int size, Color4? color = null, int zIndex = 0, Vector2? parentPos = null, Vector2? parentSize = null, bool noClipping = false)
    {
        var pPos = parentPos ?? position;
        var pSize = parentSize ?? new Vector2(99999, 99999);
        DrawList.Add(new DrawText(position.X, position.Y, text, size, color ?? Color4.White, zIndex, (int)pPos.X, (int)pPos.Y, (uint)pSize.X, (uint)pSize.Y) {NoClipping = noClipping});
    }

    public static void AddImage(float x, float y, float width, float height, uint image, int zIndex = 0)
    {
        DrawList.Add(new DrawImage(x, y, width, height, image, zIndex));
    }

    public static void AddImage(Vector2 position, Vector2 size, uint image, int zindex = 0)
    {
        DrawList.Add(new DrawImage(position.X, position.Y, size.X, size.Y, image, zindex));
    }

    public static uint QuadVao;
    public static uint QuadVbo;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static FontSet OpenSans;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static void Init()
    {
        Input.Connect();
        Images.LoadBuiltIn();
        
        OpenSans = new FontSet("VoxUI.Open_Sans.static.OpenSans-Regular.ttf");
        
        Shader = new Shader(Shaders.RectVertex, Shaders.RectFragment);
        TextShader = new Shader(Shaders.RectVertex, Shaders.TextFragment);
        ImageShader = new Shader(Shaders.RectVertex, Shaders.ImageFragment);
        
        QuadVao = Gl.GenVertexArray();
        QuadVbo = Gl.GenBuffer();
        
        Gl.BindVertexArray(QuadVao);
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, QuadVbo);
        
        Gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)_quadVerts.Length * sizeof(float), _quadVerts, BufferUsageARB.StaticDraw);
        
        const uint vertCoordLoc = 0;
        Gl.EnableVertexAttribArray(vertCoordLoc);
        Gl.VertexAttribPointer(vertCoordLoc, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        
        Gl.BindVertexArray(0);
        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }
    
    private static float[] _quadVerts = new[]
    {
        0f,0f,0f,
        1f,0f,0f,
        1f,1f,0f,
        
        0f,1f,0f,
        0f,0f,0f,
        1f,1f,0f
    };
}

public class Shader
{
    uint _handle;

    private GL Gl => Renderer.Gl;

    public Shader(string vertexContent, string fragmentContent)
    {
        uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
        Gl.ShaderSource(vertexShader, vertexContent);

        uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
        Gl.ShaderSource(fragmentShader, fragmentContent);
        
        Gl.CompileShader(vertexShader);

        Gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vSuccess);
        if (vSuccess == 0)
        {
            string infoLog = Gl.GetShaderInfoLog(vertexShader);
            Console.WriteLine(infoLog);
        }

        Gl.CompileShader(fragmentShader);

        Gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fSuccess);
        if (fSuccess == 0)
        {
            string infoLog = Gl.GetShaderInfoLog(fragmentShader);
            Console.WriteLine(infoLog);
        }

        _handle = Gl.CreateProgram();
        
        Gl.AttachShader(_handle, vertexShader);
        Gl.AttachShader(_handle, fragmentShader);

        Gl.LinkProgram(_handle);

        Gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int success);
        if (success == 0)
        {
            string infoLog = Gl.GetProgramInfoLog(_handle);
            Console.WriteLine(infoLog);
        }
        
        Gl.DetachShader(_handle, vertexShader);
        Gl.DetachShader(_handle, fragmentShader);
        Gl.DeleteShader(fragmentShader);
        Gl.DeleteShader(vertexShader);
    }

    public void Use()
    {
        Renderer.Gl.UseProgram(_handle);
    }

    public void SetMatrix4(string name, Matrix4X4<float> matrix)
    {
        int location = Gl.GetUniformLocation(_handle, name);
        unsafe { Gl.UniformMatrix4(location, 1, false, (float*)&matrix); }
    }

    public void SetFloat(string name, float flt)
    {
        int location = Gl.GetUniformLocation(_handle, name);
        Gl.Uniform1(location, flt);
    }

    public void SetVector3(string name, Vector3 vec)
    {
        int location = Gl.GetUniformLocation(_handle, name);
        Gl.Uniform3(location, vec);
    }

    public void SetVector2(string name, Vector2 vec)
    {
        int location = Gl.GetUniformLocation(_handle, name);
        Gl.Uniform2(location, vec);
    }
    
    public void SetVector4(string name, Vector4 vec)
    {
        int location = Gl.GetUniformLocation(_handle, name);
        Gl.Uniform4(location, vec);
    }

    public void SetInt(string name, int val)
    {
        int location = Gl.GetUniformLocation(_handle, name);
        Gl.Uniform1(location, val);
    }

    public void SetUint(string name, uint val)
    {
        int location = Gl.GetUniformLocation(_handle, name);
        Gl.Uniform1(location, val);
    }
    
    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            Gl.DeleteProgram(_handle);

            _disposedValue = true;
        }
    }

    ~Shader()
    {
        if (_disposedValue == false)
        {
            Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
        }
    }


    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}