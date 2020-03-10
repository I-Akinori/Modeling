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
    public int Count
    {
        set { count = value; }
        get { return count; }
    }
    public int Index
    {
        set { index = value; }
        get { return index; }
    }
}

public class DEdge
{
    DPoint[] ends;
    Triangle[] faces;

    public DEdge(DPoint P1, DPoint P2, Triangle T1, Triangle T2) {      // 初期設定は全部やる必要がある
        ends = new DPoint[] { P1, P2 };
        faces = new Triangle[] { T1, T2 };                              // rootの周りだけは面が1つなので同じものを入れておく
    }

    public DPoint[] Ends
    {
        set { ends = value; }
        get { return ends; }
    }
    public Triangle[] Faces
    {
        set { faces = value; }
        get { return faces; }
    }
}
public class Triangle                                                       //        v2
{                                                                           //        /|
    DPoint[] vertices;                                                      //    e1 / |
    DEdge[] edges;                                                          //      /  |e0 
    List<Triangle> children;                                                //     /___|
                                                                            //  v0  e2 v1
    public Triangle(DPoint P1, DPoint P2, DPoint P3)
    {
        vertices = new DPoint[] {P1, P2, P3};
        edges = new DEdge[3];                   // エッジの設定は自分でやる
        children = new List<Triangle>();
    }

    public bool include(DPoint P)
    {
        bool b1 = (vertices[1].Pos.x - P.Pos.x) * (vertices[2].Pos.y - P.Pos.y) - (vertices[2].Pos.x - P.Pos.x) * (vertices[1].Pos.y - P.Pos.y) > 0f;
        bool b2 = (vertices[2].Pos.x - P.Pos.x) * (vertices[0].Pos.y - P.Pos.y) - (vertices[0].Pos.x - P.Pos.x) * (vertices[2].Pos.y - P.Pos.y) > 0f;
        bool b3 = (vertices[0].Pos.x - P.Pos.x) * (vertices[1].Pos.y - P.Pos.y) - (vertices[1].Pos.x - P.Pos.x) * (vertices[0].Pos.y - P.Pos.y) > 0f;

        return (b1 == b2) && (b2 == b3);
    }
    public bool circleinclude(DPoint P)
    {
        Vector2 Center = new Vector2(0, 0);

        float x1 = vertices[0].Pos.x;
        float x2 = vertices[1].Pos.x;
        float x3 = vertices[2].Pos.x;
        float y1 = vertices[0].Pos.y;
        float y2 = vertices[1].Pos.y;
        float y3 = vertices[2].Pos.y;

        Center.x = ((y1 - y3) * (y1 * y1 - y2 * y2 + x1 * x1 - x2 * x2) - (y1 - y2) * (y1 * y1 - y3 * y3 + x1 * x1 - x3 * x3)) / (2 * (y1 - y3) * (x1 - x2) - 2 * (y1 - y2) * (x1 - x3));
        Center.y = ((x3 - x1) * (x1 * x1 - x2 * x2 + y1 * y1 - y2 * y2) + (x1 - x2) * (x1 * x1 - x3 * x3 + y1 * y1 - y3 * y3)) / (2 * (y1 - y3) * (x1 - x2) - 2 * (y1 - y2) * (x1 - x3));

        return Vector2.Distance(Center, P.Pos) < Vector2.Distance(Center, vertices[0].Pos);
    }

    public DPoint[] Vertices
    {
        set { vertices = value; }
        get { return vertices; }
    }
    public DEdge[] Edges
    {
        set { edges = value; }
        get { return edges; }
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
    List<Triangle> LefList = new List<Triangle>();
    List<DPoint> PoiList = new List<DPoint>();
    float length = 10.0f;
    Triangle root;
    void List2Mesh()
    {
        Array.Resize(ref _positions, PoiList.Count);
        Array.Resize(ref _triangles, LefList.Count * 3);
        Array.Resize(ref _normals, PoiList.Count);
        Array.Resize(ref _uvs, PoiList.Count);

        for (int i = 0; i < _positions.Length; i++)
        {
            _positions[i] = PoiList[i].Pos;
        }

        for (int i = 0; i * 3 < _triangles.Length; i++)
        {
            _triangles[3 * i] = LefList[i].Vertices[0].Index;
            _triangles[3 * i + 1] = LefList[i].Vertices[1].Index;
            _triangles[3 * i + 2] = LefList[i].Vertices[2].Index;
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

        T.Children[0].Edges[0] = T.Edges[2];
        T.Children[1].Edges[0] = T.Edges[0];
        T.Children[2].Edges[0] = T.Edges[1];

        T.Children[0].Edges[2] = T.Children[2].Edges[1] = new DEdge(P, T.Vertices[0], T.Children[0], T.Children[2]);
        T.Children[0].Edges[1] = T.Children[1].Edges[2] = new DEdge(P, T.Vertices[1], T.Children[0], T.Children[1]);
        T.Children[1].Edges[1] = T.Children[2].Edges[2] = new DEdge(P, T.Vertices[2], T.Children[1], T.Children[2]);

                                                                                // もともとの三角形の稜線について、参照先の三角形を分割後のものに変更する
        if (T.Edges[0].Faces[0] == T) T.Edges[0].Faces[0] = T.Children[1];      // 一番外側にある辺のFacesは同じものを指すようにしている
        if (T.Edges[0].Faces[1] == T) T.Edges[0].Faces[1] = T.Children[1];
        if (T.Edges[1].Faces[0] == T) T.Edges[1].Faces[0] = T.Children[2];
        if (T.Edges[1].Faces[1] == T) T.Edges[1].Faces[1] = T.Children[2];
        if (T.Edges[2].Faces[0] == T) T.Edges[2].Faces[0] = T.Children[0];
        if (T.Edges[2].Faces[1] == T) T.Edges[2].Faces[1] = T.Children[0];

        PoiList.Add(P);
;       TriList.Remove(T);
        TriList.Add(T.Children[0]);
        TriList.Add(T.Children[1]);
        TriList.Add(T.Children[2]);

        
        // 違反辺チェック&フリップ処理
        List<DEdge> doubtful = new List<DEdge>();
        doubtful.Add(T.Edges[0]);
        doubtful.Add(T.Edges[1]);
        doubtful.Add(T.Edges[2]);

        while(doubtful.Count > 0)
        {
            if (doubtful[0].Faces[0] == doubtful[0].Faces[1])                   // 一番外側の辺なら無条件で違反辺疑惑解除
            {
                doubtful.RemoveAt(0);
                continue;
            }

            // 疑わしい辺がFaces[1]にとって何番目なのか：　向かい側の頂点がFaces[1]にとって何番目なのか　を調べる
            int index0 = 0;
            int index1 = 0;
            for (int i = 0; i < 3; i++)
            {
                if (doubtful[0].Faces[1].Edges[i] == doubtful[0])
                {
                    index1 = i;
                    break;
                }  
            }

            if (doubtful[0].Faces[0].circleinclude(doubtful[0].Faces[1].Vertices[index1]))                   // Faces[0]の外接円がFaces[1]上の向かい側の点を含むなら
            {
                
                Triangle T0 = doubtful[0].Faces[0];
                Triangle T1 = doubtful[0].Faces[1];

                for (int i = 0; i < 3; i++)
                {   
                    // 同じようにFaces[0]についても頂点インデックスを調べる
                    if (doubtful[0].Faces[0].Edges[i] == doubtful[0])
                    {
                        index0 = i;
                        break;
                    }
                }

                // 新たな稜線・面の生成
                DEdge Flipped = new DEdge(T0.Vertices[index0], T1.Vertices[index1],
                    new Triangle(T0.Vertices[index0], T1.Vertices[index1], T0.Vertices[(index0 + 2) % 3]),
                    new Triangle(T1.Vertices[index1], T0.Vertices[index0], T1.Vertices[(index1 + 2) % 3])
                );

                // 新たな面の参照関係の変更
                Flipped.Faces[0].Edges[0] = T1.Edges[(index1 + 2) % 3];
                Flipped.Faces[0].Edges[1] = T0.Edges[(index0 + 1) % 3];
                Flipped.Faces[0].Edges[2] = Flipped;
                Flipped.Faces[1].Edges[0] = T0.Edges[(index0 + 2) % 3];
                Flipped.Faces[1].Edges[1] = T1.Edges[(index1 + 1) % 3];
                Flipped.Faces[1].Edges[2] = Flipped;

                // 元の面についての処理
                T0.Children.Add(Flipped.Faces[0]);
                T0.Children.Add(Flipped.Faces[1]);
                T1.Children.Add(Flipped.Faces[0]);
                T1.Children.Add(Flipped.Faces[1]);

                // 周辺の稜線についての処理
                if (T0.Edges[(index0 + 1) % 3].Faces[0] == T0) T0.Edges[(index0 + 1) % 3].Faces[0] = Flipped.Faces[0];
                if (T0.Edges[(index0 + 1) % 3].Faces[1] == T0) T0.Edges[(index0 + 1) % 3].Faces[1] = Flipped.Faces[0];
                if (T0.Edges[(index0 + 2) % 3].Faces[0] == T0) T0.Edges[(index0 + 2) % 3].Faces[0] = Flipped.Faces[1];
                if (T0.Edges[(index0 + 2) % 3].Faces[1] == T0) T0.Edges[(index0 + 2) % 3].Faces[1] = Flipped.Faces[1];

                if (T1.Edges[(index1 + 1) % 3].Faces[0] == T1) T1.Edges[(index1 + 1) % 3].Faces[0] = Flipped.Faces[1];
                if (T1.Edges[(index1 + 1) % 3].Faces[1] == T1) T1.Edges[(index1 + 1) % 3].Faces[1] = Flipped.Faces[1];
                if (T1.Edges[(index1 + 2) % 3].Faces[0] == T1) T1.Edges[(index1 + 2) % 3].Faces[0] = Flipped.Faces[0];
                if (T1.Edges[(index1 + 2) % 3].Faces[1] == T1) T1.Edges[(index1 + 2) % 3].Faces[1] = Flipped.Faces[0];

                // 描画のためのリストの処理
                TriList.Remove(T0);
                TriList.Remove(T1);
                TriList.Add(Flipped.Faces[0]);
                TriList.Add(Flipped.Faces[1]);

                // 疑惑リストの変更
                doubtful.RemoveAt(0);
                doubtful.Add(T0.Edges[(index0 + 1) % 3]);
                doubtful.Add(T0.Edges[(index0 + 2) % 3]);
                doubtful.Add(T1.Edges[(index1 + 1) % 3]);
                doubtful.Add(T1.Edges[(index1 + 2) % 3]);
                
            } else
            {
                doubtful.RemoveAt(0);
            }
        }
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

        root.Edges[0] = new DEdge(root.Vertices[1], root.Vertices[2], root, root);
        root.Edges[1] = new DEdge(root.Vertices[2], root.Vertices[0], root, root);
        root.Edges[2] = new DEdge(root.Vertices[0], root.Vertices[1], root, root);

        TriList.Add(root);
        PoiList.Add(root.Vertices[0]);
        PoiList.Add(root.Vertices[1]);
        PoiList.Add(root.Vertices[2]);
        
        for (int i = 0; i < Sampling.confirmed.Count; i++)
        {
            DPoint DP = new DPoint(Sampling.confirmed[i]);

            Triangle Target = SearchLeaf(DP, root);
            AddDPoint(DP, Target);
        }

        bool outTriangle;
        for (int i = TriList.Count - 1; i >= 0 ; i--)
        {
            outTriangle = false;

            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    if (TriList[i].Vertices[j] == PoiList[k])
                    {
                        outTriangle = true;
                        break;
                    }
                        
                }
                if (outTriangle)
                {
                    TriList.RemoveAt(i);
                    break;
                }
            }
        }

        
        for (int i = 2; i >= 0; i--)
        {
            PoiList.RemoveAt(i);
        }

        DPoint P = PoiList[0];
        P.Count -= 3;

        for (int i = 0; i < PoiList.Count; i++)
        {
            PoiList[i].Index -= 3;
        }

        for (int i = 0; i < TriList.Count; i++)
        {
            if (TriList[i].Children.Count == 0) LefList.Add(TriList[i]);
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
