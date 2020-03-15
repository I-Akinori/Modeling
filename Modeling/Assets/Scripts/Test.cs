using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private Material _material;
    private Mesh _mesh;
    List<Vector3> _ver;
    List<Vector2> _uv;
    List<Vector3> _nor;
    List<int> _tri;
    List<Vector3> OriginalPos;
    
    // Start is called before the first frame update
    void Start()
    {
        _ver = new List<Vector3>();
        _uv = new List<Vector2>();
        _nor = new List<Vector3>();
        _tri = new List<int>();

        ObjImporter OI = new ObjImporter();
        _mesh = OI.ImportFile("./Assets/Resources/teapot.obj");
        
        _mesh.GetUVs(0, _uv);
        _mesh.GetVertices(_ver);
        _mesh.GetNormals(_nor);

        _mesh.GetTriangles(_tri, 0);

        OriginalPos = new List<Vector3>();
        for (int i = 0; i < _ver.Count; i++)
        {
            OriginalPos.Add(_ver[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        for (int i = 0; i < _ver.Count; i++)
        {
            _ver[i] = OriginalPos[i] + _nor[i] * (Mathf.Sin(Time.time) + 1.0f) * 0.1f;
        }

        //_mesh.SetUVs(0, _uv);
        _mesh.SetVertices(_ver);
        _mesh.SetNormals(_nor);
        _mesh.SetTriangles(_tri, 0);

        Graphics.DrawMesh(_mesh, Vector3.zero, Quaternion.identity, _material, 0);
    }
}
