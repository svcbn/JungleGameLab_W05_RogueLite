using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TreeGenerator : MonoBehaviour
{
    public static TreeGenerator Instance;


    [Range(3, 8)] public int points = 3;
    [SerializeField][Min(0.1f)] float outerRadius = 3f;
    [SerializeField] float branchLength = 10f;
    [Range(3, 5)] public int depth = 3;
    [Range(2, 4)] public int width = 2;

    Mesh mesh;
    Vector3[] vertices;
    int[] indices;

    Vector3[] branches;
    Vector3[] tips;

    GameObject drawPrefab;
    [SerializeField] Node nodeOrigin;
    [HideInInspector] public GameObject node;
    [SerializeField] List<GameObject> lines;
    [HideInInspector] public Transform lineOrigin;

    public List<Node> nodes;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        mesh = new Mesh();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        drawPrefab = Resources.Load<GameObject>("W05/DrawPrefab");
        node = Resources.Load<GameObject>("W05/Node"); 
    }

    private void Update()
    {
        DrawFilled(points, outerRadius);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            DrawBranches(vertices);
        }
    }

    void DrawFilled(int sides, float radius)
    {
        vertices = GetCircumferencePoints(sides, radius);

        indices = DrawFilledIndices(vertices);

        GeneratePolygon(vertices, indices);
    }

    int[] DrawFilledIndices(Vector3[] vertices)
    {
        int triangleCount = vertices.Length - 2;
        List<int> indices = new List<int>();

        for(int i = 0; i < triangleCount; i++)
        {
            indices.Add(0);
            indices.Add(i+2);
            indices.Add(i+1);
        }

        return indices.ToArray();
    }

    void GeneratePolygon(Vector3[] vertices, int[] indices)
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = indices;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    Vector3[] GetCircumferencePoints(int sides, float radius)
    {
        Vector3[] points = new Vector3[sides];
        float anglePerStep = 2 * Mathf.PI * ((float)1 / sides);

        for(int i = 0; i < sides; i++)
        {
            Vector2 point = Vector2.zero;
            float angle = anglePerStep * i;

            point.x = Mathf.Cos(angle + Mathf.PI / 2) * radius;
            point.y = Mathf.Sin(angle + Mathf.PI / 2) * radius;

            points[i] = point;
        }

        return points;
    }

    void DrawBranches(Vector3[] vertices)
    {
        ClearLine();

        branches = GetBranchPoints(vertices);
        //tips = GetBranchTips(branches);

        for (int i = 0; i < branches.Length; i++)
        {
            SetNode(branches[i].normalized);
        }
    }

    Vector3[] GetBranchPoints(Vector3[] vertices)
    {
        Vector3[] points = new Vector3[vertices.Length];

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Vector3((vertices[i].x + vertices[i + 1 >= vertices.Length ? 0 : i + 1].x) / 2,
                (vertices[i].y + vertices[i + 1 >= vertices.Length ? 0 : i + 1].y) / 2,
                -1f);
        }

        return points;
    }

    //Vector3[] GetBranchTips(Vector3[] branches)
    //{
    //    Vector3[] points = new Vector3[branches.Length];

    //    for (int i = 0; i < branches.Length; i++)
    //    {
    //        Vector2 dir = (Vector2)branches[i] - Vector2.zero;
    //        points[i] = new Vector3(branches[i].x + dir.normalized.x * branchLength, branches[i].y + dir.normalized.y * branchLength, -1f);
    //    }

    //    return points;
    //}

    public void DrawLine(Vector3 from, Vector3 to)
    {

        GameObject go = Instantiate(drawPrefab);
        lines.Add(go);
        LineRenderer line = go.GetComponent<LineRenderer>();

        line.positionCount = 2;

        line.SetPosition(0, from);
        line.SetPosition(1, to);

        go.transform.SetParent(lineOrigin);
    }

    void ClearLine()
    {
        foreach(GameObject go in lines)
        {
            Destroy(go);
        }
        lines.Clear();
    }

    void SetNode(Vector3 pos)
    {
        var go = Instantiate(node);
        go.transform.position = pos * branchLength;
        
        Node _node = go.GetComponent<Node>();
        _node.step = 1;
        _node.parentNode = nodeOrigin;
        _node.transform.SetParent(nodeOrigin.transform, true);
        _node.Initialize(pos, branchLength);
    }
} 

