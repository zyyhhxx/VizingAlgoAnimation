using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]

public class Vertex : MonoBehaviour
{
    public static bool buildingEdge = false;
    public static Vertex selected = null;

    public int degree;
    public float clickGap = 0.3f;
    public float dragGap = 0.1f;
    public int id;
    public TextMesh tm;

    private Graph graph;
    private Vector3 screenPoint;
    private SpriteRenderer sr;
    private float clickTime;
    public float dragTime;

    private void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        if (graph.CanEditGraph())
        {
            if (!buildingEdge)
            {
                //When double click, select the vertex
                if (clickTime < clickGap)
                {
                    Select();
                }
                clickTime = 0;
            }
            else
            {
                if (selected != this)
                {
                    if (!CheckAdjacent(selected))
                    {
                        graph.AddEdge(selected, this);
                    }
                    selected.UnSelect();
                }
            }
        }
    }

    private void OnMouseOver()
    {
        if (graph.CanEditGraph())
        {
            if (!buildingEdge)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    graph.ClearEdges(this);
                    graph.RemoveVertex(id);
                }
            }
        }
    }

    private void OnMouseUp()
    {
        dragTime = 0;
    }

    private void OnMouseDrag()
    {
        if (!buildingEdge)
        {
            if (dragTime > dragGap)
            {
                Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
                curPosition.z = -2;
                transform.position = curPosition;
            }
            dragTime += Time.deltaTime;
        }
    }

    public bool CheckAdjacent(Vertex other)
    {
        foreach (var edge in graph.edges)
        {
            if ((edge.v1 == this && edge.v2 == other) ||
                (edge.v1 == other && edge.v2 == this))
            {
                return true;
            }
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        clickTime = 0;
        dragTime = 0;
        sr = gameObject.GetComponent<SpriteRenderer>();
        graph = GameObject.FindWithTag("graph").GetComponent<Graph>();
    }

    // Update is called once per frame
    void Update()
    {
        if (graph.CanEditGraph())
        {
            clickTime += Time.deltaTime;
            if (selected == this)
            {
                if (Input.GetMouseButton(1))
                {
                    UnSelect();
                }
            }
        }
        tm.text = (id + 1).ToString();
    }

    private void Select()
    {
        buildingEdge = true;
        selected = gameObject.GetComponent<Vertex>();
        sr.color = new Color(255, 0, 0);
    }

    private void UnSelect()
    {
        buildingEdge = false;
        selected = null;
        sr.color = new Color(255, 255, 255);
    }
}
