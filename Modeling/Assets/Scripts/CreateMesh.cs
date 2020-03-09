using System;
using System.Collections.Generic;
using UnityEngine;

public class CreateMesh : MonoBehaviour
{
    [SerializeField]
    private Material _material;

    private Mesh _mesh;

    private List<HalfEdge> _halfedges = new List<HalfEdge>();
    private List<Vertex> _vertices = new List<Vertex>();
    private List<Face> _faces = new List<Face>();

    // (1) 頂点座標（この配列のインデックスが頂点インデックス）
    private Vector3[] _positions = new Vector3[]{};
    // (2) ポリゴンを形成する頂点インデックスを順番に指定する
    private int[] _triangles = new int[] {};

    // (3) 法線
    private Vector3[] _normals = new Vector3[]{};
    // uv座標
    private Vector2[] _uvs = new Vector2[]{};

    private void Subdivision()
    {

    }
    private void List2Array()
    {
        Array.Resize(ref _positions, _vertices.Count);
        Array.Resize(ref _triangles, _faces.Count * 3);
        Array.Resize(ref _normals,   _vertices.Count);
        Array.Resize(ref _uvs,       _vertices.Count);

        for(int i = 0; i < _vertices.Count; i++)
        {
            _positions[i] = _vertices[i].Pos;
            _normals[i]   = _vertices[i].Nor;
            _uvs[i]       = _vertices[i].UV;
        }

        for (int i = 0; i < _faces.Count; i++)
        {
            _triangles[i * 3] = _vertices.IndexOf(_faces[i].HEdge.Vert);
            _triangles[i * 3 + 1] = _vertices.IndexOf(_faces[i].HEdge.Next.Vert);
            _triangles[i * 3 + 2] = _vertices.IndexOf(_faces[i].HEdge.Prev.Vert);
        }
    }

    private void SetPairHalfEdge() // 半稜線のペアセッティング
    {
        for (int i = 0; i < _halfedges.Count; i++)
        {
            if (_halfedges[i].Pair != null) continue;
            for (int j = 0; j < _halfedges.Count; j++)
            {
                if (j == i) continue;
                if (_halfedges[i].Next.Vert == _halfedges[j].Vert && _halfedges[j].Next.Vert == _halfedges[i].Vert)
                {
                    _halfedges[i].Pair = _halfedges[j];
                    _halfedges[j].Pair = _halfedges[i];
                    break;
                }
            }
        }
    }

    private void SetHEdge() // 頂点→半稜線接続
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            for (int j = 0; j < _halfedges.Count; j++)
            {
                if (_vertices[i] == _halfedges[j].Vert)
                {
                    _vertices[i].HEdge = _halfedges[j];
                    break;
                }
            }
        }
    }

    private void EdgeCollapse(HalfEdge HE)
    {
        List<Vertex> VList = new List<Vertex>();
        HalfEdge tmpHE = HE;
        do
        {
            VList.Add(tmpHE.Vert);
            tmpHE = tmpHE.Pair.Prev;
        } while (tmpHE == HE);

        int count = 0;
        tmpHE = HE.Pair;
        do
        {
            VList.Add(tmpHE.Vert);
            tmpHE = tmpHE.Prev.Pair;
            if (VList.Contains(tmpHE.Vert)) count++;
        } while (tmpHE == HE.Pair || count < 2);

        if (count == 2)
        {
            HE.Vert.Pos = (HE.Next.Vert.Pos + HE.Vert.Pos) * 0.5f;
            HE.Vert.Nor = Vector3.Normalize(HE.Next.Vert.Nor + HE.Vert.Nor);
            HE.Vert.UV = (HE.Next.Vert.UV + HE.Vert.UV) * 0.5f;
            if (HE.Vert.HEdge == HE || HE.Vert.HEdge == HE.Pair.Next) HE.Vert.HEdge = HE.Prev.Pair;
           
            tmpHE = HE.Pair.Prev.Pair;
            tmpHE.Vert = HE.Vert;
            Vertex tmpV = HE.Next.Vert;
            while (tmpHE != HE.Pair)
            {
                tmpHE = tmpHE.Prev.Pair;
                tmpHE.Vert = HE.Vert;
            }

            HE.Next.Pair.Pair = HE.Prev.Pair;
            HE.Prev.Pair.Pair = HE.Next.Pair;
            HE.Pair.Next.Pair.Pair = HE.Pair.Prev.Pair;
            HE.Pair.Prev.Pair.Pair = HE.Pair.Next.Pair;

            for (int i = 0; i < _halfedges.Count; i++)
            {
                if (_halfedges[i].Vert == tmpV)
                {
                    _halfedges[i].Vert = HE.Vert;
                }
            }

            _vertices.Remove(tmpV);
            _faces.Remove(HE.Face);
            _faces.Remove(HE.Pair.Face);
            _halfedges.Remove(HE.Next);
            _halfedges.Remove(HE.Prev);
            _halfedges.Remove(HE.Pair.Next);
            _halfedges.Remove(HE.Pair.Prev);
            _halfedges.Remove(HE.Pair);
            _halfedges.Remove(HE);
         }
    }
    private void EdgeSwap (HalfEdge HE)
    {
        HalfEdge tmpHE = HE.Prev;
        do
        {
            tmpHE = tmpHE.Pair.Prev;
        } while (tmpHE != HE.Prev && tmpHE.Pair.Vert != HE.Pair.Prev.Vert);

        if (tmpHE.Pair.Vert != HE.Pair.Prev.Vert)
        {
            if (HE.Vert.HEdge == HE) HE.Vert.HEdge = HE.Prev.Pair;
            if (HE.Pair.Vert.HEdge == HE.Pair) HE.Pair.Vert.HEdge = HE.Next;
            HE.Face.HEdge = HE;
            HE.Pair.Face.HEdge = HE.Pair;

            HE.Pair.Vert = HE.Pair.Prev.Vert;
            HE.Vert = HE.Prev.Vert;

            HE.Prev.Next = HE.Pair.Next;
            HE.Next.Prev = HE.Pair.Prev;
            HE.Next.Next = HE;
            HE.Pair.Next.Prev = HE.Prev;
            HE.Pair.Prev.Next = HE.Next;
            HE.Pair.Next.Next = HE.Pair;
            HE.Prev.Prev = HE.Pair;
            HE.Pair.Prev.Prev = HE;
            HE.Next = HE.Pair.Prev;
            HE.Pair.Next = HE.Prev;
            HE.Prev = HE.Next.Next;
            HE.Pair.Prev = HE.Pair.Next.Next;

            HE.Next.Face = HE.Face;
            HE.Pair.Next.Face = HE.Pair.Face;
        }
    }
    private void Awake()
    {
        // 各リストの初期化
        _mesh = new Mesh();
    }
    private void Start()
    {
        float r1 = 3.0f;
        float r2 = 1.0f;
        int n = 20;
        int count;
        int right;
        int under;

        for (int i = 0; i < n ; i++)
        {
            float phi = Mathf.PI * 2.0f * i / n ;
            float tr = Mathf.Cos(phi) * r2;
            float y = Mathf.Sin(phi) * r2;

            for (int j = 0; j < n; j++)
            {
                float theta = 2.0f * Mathf.PI * j / n;
                float x = Mathf.Cos(theta) * (r1 + tr);
                float z = Mathf.Sin(theta) * (r1 + tr);

                _vertices.Add(new Vertex(new Vector3(x, y, z), new Vector3(tr * Mathf.Cos(theta), y, tr * Mathf.Sin(theta)), new Vector2(1.0f - (float)j / n, 1.0f - (float)i / n)));
            }
        }

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                count = n * j + i;
                right = i < n - 1 ? 1 : 1 - n;
                under = j < n - 1 ? n : n * (1 - n);

                // 半稜線はPair以外を接続，面はHEdgeを接続
                _halfedges.Add(new HalfEdge(_vertices[count]));
                _halfedges.Add(new HalfEdge(_vertices[count + under]));
                _halfedges.Add(new HalfEdge(_vertices[count + under + right]));
                _halfedges[_halfedges.Count - 3].Next = _halfedges[_halfedges.Count - 1].Prev = _halfedges[_halfedges.Count - 2];
                _halfedges[_halfedges.Count - 2].Next = _halfedges[_halfedges.Count - 3].Prev = _halfedges[_halfedges.Count - 1];
                _halfedges[_halfedges.Count - 1].Next = _halfedges[_halfedges.Count - 2].Prev = _halfedges[_halfedges.Count - 3];
                _faces.Add(new Face(_halfedges[_halfedges.Count - 3]));
                _halfedges[_halfedges.Count - 3].Face = _halfedges[_halfedges.Count - 2].Face = _halfedges[_halfedges.Count - 1].Face = _faces[_faces.Count - 1];

                _halfedges.Add(new HalfEdge(_vertices[count]));
                _halfedges.Add(new HalfEdge(_vertices[count + under + right]));
                _halfedges.Add(new HalfEdge(_vertices[count + right]));
                _halfedges[_halfedges.Count - 3].Next = _halfedges[_halfedges.Count - 1].Prev = _halfedges[_halfedges.Count - 2];
                _halfedges[_halfedges.Count - 2].Next = _halfedges[_halfedges.Count - 3].Prev = _halfedges[_halfedges.Count - 1];
                _halfedges[_halfedges.Count - 1].Next = _halfedges[_halfedges.Count - 2].Prev = _halfedges[_halfedges.Count - 3];
                _faces.Add(new Face(_halfedges[_halfedges.Count - 3]));
                _halfedges[_halfedges.Count - 3].Face = _halfedges[_halfedges.Count - 2].Face = _halfedges[_halfedges.Count - 1].Face = _faces[_faces.Count - 1];
 
            }

        }

        SetPairHalfEdge();
        SetHEdge();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))    // テスト用 左クリックを押したときの処理
        {

        }
        List2Array();

        Array.Resize(ref _positions, _vertices.Count);
        Array.Resize(ref _triangles, _faces.Count * 3);
        Array.Resize(ref _normals, _vertices.Count);
        Array.Resize(ref _uvs, _vertices.Count);

        _mesh.Clear();
        _mesh.vertices = _positions;
        _mesh.triangles = _triangles;
        _mesh.normals = _normals;
        _mesh.uv = _uvs;
     
        _mesh.RecalculateBounds();

        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);

    }
}