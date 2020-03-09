using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct test
{
    private int value1;
    private int value2;

    public void Print()
    {
        Debug.Log("(" + value1 + ", " + value2 + ")");
    }
    public void Set1(int v)
    {
        value1 = v;
    }
    public void Set2(int v)
    {
        value2 = v;
    }
}
public class Spring : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
        test test1 = new test();
        test1.Set1(1);
        test1.Set2(2);

        test test2 = test1;
        test2.Set1(3);
        test2.Set2(4);

        test1.Print();
        test2.Print();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
