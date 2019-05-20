using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    public int colorId;
    public Vertex v1;
    public Vertex v2;
    public LineRenderer lr;
    public int id;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        lr.SetPosition(0, v1.transform.position);
        lr.SetPosition(1, v2.transform.position);
    }

    public void ReColor(Color newColor, int id)
    {
        lr.startColor = newColor;
        lr.endColor = newColor;
        colorId = id;
    }
}
