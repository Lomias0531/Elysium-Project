using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent (typeof(MeshCollider))]
public class BaseTile : MonoBehaviour
{
    Mesh hexMesh;
    List<Vector3> vertices;
    List<int> triangles;

    const float outerRadius = 1f / 0.866025404f;
    const float innerRadius = 1f;

    public Vector2Int originalPos;
    public Vector3Int Pos;

    public List<BaseTile> adjacentTiles = new List<BaseTile>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void InitHex()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        hexMesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();

        var center = this.transform.position;

        for (int i = 0; i < 6; i++)
        {
            AddTriangle(center, center + corners[i], center + corners[i + 1]);
        }

        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();
        var collider = GetComponent<MeshCollider>();
        collider.sharedMesh = hexMesh;
    }
    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }
    Vector3[] corners =
    {
        new Vector3(0f,0f,outerRadius),
        new Vector3(innerRadius,0f,0.5f * outerRadius),
        new Vector3(innerRadius,0f,-0.5f * outerRadius),
        new Vector3(0f,0f,-outerRadius),
        new Vector3(-innerRadius,0f,-0.5f*outerRadius),
        new Vector3(-innerRadius,0f,0.5f*outerRadius),
        new Vector3(0f,0f,outerRadius)
    };

    public void PaintTile(Color color)
    {
        var mat = GetComponent<MeshRenderer>().material;
        mat.color = color;
    }
    public void GetAdjacentTiles()
    {
        CheckTileAvailable(Pos.x - 1, Pos.y, Pos.z + 1);
        CheckTileAvailable(Pos.x, Pos.y - 1, Pos.z + 1);
        CheckTileAvailable(Pos.x + 1, Pos.y - 1, Pos.z);
        CheckTileAvailable(Pos.x + 1, Pos.y, Pos.z - 1);
        CheckTileAvailable(Pos.x, Pos.y + 1, Pos.z - 1);
        CheckTileAvailable(Pos.x - 1, Pos.y + 1, Pos.z);
    }
    void CheckTileAvailable(int x, int y, int z)
    {
        Vector3Int adj = new Vector3Int(x, y, z);
        if (MapController.Instance.mapTiles.ContainsKey(adj))
        {
            adjacentTiles.Add(MapController.Instance.mapTiles[adj]);
        }
    }
}
