using System;
using System.Collections.Generic;
using UnityEngine;

public class MinimumCode : MonoBehaviour
{
    [SerializeField]
    private Material _material;
    private Mesh _mesh;

    // 頂点座標
    private Vector3[] _positions = new Vector3[] {
        new Vector3( 1, -1, 0),
        new Vector3(-1, -1, 0),
        new Vector3(-1,  1, 0),
        new Vector3( 1,  1, 0)
    };

    // 法線ベクトル
    private Vector3[] _normals = new Vector3[] {
        new Vector3(0, 0, -1),
        new Vector3(0, 0, -1),
        new Vector3(0, 0, -1),
        new Vector3(0, 0, -1)
    };

    // 頂点インデックス
    private int[] _triangles = new int[] {
        0, 1, 2, 0, 2, 3
    };

    // UV座標
    private Vector2[] _uvs = new Vector2[] {
        new Vector3( 1, 0),
        new Vector3( 0, 0),
        new Vector3( 0, 1),
        new Vector3( 1, 1)
    };

    private void Awake()
    {
        _mesh = new Mesh();
    }

    private void Update()
    {
        // Meshに情報を代入
        _mesh.vertices  = _positions;
        _mesh.triangles = _triangles;
        _mesh.normals   = _normals;
        _mesh.uv        = _uvs;

        _mesh.RecalculateBounds();

        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);
    }
}