using VoxUI.Math;

namespace VoxUI.Core;

public class WindowDefinition
{
    public static Dictionary<uint, WindowDefinition> Windows = [];
    private static readonly Dictionary<string, uint> WindowIds = [];
    private static uint _nextId;
    public WindowFlags WindowFlags = WindowFlags.None;

    public static WindowDefinition GetWindowDefinition(string windowName, string displayName)
    {
        uint? id = WindowIds.TryGetValue(windowName, out var i) ? i : null;
        return id != null ? Windows[id.Value] : new WindowDefinition(displayName, windowName);
    }

    public uint FocusOrder;
    public readonly uint Id;
    public string DisplayName;
    public readonly string WindowName;
    public bool ShowCloseButton = false;
    public bool CloseButtonHovered = false;
    
    public bool HeaderHovered = false;
    public bool IsBeingDragged = false;
    public Vector2 DragPositionOffset = new();
    
    public Vector2 Position = new(100,100);

    public Vector2 Size
    {
        get;
        set
        {
            field = value;
            if (!DockOverride)
            {
                PreDockSize = value;
            }
        }
    } = new(16,16);
    public Vector2 ContentSize = new();
    public Vector2 ScrollPosition = new();
    public Vector2 ScrollSpeed = new();
    
    public Vector2 PreDockSize = new(16,16);
    
    public Vector2 CanvasSize => Size - new Vector2(0, UIStyle.WindowHeaderSize);
    public float ScrollbarSizeX => (CanvasSize.X / ContentSize.X) * CanvasSize.X;
    public float ScrollbarSizeY => (CanvasSize.Y / ContentSize.Y) * CanvasSize.Y;

    public float ScrollProgY => ScrollPosition.Y / (ContentSize.Y - CanvasSize.Y);

    public static float CalcScrollProg(float s, float min, float max)
    {
        return (s - min) / (max - min);
    }
    public float ScrollbarYPosX => CanvasSize.X - UIStyle.ScrollbarWidth + Position.X;

    public float ScrollbarYPosY =>
        ((CanvasSize.Y - ScrollbarSizeY) * -ScrollProgY) + Position.Y + UIStyle.WindowHeaderSize;

    public bool ScrollbarYUsing;
    public float ScrollbarYDragOffset;

    public bool OverflowX => Size.X < ContentSize.X;
    public bool OverflowY => Size.Y < ContentSize.Y;
    
    public bool FirstDraw = true;

    public bool IsBeingDrawn
    {
        get;
        set
        {
            field = value;
            if (!field)
            {
                HasBeenDrawn = false;
            }
        }
    } = false;
    public bool HasBeenDrawn = false;

    internal bool DockOverride
    {
        get;
        set
        {
            field = value;
            if (!field)
            {
                Size = PreDockSize;
            }
        }
    } = false;
    internal WindowDock? OwnerDock = null;

    internal bool StackDockOverride = false;
    internal bool StackDockDrawing = false;
    internal bool StackDrawing = false;

    public void Dock(WindowDefinition window, ActiveDockDirection direction, float split = 0.5f)
    {
        if (OwnerDock == null)
            return;
        
        OwnerDock.Dock(new WindowDock(window), direction, split);
    }

    public void UnDock()
    {
        if (OwnerDock != null)
        {
            OwnerDock.WantsDispose = true;
        }
    }

    private WindowDefinition(string displayName, string windowName)
    {
        Id = _nextId++;
        DisplayName = displayName;
        WindowName = windowName;
        
        Windows.Add(Id, this);
        WindowIds.Add(windowName, Id);

        FocusOrder = Id;
    }

    public static WindowDefinition? GetWindowFromFocus(uint order)
    {
        foreach (var window in Windows)
        {
            if (window.Value.FocusOrder == order)
                return window.Value;
        }
        
        return null;
    }

    public static WindowDefinition? WindowToDock;

    public void Focus()
    {
        var prevFocus = FocusOrder;
        FocusOrder = 0;

        foreach (var window in Windows)
        {
            if (window.Value != this && window.Value.FocusOrder < prevFocus)
            {
                window.Value.FocusOrder++;
            }
        }
    }

    public bool IsFocused()
    {
        var thisOrder = FocusOrder;
        
        foreach (var window in Windows)
        {
            if (window.Value.FocusOrder < thisOrder)
            {
                return false;
            }
        }

        return true;
    }
}

public class ItemId(uint window, string item)
{
    private readonly uint _windowId = window;
    private readonly string _id = item;

    public override bool Equals(object? obj)
    {
        if (obj is ItemId id)
        {
            return id._windowId == _windowId && id._id == _id;
        }
        return false;
    }

    protected bool Equals(ItemId other)
    {
        return _windowId == other._windowId && _id == other._id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_windowId, _id);
    }
}