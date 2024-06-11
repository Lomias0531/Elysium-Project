using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class BaseTile : MonoBehaviour
{
    Mesh hexMesh;
    List<Vector3> vertices;
    List<int> triangles;

    const float outerRadius = 0.5f;
    const float innerRadius = 0.5f * 0.866025404f;
    const float extendDistanceX = 0.5f * (1f - 0.866025404f);
    const float extendDistanceZ = 0.125f;

    public Vector2Int originalPos;
    public Vector3Int Pos;
    public Color color;
    [SerializeField]
    List<Color> colors = new List<Color>();

    public Dictionary<HexDirection, BaseTile> adjacentTiles = new Dictionary<HexDirection, BaseTile>();

    public GameObject indicator;
    public MeshRenderer indicatorRenderer;
    public bool isMarked = false;
    public TileSelectionType curSelectionType = TileSelectionType.None;
    public TerrainType terrainType;

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
    Vector3[] extendCorners =
    {
        new Vector3(extendDistanceX, 0 ,outerRadius + extendDistanceZ),
        new Vector3(innerRadius + extendDistanceX, 0 ,0.5f*outerRadius + extendDistanceZ),
        new Vector3(innerRadius + extendDistanceX * 2f, 0, 0.5f * outerRadius),
        new Vector3(innerRadius + extendDistanceX * 2f, 0, -0.5f*outerRadius),
        new Vector3(innerRadius + extendDistanceX, 0, -0.5f*outerRadius - extendDistanceZ),
        new Vector3(extendDistanceX, 0, -outerRadius - extendDistanceZ),
        new Vector3(-extendDistanceX, 0, -outerRadius - extendDistanceZ),
        new Vector3(-innerRadius - extendDistanceX, 0, -0.5f*outerRadius - extendDistanceZ),
        new Vector3(-innerRadius - extendDistanceX * 2f, 0, -0.5f * outerRadius),
        new Vector3(-innerRadius - extendDistanceX * 2f, 0, 0.5f * outerRadius),
        new Vector3(-innerRadius - extendDistanceX, 0, 0.5f * outerRadius + extendDistanceZ),
        new Vector3(-extendDistanceX, 0, outerRadius * extendDistanceZ),
    };
    public enum HexDirection
    {
        NE, E, SE, SW, W, NW
    }
    public enum TileSelectionType
    {
        None,
        Hover,
        Moveable,
        Attackable,
        Interactable,
    }
    public enum TerrainType
    {
        Void,
        Barrier,
        Water,
        Plain,
        Rocks,
        Swamp,
        Road,
        Snowfield,
    }

    /// <summary>
    /// F权重，根据具体规则设置，我们这里先求和来用
    /// </summary>
    public float F => g + h;

    /// <summary>
    /// 从起点地格到此地格的成本
    /// </summary>
    public float g;

    /// <summary>
    /// 从这个地格到目的地的估计费用
    /// </summary>
    public float h;
    // Start is called before the first frame update
    void Start()
    {
        MarkTile(TileSelectionType.None);
    }

    // Update is called once per frame
    void Update()
    {

    }
    #region Create Mesh
    public void InitHex()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        hexMesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();

        //var center = this.transform.position;

        for (int i = 0; i < 6; i++)
        {
            AddTriangle(Vector3.zero, Vector3.zero + corners[i], Vector3.zero + corners[i + 1]);
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

    public void PaintTile(Color _color)
    {
        color = _color;

        colors = new List<Color>();
        for (int i = 0; i < vertices.Count; i++)
        {
            colors.Add(color);
        }

        hexMesh.SetColors(colors);
    }
    public void GetAdjacentTiles()
    {
        CheckTileAvailable(Pos.x, Pos.y - 1, Pos.z + 1, HexDirection.NE); //y-1 z+1
        CheckTileAvailable(Pos.x + 1, Pos.y - 1, Pos.z, HexDirection.E); //x+1 y-1
        CheckTileAvailable(Pos.x + 1, Pos.y, Pos.z - 1, HexDirection.SE); // x+1 z-1
        CheckTileAvailable(Pos.x, Pos.y + 1, Pos.z - 1, HexDirection.SW); //y+1 z-1
        CheckTileAvailable(Pos.x - 1, Pos.y + 1, Pos.z, HexDirection.W); //x-1 y+1
        CheckTileAvailable(Pos.x - 1, Pos.y, Pos.z + 1, HexDirection.NW); //x-1 z+1
    }
    void CheckTileAvailable(int x, int y, int z, HexDirection dir)
    {
        Vector3Int adj = new Vector3Int(x, y, z);
        if (MapController.Instance.mapTiles.ContainsKey(adj))
        {
            adjacentTiles.Add(dir, MapController.Instance.mapTiles[adj]);
        }
    }
    public void BlendAdjTileColor()
    {
        colors.Clear();
        for (int i = 0; i < vertices.Count; i++)
        {
            colors.Add(color);
        }

        GetAdjTile(HexDirection.NE);
        GetAdjTile(HexDirection.E);
        GetAdjTile(HexDirection.SE);

        GetAdjTriangles(HexDirection.NE);
        GetAdjTriangles(HexDirection.E);

        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();

        hexMesh.RecalculateNormals();
        hexMesh.RecalculateTangents();
        hexMesh.SetColors(colors);
    }
    void GetAdjTile(HexDirection dir)
    {

        if (adjacentTiles.ContainsKey(dir))
        {
            int vertexIndex = vertices.Count;

            var v1 = corners[(int)dir];
            vertices.Add(v1);
            colors.Add(color);
            var v2 = corners[(int)dir + 1];
            vertices.Add(v2);
            colors.Add(color);
            var v3 = extendCorners[(int)dir * 2] - new Vector3(0, this.transform.localPosition.y - adjacentTiles[dir].gameObject.transform.localPosition.y, 0);
            vertices.Add(v3);
            colors.Add(adjacentTiles[dir].color);
            var v4 = extendCorners[(int)dir * 2 + 1] - new Vector3(0, this.transform.localPosition.y - adjacentTiles[dir].gameObject.transform.localPosition.y, 0);
            vertices.Add(v4);
            colors.Add(adjacentTiles[dir].color);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }
    }
    void GetAdjTriangles(HexDirection dir)
    {
        if (adjacentTiles.ContainsKey(dir) && adjacentTiles.ContainsKey(dir + 1))
        {
            int vertexIndex = vertices.Count;

            var v1 = corners[(int)dir + 1];
            vertices.Add(v1);
            colors.Add(color);
            var v2 = extendCorners[(int)dir*2 + 1] - new Vector3(0, this.transform.localPosition.y - adjacentTiles[dir].gameObject.transform.localPosition.y, 0);
            vertices.Add(v2);
            colors.Add(adjacentTiles[dir].color);
            var v3 = extendCorners[(int)dir*2 + 2] - new Vector3(0, this.transform.localPosition.y - adjacentTiles[dir + 1].gameObject.transform.localPosition.y, 0);
            vertices.Add(v3);
            colors.Add(adjacentTiles[dir + 1].color);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }
    }
    #endregion

    public void MarkTile(TileSelectionType type)
    {
        if(type != TileSelectionType.None)
        {
            indicator.transform.DOScale(0.45f, 0.3f);
            indicator.transform.DOMoveY(this.gameObject.transform.position.y + 0.1f, 0.4f);
        }
        else
        {
            indicator.transform.DOScale(0f, 0.3f);
            indicator.transform.DOMoveY(this.gameObject.transform.position.y -0.1f, 0.4f);
        }
        switch(type)
        {
            case TileSelectionType.None:
                {
                    indicatorRenderer.material.color = new Color(1, 1, 1, 1f);
                    break;
                }
            case TileSelectionType.Hover:
                {
                    indicatorRenderer.material.color = new Color(1, 0.92f, 0.016f, 1f);
                    break;
                }
            case TileSelectionType.Moveable:
                {
                    indicatorRenderer.material.color = new Color(0, 1, 0, 1f);
                    break;
                }
            case TileSelectionType.Attackable:
                {
                    indicatorRenderer.material.color = new Color(1, 0, 0, 1f);
                    break;
                }
            case TileSelectionType.Interactable:
                {
                    indicatorRenderer.material.color = new Color(0, 1, 1, 1f);
                    break;
                }
        }
    }
}
