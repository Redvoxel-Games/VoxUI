using Silk.NET.Input;
using Silk.NET.Windowing;
using VoxUI.Math;
using VoxUI.Rendering;

namespace VoxUI.Core;

// ReSharper disable once InconsistentNaming
public static partial class VoxUIR
{
    public static WindowDefinition? CurrentWindow;

    private static float _currentDrawX;
    private static float _currentDrawY;
    private static float _currentMaxSizeX;
    private static float _currentMaxSizeY;

    private static float _previousItemX;
    private static float _previousItemY;
    private static float _previousItemWidth;
    private static float _previousItemHeight;
    
    private static FontSet FontSet => Renderer.OpenSans;
    public static Font HeaderFont => FontSet.GetFontForSize(UIStyle.WindowHeaderSize);
    public static Font RegularFont => FontSet.GetFontForSize(UIStyle.TextSize);

    /// <summary>
    /// Begins a window.
    /// </summary>
    /// <param name="windowName">Internal name for item tracking/data storage</param>
    /// <param name="displayName">Name to display on the header</param>
    /// <param name="position">Start position</param>
    /// <param name="size">Start size</param>
    /// <param name="open">Whether the window is open</param>
    /// <param name="windowFlags">Window flags</param>
    /// <returns>Whether the window is visible</returns>
    /// <remarks>If <paramref name="open"/> is null, window will not have a close button.</remarks>
    public static bool Begin(string windowName, string displayName, Vector2? position, Vector2? size, ref bool? open, WindowFlags windowFlags = WindowFlags.None)
    {
        _currentDrawX = 0;
        _currentDrawY = 0;
        _currentMaxSizeX = 0;
        _currentMaxSizeY = 0;
        
        _previousItemX = 0;
        _previousItemY = 0;
        _previousItemWidth = 0;
        _previousItemHeight = 0;
        
        var showCloseButton = false;

        if (open.HasValue)
        {
            showCloseButton = true;
            if (!open.Value)
            {
                var windowDef = WindowDefinition.GetWindowDefinition(windowName, displayName);
                windowDef.IsBeingDrawn = false;
                windowDef.StackDrawing = false;
                return false;
            }
        }
        
        CurrentWindow = WindowDefinition.GetWindowDefinition(windowName, displayName);
        CurrentWindow.DisplayName = displayName;
        CurrentWindow.ShowCloseButton = showCloseButton;
        CurrentWindow.IsBeingDrawn = !CurrentWindow.StackDockOverride;
        CurrentWindow.StackDrawing = true;
        CurrentWindow.WindowFlags = windowFlags;
        
        var headerSize = HeaderFont.GetTextSize(displayName);
        _currentMaxSizeX = headerSize.X + UIStyle.WindowPadding;

        if (showCloseButton)
        {
            var closeButtonSize = new Vector2(UIStyle.WindowHeaderSize,UIStyle.WindowHeaderSize);
            _currentMaxSizeX += closeButtonSize.X + 8;
            
            var buttonPosX = CurrentWindow.Size.X - closeButtonSize.X - 4;
            var buttonPosY = 0;
            var buttonEndX = CurrentWindow.Size.X;
            var buttonEndY = UIStyle.WindowHeaderSize;

            var mousePosRel = Input.MousePosition - CurrentWindow.Position;
            if (
                mousePosRel.X >= buttonPosX && mousePosRel.Y >= buttonPosY
                                         && mousePosRel.X <= buttonEndX && mousePosRel.Y <= buttonEndY
            )
            {
                CurrentWindow.CloseButtonHovered = true;
                if (IsMouseButtonJustReleased(MouseButton.Left))
                {
                    open = false;
                    CurrentWindow.IsBeingDrawn = false;
                    CurrentWindow.StackDrawing = false;
                    CurrentWindow = null;
                    return false;
                }
            }
            else
            {
                CurrentWindow.CloseButtonHovered = false;
            }
        }
        else
        {
            CurrentWindow.CloseButtonHovered = false;
        }

        if (CurrentWindow.StackDockOverride && !CurrentWindow.StackDockDrawing)
        {
            CurrentWindow.IsBeingDrawn = false;
            CurrentWindow.StackDrawing = true;
            CurrentWindow = null;
            return false;
        }
        
        if (position.HasValue && CurrentWindow.FirstDraw) CurrentWindow.Position = position.Value;
        if (size.HasValue && CurrentWindow.FirstDraw) CurrentWindow.Size = size.Value;

        var mousePos = GetMousePositionInWindowSpace(false);

        var headerInteractSize = CurrentWindow.DockOverride ? headerSize + new Vector2(UIStyle.WindowPadding*2) : new Vector2(CurrentWindow.Size.X, UIStyle.WindowHeaderSize);
        

        if (IsMouseInsideRect(CurrentWindow.Position, headerInteractSize) && Renderer.GetHoveredWindow() == CurrentWindow)
        {
            CurrentWindow.HeaderHovered = true;
            if (IsMouseButtonJustPressed(MouseButton.Left) && !windowFlags.HasFlag(WindowFlags.NoDrag))
            {
                CurrentWindow.IsBeingDragged = true;
                CurrentWindow.DragPositionOffset = mousePos;
            }
        }
        else
        {
            CurrentWindow.HeaderHovered = false;
        }

        if (CurrentWindow.IsBeingDragged)
        {
            if (!IsMouseButtonDown(MouseButton.Left) || windowFlags.HasFlag(WindowFlags.NoDrag))
            {
                CurrentWindow.IsBeingDragged = false;
            }
            else
            {
                var target = MousePosition - CurrentWindow.DragPositionOffset;
                if (CurrentWindow.DockOverride)
                {
                    if ((CurrentWindow.Position - target).Magnitude > 64)
                    {
                        CurrentWindow.UnDock();
                        CurrentWindow.Position = target;
                    }
                }
                else
                {
                    CurrentWindow.Position = target;
                }
            }
        }

        if (CurrentWindow.IsBeingDragged)
            WindowDefinition.WindowToDock = CurrentWindow;

        CurrentWindow.IsBeingDrawn = true;
        CurrentWindow.StackDrawing = true;

        return true;
    }

    public static bool Begin(string windowName, string? displayName, WindowFlags windowFlags = WindowFlags.None)
    {
        bool? o = null;
        return Begin(windowName, displayName ?? windowName, null, null, ref o, windowFlags);
    }

    public static bool Begin(string windowName, string? displayName, Vector2? position, Vector2? size, WindowFlags windowFlags = WindowFlags.None)
    {
        bool? o = null;
        return Begin(windowName, displayName ?? windowName, position, size, ref o, windowFlags);
    }

    public static bool Begin(string windowName, string? displayName, ref bool open, WindowFlags windowFlags = WindowFlags.None)
    {
        bool? o = open;
        var visible = Begin(windowName, displayName ?? windowName, null, null, ref o, windowFlags);
        open = o ?? false;
        return visible;
    }

    /// <summary>
    /// Ends the current window and draws it to the screen.
    /// </summary>
    public static void End()
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Begin() before calling End()");

        if (CurrentWindow.FirstDraw)
        {
            CurrentWindow.Size = new Vector2(_currentMaxSizeX + UIStyle.WindowPadding*2, _currentMaxSizeY + UIStyle.WindowPadding*2 + UIStyle.WindowHeaderSize);
            CurrentWindow.PreDockSize = CurrentWindow.Size;
            
            CurrentWindow.FirstDraw = false;
            CurrentWindow.Focus();
        }

        if (!CurrentWindow.HasBeenDrawn)
        {
            CurrentWindow.HasBeenDrawn = true;
            CurrentWindow.Focus();
        }
        
        bool displayHeader = (!CurrentWindow.WindowFlags.HasFlag(WindowFlags.NoHeader) || CurrentWindow.DockOverride);

        if (CurrentWindow.StackDockOverride)
        {
            displayHeader = false;
        }
        
        float headerSize = displayHeader ? UIStyle.WindowHeaderSize : 0;

        if (CurrentWindow.WindowFlags.HasFlag(WindowFlags.AutoSizeX) && !CurrentWindow.DockOverride && !CurrentWindow.StackDockOverride)
        {
            CurrentWindow.Size = new Vector2(_currentMaxSizeX + UIStyle.WindowPadding*2, CurrentWindow.Size.Y);
        }
        if (CurrentWindow.WindowFlags.HasFlag(WindowFlags.AutoSizeY) && !CurrentWindow.DockOverride && !CurrentWindow.StackDockOverride)
        {
            CurrentWindow.Size = new Vector2(CurrentWindow.Size.X, _currentMaxSizeY + UIStyle.WindowPadding*2 + headerSize);
        }
        
        CurrentWindow.ContentSize = new Vector2(_currentMaxSizeX + UIStyle.WindowPadding*2, _currentMaxSizeY + UIStyle.WindowPadding*2 + headerSize);

        if (IsCurrentWindowHovered() || CurrentWindow.ScrollbarYUsing)
        {
            var wheelDelta = GetMouseWheelDelta();
            
            var nsx = CurrentWindow.WindowFlags.HasFlag(WindowFlags.NoScrollX);
            var nsy = CurrentWindow.WindowFlags.HasFlag(WindowFlags.NoScrollY);
            wheelDelta *= new Vector2(nsx ? 0 : 1, nsy ? 0 : 1);
            
            if (CurrentWindow.OverflowX)
            {
                CurrentWindow.ScrollSpeed += wheelDelta * new Vector2(25);
            }

            if (CurrentWindow.OverflowY)
            {
                CurrentWindow.ScrollSpeed += wheelDelta * new Vector2(0,25);

                var sx = CurrentWindow.ScrollbarYPosX;
                var sy = CurrentWindow.ScrollbarYPosY;
                var ssy = CurrentWindow.ScrollbarSizeY;

                var sColor = UIStyle.ScrollbarColor;
                
                var usingBar = CurrentWindow.ScrollbarYUsing;

                if (IsMouseInsideRect(sx, sy, UIStyle.ScrollbarWidth, ssy) || usingBar)
                {
                    if (IsMouseButtonJustPressed(MouseButton.Left))
                    {
                        usingBar = true;
                        CurrentWindow.ScrollbarYDragOffset = sy - MousePosition.Y;
                    }
                    else if (!IsMouseButtonDown(MouseButton.Left))
                    {
                        sColor = sColor.Lerp(Color4.White, 0.33f);
                        usingBar = false;
                    }
                    if (usingBar)
                        sColor = sColor.Lerp(Color4.White, 0.45f);
                }
                
                CurrentWindow.ScrollbarYUsing = usingBar;

                if (usingBar)
                {
                    float scrollTrackSize = CurrentWindow.CanvasSize.Y - CurrentWindow.ScrollbarSizeY; 
                    float prog = WindowDefinition.CalcScrollProg(MousePosition.Y + CurrentWindow.ScrollbarYDragOffset, CurrentWindow.Position.Y + headerSize,  CurrentWindow.Position.Y + UIStyle.WindowHeaderSize + scrollTrackSize);
                    
                    prog = Clamp(prog, 0f, 1f);
                    CurrentWindow.ScrollPosition.Y = -(CurrentWindow.ContentSize.Y - CurrentWindow.Size.Y) * prog;
                }
                
                Renderer.AddRect(sx, sy, UIStyle.ScrollbarWidth, ssy, sColor, CornerRadii.All(UIStyle.ButtonCornerRadius), zIndex:10);
            }
        }

        float decay = 10f;
        float dt = (float)Renderer.DeltaTime;
        CurrentWindow.ScrollPosition += CurrentWindow.ScrollSpeed * dt * 8;

        if (CurrentWindow.ScrollPosition.X < -(CurrentWindow.ContentSize.X - CurrentWindow.Size.X))
            CurrentWindow.ScrollPosition.X = -(CurrentWindow.ContentSize.X - CurrentWindow.Size.X);
        if (CurrentWindow.ScrollPosition.X > 0)
            CurrentWindow.ScrollPosition.X = 0;

        if (CurrentWindow.ScrollPosition.Y < -(CurrentWindow.ContentSize.Y - CurrentWindow.Size.Y))
        {
            CurrentWindow.ScrollPosition.Y = (-(CurrentWindow.ContentSize.Y - CurrentWindow.Size.Y)) + (CurrentWindow.ScrollSpeed.Y * dt * 8);
        }
        if (CurrentWindow.ScrollPosition.Y > 0)
        {
            CurrentWindow.ScrollPosition.Y = CurrentWindow.ScrollSpeed.Y * dt * 8;
        }
        
        CurrentWindow.ScrollSpeed -= CurrentWindow.ScrollSpeed * decay * dt;
        
        var cornerRadius = CurrentWindow.DockOverride ? 0 : UIStyle.WindowCornerRadius;
        
        var headerRadii = new CornerRadii(cornerRadius, cornerRadius);
        var fullRadii = CornerRadii.All(cornerRadius);
        
        var primaryColor = UIStyle.PrimaryColor;

        if (CurrentWindow.IsBeingDragged)
        {
            primaryColor = primaryColor.Lerp(Color4.Black, 0.2f);
        }
        else if (CurrentWindow.HeaderHovered)
        {
            primaryColor = primaryColor.Lerp(Color4.White, 0.1f);
        }

        if (displayHeader && !CurrentWindow.DockOverride)
        {
            Renderer.AddRect(CurrentWindow.Position, new(CurrentWindow.Size.X, headerSize), primaryColor, headerRadii, zIndex: 10);
            Renderer.AddText(CurrentWindow.Position + new Vector2(UIStyle.WindowPadding), CurrentWindow.DisplayName, (int)headerSize, UIStyle.HeaderTextColor, 11);
        }
        else if (displayHeader && CurrentWindow.DockOverride)
        {
            var headerTextSize = HeaderFont.GetTextSize(CurrentWindow.DisplayName) + new Vector2(UIStyle.WindowPadding * 2);
            Renderer.AddRect(CurrentWindow.Position + new Vector2(0, headerSize - 2), new(CurrentWindow.Size.X, 2),
                primaryColor, zIndex: 10);
            Renderer.AddRect(CurrentWindow.Position, headerTextSize + new Vector2(UIStyle.WindowPadding), primaryColor, headerRadii, zIndex: 10);
            Renderer.AddText(CurrentWindow.Position + new Vector2(UIStyle.WindowPadding), CurrentWindow.DisplayName, (int)headerSize, UIStyle.HeaderTextColor, 11);
        }
        Renderer.AddRect(CurrentWindow.Position, CurrentWindow.Size, UIStyle.BackgroundColor, fullRadii, zIndex: -1);

        if (CurrentWindow.ShowCloseButton)
        {
            var closeButtonSize = new Vector2(headerSize,headerSize);
            if (CurrentWindow.CloseButtonHovered)
            {
                Renderer.AddRect(CurrentWindow.Position + new Vector2(CurrentWindow.Size.X-closeButtonSize.X),
                    new Vector2(headerSize,headerSize),
                    new Color4(0,0,0,0.5f), zIndex:11);
            }

            var offset = new Vector2(4, 4);
            Renderer.AddImage(CurrentWindow.Position + new Vector2(CurrentWindow.Size.X-closeButtonSize.X) + offset, closeButtonSize - offset*2, Images.Xmark, 11);
        }
        
        if (UIStyle.WindowOutline)
        {
            Renderer.AddRect(CurrentWindow.Position, CurrentWindow.Size, primaryColor, fullRadii, 1, 10);
        }
        
        CurrentWindow = null;
        _currentDrawX = 0;
        _currentDrawY = 0;
        _currentMaxSizeX = 0;
        _currentMaxSizeY = 0;
    }

    /// <summary>
    /// Gets the current scroll movement.
    /// </summary>
    public static Vector2 GetMouseWheelDelta()
    {
        return Input.MouseWheelDelta;
    }

    /// <summary>
    /// Gets the current draw position in screen space.
    /// </summary>
    public static Vector2 GetDrawPositionInScreenSpace()
    {
        if (CurrentWindow == null)
            throw new Exception("Current window is null");
        
        bool displayHeader = !CurrentWindow.WindowFlags.HasFlag(WindowFlags.NoHeader) || CurrentWindow.DockOverride;
        
        if (CurrentWindow.StackDockOverride)
        {
            displayHeader = false;
        }
        
        float headerSize = displayHeader ? UIStyle.WindowHeaderSize : 0;
        
        return new Vector2(_currentDrawX + UIStyle.WindowPadding, _currentDrawY + headerSize + UIStyle.WindowPadding) + CurrentWindow.Position + CurrentWindow.ScrollPosition;
    }

    /// <summary>
    /// Gets the current mouse position in window space.
    /// </summary>
    public static Vector2 GetMousePositionInWindowSpace(bool applyScroll = true)
    {
        if (CurrentWindow == null)
            throw new Exception("Current window is null");
        
        var result = Input.MousePosition - (CurrentWindow.Position + (applyScroll ? CurrentWindow.ScrollPosition : new Vector2())) - new Vector2(UIStyle.WindowPadding, UIStyle.WindowPadding);
        
        bool displayHeader = !CurrentWindow.WindowFlags.HasFlag(WindowFlags.NoHeader) || CurrentWindow.DockOverride;
        
        if (CurrentWindow.StackDockOverride)
        {
            displayHeader = false;
        }

        if (!displayHeader)
        {
            result.Y += UIStyle.WindowHeaderSize;
        }
        
        return result;
    }

    /// <summary>
    /// Gets if the current window is focused.
    /// </summary>
    public static bool IsCurrentWindowFocused()
    {
        if (CurrentWindow == null)
            return false;
        
        return CurrentWindow.IsFocused();
    }

    /// <summary>
    /// Gets if the current window is being hovered.
    /// </summary>
    public static bool IsCurrentWindowHovered()
    {
        if (CurrentWindow == null)
            return false;
        
        return Renderer.GetHoveredWindow() == CurrentWindow;
    }

    /// <summary>
    /// Moves the draw position and updates draw bound sizes.
    /// </summary>
    /// <param name="x">X offset</param>
    /// <param name="y">Y offset</param>
    /// <param name="width">Width of the item</param>
    /// <param name="height">Height of the item</param>
    public static void Advance(float x, float y, float width, float height)
    {
        if (CurrentWindow == null)
            throw new Exception("Current window is null");

        _previousItemX = _currentDrawX;
        _previousItemY = _currentDrawY;
        _previousItemWidth = width;
        _previousItemHeight = height;
        
        _currentMaxSizeX = MathF.Max(_currentMaxSizeX, _currentDrawX + width);
        _currentMaxSizeY = MathF.Max(_currentMaxSizeY, _currentDrawY + height);
        
        _currentDrawX += x;
        _currentDrawY += y;
    }

    /// <summary>
    /// Called after Advance() in most draw calls, returns draw position to the left of the window.
    /// </summary>
    public static void ReturnToBaseline()
    {
        if (CurrentWindow == null)
            throw new Exception("Current window is null");
        
        _currentDrawX = 0;
    }

    /// <summary>
    /// Displays text on the current window.
    /// </summary>
    /// <param name="text">Text to display</param>
    public static void Text(string text)
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Begin() before attempting to draw");
        
        var textSize = RegularFont.GetTextSize(text);
        
        Renderer.AddText(GetDrawPositionInScreenSpace(), text, UIStyle.TextSize, UIStyle.TextColor, parentPos:CurrentWindow.Position, parentSize:CurrentWindow.Size);
        
        Advance(0, textSize.Y + UIStyle.ItemSpacing/2, textSize.X, textSize.Y);
        ReturnToBaseline();
    }
    
    /// <summary>
    /// Adds a horizontal line separator.
    /// </summary>
    /// <remarks>Accounts for <see cref="UIStyle.ItemSpacing"/></remarks>
    public static void Separator()
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Begin() before attempting to draw");

        Advance(0,UIStyle.ItemSpacing, 0, 0);
        
        Renderer.AddRect(GetDrawPositionInScreenSpace() + new Vector2(2), new Vector2(CurrentWindow.Size.X-(UIStyle.WindowPadding*2 + 4), 1), UIStyle.SeparatorColor);
        
        Advance(0,UIStyle.ItemSpacing, 0, 0);
        ReturnToBaseline();
    }

    /// <summary>
    /// Acts as SameLine() but adds a vertical separator line.
    /// </summary>
    public static void VerticalSeparator()
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Begin() before attempting to draw");
        
        SameLine();
        
        Advance(UIStyle.ItemSpacing,0, 0, _previousItemHeight);
        
        Renderer.AddRect(GetDrawPositionInScreenSpace() + new Vector2(0,2), new Vector2(1, _previousItemHeight-4), UIStyle.SeparatorColor);
        
        Advance(UIStyle.ItemSpacing*2, 0, 0, _previousItemHeight);
    }

    /// <summary>
    /// Sets the draw position to the top right corner of the previous item.
    /// </summary>
    /// <remarks>Accounts for <see cref="UIStyle.ItemSpacing"/></remarks>
    public static void SameLine()
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Begin() before attempting to draw");

        _currentDrawY = _previousItemY;
        _currentDrawX = _previousItemX + _previousItemWidth + UIStyle.ItemSpacing;
        
        // Advance(_previousItemWidth + UIStyle.ItemSpacing,-_previousItemHeight - UIStyle.ItemSpacing, 0, 0);
    }
}