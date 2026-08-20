namespace VoxUI.Core;

[Flags]
public enum WindowFlags : uint
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0b0000_0000,
    
    /// <summary>
    /// Stops window from being dragged.
    /// </summary>
    NoDrag =    0b0000_0001,
    NoHeader =  0b0000_0010,
    
    // TODO
    NoResizeX = 0b0000_0100,
    NoResizeY = 0b0000_1000,
    NoResize =  0b0000_1100,
    
    /// <summary>
    /// Automatically resizes on the X axis.
    /// </summary>
    AutoSizeX = 0b0001_0100,
    
    /// <summary>
    /// Automatically resizes on the Y axis.
    /// </summary>
    AutoSizeY = 0b0010_1000,
    
    /// <summary>
    /// Automatically resizes on both axes.
    /// </summary>
    AutoSize = AutoSizeX | AutoSizeY,
    
    /// <summary>
    /// Disables scrolling on the X axis.
    /// </summary>
    NoScrollX = 0b0100_0000,
    
    /// <summary>
    /// Disables scrolling on the Y axis.
    /// </summary>
    NoScrollY = 0b1000_0000,
    
    /// <summary>
    /// Disables scrolling on both axes.
    /// </summary>
    NoScroll = NoScrollX | NoScrollY,
}

[Flags]
public enum ButtonFlags : uint
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0b0000_0000,
    
    /// <summary>
    /// Marks as inactive, disallowing input.
    /// </summary>
    Inactive = 0b0000_0001,
}

[Flags]
public enum CheckboxFlags : uint
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0b0000_0000,
    
    /// <summary>
    /// Marks as inactive, disallowing input.
    /// </summary>
    Inactive = 0b0000_0001,
    
    /// <summary>
    /// Stops value from changing, but Checkbox() will still return true on click.
    /// </summary>
    NoChange = 0b0000_0010,
    
    /// <summary>
    /// Makes the checkbox circular.
    /// </summary>
    Circular = 0b0000_0100,
    
    /// <summary>
    /// Combination of <see cref="NoChange"/> and <see cref="Circular"/>
    /// </summary>
    /// <remarks>Also changes overall visual style</remarks>
    Radio = 0b0000_0110,
}

[Flags]
public enum TextInputFlags : uint
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0b0000_0000,
    
    /// <summary>
    /// Marks as inactive, disallowing input.
    /// </summary>
    Inactive = 0b0000_0001,
    
    /// <summary>
    /// Value only updates when the input is exited.
    /// </summary>
    ExitReturnsTrue = 0b0000_0010,
    
    /// <summary>
    /// Centers the text.
    /// </summary>
    Centered = 0b0000_0100,
}

[Flags]
public enum SliderFlags : uint
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0b0000_0000,
    
    /// <summary>
    /// Marks as inactive, disallowing input.
    /// </summary>
    Inactive = 0b0000_0001,
    
    /// <summary>
    /// Disables visual knob clamping, producing funny results when the number goes out of range.
    /// </summary>
    DisableKnobClamping = 0b0000_0010,
}

[Flags]
public enum TreeNodeFlags : uint
{
    /// <summary>
    /// None.
    /// </summary>
    None = 0b0000_0000,
    
    /// <summary>
    /// Can't be opened, Will return false.
    /// </summary>
    Leaf = 0b0000_0001,
    
    /// <summary>
    /// Can't be interacted with.
    /// </summary>
    Inactive = 0b0000_0010,
    
    /// <summary>
    /// Opens by default.
    /// </summary>
    DefaultOpen = 0b0000_0100,
}