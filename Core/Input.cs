using Silk.NET.Input;
using VoxUI.Math;
using VoxUI.Rendering;

namespace VoxUI.Core;

public static class Input
{
    public static IInputContext InputContext => Renderer.InputContext;
    public static IMouse Mouse => InputContext.Mice[0];
    public static IKeyboard Keyboard => InputContext.Keyboards[0];
    
    public static Vector2 MousePosition => new(Mouse.Position.X, Mouse.Position.Y);
    public static Vector2 MouseDelta;
    public static Vector2 MouseWheelDelta;
    
    public static readonly Dictionary<MouseButton, bool> MouseJustPressed = [];
    public static readonly Dictionary<MouseButton, bool> MouseJustReleased = [];
    public static readonly Dictionary<MouseButton, bool> MouseCurrentlyPressed = [];
    public static readonly Dictionary<MouseButton, bool> MouseDoubleClicked = [];
    
    public static readonly Dictionary<Key, bool> KeyJustPressed = [];
    public static readonly Dictionary<Key, bool> KeyJustReleased = [];
    public static readonly Dictionary<Key, (bool, double)> KeyCurrentlyPressed = [];

    // Mapping of Key enum to characters
    public static readonly Dictionary<Key, char> KeyToCharMapping = new()
    {
        // Alphabet
        [Key.A] = 'a', [Key.B] = 'b', [Key.C] = 'c', [Key.D] = 'd', [Key.E] = 'e', [Key.F] = 'f',
        [Key.G] = 'g', [Key.H] = 'h', [Key.I] = 'i', [Key.J] = 'j', [Key.K] = 'k', [Key.L] = 'l',
        [Key.M] = 'm', [Key.N] = 'n', [Key.O] = 'o', [Key.P] = 'p', [Key.Q] = 'q', [Key.R] = 'r',
        [Key.S] = 's', [Key.T] = 't', [Key.U] = 'u', [Key.V] = 'v', [Key.W] = 'w', [Key.X] = 'x',
        [Key.Y] = 'y', /* and */ [Key.Z] = 'z',
        
        // Numbers
        [Key.Number0] = '0', [Key.Number1] = '1', [Key.Number2] = '2', [Key.Number3] = '3', [Key.Number4] = '4',
        [Key.Number5] = '5', [Key.Number6] = '6', [Key.Number7] = '7', [Key.Number8] = '8', [Key.Number9] = '9',
        
        // Special
        [Key.Apostrophe] = '\'', [Key.GraveAccent] = '`', [Key.Slash] = '/', [Key.BackSlash] = '\\', [Key.LeftBracket] = '[', [Key.RightBracket] = ']',
        [Key.Semicolon] = ';', [Key.Comma] = ',', [Key.Period] = '.', [Key.Minus] = '-', /* Should be called "hypen" but whatever */ [Key.Equal] = '=',
        
        [Key.Space] = ' ',
    };

    // Mapping of lowercase to uppercase letters (or resulting characters for shift)
    public static readonly Dictionary<char, char> CapitalMapping = new()
    {
        ['a'] = 'A', ['b'] = 'B', ['c'] = 'C', ['d'] = 'D', ['e'] = 'E', ['f'] = 'F',
        ['g'] = 'G', ['h'] = 'H', ['i'] = 'I', ['j'] = 'J', ['k'] = 'K', ['l'] = 'L',
        ['m'] = 'M', ['n'] = 'N', ['o'] = 'O', ['p'] = 'P', ['q'] = 'Q', ['r'] = 'R',
        ['s'] = 'S', ['t'] = 'T', ['u'] = 'U', ['v'] = 'V', ['w'] = 'W', ['x'] = 'X',
        ['y'] = 'Y', ['z'] = 'Z',
        
        ['0'] = ')', ['1'] = '!', ['2'] = '@', ['3'] = '#', ['4'] = '$',
        ['5'] = '%', ['6'] = '^', ['7'] = '&', ['8'] = '*', ['9'] = '(',
        
        ['\''] = '"', ['`'] = '~', ['/'] = '?', ['\\'] = '|', ['['] = '{', [']'] = '}',
        [';'] = ':', [','] = '<', ['.'] = '>', ['-'] = '_', ['='] = '+'
    };

    public static void Connect()
    {
        Mouse.MouseDown += (_, button) =>
        {
            MouseJustPressed[button] = true;
            MouseCurrentlyPressed[button] = true;
        };
        Mouse.MouseUp += (_, button) =>
        {
            MouseCurrentlyPressed[button] = false;
            MouseJustReleased[button] = true;
        };
        Mouse.DoubleClick += (_, button, _) =>
        {
            MouseDoubleClicked[button] = true;
        };
        Mouse.Scroll += (_, scroll) =>
        {
            var newScroll = new Vector2(scroll.X, scroll.Y);
            MouseWheelDelta = newScroll;
        };

        Keyboard.KeyDown += (_, key, _) =>
        {
            KeyJustPressed[key] = true;
            KeyCurrentlyPressed[key] = (true, 0);
        };
        Keyboard.KeyUp += (_, key, _) =>
        {
            KeyJustReleased[key] = true;
            KeyCurrentlyPressed[key] = (false, 0);
        };
    }

    public static void Update(double deltaTime)
    {
        foreach (var pair in KeyCurrentlyPressed)
        {
            if (pair.Value.Item1)
            {
                KeyCurrentlyPressed[pair.Key] = (true, pair.Value.Item2 + deltaTime);
            }
        }
    }

    public static void Clear()
    {
        MouseJustPressed.Clear();
        MouseJustReleased.Clear();
        MouseDelta = new Vector2();
        MouseWheelDelta = new Vector2();
        
        KeyJustPressed.Clear();
        KeyJustReleased.Clear();
    }
}

// ReSharper disable once InconsistentNaming
public static partial class VoxUIR
{
    public static Vector2 MousePosition => Input.MousePosition;
    public static Vector2 MouseDelta => Input.MouseDelta;

    public static bool IsMouseInsideRectBase(float x1, float y1, float x2, float y2)
    {
        return MousePosition.X >= x1 && MousePosition.X <= x2 && MousePosition.Y >= y1 && MousePosition.Y <= y2;
    }

    public static bool IsMouseInsideRect(float x, float y, float width, float height)
    {
        return IsMouseInsideRectBase(x, y, x+width, y+height);
    }

    public static bool IsMouseInsideRect(Vector2 pos, Vector2 size)
    {
        return IsMouseInsideRect(pos.X, pos.Y, size.X, size.Y);
    }
    
    public static bool IsMouseButtonDown(MouseButton button)
    {
        return Input.MouseCurrentlyPressed.TryGetValue(button, out var b) && b;
    }

    public static bool IsMouseButtonJustPressed(MouseButton button)
    {
        return Input.MouseJustPressed.TryGetValue(button, out var b) && b;
    }

    public static bool IsMouseButtonJustReleased(MouseButton button)
    {
        return Input.MouseJustReleased.TryGetValue(button, out var b) && b;
    }

    public static bool IsMouseButtonDoubleClicked(MouseButton button)
    {
        return Input.MouseDoubleClicked.TryGetValue(button, out var b) && b;
    }


    public static bool IsKeyJustPressed(Key key, bool repeat = false)
    {
        if (Input.KeyJustPressed.TryGetValue(key, out var b))
        {
            return b;
        }
        else if (Input.KeyCurrentlyPressed.TryGetValue(key, out var v))
        {
            if (repeat && v.Item2 > 0.5)
            {
                return v.Item1;
            }
        }

        return false;
    }

    public static Key[] GetKeysJustPressed(bool repeat = false)
    {
        if (repeat)
        {
            List<Key> keys = [];
            foreach (var pair in Input.KeyCurrentlyPressed)
            {
                if ((pair.Value.Item1 && pair.Value.Item2 > 0.5) || (Input.KeyJustPressed.TryGetValue(pair.Key, out var v) && v))
                {
                    keys.Add(pair.Key);
                }
            }
            
            return keys.ToArray();
        }
        else
        {
            return Input.KeyJustPressed.Keys.ToArray();
        }
    }

    public static bool IsKeyJustReleased(Key key)
    {
        return Input.KeyJustReleased.TryGetValue(key, out var b) && b;
    }

    public static bool IsKeyDown(Key key)
    {
        return Input.KeyCurrentlyPressed.TryGetValue(key, out var b) && b.Item1;
    }
}