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

