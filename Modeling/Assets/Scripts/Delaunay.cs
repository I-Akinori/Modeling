using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DPoint
{
    private static int count = 0;
    private Vector2 pos;
    private int index;

    public DPoint(float x, float y)
    {
        index = count;
        count++;
        pos = new Vector2(x, y);
    }

    public DPoint(Vector2 v)
    {
        index = count;
        count++;
        pos = v;
    }

    public DPoint(Point P)
    {
        index = count;
        count++;
        pos = P.Pos;
    }

    public Vector2 Pos
    {
        set { pos = value; }
        get { return pos; }
    }
    public int Index
    {
        set { index = value; }
        get { return index; }
    }
}
public class Triangle
{
    DPoint[] vertices;
    List<Triangle> children;
    
    public Triangle(DPoint P1, DPoint P2, DPoint P3)
    {
        vertices = new DPoint[] {P1, P2, P3};
        children = new List<Triangle>();
    }

    public bool include(DPoint P)
    {
        bool b1 = (vertices[1].Pos.x - P.Pos.x) * (vertices[2].Pos.y - P.Pos.y) - (vertices[2].Pos.x - P.Pos.x) * (vertices[1].Pos.y - P.Pos.y) > 0f;
        bool b2 = (vertices[2].Pos.x - P.Pos.x) * (vertices[0].Pos.y - P.Pos.y) - (vertices[0].Pos.x - P.Pos.x) * (vertices[2].Pos.y - P.Pos.y) > 0f;
        bool b3 = (vertices[0].Pos.x - P.Pos.x) * (vertices[1].Pos.y - P.Pos.y) - (vertices[1].Pos.x - P.Pos.x) * (vertices[0].Pos.y - P.Pos.y) > 0f;

        return (b1 == b2) && (b2 == b3);
    }
    public DPoint[] Vertices
    {
        set { vertices = value; }
        get { return vertices; }
    }

    public List<Triangle> Children
    {
        set { children = value; }
        get { return children; }
    }
}
public class Delaunay : MonoBehaviour
{
    [SerializeField]
    private Material _material;
    private Mesh _mesh;
    // 頂点座標
    private Vector3[] _positions = new Vector3[] { };
    private Vector3[] _normals = new Vector3[] { };
    private int[] _triangles = new int[] { };
    private Vector2[] _uvs = new Vector2[] { };
    List<Triangle> TriList = new List<Triangle>();
    List<DPoint> PoiList = new List<DPoint>();
    float length = 10.0f;
    Triangle root;
    void List2Mesh()
    {
        Array.Resize(ref _positions, PoiList.Count);
        Array.Resize(ref _triangles, TriList.Count * 3);
        Array.Resize(ref _normals, PoiList.Count);
        Array.Resize(ref _uvs, PoiList.Count);

        for (int i = 0; i < _positions.Length; i++)
        {
            _positions[i] = PoiList[i].Pos;
        }

        for (int i = 0; i * 3 < _triangles.Length; i++)
        {
            if (TriList[i].Children.Count > 0)
                continue;
            _triangles[3 * i] = TriList[i].Vertices[0].Index;
            _triangles[3 * i + 1] = TriList[i].Vertices[1].Index;
            _triangles[3 * i + 2] = TriList[i].Vertices[2].Index;
        }

        for (int i = 0; i < _positions.Length; i++)
        {
            _normals[i] = new Vector3(0, 0, -1f);
        }

        for (int i = 0; i < _positions.Length; i++)
        {
            _uvs[i] = new Vector2(_positions[i].x / length + 0.5f, _positions[i].y / length + 0.5f);
        }


        _mesh.vertices = _positions;
        _mesh.triangles = _triangles;
        _mesh.normals = _normals;
        _mesh.uv = _uvs;

        _mesh.RecalculateBounds();
        _mesh.RecalculateBounds();
    }

    Triangle SearchLeaf(DPoint P, Triangle T)        // PがTの内部にあることは既知とする
    {
        if (T.Children.Count == 0)
        {
            return T;
        }

        for (int i = 0; i < T.Children.Count; i++)
        {
            if (T.Children[i].include(P))
            {
                return SearchLeaf(P, T.Children[i]);
            }
        }

        return null;
    }
    void AddDPoint(DPoint P, Triangle T)
    {
        T.Children.Add(new Triangle(P, T.Vertices[0], T.Vertices[1]));
        T.Children.Add(new Triangle(P, T.Vertices[1], T.Vertices[2]));
        T.Children.Add(new Triangle(P, T.Vertices[2], T.Vertices[0]));
    }
    // Start is called before the first frame update
    void Start()
    {
        length = Sampling.length;
        root = new Triangle(
            new DPoint(new Vector2(0, +2.0f * length)),
            new DPoint(new Vector2(+1.5f * length, -length)),
            new DPoint(new Vector2(-1.5f * length, -length))
        );
        TriList.Add(root);
        PoiList.Add(root.Vertices[0]);
        PoiList.Add(root.Vertices[1]);
        PoiList.Add(root.Vertices[2]);

        for (int i = 0; i < Sampling.confirmed.Count; i++)
        {
            DPoint DP = new DPoint(Sampling.confirmed[i]);
            PoiList.Add(DP);

            Triangle Target = SearchLeaf(DP, root);
            AddDPoint(DP, Target);
            TriList.Remove(Target);
            TriList.Add(Target.Children[0]);
            TriList.Add(Target.Children[1]);
            TriList.Add(Target.Children[2]);
        }

    }

    private void Awake()
    {
        _mesh = new Mesh();
    }

    // Update is called once per frame
    void Update()
    {
        List2Mesh();
        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);
    }
}
