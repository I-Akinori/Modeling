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
    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
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
        T.Children[0].Edges[1] = T.Children[1].Edges[2] = new DEdge(P, T.Vertices[1], T.Children[1], T.Children[0]);
        T.Children[1].Edges[1] = T.Children[2].Edges[2] = new DEdge(P, T.Vertices[2], T.Children[2], T.Children[1]);

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

    bool Split(DEdge E)
    {
        int index0 = 0;
        int index1 = 0;

        DPoint MidP = new DPoint((E.Ends[0].Pos + E.Ends[1].Pos) / 2.0f);

        PoiList.Add(MidP);
        Triangle F0 = E.Faces[0];
        Triangle F1 = E.Faces[1];

        for (int i = 0; i < 3; i++)
        {
            if (F0.Edges[i] == E)
            {
                index0 = i;
                break;
            }
        }

        Triangle NewF0 = new Triangle(F0.Vertices[index0], MidP, F0.Vertices[(index0 + 2) % 3]);
        LefList.Add(NewF0);
        NewF0.Edges[1] = F0.Edges[(index0 + 1) % 3];
        DEdge split0 = new DEdge(F0.Vertices[index0], MidP, NewF0, F0);
        NewF0.Edges[2] = split0;
        F0.Edges[(index0 + 1) % 3] = split0;
        F0.Vertices[(index0 + 2) % 3] = MidP;
        F0.Edges[index0].Ends[1] = MidP;
        F0.Edges[index0].Ends[0] = F0.Vertices[(index0 + 1) % 3];

        if (NewF0.Edges[1].Faces[0] == F0) NewF0.Edges[1].Faces[0] = NewF0;
        if (NewF0.Edges[1].Faces[1] == F0) NewF0.Edges[1].Faces[1] = NewF0;

        if (F0 == F1)
        {
            
            DEdge add0 = new DEdge(NewF0.Vertices[1], NewF0.Vertices[2], NewF0, NewF0);
            NewF0.Edges[0] = add0;

        } else
        {
            
            for (int i = 0; i < 3; i++)
            {
                if (E.Faces[1].Edges[i] == E)
                {
                    index1 = i;
                    break;
                }
            }

            Triangle NewF1 = new Triangle(F1.Vertices[index1], F1.Vertices[(index1 + 1) % 3], MidP);
            LefList.Add(NewF1);
            NewF0.Edges[0] = new DEdge(MidP, NewF0.Vertices[2], NewF0, NewF1);
            F1.Vertices[(index1 + 1) % 3] = MidP;
            DEdge split1 = new DEdge(F1.Vertices[index1], MidP, F1, NewF1);
            NewF1.Edges[0] = NewF0.Edges[0];
            NewF1.Edges[1] = split1;
            NewF1.Edges[2] = F1.Edges[(index1 + 2) % 3];
            F1.Edges[(index1 + 2) % 3] = split1;

            if (NewF1.Edges[2].Faces[0] == F1) NewF1.Edges[2].Faces[0] = NewF1;
            if (NewF1.Edges[2].Faces[1] == F1) NewF1.Edges[2].Faces[1] = NewF1;
        }

        return true;
    }

    bool Flip(DEdge E)
    {
        
        int index0 = 0;
        int index1 = 0;

        Triangle F0 = E.Faces[0];
        Triangle F1 = E.Faces[1];

        if (F0 != F1)
        {
            for (int i = 0; i < 3; i++)
            {
                if (F0.Edges[i] == E)
                {
                    index0 = i;
                    break;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (F1.Edges[i] == E)
                {
                    index1 = i;
                    break;
                }
            }

            bool neadFlip = true;

            /*
            Vector2 V01 = F0.Vertices[(index0 + 1) % 3].Pos - F0.Vertices[index0].Pos;
            Vector2 V02 = F0.Vertices[(index0 + 2) % 3].Pos - F0.Vertices[index0].Pos;
            Vector2 V11 = F1.Vertices[(index1 + 1) % 3].Pos - F1.Vertices[index1].Pos;
            Vector2 V12 = F1.Vertices[(index1 + 2) % 3].Pos - F1.Vertices[index1].Pos;

            if ((V01.x * V12.y - V01.y * V12.x) * (V11.x * V02.y - V11.y * V02.x) < 0) neadFlip = false;
            */

            if (!Front(F0.Vertices[index0], F0.Vertices[(index0 + 1) % 3], F1.Vertices[index1]) ||
                !Front(F1.Vertices[index1], F1.Vertices[(index1 + 1) % 3], F0.Vertices[index0])) neadFlip = false;

            if (neadFlip)
            {
                DEdge Flipped = new DEdge(F0.Vertices[index0], F1.Vertices[index1], F1, F0);
                F0.Vertices[(index0 + 2) % 3] = F1.Vertices[index1];
                F0.Edges[index0] = F1.Edges[(index1 + 1) % 3];
                F1.Vertices[(index1 + 2) % 3] = F0.Vertices[index0];
                F1.Edges[index1] = F0.Edges[(index0 + 1) % 3];
                F0.Edges[(index0 + 1) % 3] = Flipped;
                F1.Edges[(index1 + 1) % 3] = Flipped;

                if (F0.Edges[index0].Faces[0] == F1) F0.Edges[index0].Faces[0] = F0;
                if (F0.Edges[index0].Faces[1] == F1) F0.Edges[index0].Faces[1] = F0;
                if (F1.Edges[index1].Faces[0] == F0) F1.Edges[index1].Faces[0] = F1;
                if (F1.Edges[index1].Faces[1] == F0) F1.Edges[index1].Faces[1] = F1;

                return true;
            }
        }
        return false;
    }
    bool Collapse(DEdge E)
    {
        int index0 = 0;
        int index1 = 0;

        DPoint Center = E.Ends[0];
        DPoint Moved = E.Ends[1];

        Triangle F0 = E.Faces[0];
        Triangle F1 = E.Faces[1];

        Triangle tmpF = F0;
        DEdge tmpE;
        DEdge left;
        DEdge right;
        int indexT = 0;
        bool needCollapse = true;

        List<Triangle> needMoveT = new List<Triangle>();
        List<DEdge> needMoveE = new List<DEdge>();
        List<int> Tindex = new List<int>();
        List<int> Eindex = new List<int>();

        int debug = 0;
        if (F0 != F1)
        {
            for (int i = 0; i < 3; i++)
            {
                if (F1.Vertices[i] == Center)
                {
                    index1 = i;
                    break;
                }
            }
            left = F1.Edges[(index1 + 2) % 3];

            for (int i = 0; i < 3; i++)
            {
                if (F0.Vertices[i] == Center)
                {
                    index0 = i;
                    break;
                }
            }
            right = F0.Edges[(index0 + 1) % 3];
            tmpE = right;

            do
            {
                debug++;
                if (debug > 20)
                {
                    Debug.Log("Falure");
                    if (right == left)
                        Debug.Log("right == left");
                    break;
                }

                if (tmpE.Faces[0] == tmpE.Faces[1])                 // 輪郭が変わるような変形はNG → F0.[index0] が端ならこれの移動はあきらめる
                {
                    needCollapse = false;
                    break;
                }

                needMoveE.Add(tmpE);                                // right は要素の最初
                Eindex.Add(tmpE.Ends[0] == Center ? 0 : 1);         
                tmpF = tmpE.Faces[tmpE.Ends[0] == Center ? 0 : 1];

                for (int i = 0; i < 3; i++)
                {
                    if (tmpF.Vertices[i] == Center)
                    {
                        indexT = i;
                        break;
                    }
                }

                if (!Front(Moved, tmpF.Vertices[(indexT + 1) % 3], tmpF.Vertices[(indexT + 2) % 3]))
                {
                    needCollapse = false;
                    break;
                }

                needMoveT.Add(tmpF);
                Tindex.Add(indexT);

                tmpE = tmpF.Edges[(indexT + 1) % 3];

            } while (tmpE != left);

            if (needCollapse)
            {
                for (int i = 0; i < Tindex.Count; i++)
                {
                    needMoveT[i].Vertices[Tindex[i]] = Moved;
                }
                for (int i = 0; i < Eindex.Count; i++)
                {
                    needMoveE[i].Ends[Eindex[i]] = Moved;
                }
                                                                                                        // 左 F1 側の参照関係修正
                if (F1.Edges[index1].Faces[0] == F1.Edges[index1].Faces[1])                                 // left が輪郭辺となるとき
                {
                    if (left.Ends[0] == Center)                                                             
                    {
                        left.Ends[0] = Moved;
                        
                        left.Faces[0] = left.Faces[1];
                    }
                    else
                    {
                        left.Ends[1] = Moved;

                        left.Faces[1] = left.Faces[0];
                    }

                }
                else
                {                                                                                         // leftが輪郭辺とならないとき
                    if (left.Ends[0] == Center)                                                             
                    {
                        left.Ends[0] = Moved;
                        if (F1.Edges[index1].Faces[0] == F1)
                        {
                            left.Faces[0] = F1.Edges[index1].Faces[1];
                        }
                        else
                        {
                            left.Faces[0] = F1.Edges[index1].Faces[0];
                        }
                        if (left.Faces[0].Edges[0] == F1.Edges[index1]) left.Faces[0].Edges[0] = left;
                        if (left.Faces[0].Edges[1] == F1.Edges[index1]) left.Faces[0].Edges[1] = left;
                        if (left.Faces[0].Edges[2] == F1.Edges[index1]) left.Faces[0].Edges[2] = left;
                    }
                    else
                    {
                        left.Ends[1] = Moved;
                        if (F1.Edges[index1].Faces[0] == F1)
                        {
                            left.Faces[1] = F1.Edges[index1].Faces[1];
                        }
                        else
                        {
                            left.Faces[1] = F1.Edges[index1].Faces[0];
                        }
                        if (left.Faces[1].Edges[0] == F1.Edges[index1]) left.Faces[1].Edges[0] = left;
                        if (left.Faces[1].Edges[1] == F1.Edges[index1]) left.Faces[1].Edges[1] = left;
                        if (left.Faces[1].Edges[2] == F1.Edges[index1]) left.Faces[1].Edges[2] = left;
                    }
                }

                if (F0.Edges[index0].Faces[0] == F0.Edges[index0].Faces[1])                                 // left が輪郭辺となるとき
                {
                    right.Faces[(Eindex[0] + 1) % 2] = right.Faces[Eindex[0]];
                }
                else {
                    if (F0.Edges[index0].Faces[0] == F0)                                                    // 右 F0 側の参照関係修正
                    {
                        right.Faces[(Eindex[0] + 1) % 2] = F0.Edges[index0].Faces[1];
                    }
                    else
                    {
                        right.Faces[(Eindex[0] + 1) % 2] = F0.Edges[index0].Faces[0];
                    }

                    if (right.Faces[(Eindex[0] + 1) % 2].Edges[0] == F0.Edges[index0]) right.Faces[(Eindex[0] + 1) % 2].Edges[0] = right;
                    if (right.Faces[(Eindex[0] + 1) % 2].Edges[1] == F0.Edges[index0]) right.Faces[(Eindex[0] + 1) % 2].Edges[1] = right;
                    if (right.Faces[(Eindex[0] + 1) % 2].Edges[2] == F0.Edges[index0]) right.Faces[(Eindex[0] + 1) % 2].Edges[2] = right;
                }
                

                LefList.Remove(F0);
                LefList.Remove(F1);

                return true;
            }
        }
        return false;
    }

    bool Front(Triangle T)
    {
        return Front(T.Vertices[0], T.Vertices[1], T.Vertices[2]);
    }
    bool Front(DPoint P1, DPoint P2, DPoint P3)
    {
        Vector2 V12 = P2.Pos - P1.Pos;
        Vector2 V23 = P3.Pos - P2.Pos;

        return V12.x * V23.y - V12.y * V23.x < 0;
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

        /* ランダムサンプリングで作るメッシュ
        for (int i = 0; i < 260; i++)
        {
            DPoint DP = new DPoint(new Vector2(UnityEngine.Random.Range(-5.0f, 5.0f), UnityEngine.Random.Range(-5.0f, 5.0f)));

            Triangle Target = SearchLeaf(DP, root);
            AddDPoint(DP, Target);
        }*/

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

                        if (TriList[i].Edges[j].Faces[0] == TriList[i]) TriList[i].Edges[j].Faces[0] = TriList[i].Edges[j].Faces[1];
                        if (TriList[i].Edges[j].Faces[1] == TriList[i]) TriList[i].Edges[j].Faces[1] = TriList[i].Edges[j].Faces[0];


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

        // この時点でPoiList，LefListは描画に過不足ない

        for (int i = 0; i < PoiList.Count; i++)
        {
            if (PoiList[i].Pos.x > 5.0f)
            {
                Debug.Log(i + ": " + PoiList[i].Pos);
            }
        }

        List2Mesh();
        sw.Stop();
        Debug.Log("Subdivide Time : " + sw.Elapsed);
    }

    private void Awake()
    {
        sw.Start();
        
        _mesh = new Mesh();

    }

    // Update is called once per frame
    void Update()
    {
        
        float randF = UnityEngine.Random.value;
        if (randF < 1.1f)
        {
            int randI1 = UnityEngine.Random.Range(0, LefList.Count);
            int randI2 = UnityEngine.Random.Range(0, 3);
            int judge = UnityEngine.Random.Range(0, 10);

            if (judge == 0)
                Split(LefList[randI1].Edges[randI2]);
            if (judge == 1)
                Flip(LefList[randI1].Edges[randI2]);
            if (judge > 1)
                Collapse(LefList[randI1].Edges[randI2]);
        }

        for (int i = 0; i < LefList.Count; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (LefList[i].Edges[j].Ends[0] == LefList[i].Vertices[(j + 1) % 3] && LefList[i].Edges[j].Faces[0] != LefList[i]) Debug.Log("Error 0");
                if (LefList[i].Edges[j].Ends[1] == LefList[i].Vertices[(j + 1) % 3] && LefList[i].Edges[j].Faces[1] != LefList[i]) Debug.Log("Error 1");
            }
        }
        List2Mesh();
        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);
    }
}
