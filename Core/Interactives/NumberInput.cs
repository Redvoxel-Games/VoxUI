namespace VoxUI.Core;

// ReSharper disable once InconsistentNaming
public static partial class VoxUIR
{
    public static bool FloatInput(string itemName, string label, ref float value, float? min = null, float? max = null)
    {
        // This will have to be replaced when we add dragging over these and when we want to do FloatInput2 and 3
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
        
        var changed = TextInput(itemName + "_TEXT", label, ref valueString, textInputFlags:TextInputFlags.ExitReturnsTrue | TextInputFlags.Centered);

        if (changed)
        {
            var flt = float.Parse(valueString);
            
            if (min.HasValue && flt < min.Value)
                flt = min.Value;
            if (max.HasValue && flt > max.Value)
                flt = max.Value;
            
            value = flt;
        }
        
        return changed;
    }
}