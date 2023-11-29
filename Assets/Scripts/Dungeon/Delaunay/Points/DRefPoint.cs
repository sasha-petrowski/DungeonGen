using UnityEngine;

namespace Delaunay
{
    public class DRefPoint<Tref> : DPoint
    {
        public Tref Reference;

        public DRefPoint(Vector2 position, Tref reference) : base(position)
        {
            Reference = reference;
        }
    }
}