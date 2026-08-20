using Silk.NET.Input;
using VoxUI.Math;
using VoxUI.Rendering;

namespace VoxUI.Core;

// ReSharper disable once InconsistentNaming
public static partial class VoxUIR
{
    public static float GetInputWidth()
    {
        if (CurrentWindow == null)
            throw new Exception("Current window is null");

        return MathF.Max(CurrentWindow.Size.X / 2, 100);
    }

    private static ItemId? _currentInput;
    private static int? _currentInputCursorLocation;
    private static readonly Dictionary<ItemId, string> InputStoredText = [];

    private static string? GetStoredText(ItemId itemId, string? defaultText = null)
    {
        return InputStoredText.TryGetValue(itemId, out var text) ? text : defaultText;
    }

    private static void SetStoredText(ItemId itemId, string? text)
    {
        if (text == null)
        {
            InputStoredText.Remove(itemId);
        }
        else
        {
            InputStoredText[itemId] = text;
        }
    }
    
    /// <summary>
    /// Creates a text input.
    /// </summary>
    /// <param name="itemName">Internal name for item tracking/data storage.</param>
    /// <param name="label">Label.</param>
    /// <param name="value">Current value.</param>
    /// <param name="maxLength">Maximum length of the text. (-1 = no max)</param>
    /// <param name="textInputFlags">Flags.</param>
    /// <returns>Whether the value has been updated.</returns>
    public static bool TextInput(string itemName, string label, ref string value, int maxLength = -1, TextInputFlags textInputFlags = TextInputFlags.None)
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Begin() before attempting to draw");
        
        var size = new Vector2(GetInputWidth(), UIStyle.TextSize);
        size = GetNextItemSize(size);
        
        var textSize = RegularFont.GetTextSize(label);

        var itemId = new ItemId(CurrentWindow.Id, itemName);
        
        var inactive = textInputFlags.HasFlag(TextInputFlags.Inactive);
        
        var color = UIStyle.ButtonColor.Lerp(Color4.Black, 0.33f);
        var mousePos = GetMousePositionInWindowSpace();

        var beingEdited = Equals(_currentInput, itemId);
        var exited = false;

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
                    _currentInput = itemId;
                    beingEdited = true;
                    _currentInputCursorLocation = value.Length;
                    SetStoredText(itemId, value);
                }
            }
        }
        else if (inactive && beingEdited)
        {
            exited = true;
            beingEdited = false;
            _currentInput = null;
            SetStoredText(itemId, null);
        }
        else
        {
            if (IsMouseButtonDown(MouseButton.Left) && beingEdited)
            {
                exited = true;
                beingEdited = false;
                _currentInput = null;
                SetStoredText(itemId, null);
            }
        }

        if (IsKeyJustPressed(Key.Enter) && beingEdited)
        {
            exited = true;
            beingEdited = false;
            _currentInput = null;
        }
        
        string allText = GetStoredText(itemId, value) ?? string.Empty;
        
        var pos = GetDrawPositionInScreenSpace();
        var inputTextSize = RegularFont.GetTextSize(allText);
        float posOffset = 0;

        if (textInputFlags.HasFlag(TextInputFlags.Centered))
        {
            posOffset = (size.X - inputTextSize.X) / 2;
        }

        if (beingEdited)
        {
            color = UIStyle.ButtonColor.Lerp(Color4.Black, 0.1f);
        }
        else if (!exited)
        {
            allText = value;
        }
        
        Renderer.AddRect(pos, size, color, CornerRadii.All(UIStyle.ButtonCornerRadius));
        Renderer.AddText(pos + new Vector2(size.X + 2), label, UIStyle.TextSize, UIStyle.TextColor, parentPos:CurrentWindow.Position, parentSize:CurrentWindow.Size);
        Renderer.AddText(pos + new Vector2(2+posOffset), allText, UIStyle.TextSize, UIStyle.ButtonTextColor, parentPos:pos, parentSize:size);

        bool textChanged = false;
        
        if (beingEdited)
        {
            // Draw cursor
            _currentInputCursorLocation ??= allText.Length;

            if (IsKeyJustPressed(Key.Left, true))
            {
                _currentInputCursorLocation--;
            }

            if (IsKeyJustPressed(Key.Right, true))
            {
                _currentInputCursorLocation++;
            }

            if (_currentInputCursorLocation < 0)
                _currentInputCursorLocation = 0;
            
            if (_currentInputCursorLocation > allText.Length)
                _currentInputCursorLocation = allText.Length;
            
            string textBeforeCursor = allText[.._currentInputCursorLocation.Value];

            foreach (var key in GetKeysJustPressed(true))
            {
                string textAfter = allText[_currentInputCursorLocation.Value..];
                
                if (Input.KeyToCharMapping.TryGetValue(key, out char keyChar) && (maxLength < 0 || allText.Length < maxLength))
                {
                    if (IsKeyDown(Key.ShiftLeft) || IsKeyDown(Key.ShiftRight))
                    {
                        keyChar = Input.CapitalMapping.TryGetValue(keyChar, out char c) ? c : keyChar;
                    }

                    allText = textBeforeCursor + keyChar + textAfter;
                    _currentInputCursorLocation++;

                    textChanged = true;
                    
                    break;
                }
                else if (key == Key.Backspace)
                {
                    if (textBeforeCursor.Length > 0)
                    {
                        allText = textBeforeCursor.Remove(textBeforeCursor.Length - 1) + textAfter;
                    }
                    else
                    {
                        allText = textAfter;
                    }
                    
                    _currentInputCursorLocation--;

                    textChanged = true;
                    
                    break;
                }
            }
            
            var cursorPos = pos + (RegularFont.GetTextSize(textBeforeCursor) - new Vector2(-2,UIStyle.TextSize));
            
            Renderer.AddRect(cursorPos + new Vector2(posOffset,2), new Vector2(1,UIStyle.TextSize-4), UIStyle.ButtonTextColor, zIndex:1);
        }

        if (textChanged && beingEdited)
        {
            SetStoredText(itemId, allText);
        }
        
        if (inactive)
        {
            Renderer.AddRect(pos, size, new Color4(0,0,0,0.3f), CornerRadii.All(UIStyle.ButtonCornerRadius), zIndex:2);
        }
        
        Advance(0, size.Y + UIStyle.ItemSpacing, size.X + textSize.X, size.Y);
        ReturnToBaseline();

        if (textInputFlags.HasFlag(TextInputFlags.ExitReturnsTrue))
        {
            if (exited)
            {
                value = allText;
                SetStoredText(itemId, null);
            }
            return exited;
        }
        else
        {
            if (textChanged)
                value = allText;
            
            return textChanged;
        }
    }
}