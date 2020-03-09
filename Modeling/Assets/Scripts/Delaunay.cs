using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
    Point[] vertices;
    Triangle parent;
    List<Triangle> children;
    
    public Triangle(Point P1, Point P2, Point P3)
    {
        vertices = new Point[] {P1, P2, P3};
        children = new List<Triangle>();
    }

    public bool include(Point P)
    {
        return true;
    }
    public Point[] Vertices
    {
        set { vertices = value; }
        get { return vertices; }
    }
    public Triangle Parent
    {
        set { parent = value; }
        get { return parent; }
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
    List<Point> PoiList = new List<Point>();
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
    }
    // Start is called before the first frame update
    void Start()
    {
        length = Sampling.length;
        root = new Triangle(
            new Point(new Vector2(0, +2.0f * length)),
            new Point(new Vector2(+1.5f * length, -length)),
            new Point(new Vector2(-1.5f * length, -length))
        );
        TriList.Add(root);
        PoiList.Add(root.Vertices[0]);
        PoiList.Add(root.Vertices[1]);
        PoiList.Add(root.Vertices[2]);
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
