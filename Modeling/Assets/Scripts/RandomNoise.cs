using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNoise : MonoBehaviour
{
    List<Point> confirmed = new List<Point>();
    List<Point> possible = new List<Point>();
    float radius = 0.3f;
    int debugcount = 0;
    Texture2D Map;
void Start()
    {
        confirmed.Add(new Point(0, 0));
        possible.Add(confirmed[0]);
        int PointN = 692;
        Map = (Texture2D)Resources.Load("FJLOGO");

        GameObject obj = (GameObject)Resources.Load("Circle");

        for (int i = 0; i < PointN; i++)
        {
            float x = UnityEngine.Random.RandomRange(-5.0f, +5.0f);
            float y = UnityEngine.Random.RandomRange(-5.0f, +5.0f);

            GameObject clone = Instantiate(obj, new Vector3(x,y, 0.0f), Quaternion.identity);
            clone.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            clone.GetComponent<Renderer>().material.color = new Color(1.0f, 0, 0);
        }

    }

    // Update is called once per frame
    void Update()
    {


    }
}
