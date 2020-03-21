using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalfEdge
{
    private HalfEdge pair;
    private HalfEdge next;
    private HalfEdge prev;
    private Vertex vert;
    private Face face;

    public HalfEdge Pair
    {
        set { pair = value; }
        get { return pair; }
    }
    public HalfEdge Next
    {
        set { next = value; }
        get { return next; }
    }
    public HalfEdge Prev
    {
        set { prev = value; }
        get { return prev; }
    }
    public Vertex Vert
    {
        set { vert = value; }
        get { return vert; }
    }
    public Face Face
    {
        set { face = value; }
        get { return face; }
    }
   
    public HalfEdge(Vertex V)
    {
        pair = null;
        next = null;
        prev = null;
        vert = V;
        face = null;
    }
}
public class Vertex
{
    private Vector3 pos;
    private Vector3 nor;
    private Vector2 uv;
    private HalfEdge hEdge;

    public Vector3 Pos {
        set { pos = value; }
        get { return pos; }
     }
    public Vector3 Nor
    {
        set { nor = value; }
        get { return nor; }
    }
    public Vector2 UV
    {
        set { uv = value; }
        get { return uv; }
    }
    public HalfEdge HEdge
    {
        set { hEdge = value; }
        get { return hEdge; }
    }

    public Vertex(Vector3 P, Vector3 N, Vector2 T)
    {
        pos = P;
        nor = N;
        uv =  T;
        hEdge = null;
    }
}
public class Face
{
    private HalfEdge hEdge;
   
    public HalfEdge HEdge
    {
        set { hEdge = value; }
        get { return hEdge; }
    }
    public Face(HalfEdge HE)
    {
        hEdge = HE;
    }
}
/*
public class HEStructure
{
    private List<Vertex> _vertices;
    private List<HalfEdge> _hEdges;
    private List<Face> _faces;

    public HEStructure()
    {
        _vertices = new List<Vertex>();
        _hEdges = new List<HalfEdge>();
        _faces = new List<Face>();
    }
    public List<Vertex> _Vertices
    {
        set { _vertices = value; }
        get { return _vertices; }
    }
    public List<HalfEdge> _HEdges
    {
        set { _hEdges = value; }
        get { return _hEdges; }
    }
    public List<Face> _Faces
    {
        set { _faces = value; }
        get { return _faces; }
    }
    public bool Flip(HalfEdge HE)
    {
        if (HE.Pair == null)
            return false;

        if (!Front(HE.Vert, HE.Pair.Prev.Vert, HE.Prev.Vert)) return false;
        if (!Front(HE.Pair.Vert, HE.Prev.Vert, HE.Pair.Prev.Vert)) return false;

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

        return true;
    }

    private bool Front(Vertex P1, Vertex P2, Vertex P3)
    {
        Vector2 V12 = P2.Pos - P1.Pos;
        Vector2 V23 = P3.Pos - P2.Pos;

        return V12.x * V23.y - V12.y * V23.x < 0;
    }
}*/