namespace VoxUI.Math;

public struct Vector3(float x=0, float y=0, float z=0)
{
    public float X = x;
    public float Y = y;
    public float Z = z;

    public static implicit operator System.Numerics.Vector3(Vector3 v)
    {
        return new System.Numerics.Vector3(v.X, v.Y, v.Z);
    }
}