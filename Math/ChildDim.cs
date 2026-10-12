namespace VoxUI.Math;

public struct ChildDim(float px=0, float py=0, float ox=0, float oy=0, float pw=0, float ph=0, float ow=0, float oh=0)
{
    public float percentX = px;
    public float percentY = py;
    public float offsetX  = ox;
    public float offsetY  = oy;

    public float percentWidth  = pw;
    public float percentHeight = ph;
    public float offsetWidth   = ow;
    public float offsetHeight  = oh;

    public static ChildDim Percent(float percentX, float percentY, float percentWidth, float percentHeight)
    {
        return new ChildDim
        {
            percentX = percentX,
            percentY = percentY,
            percentWidth = percentWidth,
            percentHeight = percentHeight,
        };
    }

    public static ChildDim Offset(float X, float Y, float Width, float Height)
    {
        return new ChildDim
        {
            offsetX = X,
            offsetY = Y,
            offsetWidth = Width,
            offsetHeight = Height,
        };
    }
}