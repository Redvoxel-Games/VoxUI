using VoxUI.Math;
using VoxUI.Rendering;

namespace VoxUI.Core;

// ReSharper disable once InconsistentNaming
public static partial class VoxUIR
{
    public static bool Combo(string itemName, string label, string[] options, ref int selectedItem)
    {
        var size = new Vector2(GetInputWidth(), UIStyle.TextSize);
        size = GetNextItemSize(size);
        
        var labelSize = RegularFont.GetTextSize(label);

        var arrowSize = new Vector2(size.Y, size.Y) - 6;

        var drawPos = GetDrawPositionInScreenSpace();

        var allCorner = CornerRadii.All(UIStyle.ButtonCornerRadius);
        var leftCorner = CornerRadii.LeftRight(UIStyle.ButtonCornerRadius, 0);

        uint arrowImage = Images.RightArrow;
        
        Renderer.AddRect(drawPos, size, UIStyle.ButtonColor.Lerp(Color4.Black, 0.1f), allCorner);
        Renderer.AddRect(drawPos, new Vector2(size.Y, size.Y), UIStyle.ButtonColor.Lerp(Color4.Black, 0.5f), leftCorner, zIndex:1);
        Renderer.AddImage(drawPos+3, arrowSize, arrowImage, zindex:2);
        
        string selectedText = options[selectedItem];
        Renderer.AddText(drawPos + new Vector2(size.Y + 2), selectedText, UIStyle.TextSize, UIStyle.TextColor, parentPos:drawPos, parentSize:size);
        
        Renderer.AddText(drawPos + new Vector2(size.X + 2), label, UIStyle.TextSize, UIStyle.TextColor);
        
        Advance(0, size.Y + UIStyle.ItemSpacing, size.X + labelSize.X + 2, size.Y);
        ReturnToBaseline();

        return false;
    }
}