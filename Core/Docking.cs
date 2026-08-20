using System.Xml;
using Silk.NET.Input;
using VoxUI.Math;
using VoxUI.Rendering;

namespace VoxUI.Core;

public abstract class DockMember : IDisposable
{
    public Vector2 Size
    {
        get;
        set
        {
            field = value;
            Update();
        }
    } = new();

    public Vector2 Position
    {
        get;
        set
        {
            field = value;
            Update();
        }
    } = new();

    public bool WantsDispose = false;

    protected abstract void Update();

    public virtual void Handle() { }
    public virtual void OnDispose() { }
    
    public void Dispose()
    {
        OnDispose();
    }
    
    public void Dock(DockMember member, ActiveDockDirection direction, float split = 0.5f)
    {
        var parent = ParentMember;
        var newDock = new Dock(this, member, direction, split);

        if (parent is Dock dock)
        {
            if (dock.Member1 == this)
            {
                dock.Member1 = newDock;
            }
            else
            {
                dock.Member2 = newDock;
            }
        }

        if (parent is RootDock root)
        {
            root.Member = newDock;
        }
    }

    public DockMember? ParentMember = null;
}

public enum DockDirection
{
    Horizontal,
    Vertical
}

public enum ActiveDockDirection
{
    Left,
    Right,
    Up,
    Down,
    Stack
}

public class Dock : DockMember
{
    public DockMember Member1
    {
        get;
        set
        {
            field = value;
            field.ParentMember = this;
        }
    }

    public DockMember Member2
    {
        get;
        set
        {
            field = value;
            field.ParentMember = this;
        }
    }

    public float Split
    {
        get;
        set
        {
            field = value;
            Update();
        }
    } = 0.5f;
    public readonly DockDirection Direction;

    public Dock(DockMember member1, DockMember member2, DockDirection direction, float split = 0.5f)
    {
        Member1 = member1;
        Member2 = member2;
        Member1.ParentMember = this;
        Member2.ParentMember = this;
        
        Direction = direction;
        
        Split = split;
    }

    public Dock(DockMember member1, DockMember member2, ActiveDockDirection direction, float split)
    {
        switch (direction)
        {
            case ActiveDockDirection.Left:
                Member1 = member2;
                Member2 = member1;
                Direction = DockDirection.Horizontal;
                Split = 1 - split;
                break;
            case ActiveDockDirection.Right:
                Member1 = member1;
                Member2 = member2;
                Direction = DockDirection.Horizontal;
                Split = split;
                break;
            case ActiveDockDirection.Down:
                Member1 = member1;
                Member2 = member2;
                Direction = DockDirection.Vertical;
                Split = split;
                break;
            case ActiveDockDirection.Up:
                Member1 = member2;
                Member2 = member1;
                Direction = DockDirection.Vertical;
                Split = 1 - split;
                break;
            default:
                throw new Exception("Invalid direction");
        }
    }

    protected override void Update()
    {
        if (Direction == DockDirection.Horizontal)
        {
            Member1.Size = new Vector2(Size.X*Split, Size.Y);
            Member1.Position = Position;
            Member2.Size = new Vector2(Size.X*(1-Split), Size.Y);
            Member2.Position = Position + new Vector2(Member1.Size.X);
        }
        else
        {
            Member1.Size = new Vector2(Size.X, Size.Y*Split);
            Member1.Position = Position;
            Member2.Size = new Vector2(Size.X, Size.Y*(1-Split));
            Member2.Position = Position + new Vector2(0, Member1.Size.Y);
        }
    }

    public override void Handle()
    {
        Member1.Handle();
        Member2.Handle();

        if (Member1.WantsDispose)
        {
            Member1.Dispose();

            if (ParentMember is Dock dock)
            {
                if (dock.Member1 == this)
                {
                    dock.Member1 = Member2;
                }
                else
                {
                    dock.Member2 = Member2;
                }
            }

            if (ParentMember is RootDock root)
            {
                root.Member = Member2;
            }

            WantsDispose = true;
            Dispose();
            return;
        }

        if (Member2.WantsDispose)
        {
            Member2.Dispose();
            
            if (ParentMember is Dock dock)
            {
                if (dock.Member1 == this)
                {
                    dock.Member1 = Member1;
                }
                else
                {
                    dock.Member2 = Member1;
                }
            }

            if (ParentMember is RootDock root)
            {
                root.Member = Member1;
            }
            
            WantsDispose = true;
            Dispose();
            return;
        }
        
        // Handle resizing
        if (Direction == DockDirection.Horizontal)
        {
            var resizeHandle = Position.X + Size.X*Split;
            var mousePos = Input.MousePosition;
            
            if (MathF.Abs(mousePos.X - resizeHandle) < 4f)
            {
                if (VoxUIR.IsMouseButtonJustPressed(MouseButton.Left))
                {
                    _resizing = true;
                }
            }

            if (_resizing)
            {
                if (!VoxUIR.IsMouseButtonDown(MouseButton.Left))
                {
                    _resizing = false;
                }
                
                Split = -(mousePos.X - Position.X) / (Position.X - Size.X);
            }
        }

        if (Direction == DockDirection.Vertical)
        {
            var resizeHandle = Position.Y + Size.Y*Split;
            var mousePos = Input.MousePosition;
            
            if (MathF.Abs(mousePos.Y - resizeHandle) < 4f)
            {
                if (VoxUIR.IsMouseButtonJustPressed(MouseButton.Left))
                {
                    _resizing = true;
                }
            }

            if (_resizing)
            {
                if (!VoxUIR.IsMouseButtonDown(MouseButton.Left))
                {
                    _resizing = false;
                }
                
                Split = -(mousePos.Y - Position.Y) / (Position.Y - Size.Y);
            }
        }
    }

    private bool _resizing = false;
}

public class WindowDock : DockMember
{
    public WindowDefinition Window
    {
        get;
        set
        {
            field?.DockOverride = false;
            field?.OwnerDock = null;

            field = value;

            field.DockOverride = true;
            field.OwnerDock = this;
        }
    }

    internal static readonly List<WindowDock> DockableWindows = [];

    public WindowDock(WindowDefinition window)
    {
        Window = window;
        Window.DockOverride = true;
        Window.OwnerDock = this;
        
        DockableWindows.Add(this);
    }

    protected override void Update()
    {
        if (Window.DockOverride)
        {
            Window.Position = Position;
            Window.Size = Size;
        }
    }

    public override void Handle()
    {
        if (Window.DockOverride)
        {
            Window.Position = Position;
            Window.Size = Size;
        }

        if (!Window.IsBeingDrawn)
        {
            WantsDispose = true;
        }
        
        var mousePos = Input.MousePosition;

        var draggedWindow = WindowDefinition.WindowToDock;

        if (VoxUIR.IsMouseInsideRect(Position, Size) && draggedWindow != null)
        {
            var center = Position + (Size / 2f);
            var cornerRadii = CornerRadii.All(UIStyle.ButtonCornerRadius);
            
            var color = UIStyle.PrimaryColor;
            color *= new Color4(1, 1, 1, 0.5f);
            
            // Dock left/right
            if (Size.X > 128 + 32*2 && Size.Y > 64)
            {
                // Left
                var dockRectPos = center - new Vector2(64f + 32f, 32f);
                var dockRectSize = new Vector2(32f, 64f);

                if (VoxUIR.IsMouseInsideRect(dockRectPos, dockRectSize))
                {
                    color *= new Color4(1, 1, 1, 1.5f);

                    if (VoxUIR.IsMouseButtonJustReleased(MouseButton.Left))
                    {
                        Window.Dock(draggedWindow,ActiveDockDirection.Left);
                    }
                }
                Renderer.AddRect(dockRectPos, dockRectSize, color, cornerRadii, zIndex:10, focusOrder:Window.FocusOrder);
                
                // Right
                dockRectPos = center + new Vector2(64f, -32f);
                color = UIStyle.PrimaryColor * new Color4(1, 1, 1, 0.5f);

                if (VoxUIR.IsMouseInsideRect(dockRectPos, dockRectSize))
                {
                    color *= new Color4(1, 1, 1, 1.5f);

                    if (VoxUIR.IsMouseButtonJustReleased(MouseButton.Left))
                    {
                        Window.Dock(draggedWindow, ActiveDockDirection.Right);
                    }
                }
                Renderer.AddRect(dockRectPos, dockRectSize, color, cornerRadii, zIndex:10, focusOrder:Window.FocusOrder);
            }
            
            // Dock up/down
            if (Size.Y > 128 + 32 * 2 && Size.X > 64)
            {
                // Up
                var dockRectPos = center - new Vector2(32f, 64f + 32f);
                var dockRectSize = new Vector2(64f, 32f);
                color = UIStyle.PrimaryColor * new Color4(1, 1, 1, 0.5f);

                if (VoxUIR.IsMouseInsideRect(dockRectPos, dockRectSize))
                {
                    color *= new Color4(1, 1, 1, 1.5f);

                    if (VoxUIR.IsMouseButtonJustReleased(MouseButton.Left))
                    {
                        Window.Dock(draggedWindow,ActiveDockDirection.Up);
                    }
                }
                Renderer.AddRect(dockRectPos, dockRectSize, color, cornerRadii, zIndex:10, focusOrder:Window.FocusOrder);
                
                // Down
                dockRectPos = center + new Vector2(-32f, 64f);
                color = UIStyle.PrimaryColor * new Color4(1, 1, 1, 0.5f);

                if (VoxUIR.IsMouseInsideRect(dockRectPos, dockRectSize))
                {
                    color *= new Color4(1, 1, 1, 1.5f);

                    if (VoxUIR.IsMouseButtonJustReleased(MouseButton.Left))
                    {
                        Window.Dock(draggedWindow, ActiveDockDirection.Down);
                    }
                }
                Renderer.AddRect(dockRectPos, dockRectSize, color, cornerRadii, zIndex:10, focusOrder:Window.FocusOrder);
            }

            if (Size.X > 64 && Size.Y > 64)
            {
                // Center
                var dockRectPos = center - new Vector2(32, 32);
                var dockRectSize = new Vector2(64f, 64f);
                
                color = UIStyle.PrimaryColor * new Color4(1, 1, 1, 0.5f);
                if (VoxUIR.IsMouseInsideRect(dockRectPos, dockRectSize))
                {
                    color *= new Color4(1, 1, 1, 1.5f);

                    if (VoxUIR.IsMouseButtonJustReleased(MouseButton.Left))
                    {
                        var stack = new StackedDock();
                        stack.Add(Window);
                        stack.Add(draggedWindow);
                        
                        if (ParentMember is Dock dock)
                        {
                            if (dock.Member1 == this)
                            {
                                dock.Member1 = stack;
                            }
                            else
                            {
                                dock.Member2 = stack;
                            }
                        }

                        if (ParentMember is RootDock root)
                        {
                            root.Member = stack;
                        }
                    }
                }
                Renderer.AddRect(dockRectPos, dockRectSize, color, cornerRadii, zIndex:10, focusOrder:Window.FocusOrder);
            }
        }
    }

    public override void OnDispose()
    {
        DockableWindows.Remove(this);
        
        Window.DockOverride = false;
        Window.OwnerDock = null;
    }
}

public class StackedDock : DockMember
{
    private readonly List<WindowDefinition> _windows = [];
    public int OpenWindow = -1;
    
    private int? _draggingWindow = null;
    private float _draggingWindowOffset = 0;
    private float _draggingWindowTabSize = 0;

    public void Add(WindowDefinition window)
    {
        window.StackDockOverride = true;
        window.DockOverride = true;
        window.OwnerDock = null;
        window.IsBeingDragged = false;
        
        _windows.Add(window);
        OpenWindow = _windows.Count - 1;
    }

    public void Remove(WindowDefinition window)
    {
        window.StackDockOverride = false;
        window.DockOverride = false;
        window.IsBeingDragged = false;
        
        window.Focus();
        
        _windows.Remove(window);
        OpenWindow--;
    }
    
    protected override void Update()
    {
        
    }

    public override void Handle()
    {
        if (_windows.Count <= 1)
        {
            if (_windows.Count == 1)
            {
                var lastWindow = _windows[0];
                
                Remove(lastWindow);

                var windowDock = new WindowDock(lastWindow);

                if (ParentMember is Dock dock)
                {
                    if (dock.Member1 == this)
                    {
                        dock.Member1 = windowDock;
                    }
                    else
                    {
                        dock.Member2 = windowDock;
                    }
                }

                if (ParentMember is RootDock root)
                {
                    root.Member = windowDock;
                }
            }

            WantsDispose = true;
            return;
        }
        if (OpenWindow == -1)
            return;
        
        var mousePos = Input.MousePosition;

        int offsetAfter = _draggingWindow ?? int.MaxValue;
        float offsetSize = _draggingWindowTabSize;
        float draggedTabPos = mousePos.X - _draggingWindowOffset;

        var focusOrder = _windows[OpenWindow].FocusOrder;

        float accumulatedOffset = 0;

        int? dragNewIndex = null;

        var dragConsumed = false;
        
        for (int index = 0; index < _windows.Count; index++)
        {
            var window = _windows[index];

            var headerTextSize = VoxUIR.HeaderFont.GetTextSize(window.DisplayName) + new Vector2(UIStyle.WindowPadding*2,0);
            
            var tabPos = Position + new Vector2(accumulatedOffset);

            if ((tabPos.X - draggedTabPos) + _draggingWindowTabSize/2 >= 0 && _draggingWindow != null)
            {
                tabPos += new Vector2(_draggingWindowTabSize+2);

                if (!dragNewIndex.HasValue)
                {
                    dragNewIndex = index <= _draggingWindow ? index : index-1;
                }
            }

            if (_draggingWindow == index)
            {
                if (MathF.Abs(Position.Y + UIStyle.WindowHeaderSize - mousePos.Y) >= 32)
                {
                    Remove(window);

                    window.IsBeingDragged = true;
                    window.DragPositionOffset = new Vector2(_draggingWindowOffset, UIStyle.WindowHeaderSize/2f);

                    _draggingWindow = null;
                    OpenWindow--;
                    if (OpenWindow < 0)
                        OpenWindow = 0;
                    
                    continue;
                }
                
                _draggingWindowTabSize = headerTextSize.X;
                tabPos = new Vector2(VoxUIR.Clamp(draggedTabPos, Position.X, Position.X + Size.X - headerTextSize.X), tabPos.Y);
            }
            
            var color = UIStyle.PrimaryColor;

            if (VoxUIR.IsMouseInsideRect(tabPos, headerTextSize) || _draggingWindow == index)
            {
                if (VoxUIR.IsMouseButtonDown(MouseButton.Left))
                {
                    color = color.Lerp(Color4.Black, 0.2f);
                }
                else
                {
                    color = color.Lerp(Color4.White, 0.1f);
                }

                if (VoxUIR.IsMouseButtonJustPressed(MouseButton.Left) && !dragConsumed)
                {
                    OpenWindow = index;
                    window.Focus();

                    _draggingWindow = index;
                    _draggingWindowOffset = mousePos.X - tabPos.X;
                    _draggingWindowTabSize = headerTextSize.X;

                    dragConsumed = true;
                }
            }

            int zIndex = 10;

            if (_draggingWindow == index)
                zIndex += 2;
            
            Renderer.AddRect(tabPos, headerTextSize, color, zIndex:zIndex, focusOrder:focusOrder, noClipping: true);
            Renderer.AddText(tabPos + new Vector2(UIStyle.WindowPadding), window.DisplayName, UIStyle.WindowHeaderSize, UIStyle.HeaderTextColor, zIndex+1, noClipping:true);
            
            if (_draggingWindow != index)
                accumulatedOffset += headerTextSize.X + 2;
            
            window.Position = Position + new Vector2(0, UIStyle.WindowHeaderSize);
            window.Size = Size - new Vector2(0, UIStyle.WindowHeaderSize);

            if (!window.StackDrawing)
            {
                Remove(window);
            }
            else
            {
                window.StackDockDrawing = index == OpenWindow;
            }
        }
        
        if (_draggingWindow.HasValue)
        {
            if (!VoxUIR.IsMouseButtonDown(MouseButton.Left))
            {
                var newIndex = dragNewIndex ?? _windows.Count-1;
                var window = _windows[_draggingWindow.Value];
                    
                _windows.RemoveAt(_draggingWindow.Value);
                _windows.Insert(newIndex, window);
                OpenWindow = newIndex;
                _draggingWindow = null;
            }
        }
        
        var draggedWindow = WindowDefinition.WindowToDock;

        if (VoxUIR.IsMouseInsideRect(Position, Size) && draggedWindow != null)
        {
            var center = Position + (Size / 2f);
            var cornerRadii = CornerRadii.All(UIStyle.ButtonCornerRadius);
            
            var color = UIStyle.PrimaryColor;
            color *= new Color4(1, 1, 1, 0.5f);
            
            // Dock left/right
            if (Size.X > 128 + 32*2 && Size.Y > 64)
            {
                // Left
                var dockRectPos = center - new Vector2(64f + 32f, 32f);
                var dockRectSize = new Vector2(32f, 64f);

                if (VoxUIR.IsMouseInsideRect(dockRectPos, dockRectSize))
                {
                    color *= new Color4(1, 1, 1, 1.5f);

                    if (VoxUIR.IsMouseButtonJustReleased(MouseButton.Left))
                    {
                        Dock(new WindowDock(draggedWindow),ActiveDockDirection.Left);
                    }
                }
                Renderer.AddRect(dockRectPos, dockRectSize, color, cornerRadii, zIndex:10, focusOrder:focusOrder);
                
                // Right
                dockRectPos = center + new Vector2(64f, -32f);
                color = UIStyle.PrimaryColor * new Color4(1, 1, 1, 0.5f);

                if (VoxUIR.IsMouseInsideRect(dockRectPos, dockRectSize))
                {
                    color *= new Color4(1, 1, 1, 1.5f);

                    if (VoxUIR.IsMouseButtonJustReleased(MouseButton.Left))
                    {
                        Dock(new WindowDock(draggedWindow), ActiveDockDirection.Right);
                    }
                }
                Renderer.AddRect(dockRectPos, dockRectSize, color, cornerRadii, zIndex:10, focusOrder:focusOrder);
            }
            
            // Dock up/down
            if (Size.Y > 128 + 32 * 2 && Size.X > 64)
            {
                // Up
                var dockRectPos = center - new Vector2(32f, 64f + 32f);
                var dockRectSize = new Vector2(64f, 32f);
                color = UIStyle.PrimaryColor * new Color4(1, 1, 1, 0.5f);

                if (VoxUIR.IsMouseInsideRect(dockRectPos, dockRectSize))
                {
                    color *= new Color4(1, 1, 1, 1.5f);

                    if (VoxUIR.IsMouseButtonJustReleased(MouseButton.Left))
                    {
                        Dock(new WindowDock(draggedWindow),ActiveDockDirection.Up);
                    }
                }
                Renderer.AddRect(dockRectPos, dockRectSize, color, cornerRadii, zIndex:10, focusOrder:focusOrder);
                
                // Down
                dockRectPos = center + new Vector2(-32f, 64f);
                color = UIStyle.PrimaryColor * new Color4(1, 1, 1, 0.5f);

                if (VoxUIR.IsMouseInsideRect(dockRectPos, dockRectSize))
                {
                    color *= new Color4(1, 1, 1, 1.5f);

                    if (VoxUIR.IsMouseButtonJustReleased(MouseButton.Left))
                    {
                        Dock(new WindowDock(draggedWindow), ActiveDockDirection.Down);
                    }
                }
                Renderer.AddRect(dockRectPos, dockRectSize, color, cornerRadii, zIndex:10, focusOrder:focusOrder);
            }

            if (Size.X > 64 && Size.Y > 64)
            {
                // Center
                var dockRectPos = center - new Vector2(32, 32);
                var dockRectSize = new Vector2(64f, 64f);
                
                color = UIStyle.PrimaryColor * new Color4(1, 1, 1, 0.5f);
                if (VoxUIR.IsMouseInsideRect(dockRectPos, dockRectSize))
                {
                    color *= new Color4(1, 1, 1, 1.5f);

                    if (VoxUIR.IsMouseButtonJustReleased(MouseButton.Left))
                    {
                        Add(draggedWindow);
                    }
                }
                Renderer.AddRect(dockRectPos, dockRectSize, color, cornerRadii, zIndex:10, focusOrder:focusOrder);
            }
        }
    }
}

public class RootDock : DockMember
{
    protected override void Update()
    {
        
    }

    public DockMember? Member
    {
        get;
        set
        {
            field = value;
            field?.ParentMember = this;
        }
    } = null;

    public override void Handle()
    {
        Member?.ParentMember = this;
        Member?.Handle();
        Member?.Size = Size;
        Member?.Position = Position;
    }
}