using VoxUI.Rendering;

namespace VoxUI.Core;

// ReSharper disable once InconsistentNaming
public static class UIStyle
{
    public static float WindowPadding = 4;
    public static int WindowHeaderSize = 16;
    public static int TextSize = 16;
    public static float ItemSpacing = 2;
    public static float TreeNodeIndentFactor = 16;
    
    public static Color4 PrimaryColor = Color4.FromRgb(200,0,0);
    public static Color4 BackgroundColor = Color4.FromRgb(30,30,30);
    public static Color4 HeaderTextColor = Color4.FromRgb(255,255,255);
    public static Color4 TextColor = Color4.FromRgb(255,255,255);
    public static Color4 ButtonColor = Color4.FromRgb(75,75,75);
    public static Color4 ButtonTextColor = Color4.FromRgb(255,255,255);
    public static Color4 SeparatorColor = Color4.FromRgb(150,75,75);
    public static Color4 RadioButtonFillColor = Color4.FromRgb(200,0,0);
    public static Color4 ScrollbarColor = new Color4(0, 0, 0, 0.75f);
    public static Color4 TreeNodeAlignmentbarColor = Color4.FromRgb(100,100,100);
    
    public static bool WindowOutline = true;
    public static float WindowCornerRadius = 8;
    public static float ButtonCornerRadius = 4;

    public static float ScrollbarWidth = 8;
}