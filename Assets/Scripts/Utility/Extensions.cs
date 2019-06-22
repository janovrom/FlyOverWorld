using UnityEngine;

namespace Assets.Scripts.Utility
{

    /// <summary>
    /// Provides specific mathematic operations as lerp and bilerp.
    /// </summary>
    public static class MathExt
    {

        /// <summary>
        /// Do linear interpolation from val0 to val2.
        /// </summary>
        /// <param name="val0"></param>
        /// <param name="val1"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float Lerp(float val0, float val1, float t)
        {
            return (1.0f - t) * val0 + t * val1;
        }

        /// <summary>
        /// Do bilinear interpolation.
        /// </summary>
        /// <param name="val0"></param>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <param name="val3"></param>
        /// <param name="t0"></param>
        /// <param name="t1"></param>
        /// <returns></returns>
        public static float Bilerp(float val0, float val1, float val2, float val3, float t0, float t1)
        {
            float val01 = Lerp(val0, val1, t0);
            float val23 = Lerp(val2, val3, t0);
            return Lerp(val01, val23, t1);
        }

    }

    /// <summary>
    /// Extensions to existing classes in Unity as Vector3 or Vector2.
    /// </summary>
    public static class Extensions
    {

        public static Vector3 DistancePerCoordinate(this Vector3 u, Vector3 v)
        {
            return new Vector3(Mathf.Abs(u.x - v.x), Mathf.Abs(u.y - v.y), Mathf.Abs(u.z - v.z));
        }

        public static Vector3 Min(this Vector3 min, Vector3 max)
        {
            return new Vector3(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y), Mathf.Min(min.z, max.z));
        }

        public static Vector3 Max(this Vector3 max, Vector3 min)
        {
            return new Vector3(Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y), Mathf.Max(min.z, max.z));
        }

        public static Vector2 Min(this Vector2 min, Vector2 max)
        {
            return new Vector2(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y));
        }

        public static Vector3 Div(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static Vector2 Max(this Vector2 max, Vector2 min)
        {
            return new Vector2(Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
        }

        public static Vector2 xz(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector2 xy(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector2 yz(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector3 ToVector3xz(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }

        /// <summary>
        /// Creates color from vector and alpha.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Color rgba(this Vector3 v, float a)
        {
            return new Color(v.x, v.y, v.z, a);
        }

        public static bool EqualsRGB(this Color c1, Color c2)
        {
            return Mathf.Abs(c1.r - c2.r) <= 0.001f && Mathf.Abs(c1.g - c2.g) <= 0.001f && Mathf.Abs(c1.b - c2.b) <= 0.001f;
        }

    }

    /// <summary>
    /// Class for storing position of tiles. Position is given in x,y 
    /// and in integer values.
    /// </summary>
    public class Vector2i
    {
        public int x;
        public int y;

        public Vector2i()
        {
            x = 0;
            y = 0;
        }

        public Vector2i(Vector2i v)
        {
            this.x = v.x;
            this.y = v.y;
        }

        public Vector2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override int GetHashCode()
        {
            // Since maximum zoom is 15, it is enough to make it to 1 integer - always unique
            return ((x << 16) | (y & 0xffff)).GetHashCode();
        }

        public static bool operator ==(Vector2i obj1, Vector2i obj2)
        {
            if (object.ReferenceEquals(obj1, null) || object.ReferenceEquals(obj2, null))
                return false;

            return obj1.x == obj2.x && obj1.y == obj2.y;
        }

        public static bool operator !=(Vector2i obj1, Vector2i obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Vector2i;
            if (other == null)
                return false;

            return this.x == other.x && this.y == other.y;
        }

        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }
    }

    /// <summary>
    /// Support class for longitude and latitude. Used for easier understanding.
    /// </summary>
    public class LatLon
    {

        public double latitude;
        public double longitude;

        public LatLon(double latitude, double longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
        }

    }
}
