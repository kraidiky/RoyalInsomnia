using UnityEngine;

public struct Position {
    public static Position ZERO = new Position(0, 0, 0);
    public static Position ONE = new Position(1, 1, 1);

    public int X;
    public int Y;
    public int Z;
    public Position(int x = 0, int y = 0, int z = 0) {
        X = x;
        Y = y;
        Z = z;
    }
    /// <summary> Длинна вектора в плоскости XY. </summary>
    public float Magnitude() {
        return Mathf.Sqrt(X * X + Y * Y + Z * Z);
    }
    /// <summary> Квадрат длинны вектора в плоскости XY. </summary>
    public int SqrMagnitude() {
        return X * X + Y * Y + Z * Z;
    }
    /// <summary> Дистанция с учётом глубины. </summary>
    public float Magnitude2D() {
        return Mathf.Sqrt(X * X + Y * Y);
    }
    public int SqrMagnitude2D() {
        return X * X + Y * Y;
    }

    public static Position operator +(Position first, Position second) {
        return new Position(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
    }
    public static Position operator -(Position first, Position second) {
        return new Position(first.X - second.X, first.Y - second.Y, first.Z - second.Z);
    }
    public static Position operator -(Position vector) {
        return new Position(-vector.X, -vector.Y, -vector.Z);
    }
    public static int operator *(Position first, Position second) {
        return first.X * second.X + first.Y * second.Y + first.X * second.Y;
    }
    public static Position operator *(int first, Position second) {
        return new Position(first * second.X, first * second.Y, first * second.Z);
    }
    public static Position operator *(Position first, int second) {
        return new Position(first.X * second, first.Y * second, first.Z * second);
    }
    public static Position operator %(Position first, int second) {
        return new Position(first.X % second, first.Y % second, second % second);
    }
    public static explicit operator Vector2(Position source) {
        return new Vector2(source.X, source.Y);
    }
    public static explicit operator Vector3(Position source) {
        return new Vector3(source.X, source.Y, source.Z);
    }
    public override bool Equals(object obj) {
        if (obj is Position)
            Equals((Position)obj);
        return false;
    }
    public bool Equals(Position target) {
        return target.X == X && target.Y == Y;
    }
    public override int GetHashCode() {
        return X ^ Y;
    }
    public static bool operator ==(Position a, Position b) {
        return a.X == b.X && a.Y == b.Y;
    }
    public static bool operator !=(Position a, Position b) {
        return a.X != b.X || a.Y != b.Y;
    }

    public static Position Round(Vector3 source) {
        return new Position(Mathf.RoundToInt(source.x), Mathf.RoundToInt(source.y), Mathf.RoundToInt(source.z));
    }
    public static Position Min(Position p1, Position p2) {
        return new Position(Mathf.Min(p1.X, p2.X), Mathf.Min(p1.Y, p2.Y));
    }
    public static Position Max(Position p1, Position p2) {
        return new Position(Mathf.Max(p1.X, p2.X), Mathf.Max(p1.Y, p2.Y));
    }
    public Position Between(Position min, Position max) {
        return new Position(Mathf.Min(max.X, Mathf.Max(min.X, X)), Mathf.Min(max.Y, Mathf.Max(min.Y, Y)));
    }
    public Position XBetween(int min, int max) {
        return new Position(Mathf.Min(max, Mathf.Max(min, X)), Y);
    }
    public static Vector2 Between(Vector2 target, Vector2 min, Vector2 max) {
        return new Vector2(Mathf.Min(max.x, Mathf.Max(min.x, target.x)), Mathf.Min(max.y, Mathf.Max(min.y, target.y)));
    }

    public int this[int i] {
        get {
            if (i == 0) {
                return X;
            } else if (i == 1) {
                return Y;
            } else {
                return 0;
            }
        }
        set {
            if (i == 0) {
                X = value;
            } else if (i == 1) {
                Y = value;
            }
        }
    }
    public override string ToString() {
        return "(" + X + "," + Y + "," + Z + ")";
    }
}
