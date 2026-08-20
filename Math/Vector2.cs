namespace VoxUI.Math;

public struct Vector2(float x=0, float y=0)
{
    public float X = x, Y = y;

    public static Vector2 operator +(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X + b.X, a.Y + b.Y);
    }

    public static Vector2 operator +(Vector2 a, float b)
    {
        return new Vector2(a.X + b, a.Y + b);
    }

    public static Vector2 operator -(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X - b.X, a.Y - b.Y);
    }

    public static Vector2 operator -(Vector2 a, float b)
    {
        return new Vector2(a.X - b, a.Y - b);
    }

    public static Vector2 operator *(Vector2 a, Vector2 b)
    {
        return new Vector2(a.X * b.X, a.Y * b.Y);
    }

    public static Vector2 operator *(Vector2 vec, float scl)
    {
        return new Vector2(vec.X * scl, vec.Y * scl);
    }

    public static Vector2 operator /(Vector2 vec, float scl)
    {
        return new Vector2(vec.X / scl, vec.Y / scl);
    }

    public static implicit operator System.Numerics.Vector2(Vector2 vec)
    {
        return new System.Numerics.Vector2(vec.X, vec.Y);
    }
    
    public float Magnitude => MathF.Sqrt(X * X + Y * Y);
    public Vector2 Normalized => this/Magnitude;
    
    public Vector2 Clone()
    {
        return new Vector2(X, Y);
    }

    public override string ToString()
    {
        return $"({X},{Y})";
    }
}