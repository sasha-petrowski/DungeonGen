using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

namespace Delaunay
{
    public class DPoint
    {
        public float x => Position.x;
        public float y => Position.y;

        public Vector2 Position;

        public List<DPoint> Links = new List<DPoint>();



        public DPoint(Vector2 position)
        {
            Position = position;
        }

        public virtual bool IsVirtual() { return false; }

        public void LinkTo(DPoint other)
        {
            if (!Links.Contains(other))
            {
                Links.Add(other);
                other.Links.Add(this);
            }
        }
        public void RemoveLink(DPoint other)
        {
            Links.Remove(other);
            other.Links.Remove(this);
        }


        // Given three collinear points p, q, r, the function checks if 
        // point q lies on line segment 'pr' 
        static bool OnSegment(DPoint p1, DPoint p2, DPoint other)
        {
            if (p2.x <= Math.Max(p1.x, other.x) && p2.x >= Math.Min(p1.x, other.x) &&
                p2.y <= Math.Max(p1.y, other.y) && p2.y >= Math.Min(p1.y, other.y))
                return true;

            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are collinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        static int Orientation(DPoint p1, DPoint p2, DPoint other)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/ 
            // for details of below formula. 
            int val = Mathf.RoundToInt((p2.y - p1.y) * (other.x - p2.x) - (p2.x - p1.x) * (other.y - p2.y));

            if (val == 0) return 0; // collinear 

            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }

        // The main function that returns true if line segment 'p1q1' 
        // and 'p2q2' intersect. 
        public static bool Intersect(DPoint a1, DPoint a2, DPoint b1, DPoint b2)
        {
            // Find the four orientations needed for general and 
            // special cases 
            int o1 = Orientation(a1, a2, b1);
            int o2 = Orientation(a1, a2, b2);
            int o3 = Orientation(b1, b2, a1);
            int o4 = Orientation(b1, b2, a2);

            // General case 
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases 
            // p1, q1 and p2 are collinear and p2 lies on segment p1q1 
            if (o1 == 0 && OnSegment(a1, b1, a2)) return true;

            // p1, q1 and q2 are collinear and q2 lies on segment p1q1 
            if (o2 == 0 && OnSegment(a1, b2, a2)) return true;

            // p2, q2 and p1 are collinear and p1 lies on segment p2q2 
            if (o3 == 0 && OnSegment(b1, a1, b2)) return true;

            // p2, q2 and q1 are collinear and q1 lies on segment p2q2 
            if (o4 == 0 && OnSegment(b1, a2, b2)) return true;

            return false; // Doesn't fall in any of the above cases 
        }
    }
}