using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Point
{
    private static int count = 0;
    private Vector2 pos;
    private int index;

    public Point (float x, float y)
    {
        index = count;
        count++;
        pos = new Vector2(x, y);
    }

    public Point(Vector2 v)
    {
        index = count;
        count++;
        pos = v;
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

public class Cell
{
    private List<Point> content;
    private Cell[] nearby;

    public Cell()
    {
        content = new List<Point>();
        nearby = new Cell[9];
    }

    public List<Point> Content
    {
        set { content = value; }
        get { return content; }
    }
    public Cell[] Nearby
    {
        set { nearby = value; }
        get { return nearby; }
    }
}

public class Sampling : MonoBehaviour
{
    public static List<Point> confirmed = new List<Point>();
    private static List<Point> possible = new List<Point>();
    public static float length = 10.0f;
    public static float maxrad = 0.2f;
    public static float minrad = 0.008f;
    public static float radius = 1.0f;
    int count = 0;
    int debugcount = 0;
    Texture2D Map;
    int cellN;
    int cellN2;
    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    GameObject obj;

    private float distance(Point A, Point B)
    {
        return Vector2.Distance(A.Pos, B.Pos);
    }

    private int Coadination2Index(Vector2 v)
    {
        float cellSize = length / cellN;
        return Mathf.FloorToInt((v.x + length * 0.5f) / cellSize) + Mathf.FloorToInt((v.y + length * 0.5f) / cellSize) * cellN;
    }

    // Start is called before the first frame update
    [System.Obsolete]
    void Start()
    {
        int trycount = 15;
        Map = (Texture2D)Resources.Load("FUJILOGO");
        Debug.Log("Log2: " + Mathf.Log(length / maxrad, 2f));
        //cellN = Mathf.FloorToInt(Mathf.Log(length / maxrad, 2f));
        //cellN = (int)Mathf.Pow(2, cellN);
        cellN = Mathf.FloorToInt(length / maxrad);
        cellN2 = cellN * cellN;
        

        // グリッド生成
        Cell[] maingrid = new Cell[cellN2];
        for (int i = 0; i < cellN2; i++)
        {
            maingrid[i] = new Cell();
        }

        for (int i = 0; i < cellN2; i++)
        {
            maingrid[i].Nearby[0] = i - 1 - cellN < 0 ? null : maingrid[i - 1 - cellN];
            maingrid[i].Nearby[1] = i - 0 - cellN < 0 ? null : maingrid[i - 0 - cellN];
            maingrid[i].Nearby[2] = i + 1 - cellN < 0 ? null : maingrid[i + 1 - cellN];

            maingrid[i].Nearby[3] = i - 1 - 0 < 0 ? null : maingrid[i - 1 - 0];
            maingrid[i].Nearby[4] =                        maingrid[i        ];
            maingrid[i].Nearby[5] = i + 1 - 0 > cellN2 - 1 ? null : maingrid[i + 1 - 0];

            maingrid[i].Nearby[6] = i - 1 + cellN > cellN2 - 1 ? null : maingrid[i - 1 + cellN];
            maingrid[i].Nearby[7] = i - 0 + cellN > cellN2 - 1 ? null : maingrid[i - 0 + cellN];
            maingrid[i].Nearby[8] = i + 1 + cellN > cellN2 - 1 ? null : maingrid[i + 1 + cellN];

            if (i % cellN == 0)
            {
                maingrid[i].Nearby[0] = null;
                maingrid[i].Nearby[3] = null;
                maingrid[i].Nearby[6] = null;
            } else if (i % cellN == cellN - 1)
            {
                maingrid[i].Nearby[2] = null;
                maingrid[i].Nearby[5] = null;
                maingrid[i].Nearby[8] = null;
            }
        }

        //sw.Start();
        confirmed.Add(new Point(0, 0.0f));
        possible.Add(confirmed[0]);
        Debug.Log(Coadination2Index(confirmed[0].Pos));
        maingrid[Coadination2Index(confirmed[0].Pos)].Content.Add(confirmed[0]);

        while (possible.Count > 0)
        {
            if (debugcount > 100 * 100 * 10) break;
            debugcount++;

            for (int i = 0; i < trycount; i++)                                                                    // 新頂点は15回以内に見つけなくてはならない
            {
                
                /* 点の密度変更オフ
                radius = minrad + (maxrad - minrad) * (0.0f + Map.GetPixel(
                    (int)((possible[possible.Count - 1].Pos.x / length + 0.5f) * Map.width),
                    (int)((possible[possible.Count - 1].Pos.y / length + 0.5f) * Map.height)
                    ).grayscale);
                */

                float dis = UnityEngine.Random.RandomRange(radius, 2 * radius);
                float dir = UnityEngine.Random.RandomRange(0f, 2 * Mathf.PI);
                Point P = new Point(possible[possible.Count - 1].Pos + new Vector2(Mathf.Cos(dir), Mathf.Sin(dir)) * dis);   // 新頂点候補P
                bool close = false;

                if (P.Pos.x >  length * 0.5f || P.Pos.x < -length * 0.5f || P.Pos.y > length * 0.5f || P.Pos.y < -length * 0.5f)
                {
                    continue;
                }


                /*
                foreach (Point CP in confirmed)                                                             // 確定頂点CPとPが十分離れていることを確認
                {
                    if (distance(P, CP) < radius)
                    {
                            close = true;
                            break;
                     }
                   
                }*/


                foreach (Cell NC in maingrid[Coadination2Index(P.Pos)].Nearby)                                                             // 確定頂点CPとPが十分離れていることを確認
                {
                    if (NC == null)
                        continue;
                    foreach(Point CP in NC.Content)
                    {
                        if (distance(P, CP) < radius)
                        {
                            close = true;
                            break;
                        }
                    }

                    if (close)
                    {
                        break;
                    }
                }

                if (!close)                                                                                 // 十分離れているなら
                {                                                                                           // 確定頂点リスト，可能性頂点リスト両方に追加
                    confirmed.Add(P);                                                                       // 15回以内に見つけられたのでループ脱出
                    possible.Add(P);
                    maingrid[Coadination2Index(P.Pos)].Content.Add(P);
                    break;
                } else if (i == trycount - 1)                                                                     // 最終ループなら可能性頂点を削除
                {
                    possible.RemoveAt(possible.Count - 1);
                }

            }
        }
        
        /*  描画モードオフ
        obj = (GameObject)Resources.Load("Circle");
        // Cubeプレハブを元に、インスタンスを生成、
        
        foreach (Point P in confirmed)
        {
            GameObject clone = Instantiate(obj, new Vector3(P.Pos.x, P.Pos.y, 0.0f), Quaternion.identity);
            clone.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            clone.GetComponent<Renderer>().material.color = new Color(210f / 255f, 85f / 255f, 143f / 255f);
        }

        Debug.Log(confirmed.Count);

        sw.Stop();
        Debug.Log("Points: " + confirmed.Count);
        Debug.Log("Time  : " + sw.Elapsed);
        Debug.Log("CellN : " + cellN);
        */
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }
}
