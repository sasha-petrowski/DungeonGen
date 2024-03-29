using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Delaunay
{
    public class DelaunayGraph
    {
        public DTriangle Root;

        public int Index = 0;
        public bool Finished = false;

        private DPoint[] _points;

        public DelaunayGraph(DPoint[] points, float minX, float maxX, float minY, float maxY)
        {
            _points = points;

            float stepX = (minX - maxX) * 10;
            float stepY = (minY - maxY) * 10;

            Root = new DTriangle(
                new DVirtualPoint(new Vector2((minX + maxX) / 2, 9999)), //top
                new DVirtualPoint(new Vector2( -9999, -9999)), //left
                new DVirtualPoint(new Vector2(  9999, -9999))  //right
                );

            Root.Insert(points[0]);
            Index = 1;
        }
        public void Next()
        {
            if (Index < _points.Length)
            {
                DTriangle leaf = Root.GetLeaf(_points[Index]);
                leaf?.Insert(_points[Index]);
                Index++;
            }
            else
            {
                Finished = true;
                Root.LinkPoints();
            }
        }
        public void Complete()
        {
            try
            {
                while(Index < _points.Length)
                {
                    DTriangle leaf = Root.GetLeaf(_points[Index]);

                    leaf?.Insert(_points[Index]);

                    Index++;
                }

                Finished = true;
                Root.LinkPoints();
            }
            catch (Exception e)
            {
                Finished = true;
                Debug.LogError(e);
                throw;
            }
        }
    }
}