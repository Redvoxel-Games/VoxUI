using VoxUI.Math;

namespace VoxUI.Core;

public static partial class VoxUIR
{
    public static List<WindowDefinition> WindowChildList = [];

    /// <summary>
    /// Create a child window inside the current one.
    /// </summary>
    /// <param name="dimensions">Dimensions of the child</param>
    /// <param name="childName">Unique name for the child, not displayed.</param>
    /// <remarks>Percent means 0 to 1, taken from the respective axes' width.</remarks>
    public static void BeginChild(ChildDim dimensions, string childName)
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Begin() before BeginChild()");
        
        WindowChildList.Add(CurrentWindow);

        var parentWindow = CurrentWindow;

        parentWindow.StoredDrawX = _currentDrawX;
        parentWindow.StoredDrawY = _currentDrawY;
        parentWindow.StoredMaxSizeX = _currentMaxSizeX;
        parentWindow.StoredMaxSizeY = _currentMaxSizeY;
        parentWindow.StoredPrevItemX = _previousItemX;
        parentWindow.StoredPrevItemY = _previousItemY;
        parentWindow.StoredPrevItemWidth = _previousItemWidth;
        parentWindow.StoredPrevItemHeight = _previousItemHeight;

        _currentDrawX = 0;
        _currentDrawY = 0;
        _currentMaxSizeX = 0;
        _currentMaxSizeY = 0;
        
        _previousItemX = 0;
        _previousItemY = 0;
        _previousItemWidth = 0;
        _previousItemHeight = 0;
        
        CurrentWindow = WindowDefinition.GetWindowDefinition(parentWindow.WindowName + "__CHILD_" + childName, "Child of '" + parentWindow.DisplayName + "'");
        CurrentWindow.WindowFlags |= WindowFlags.NoHeader;
        CurrentWindow.WindowFlags |= WindowFlags.IsChild;
        CurrentWindow.FocusOrder = parentWindow.FocusOrder - 1;

        var px = dimensions.percentX;
        var py = dimensions.percentY;
        var pw = dimensions.percentWidth;
        var ph = dimensions.percentHeight;
        
        var ox = dimensions.offsetX;
        var oy = dimensions.offsetY;
        var ow = dimensions.offsetWidth;
        var oh = dimensions.offsetHeight;
        
        Vector2 position = parentWindow.Size * new Vector2(px, py) + new Vector2(ox, oy);
        Vector2 size = parentWindow.Size * new Vector2(pw, ph) + new Vector2(ow, oh);
        
        CurrentWindow.Position = parentWindow.Position + position;
        CurrentWindow.Size = size;
        CurrentWindow.IsBeingDrawn = true;
    }

    /// <summary>
    /// Ends current window child
    /// </summary>
    public static void EndChild()
    {
        if (CurrentWindow == null)
            throw new Exception("You must call BeginChild() before EndChild()");

        CurrentWindow.ContentSize = new Vector2(_currentMaxSizeX + UIStyle.WindowPadding*2, _currentMaxSizeY + UIStyle.WindowPadding*2);
        
        _handleScroll();
        
        var childWindow = CurrentWindow;
        childWindow.IsBeingDrawn = false;
        
        CurrentWindow = WindowChildList.Last();
        WindowChildList.RemoveAt(WindowChildList.Count - 1);
        
        _currentDrawX = childWindow.Position.X;
        _currentDrawY = CurrentWindow.StoredDrawY;
        _currentMaxSizeX = CurrentWindow.StoredMaxSizeX;
        _currentMaxSizeY = CurrentWindow.StoredMaxSizeY;
        
        _previousItemX = childWindow.Position.X - CurrentWindow.Position.X;
        _previousItemY = childWindow.Position.Y - CurrentWindow.Position.Y;
        _previousItemWidth = childWindow.Size.X;
        _previousItemHeight = childWindow.Size.Y;
    }

    public static WindowDefinition? GetRootWindow()
    {
        if (CurrentWindow == null)
            return null;

        if (WindowChildList.Count > 0)
            return WindowChildList[0];

        return CurrentWindow;
    }
}