using Silk.NET.Input;
using VoxUI.Math;
using VoxUI.Rendering;

namespace VoxUI.Core;

// ReSharper disable once InconsistentNaming
public static partial class VoxUIR
{
    private static float? _nextItemMinWidth;
    private static float? _nextItemMaxWidth;
    private static float? _nextItemMinHeight;
    private static float? _nextItemMaxHeight;

    public static void SetNextItemMinWidth(float minWidth)
    {
        _nextItemMinWidth = minWidth;
    }
    public static void SetNextItemMaxWidth(float maxWidth)
    {
        _nextItemMaxWidth = maxWidth;
    }

    public static void SetNextItemMinHeight(float minHeight)
    {
        _nextItemMinHeight = minHeight;
    }
    public static void SetNextItemMaxHeight(float maxHeight)
    {
        _nextItemMaxHeight = maxHeight;
    }

    public static void SetNextItemWidth(float width)
    {
        _nextItemMinWidth = width;
        _nextItemMaxWidth = width;
    }

    private static Vector2 GetNextItemSize(Vector2 size)
    {
        if (_nextItemMinWidth != null)
        {
            size = new Vector2(MathF.Max(size.X, (float)_nextItemMinWidth), size.Y);
            _nextItemMinWidth = null;
        }
        if (_nextItemMaxWidth != null)
        {
            size = new Vector2(MathF.Min(size.X, (float)_nextItemMaxWidth), size.Y);
            _nextItemMaxWidth = null;
        }
        if (_nextItemMinHeight != null)
        {
            size = new Vector2(size.X, MathF.Max(size.Y, (float)_nextItemMinHeight));
            _nextItemMinHeight = null;
        }
        if (_nextItemMaxHeight != null)
        {
            size = new Vector2(size.X, MathF.Min(size.Y, (float)_nextItemMaxHeight));
            _nextItemMaxHeight = null;
        }

        return size;
    }
    
    /// <summary>
    /// Creates a button.
    /// </summary>
    /// <param name="text">Text to display</param>
    /// <param name="buttonFlags">Flags</param>
    /// <returns>True when clicked</returns>
    public static bool Button(string text, ButtonFlags buttonFlags = ButtonFlags.None)
    {
        // Get the size of the text
        var size = RegularFont.GetTextSize(text);
        size.X += 4;

        // Account for next item size calls
        size = GetNextItemSize(size);

        var inactive = buttonFlags.HasFlag(ButtonFlags.Inactive);
        
        var color = UIStyle.ButtonColor;
        var pos = GetDrawPositionInScreenSpace();
        
        var clicked = false;

        // Handle input
        if (!inactive
            && IsMouseInsideRect(pos, size)
            && IsCurrentWindowHovered()
        )
        {
            if (IsMouseButtonDown(MouseButton.Left))
            {
                color = color.Lerp(Color4.Black, 0.2f);
            }
            else
            {
                color = color.Lerp(Color4.White, 0.1f);
                if (IsMouseButtonJustReleased(MouseButton.Left))
                {
                    clicked = true;
                }
            }
        }
        
        // Render
        Renderer.AddRect(pos, size, color, CornerRadii.All(UIStyle.ButtonCornerRadius));
        Renderer.AddText(pos + new Vector2(2), text, UIStyle.TextSize, UIStyle.ButtonTextColor);

        if (inactive)
        {
            Renderer.AddRect(pos, size, new Color4(0,0,0,0.3f), CornerRadii.All(UIStyle.ButtonCornerRadius), zIndex:1);
        }
        
        // Move the draw cursor
        Advance(0, size.Y + UIStyle.ItemSpacing, size.X, size.Y);
        
        // Return to X=0 in the case this was done after a SameLine() or VerticalSeparator() call
        ReturnToBaseline();

        return clicked;
    }
}