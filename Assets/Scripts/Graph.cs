using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Graph : MonoBehaviour
{
    public List<Vertex> vertices;
    public List<Edge> edges;

    public GameObject vertexPrefab;
    public GameObject edgePrefab;
    private List<Step> steps;
    private int stepNum;
    private Colors colors;

    public Button AddVertexButton;
    public Button ColorEdgesButton;
    public Button BackToDrawingButton;
    public Button PreviousButton;
    public Button NextButton;

    public Text infoText;
    public Text DeltaText;

    // Start is called before the first frame update
    void Start()
    {
        stepNum = 0;
        steps = new List<Step>();
        colors = new Colors();

        BackToDrawingButton.interactable = false;
        PreviousButton.interactable = false;
        NextButton.interactable = false;
        infoText.text = "";

        ReCalculateDelta();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanEditGraph()
    {
        return !BackToDrawingButton.IsInteractable();
    }

    public void AddVertex()
    {
        Vector3 newPos = new Vector3(transform.position.x, transform.position.y, -2);
        GameObject newVertex = GameObject.Instantiate(vertexPrefab, newPos, Quaternion.identity);
        var v = newVertex.GetComponent<Vertex>();
        v.id = vertices.Count;
        vertices.Add(v);
    }

    public void RemoveVertex(int index)
    {
        var v = vertices[index];
        vertices.RemoveAt(index);
        ReCalculateDelta();
        Destroy(v.gameObject);
        ReNumberVertices();
    }

    public void AddEdge(Vertex v1, Vertex v2)
    {
        Vector3 newPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        GameObject newEdge = GameObject.Instantiate(edgePrefab, newPos, Quaternion.identity);
        var e = newEdge.GetComponent<Edge>();
        e.v1 = v1;
        e.v2 = v2;
        e.v1.degree++;
        e.v2.degree++;
        ReCalculateDelta();
        e.id = edges.Count;
        edges.Add(e);
    }

    public void RemoveEdge(int index)
    {
        var e = edges[index];
        edges.RemoveAt(index);
        e.v1.degree--;
        e.v2.degree--;
        ReCalculateDelta();
        Destroy(e.gameObject);
    }

    public void ReCalculateDelta()
    {
        int maxDegree = 0;
        foreach(var vertex in vertices)
        {
            if(vertex.degree > maxDegree)
            {
                maxDegree = vertex.degree;
            }
        }

        int maxColor = 0;
        foreach (var edge in edges)
        {
            if (edge.colorId > maxColor)
            {
                maxColor = edge.colorId;
            }
        }

        DeltaText.text = "Delta: " + maxDegree.ToString() + "\nColors Used: " + maxColor.ToString();
    }

    public void ReNumberVertices()
    {
        for(int i = 0; i < vertices.Count; i++)
        {
            vertices[i].id = i;
        }
    }

    public void ClearEdges(Vertex v)
    {
        for (int i = 0; i < edges.Count; i++)
        {
            if (edges[i].v1 == v || edges[i].v2 == v)
            {
                RemoveEdge(i);
                i--;
            }
        }
    }

    public void ColorEdges()
    {
        List<int> numList = new List<int>();
        numList.Add(vertices.Count);
        numList.Add(edges.Count);
        for(int i = 0; i < edges.Count; i++)
        {
            numList.Add(edges[i].v1.id + 1);
            numList.Add(edges[i].v2.id + 1);
        }

        int[] temp = numList.ToArray();
        getGraphColorJson(temp, temp.Length);
        var json = System.IO.File.ReadAllText("edge colors.json");
        var stepsinfo = JsonConvert.DeserializeObject<List<List<List<int>>>>(json);
        foreach (var stepinfo in stepsinfo)
        {
            var step = new Step(stepinfo[0][0], stepinfo[0][1]);
            steps.Add(step);
            for (int i = 1; i < stepinfo.Count; i++)
            {
                step.changes.Add(new Change(stepinfo[i][0], stepinfo[i][1], stepinfo[i][2], stepinfo[i][3]));
            }
        }

        PreviousButton.interactable = false;
        NextButton.interactable = true;
        BackToDrawingButton.interactable = true;
        ColorEdgesButton.interactable = false;
        AddVertexButton.interactable = false;
    }

    public void ChangeToDrawing()
    {
        stepNum = 0;
        steps = new List<Step>();
        colors = new Colors();
        ReCalculateDelta();

        PreviousButton.interactable = false;
        NextButton.interactable = false;
        BackToDrawingButton.interactable = false;
        ColorEdgesButton.interactable = true;
        AddVertexButton.interactable = true;

        foreach(var edge in edges)
        {
            edge.ReColor(colors.GetColor(0), 0);
        }
    }

    public void NextStep()
    {
        stepNum++;
        var step = steps[stepNum - 1];
        UpdateInfo(step);
        foreach (var change in step.changes)
        {
            Edge e = null;
            for(int i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                if ((change.changedV1 - 1 == edge.v1.id && change.changedV2 - 1 == edge.v2.id) ||
                    (change.changedV1 - 1 == edge.v2.id && change.changedV2 - 1 == edge.v1.id))
                {
                    e = edge;
                }
            }
            e.ReColor(colors.GetColor(change.changedNewColor), change.changedNewColor);
        }

        PreviousButton.interactable = true;
        if (stepNum >= steps.Count)
        {
            NextButton.interactable = false;
        }
        ReCalculateDelta();
    }

    public void PreviousStep()
    {
        var step = steps[stepNum - 1];
        stepNum--;

        if (stepNum <= 0)
            infoText.text = "";
        else
            UpdateInfo(steps[stepNum - 1]);

        for (int j = step.changes.Count - 1; j >= 0; j--)
        {
            var change = step.changes[j];
            Edge e = null;
            for (int i = 0; i < edges.Count; i++)
            {
                var edge = edges[i];
                if ((change.changedV1 - 1 == edge.v1.id && change.changedV2 - 1 == edge.v2.id) ||
                    (change.changedV1 - 1 == edge.v2.id && change.changedV2 - 1 == edge.v1.id))
                {
                    e = edge;
                }
            }
            e.ReColor(colors.GetColor(change.changedOldColor), change.changedOldColor);
        }

        NextButton.interactable = true;
        if(stepNum <= 0)
        {
            PreviousButton.interactable = false;
        }
        ReCalculateDelta();
    }

    public void UpdateInfo(Step step)
    {
        var result = "";
        result += "Coloring edge " + step.coloredV1.ToString() + "," + step.coloredV2.ToString() + "\n";
        foreach(var change in step.changes)
        {
            result += "Edge " + change.changedV1.ToString() + "," + change.changedV2.ToString() +
                " is recolored from " + change.changedOldColor.ToString() + " to " + change.changedNewColor.ToString() + "\n";
        }
        infoText.text = result;
    }

    [DllImport("Vizing's Algorithm.dll", CallingConvention = CallingConvention.Cdecl)]
    private extern static void getGraphColorJson(int[] input, int size);

    public class Step
    {
        public int coloredV1;
        public int coloredV2;
        public List<Change> changes;

        public Step(int coloredV1, int coloredV2)
        {
            this.coloredV1 = coloredV1;
            this.coloredV2 = coloredV2;
            changes = new List<Change>();
        }
    }

    public class Change
    {
        public int changedV1;
        public int changedV2;
        public int changedOldColor;
        public int changedNewColor;

        public Change(int changedV1, int changedV2, int changedOldColor, int changedNewColor)
        {
            this.changedV1 = changedV1;
            this.changedV2 = changedV2;
            this.changedOldColor = changedOldColor;
            this.changedNewColor = changedNewColor;
        }
    }

    public class Colors
    {
        public List<Color> colors;

        public Colors() {
            colors = new List<Color>()
            {
                new Color(1, 1, 1),
                new Color(1, 0, 0),
                new Color(0, 1, 0),
                new Color(0, 0, 1),
                new Color(1, 1, 0),
                new Color(1, 0, 1),
                new Color(0, 1, 1)
            };
        }

        public Color GetColor(int i)
        {
            if(i < colors.Count)
            {
                return colors[i];
            }
            //Need more colors. Autogenerate one

            var newColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            colors.Add(newColor);
            return newColor;
        }
    }
}

