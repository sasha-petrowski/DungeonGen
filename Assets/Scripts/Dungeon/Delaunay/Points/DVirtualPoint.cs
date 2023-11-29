using UnityEngine;
using UnityEngine.UIElements;

namespace Delaunay
{
    public class DVirtualPoint : DPoint
    {
        public DVirtualPoint(Vector2 position) : base(position) { }

        public override bool IsVirtual() { return true; }
    }
}