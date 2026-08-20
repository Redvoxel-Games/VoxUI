using Silk.NET.Input;
using VoxUI.Math;
using VoxUI.Rendering;

namespace VoxUI.Core;

// ReSharper disable once InconsistentNaming
public static partial class VoxUIR
{
    public static Vector2 GetDrawPositionInWindowSpace()
    {
        return new Vector2(_currentDrawX, _currentDrawY);
    }
    
    /// <summary>
    /// Creates a slider input.
    /// </summary>
    /// <param name="itemName">Internal name for item tracking/data storage</param>
    /// <param name="label">Label to display</param>
    /// <param name="value">Current value</param>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <param name="rounding">Value rounding</param>
    /// <param name="sliderFlags">Flags</param>
    /// <returns>True if the slider is being edited, otherwise false</returns>
    public static bool FloatSlider(string itemName, string label, ref float value, float min, float max, float rounding = 0.01f,
        SliderFlags sliderFlags = SliderFlags.None)
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Begin() before attempting to draw.");
        
        var itemId = new ItemId(CurrentWindow.Id, itemName);
        
        var size = new Vector2(GetInputWidth(), UIStyle.TextSize);
        var labelSize = RegularFont.GetTextSize(label);
        
        var sliderSize = new Vector2(8, size.Y - 4);
        var trackSize = size.X - 4 - sliderSize.X;

        var valueOffset = (value - min) / (max - min);
        
        if (!sliderFlags.HasFlag(SliderFlags.DisableKnobClamping))
            valueOffset = Clamp(valueOffset, 0,1);
        
        var sliderPosOffset = new Vector2(trackSize * valueOffset, 0);

        var pos = GetDrawPositionInScreenSpace();
        var mousePos = GetMousePositionInWindowSpace();
        
        var color = UIStyle.ButtonColor.Lerp(Color4.Black, 0.33f);
        var sliderColor = UIStyle.ButtonColor;
        var active = !sliderFlags.HasFlag(SliderFlags.Inactive);
        
        var beingEdited = Equals(_currentInput, itemId);

        if (active
            && mousePos.X >= _currentDrawX && mousePos.X <= _currentDrawX + size.X 
            && mousePos.Y >= _currentDrawY + size.Y && mousePos.Y <= _currentDrawY + size.Y*2
            && IsCurrentWindowHovered()
           )
        {
            if (!IsMouseButtonDown(MouseButton.Left))
            {
                color = color.Lerp(Color4.White, 0.1f);
                sliderColor = sliderColor.Lerp(Color4.White, 0.1f);
            }
            if (IsMouseButtonJustPressed(MouseButton.Left))
            {
                _currentInput = itemId;
                beingEdited = true;
            }
        }

        if (beingEdited && (!IsMouseButtonDown(MouseButton.Left) || !active))
        {
            _currentInput = null;
            beingEdited = false;
        }
        
        if (beingEdited)
        {
            color = color.Lerp(UIStyle.PrimaryColor, 0.1f);
            sliderColor = sliderColor.Lerp(UIStyle.PrimaryColor, 0.1f);
            
            var posInWindow = GetDrawPositionInWindowSpace();

            var mouseX = mousePos.X - (posInWindow.X + 2 + (sliderSize.X/2));
            var mouseProg = Clamp(mouseX / trackSize, 0, 1);
            
            value = mouseProg * (max - min) + min;
            value = Clamp(MathF.Round(value/rounding) * rounding, min, max);
        }

        var sliderScreenPos = pos + new Vector2(2, 2) + sliderPosOffset; 
        
        Renderer.AddRect(pos, size, color, CornerRadii.All(UIStyle.ButtonCornerRadius));
        Renderer.AddRect(sliderScreenPos, sliderSize, sliderColor, CornerRadii.All(UIStyle.ButtonCornerRadius));
        Renderer.AddText(pos + new Vector2(2 + size.X), label, UIStyle.TextSize, UIStyle.TextColor, parentPos:CurrentWindow.Position, parentSize:CurrentWindow.Size);

        string valueString = "";
        
        var valueFloor = MathF.Floor(value);
        valueString += valueFloor;
        var valueDecimal = value - valueFloor;
        var decimalString = valueDecimal.ToString();

        if (decimalString.Length > 1)
        {
            decimalString += "00";
            decimalString = decimalString.Substring(2, 3);
        }
        decimalString += RepeatString("0", System.Math.Max(3 - decimalString.Length, 0));
        valueString += "." + decimalString;
        
        var valueTextSize = RegularFont.GetTextSize(valueString);
        
        Renderer.AddText(pos + new Vector2((size.X - valueTextSize.X)/2), valueString, UIStyle.TextSize, UIStyle.ButtonTextColor * new Color4(1,1,1,0.5f), zIndex:1);
        
        Advance(0, size.Y + UIStyle.ItemSpacing, size.X + labelSize.X + 2, size.Y);
        ReturnToBaseline();
        
        return beingEdited;
    }

    public static float Clamp(float x, float min, float max)
    {
        return MathF.Max(MathF.Min(x, max), min);
    }

    private static string RepeatString(string str, int count)
    {
        return string.Concat(Enumerable.Repeat(str, count));
    }
}