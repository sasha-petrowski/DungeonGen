using System.Collections.Generic;
using UnityEngine;

namespace Delaunay
{
    public class DTriangle
    {
        public Color DebugColor;

        public DPoint A;
        public DPoint B;
        public DPoint C;

        public DTriangle AB;
        public DTriangle BC;
        public DTriangle CA;

        public List<DTriangle> Parents = new List<DTriangle>();

        public List<DTriangle> Childs = new List<DTriangle>();

        public bool IsLeaf = true;

        public Vector3 Position => (A.Position + B.Position + C.Position) / 3;

        public Vector3 InnerA => (A.Position + (B.Position + C.Position) * 0.01f) / 1.02f;
        public Vector3 InnerB => (B.Position + (A.Position + C.Position) * 0.01f) / 1.02f;
        public Vector3 InnerC => (C.Position + (B.Position + A.Position) * 0.01f) / 1.02f;

        public DTriangle(DPoint a, DPoint b, DPoint c)
        {
            this.A = a;
            this.B = b;
            this.C = c;

            DebugColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        }

        public DTriangle GetLeaf(DPoint point)
        {
            if(IsLeaf)
            {
                return this;
            }

            foreach (DTriangle child in Childs)
            {
                if (child.IsInside(point))
                {
                    return child.GetLeaf(point);
                }
            }
            return null;
        }

        public void Insert(DPoint point)
        {
            IsLeaf = false;

            Insert(point, A, B, AB, out DTriangle ABb, out DTriangle ABc);
            Insert(point, B, C, BC, out DTriangle BCb, out DTriangle BCc);
            Insert(point, C, A, CA, out DTriangle CAb, out DTriangle CAc);

            ABb.AB = CAc;
            CAc.CA = ABb;

            BCb.AB = ABc;
            ABc.CA = BCb;

            CAb.AB = BCc;
            BCc.CA = CAb;
        }
        private void Insert(DPoint a, DPoint b, DPoint c, DTriangle other, out DTriangle left, out DTriangle right)
        {
            #region Lawson flip
            if (other != null)
            {
                DPoint opposite = other.Opposite(b, c);
                Vector2 center = (a.Position + opposite.Position) / 2;
                float distance = Vector2.Distance(center, a.Position);

                if (distance < Vector2.Distance(center, b.Position) && distance < Vector2.Distance(center, c.Position))
                {
                    // Lawson flip

                    left = new DTriangle(a, b, opposite);
                    right = new DTriangle(a, opposite, c);

                    left.CA = right;
                    right.AB = left;

                    DTriangle edgeLeft = other.GetEdge(opposite, b);
                    left.BC = edgeLeft;
                    edgeLeft?.AddEdge(opposite, b, left);

                    DTriangle edgeRight = other.GetEdge(opposite, c);
                    right.BC = edgeRight;
                    edgeRight?.AddEdge(opposite, c, right);

                    other.IsLeaf = false;

                    other.Childs.Add(left);
                    other.Childs.Add(right);
                    left.Parents.Add(other);
                    right.Parents.Add(other);

                    left.Parents.Add(this);
                    right.Parents.Add(this);
                    Childs.Add(left);
                    Childs.Add(right);

                    return;
                }
            }
            #endregion
            #region Normal triangle
            left = new DTriangle(a, b, c);
            right = left;

            left.BC = other;
            other?.AddEdge(b, c, left);

            Childs.Add(left);
            left.Parents.Add(this);
            #endregion
        }


        public DPoint Opposite(DPoint a, DPoint b)
        {
            if (a == A)
            {
                return b == B ? C : B;
            }
            else if (a == B)
            {
                return b == C ? A : C;
            }
            else // a == C
            {
                return b == A ? B : A;
            }
        }
        public DTriangle GetEdge(DPoint a, DPoint b)
        {
            if (a == A)
            {
                return b == B ? AB : CA;
            }
            else if (a == B)
            {
                return b == C ? BC : AB;
            }
            else // a == C
            {
                return b == A ? CA : BC;
            }
        }
        private void AddEdge(DPoint a, DPoint b, DTriangle edge)
        {
            if (a == A)
            {
                if (b == B) AB = edge;
                else CA = edge;
            }
            else if (a == B)
            {
                if (b == C) BC = edge;
                else AB = edge;
            }
            else // a == C
            {
                if (b == A) CA = edge;
                else BC = edge;
            }
        }

        public bool IsInside(DPoint point)
        {
            float as_x = point.x - A.x;
            float as_y = point.y - A.y;

            bool s_ab = (B.x - A.x) * as_y - (B.y - A.y) * as_x > 0;

            if ((C.x - A.x) * as_y - (C.y - A.y) * as_x > 0 == s_ab)
                return false;
            if ((C.x - B.x) * (point.y - B.y) - (C.y - B.y) * (point.x - B.x) > 0 != s_ab)
                return false;
            return true;
        }
        private static float sign(DPoint p1, DPoint p2, DPoint p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }


        public void LinkPoints()
        {
            if (IsLeaf)
            {
                /*
                if (!A.IsVirtual() && !B.IsVirtual()) A.LinkTo(B);
                if (!B.IsVirtual() && !C.IsVirtual()) B.LinkTo(C);
                if (!C.IsVirtual() && !A.IsVirtual()) C.LinkTo(A);
                */
                A.LinkTo(B);
                B.LinkTo(C);
                C.LinkTo(A);
                
            }
            else
            {
                foreach (DTriangle child in Childs)
                {
                    child.LinkPoints();
                }
            }
        }

        public IEnumerable<DTriangle> ExploreTree()
        {
            yield return this;

            foreach (DTriangle child in Childs)
            {
                foreach (DTriangle childBranch in child.ExploreTree())
                {
                    yield return childBranch;
                }
            }
        }
    }
}