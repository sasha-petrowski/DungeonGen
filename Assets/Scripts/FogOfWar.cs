using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.Image;

[RequireComponent(typeof(MeshFilter))]
public class FogOfWar : MonoBehaviour
{
    [Range(1f, 10f)]
    public float Distance = 5;

    [Range(1, 10)]
    public int RayRecursion = 3;

    [Range(1, 30)]
    public int RaycountPerSide = 9;

    private float[] _cos;
    private float[] _sin;
    private Vector2[] _dir;

    private Vector2[] _points;
    private Vector2[] _normals;
    private bool[] _hits;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;

    private MeshFilter _meshFilter;

    private int _vCount;
    private int _tCount;

    private void OnValidate()
    {
        Initialise();
    }
    private void Awake()
    {
        Initialise();
    }

    private void LateUpdate()
    {
        Generate();
    }
    private void AddTriangle(int a, int b, int c)
    {
        _triangles[_tCount] = a;
        _triangles[_tCount+1] = b;
        _triangles[_tCount+2] = c;
        _tCount += 3;
    }
    private void Initialise()
    {
        _meshFilter = GetComponent<MeshFilter>();

        #region Bake Sin Cos & Dir
        _cos = new float[RaycountPerSide * 4];
        _sin = new float[RaycountPerSide * 4];
        _dir = new Vector2[RaycountPerSide * 4];

        for (int i = 0; i < RaycountPerSide * 4; i++) 
        {
            float angle = (90f / RaycountPerSide) * i * Mathf.Deg2Rad;

            _cos[i] = Mathf.Cos(angle);
            _sin[i] = Mathf.Sin(angle);
            _dir[i] = new Vector2(_cos[i], _sin[i]);
        }
        #endregion

        _vertices = new Vector3[1 + RaycountPerSide * 4 * RayRecursion];
        _triangles = new int[RaycountPerSide * 4 * RayRecursion * 3];


        _points = new Vector2[RaycountPerSide * 4];
        _normals = new Vector2[RaycountPerSide * 4];
        _hits = new bool[RaycountPerSide * 4];
    }
    private bool FindBestLink(Vector2 normal, Vector2 self, Vector2 other, out Vector2 bestHit, out Vector2 bestRay)
    {
        Vector2 origin = transform.position;

        other.Normalize();
        float mag = self.magnitude;
        self /= mag;

        bestRay = other;

        float fraction = 0.5f;
        float lerp = 0.5f;
        Vector2 dir;

        float bestMag = mag;

        Gizmos.color = Color.white;

        for (int i = 0; i < RayRecursion; i++)
        {
            fraction /= 2f;
            dir = (self * lerp + other * (1f - lerp)).normalized;

            RaycastHit2D hit = Physics2D.Raycast(origin, dir, Distance, (int)UnityLayerMask.Wall);


            if (hit.collider)
            {
                lerp -= fraction;

                float newMag = (hit.point - (Vector2)transform.position).magnitude;
                if (hit.normal == normal | newMag < bestMag) bestMag = newMag;
            }
            else
            {
                lerp += fraction;
                bestRay = dir;
            }
        }

        bestHit = bestRay * bestMag;
        bestRay *= Distance;

        return true;
    }
    private Vector2 ProjectLink(float distance, Vector2 direction, float lerp, float angle)
    {
        float angleA = (90f / RaycountPerSide) * lerp;
        float angleB = angle - 90;
        float angleC = 180 - angleA - angleB;

        float edgeB = (distance * Mathf.Sin(angleB * Mathf.Deg2Rad)) / Mathf.Sin(angleC * Mathf.Deg2Rad);

        return direction * edgeB;
    }
    public void Generate()
    {
        Vector2 origin = transform.position;

        _vertices[0] = Vector3.zero;

        _vCount = RaycountPerSide * 4 + 1;
        _tCount = 0;

        #region bake circle of points with raycast
        for (int i = 0; i < RaycountPerSide * 4; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, _dir[i], Distance, (int)UnityLayerMask.Wall);
            if (hit.collider)
            {
                _points[i] = hit.point - origin;
                _normals[i] = hit.normal;
                _hits[i] = true;
            }
            else
            {
                _points[i] = _dir[i] * Distance;
                _hits[i] = false;
            }

            _vertices[i+1] = _points[i];
        }
        #endregion


        for (int i = 0; i < RaycountPerSide * 4; i++)
        {
            int next = (i + 1) % (RaycountPerSide * 4);
            int tIndex = i + 1;
            int tNext = next + 1;

            #region Create mesh
            if (_hits[i])
            {
                if (_hits[next] && _normals[i] == _normals[next])
                {
                    AddTriangle(0, tNext, tIndex);
                }
                else
                {
                    if (FindBestLink(_normals[i], _points[i], _points[next], out Vector2 bestHit, out Vector2 bestRay))
                    {
                        _vertices[_vCount] = bestHit;
                        AddTriangle(0, _vCount, tIndex);
                        _vCount++;

                        _vertices[_vCount] = bestRay;
                        AddTriangle(0, tNext, _vCount);
                        _vCount++;
                    }
                }
            }
            else
            {

                if (_hits[next])
                {
                    if (FindBestLink(_normals[next], _points[next], _points[i], out Vector2 bestHit, out Vector2 bestRay))
                    {
                        _vertices[_vCount] = bestRay;
                        AddTriangle(0, _vCount, tIndex);
                        _vCount++;

                        _vertices[_vCount] = bestHit;
                        AddTriangle(0, tNext, _vCount);
                        _vCount++;
                    }
                }
                else
                {
                    AddTriangle(0, tNext, tIndex);
                }
            }
            #endregion

            #region Gizmos
            /*
            Vector2 point = transform.position + (Vector3)_points[i];

            if (_hits[i])
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(point, point - _normals[i]);

                if (_hits[next] && _normals[i] == _normals[next])
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, point);
                    Gizmos.DrawLine(point, transform.position + (Vector3)_points[next]);
                }
                else
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(transform.position, point);

                    if (FindBestLink(_normals[i], _points[i], _points[next], out Vector2 bestHit, out Vector2 bestRay))
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(transform.position + (Vector3)_points[next], transform.position + (Vector3)bestRay);
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(point, transform.position + (Vector3)bestHit);
                        //Gizmos.DrawLine(transform.position, transform.position + (Vector3)bestRay);
                    }
                }
            }
            else
            {

                if (_hits[next])
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(transform.position, point);

                    if (FindBestLink(_normals[next], _points[next], _points[i], out Vector2 bestHit, out Vector2 bestRay))
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(point, transform.position + (Vector3)bestRay);
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(transform.position + (Vector3)_points[next], transform.position + (Vector3)bestHit);
                        //Gizmos.DrawLine(transform.position, transform.position + (Vector3)bestRay);
                    }
                }
                else
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, point);
                    Gizmos.DrawLine(point, transform.position + (Vector3)_points[next]);
                }
            }
            */
            #endregion
        }

        GameObject.DestroyImmediate(_mesh);

        _mesh = new Mesh();
        //Debug.Log($"{vCount} / {tCount}");
        _mesh.SetVertices(_vertices, 0, _vCount);
        _mesh.SetTriangles(_triangles, 0, _tCount, 0);

        _meshFilter.mesh = _mesh;
    }
    private void OnDrawGizmosSelected()
    {
        if(!Application.isPlaying) Generate();
    }
}
