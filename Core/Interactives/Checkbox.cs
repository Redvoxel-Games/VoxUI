using System.Runtime.CompilerServices;
using Silk.NET.Input;
using VoxUI.Math;
using VoxUI.Rendering;

namespace VoxUI.Core;

// ReSharper disable once InconsistentNaming
public static unsafe partial class VoxUIR
{
    /// <summary>
    /// Creates a checkbox.
    /// </summary>
    /// <param name="label">Item label</param>
    /// <param name="value">Current value</param>
    /// <param name="checkboxFlags">Flags</param>
    /// <returns>True on the frame the checkbox is changed, otherwise false.</returns>
    public static bool Checkbox(string label, ref bool value, CheckboxFlags checkboxFlags = CheckboxFlags.None)
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Begin() before attempting to draw.");
        
        var size = new Vector2(UIStyle.TextSize, UIStyle.TextSize);
        var textSize = RegularFont.GetTextSize(label);

        if (_nextItemMinWidth != null)
        {
            size = new Vector2(MathF.Max(size.X, (float)_nextItemMinWidth), size.Y);
        }
        if (_nextItemMaxWidth != null)
        {
            size = new Vector2(MathF.Min(size.X, (float)_nextItemMaxWidth), size.Y);
        }
        if (_nextItemMinHeight != null)
        {
            size = new Vector2(size.X, MathF.Max(size.Y, (float)_nextItemMinHeight));
        }
        if (_nextItemMaxHeight != null)
        {
            size = new Vector2(size.X, MathF.Min(size.Y, (float)_nextItemMaxHeight));
        }

        var inactive = checkboxFlags.HasFlag(CheckboxFlags.Inactive);
        
        var color = UIStyle.ButtonColor.Lerp(Color4.Black,0.33f);
        var mousePos = GetMousePositionInWindowSpace();
        
        var clicked = false;
        
        if (value)
        {
            color = UIStyle.ButtonColor;
        }

        if (!inactive
            && mousePos.X >= _currentDrawX && mousePos.X <= _currentDrawX + size.X 
            && mousePos.Y >= _currentDrawY + size.Y && mousePos.Y <= _currentDrawY + size.Y*2
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

        var pos = GetDrawPositionInScreenSpace();
        
        var radii = checkboxFlags.HasFlag(CheckboxFlags.Circular) ? size.X/2 : UIStyle.ButtonCornerRadius;
        var cornerRadii = CornerRadii.All(radii);
        
        float sizeOffset = 0;
        if (checkboxFlags.HasFlag(CheckboxFlags.Radio))
        {
            sizeOffset = 2;
        }
        var sizeOffVec = new Vector2(sizeOffset, sizeOffset);
        
        Renderer.AddRect(pos + sizeOffVec, size - sizeOffVec*2, color, cornerRadii);
        Renderer.AddText(pos + new Vector2(size.X+2), label, UIStyle.TextSize, UIStyle.ButtonTextColor, parentPos:CurrentWindow.Position, parentSize:CurrentWindow.Size);

        if (checkboxFlags.HasFlag(CheckboxFlags.Radio) && value)
        {
            float fillPadding = 2;
            var fillOffset = new Vector2(fillPadding, fillPadding) + sizeOffVec;
            Renderer.AddRect(pos + fillOffset, size - fillOffset*2, UIStyle.RadioButtonFillColor, cornerRadii, zIndex:1);
        }
        else if (value)
        {
            float fillPadding = 2;
            var fillOffset = new Vector2(fillPadding, fillPadding) + sizeOffVec;
            Renderer.AddImage(pos + fillOffset, size - fillOffset*2, Images.Checkmark, 1);
        }

        if (inactive)
        {
            Renderer.AddRect(pos + sizeOffVec, size - sizeOffVec/2, new Color4(0,0,0,0.3f), cornerRadii, zIndex:2);
        }
        
        Advance(0, size.Y + UIStyle.ItemSpacing, size.X + textSize.X, size.Y);
        ReturnToBaseline();

        if (clicked && !checkboxFlags.HasFlag(CheckboxFlags.NoChange))
        {
            value = !value;
        }

        return clicked;
    }

    private static bool _insideRadioButton;
    private static bool* _selectedRadio = null;
    private static readonly List<BoolPtr> RadioValues = [];

    private readonly struct BoolPtr(bool* ptr)
    {
        public bool* Value => ptr;
    }

    /// <summary>
    /// Begins a radio button set.
    /// </summary>
    public static void BeginRadio()
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Begin() before BeginRadio()");
        _insideRadioButton = true;
        
        RadioValues.Clear();
        _selectedRadio = null;
    }

    /// <summary>
    /// Ends current radio button set.
    /// </summary>
    /// <returns>Whether the values have changed this frame</returns>
    public static bool EndRadio()
    {
        if (!_insideRadioButton)
            throw new Exception("You must call BeginRadio() first");
        
        _insideRadioButton = false;

        if (_selectedRadio != null)
        {
            foreach (var ptr in RadioValues)
                *ptr.Value = ptr.Value == _selectedRadio;
            
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Creates a radio button.
    /// </summary>
    /// <param name="label">Item label</param>
    /// <param name="value">Current value</param>
    public static void RadioButton(string label, ref bool value)
    {
        if (!_insideRadioButton)
            throw new Exception("You must call BeginRadio() first");
        
        bool* ptr = (bool*)Unsafe.AsPointer(ref value);
        
        RadioValues.Add(new BoolPtr(ptr));

        var clicked = Checkbox(label, ref value, CheckboxFlags.Radio);

        if (clicked)
            _selectedRadio = ptr;
    }
}