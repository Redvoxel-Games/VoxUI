using Silk.NET.Input;
using VoxUI.Math;
using VoxUI.Rendering;

namespace VoxUI.Core;

public record struct TreeNodeResult(bool IsOpen, bool WasClicked);

// ReSharper disable once InconsistentNaming
public partial class VoxUIR
{
    private static readonly Dictionary<ItemId, bool> OpenTreeNodes = [];
    private static readonly Dictionary<ItemId, float> TreeNodeAlignStarts = [];
    private static readonly List<ItemId> TreeNodeBuffer = [];

    private static uint _treeNodeDepth = 0;

    public static TreeNodeResult TreeNode(string itemName, string label, uint? image = null, TreeNodeFlags treeNodeFlags = TreeNodeFlags.None)
    {
        if (CurrentWindow == null)
            throw new Exception("You must call Being() before attempting to draw");

        var textSize = RegularFont.GetTextSize(label);
        
        var itemId = new ItemId(CurrentWindow.Id, itemName);

        bool isOpen;
        var isLeaf = treeNodeFlags.HasFlag(TreeNodeFlags.Leaf);

        if (OpenTreeNodes.TryGetValue(itemId, out var v))
        {
            isOpen = v;
        }
        else
        {
            isOpen = treeNodeFlags.HasFlag(TreeNodeFlags.DefaultOpen);
        }
        
        var pos = GetDrawPositionInScreenSpace();
        var buttonSize = new Vector2(UIStyle.TextSize, UIStyle.TextSize);
        
        if (IsMouseInsideRect(pos, buttonSize) && !isLeaf)
        {
            var bColor = UIStyle.ButtonColor.Lerp(Color4.White, 0.1f);

            if (IsMouseButtonJustReleased(MouseButton.Left))
            {
                isOpen = !isOpen;
            }
            else if (IsMouseButtonDown(MouseButton.Left))
            {
                bColor = UIStyle.ButtonColor.Lerp(Color4.Black, 0.2f);
            }
            
            Renderer.AddRect(pos, buttonSize, bColor);
        }
        
        var nodeImage = isOpen ? Images.DownArrow : Images.RightArrow;
        
        if (!isLeaf) Renderer.AddImage(pos+2, buttonSize-4, nodeImage, 1);

        float imgOffset = 0;
        if (image.HasValue)
        {
            imgOffset = buttonSize.X + 2;
            
            Renderer.AddImage(pos.X + buttonSize.X + 2, pos.Y, UIStyle.TextSize, UIStyle.TextSize, image.Value);
        }
        Renderer.AddText(pos.X + buttonSize.X + 2 + imgOffset, pos.Y, label, UIStyle.TextSize, UIStyle.TextColor);
        
        TreeNodeAlignStarts[itemId] = pos.Y + UIStyle.TextSize + 1;

        if (isOpen)
        {
            _treeNodeDepth++;
            TreeNodeBuffer.Add(itemId);
        }
        OpenTreeNodes[itemId] = isOpen;
        
        Advance(isOpen ? UIStyle.TreeNodeIndentFactor : 0, UIStyle.TextSize + UIStyle.ItemSpacing, buttonSize.X + textSize.X + 2 + imgOffset, buttonSize.Y);
        
        return new TreeNodeResult(isOpen, false);
    }

    public static void EndTreeNode()
    {
        _treeNodeDepth--;

        ItemId itemId = TreeNodeBuffer.Last();
        TreeNodeBuffer.RemoveAt(TreeNodeBuffer.Count - 1);
        float start = TreeNodeAlignStarts[itemId];
        TreeNodeAlignStarts.Remove(itemId);

        var pos = GetDrawPositionInScreenSpace();
        
        float end = pos.Y;
        float height = end - start;
        
        pos -= new Vector2(UIStyle.TreeNodeIndentFactor/2, height);
        
        Renderer.AddRect(pos, new Vector2(1, height-UIStyle.TextSize/2), UIStyle.TreeNodeAlignmentbarColor, zIndex:1);

        
        Advance(-UIStyle.TreeNodeIndentFactor, 0, 0, 0);
    }
}