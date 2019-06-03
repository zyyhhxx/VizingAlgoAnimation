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

    public int degree;
    private List<Step> steps;
    private int stepNum;
    private Colors colors;

    public Button AddVertexButton;
    public Button ColorEdgesButton;
    public Button BackToDrawingButton;
    public Button PreviousButton;
    public Button NextButton;

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

        degree = maxDegree;
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
        foreach(var change in step.changes)
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
        foreach (var change in step.changes)
        {
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

    public class VizingColoring
    {
        private int max;
        private List<Step> steps;
        private Graph graph;
        private int[,] colorMatrix;
        private Step currentStep;
        private int count;

        public VizingColoring(Graph g)
        {
            steps = new List<Step>();
            max = g.degree;
            graph = g;
            count = 0;

            //Color initialization
            colorMatrix = new int[g.vertices.Count, g.vertices.Count];
            for(int i = 0; i < g.vertices.Count; i++)
            {
                for (int j = 0; j < g.vertices.Count; j++)
                {
                    colorMatrix[i, j] = 0;
                }
            }

            while(count < g.edges.Count)
            {
                var step = new Step(g.edges[count].v1.id, g.edges[count].v2.id);
                currentStep = step;
                steps.Add(step);
                Debug.Log("Coloring: " + g.edges[count].ToString());
                if (count < max)
                {
                    int start = g.edges[count].v1.id;
                    int end = g.edges[count].v2.id;
                    colorMatrix[start - 1, end - 1] = ++count;
                    colorMatrix[end - 1, start - 1] = count;
                    ReportChange(start, end, 0, count);
                }
                else
                {
                    //Recolor
                    AddOneEdge(A, count++);
                }
            }
        }

        public void ReportChange(int v1, int v2, int oldColor, int newColor)
        {
            var change = new Change(v1, v2, oldColor, newColor);
            currentStep.changes.Add(change);
            Debug.Log(change);
        }

        public int MiissingColor(int vertex)
        {
            int tmp = 0;
            for(int i = 1; i <= max; i++)
            {
                if(!IsColor(vertex, i))
                {
                    tmp = i;
                    i = max + 1;
                }
            }
            return tmp;
        }

        public bool IsColor(int vertex, int color)
        {
            for(int i = 0; i < graph.vertices.Count)
            {
                if (colorMatrix[vertex - 1, i] == color)
                    return true;
            }
            return false;
        }

        public Tuple<int, int> LocateEdge(int vertex, int color)
        {
            int vertices = graph.vertices.Count;
            for(int i = 0; i < vertices; i++)
            {
                if(colorMatrix[vertex - 1, i] == color)
                {
                    return new Tuple<int, int>(vertex, i + 1);
                }
            }
            return new Tuple<int, int>(0, 0);
        }

        public bool IsIn(int edVertex, List<int> endVertices, int range, ref int position)
        {
            for(int i = 0; i < range; i++)
            {
                if(edVertex == endVertices[i])
                {
                    position = i;
                    return true;
                }
            }
            return false;
        }

        public void Recolor(int x, List<int> endVertices, List<int> missingCol, int start, int position)
        {
            int end;
            int oldColor;
            for (int i = start; i < position; i++)
            {
                end = endVertices[i];
                oldColor = colorMatrix[x - 1,end - 1];
                colorMatrix[x - 1, end - 1] = missingCol[end - 1];
                colorMatrix[end - 1,x - 1] = missingCol[end - 1];
                ReportChange(x, end, oldColor, missingCol[end - 1]);
            }
            end = endVertices[position];
            oldColor = colorMatrix[x - 1, end - 1];
            colorMatrix[x - 1, end - 1] = 0;
            colorMatrix[end - 1, x - 1] = 0;
            ReportChange(x, end, oldColor, 0);
        }

        public List<Edge> SearchEdges(List<Edge> A, int s, int t)
        {
            var first = new List<Edge>();
            int tmp = 0;
            for (int i = 0; i < count; i++)
            {
                int start = A[i].v1.id;
                int end = A[i].v2.id;

                if (colorMatrix[start - 1, end - 1] == s || colorMatrix[start - 1, end - 1] == t)
                {
                    var choose = SearchEdge(start - 1, end - 1);
                    first.Insert(tmp++, choose);
                }
            }

            var final = new List<Edge>();

            for (int i = 0; i < tmp; i++)
            {
                final.Insert(i, first[i]);
            }

            return final;
        }

        public Edge SearchEdge(int v1, int v2)
        {
            for(int i = 0; i < graph.edges.Count; i++)
            {
                var edge = graph.edges[i];
                if ((edge.v1.id == v1 && edge.v2.id == v2) || (edge.v1.id == v2 && edge.v2.id == v1))
                {
                    return edge;
                }
            }
            return null;
        }

        public void SwitchColor(Graph subPtr, Color s, Color t, int vertex)
        {
            int v = subPtr.vertices.Count;

            for (int i = 0; i < v; i++)
            {
                var L = *((subPtr->theAdj)->adj[i]);
                ListIterator I;
                if (subPtr->isConnected(i + 1, vertex))
                {
                    for (I.start(L); !I.done(); I++)
                    {

                        if ((i + 1) < I())
                        {
                            if (colorMatrix[i, I() - 1] == s)
                            {
                                colorMatrix[i, I() - 1] = t;
                                colorMatrix[I() - 1, i] = t;
                                ReportChange(i + 1, I(), s, t);
                            }
                            else
                            {
                                colorMatrix[i, I() - 1] = s;
                                colorMatrix[I() - 1, i] = s;
                                ReportChange(i + 1, I(), t, s);
                            }
                        }
                    }
                }
            }
        }

        public List<Step> VizingColor()
        {

        }


    }
}

