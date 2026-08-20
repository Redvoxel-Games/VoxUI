namespace VoxUI.Math;

public struct Vector4(float x = 0, float y = 0, float z = 0, float w = 1)
{
    public float X = x;
    public float Y = y;
    public float Z = z;
    public float W = w;

    public static implicit operator System.Numerics.Vector4(Vector4 v)
    {
        return new System.Numerics.Vector4(v.X, v.Y, v.Z, v.W);
    }
}