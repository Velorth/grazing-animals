using UnityEngine;

namespace GrazingAnimals
{
    internal static class MathExtensions
    {
        public static Vector2 To2D(this Vector3 v) => new(v.x, v.z);
        
        public static Vector3 To3D(this Vector2 v) => new(v.x, 0, v.y);

    }
}