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
    [NonSerialized]
    List<Vector3> terrainTypes;
    List<int> triangles;
    public string tileName;

    public BaseObj? curObj;

    public Vector2Int originalPos;
    public Vector3Int Pos;
    public Color color;
    [NonSerialized]
    List<Color> colors = new List<Color>();

    public Dictionary<HexDirection, BaseTile> adjacentTiles = new Dictionary<HexDirection, BaseTile>();

    public GameObject indicator;
    public MeshRenderer indicatorRenderer;
    public bool isMarked = false;
    public TileSelectionType curSelectionType = TileSelectionType.None;
    public TerrainType terrainType;
    public float terrainIndex;

    
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
        Selected,
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
        DeepWater,
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
        terrainTypes = new List<Vector3>();

        //var center = this.transform.position;

        for (int i = 0; i < 6; i++)
        {
            AddTriangle(Vector3.zero, Vector3.zero + ToolsUtility.corners[i], Vector3.zero + ToolsUtility.corners[i + 1]);
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

    public void PaintTile(Color _color, float _terrainIndex)
    {
        color = _color;
        terrainIndex = _terrainIndex;
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
        terrainTypes.Clear();
        for (int i = 0; i < vertices.Count; i++)
        {
            colors.Add(color);
            terrainTypes.Add(new Vector3(terrainIndex, terrainIndex, terrainIndex));
        }

        GetAdjTile(HexDirection.NE);
        GetAdjTile(HexDirection.E);
        GetAdjTile(HexDirection.SE);

        GetAdjTriangles(HexDirection.NE);
        GetAdjTriangles(HexDirection.E);

        for (int i = 0; i < 6; i++)
        {
            if (!adjacentTiles.ContainsKey((HexDirection)i))
            {
                var v1 = ToolsUtility.corners[(int)i];

                var v2 = ToolsUtility.corners[(int)i + 1];

                var v3 = v1 - new Vector3(0, this.transform.position.y + 1f, 0);

                var v4 = v2 - new Vector3(0, this.transform.position.y + 1f, 0);

                for (int t = 0; t < 4; t++)
                {
                    var p1 = Vector3.Lerp(v1, v3, (float)t / 4);
                    var p2 = Vector3.Lerp(v2, v4, (float)t / 4);
                    var p3 = Vector3.Lerp(v1, v3, (float)(t + 1) / 4);
                    var p4 = Vector3.Lerp(v2, v4, (float)(t + 1) / 4);

                    AddQuad(p1, p2, p3, p4, color, color, terrainIndex, terrainIndex);
                }

                var f = i + 1;
                if (f >= 6) f = 0;
                if (adjacentTiles.ContainsKey((HexDirection)f))
                {
                    var v5 = ToolsUtility.corners[f];

                    var v6 = ToolsUtility.extendCorners[f * 2] - new Vector3(0, this.transform.localPosition.y - adjacentTiles[(HexDirection)f].gameObject.transform.localPosition.y, 0);

                    var v7 = v5 - new Vector3(0, this.transform.position.y + 1f, 0);

                    var v8 = v6 - new Vector3(0, adjacentTiles[(HexDirection)f].gameObject.transform.position.y + 1f, 0);

                    AddQuad(v5, v6, v7, v8, color, adjacentTiles[(HexDirection)f].color, terrainIndex, adjacentTiles[(HexDirection)f].terrainIndex);
                }
            }
        }

        hexMesh.SetVertices(vertices);
        hexMesh.SetTriangles(triangles, 0);
        hexMesh.RecalculateNormals();
        hexMesh.SetColors(colors);
        hexMesh.SetUVs(2, terrainTypes);
    }
    void GetAdjTile(HexDirection dir)
    {

        if (adjacentTiles.ContainsKey(dir))
        {
            var v1 = ToolsUtility.corners[(int)dir];

            var v2 = ToolsUtility.corners[(int)dir + 1];

            var v3 = ToolsUtility.extendCorners[(int)dir * 2] - new Vector3(0, this.transform.localPosition.y - adjacentTiles[dir].gameObject.transform.localPosition.y, 0);

            var v4 = ToolsUtility.extendCorners[(int)dir * 2 + 1] - new Vector3(0, this.transform.localPosition.y - adjacentTiles[dir].gameObject.transform.localPosition.y, 0);

            for (int i = 0; i < 4; i++)
            {
                var p1 = Vector3.Lerp(v1, v3,  (float)i / 4);
                var p2 = Vector3.Lerp(v2, v4, (float)i / 4);
                var p3 = Vector3.Lerp(v1, v3, (float)(i + 1) / 4);
                var p4 = Vector3.Lerp(v2, v4, (float)(i + 1) / 4);

                AddQuad(p1, p2, p3, p4, color, adjacentTiles[dir].color, terrainIndex, adjacentTiles[dir].terrainIndex);
            }
        }
    }
    void GetAdjTriangles(HexDirection dir)
    {
        if (adjacentTiles.ContainsKey(dir) && adjacentTiles.ContainsKey(dir + 1))
        {
            var v1 = ToolsUtility.corners[(int)dir + 1];

            var v2 = ToolsUtility.extendCorners[(int)dir*2 + 1] - new Vector3(0, this.transform.localPosition.y - adjacentTiles[dir].gameObject.transform.localPosition.y, 0);

            var v3 = ToolsUtility.extendCorners[(int)dir*2 + 2] - new Vector3(0, this.transform.localPosition.y - adjacentTiles[dir + 1].gameObject.transform.localPosition.y, 0);

            var d1 = Vector3.Lerp(v1, v2, 0.25f);
            var d2 = Vector3.Lerp(v1, v3, 0.25f);

            AddTriangle(v1, d1, d2, color, adjacentTiles[dir].color, adjacentTiles[dir + 1].color, terrainIndex, adjacentTiles[dir].terrainIndex, adjacentTiles[dir + 1].terrainIndex);

            for(int i = 1;i<4;i++)
            {
                var p1 = Vector3.Lerp(v1, v2, (float)i / 4);
                var p2 = Vector3.Lerp(v1, v3, (float)i / 4);
                var p3 = Vector3.Lerp(v1, v2, (float)(i + 1) / 4);
                var p4 = Vector3.Lerp(v1, v3, (float)(i + 1) / 4);

                AddQuad(p1, p2, p3, p4, color, adjacentTiles[dir].color, adjacentTiles[dir + 1].color, terrainIndex, adjacentTiles[dir].terrainIndex, adjacentTiles[dir + 1].terrainIndex);
            }
        }
    }
    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4,Color c1, Color c2, float t1, float t2)
    {
        int vertexIndex = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);

        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);

        terrainTypes.Add(new Vector3(t1, t2, t1));
        terrainTypes.Add(new Vector3(t1, t2, t1));
        terrainTypes.Add(new Vector3(t1, t2, t1));
        terrainTypes.Add(new Vector3(t1, t2, t1));

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }
    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Color c1, Color c2, Color c3, float t1, float t2, float t3)
    {
        int vertexIndex = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);

        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);

        terrainTypes.Add(new Vector3(t1, t2, t1));
        terrainTypes.Add(new Vector3(t1, t2, t1));
        terrainTypes.Add(new Vector3(t1, t2, t1));
        terrainTypes.Add(new Vector3(t1, t2, t1));

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }
    void AddTriangle(Vector3 v1,Vector3 v2, Vector3 v3, Color c1, Color c2, Color c3, float t1, float t2, float t3)
    {
        int vertexIndex = vertices.Count;

        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);

        terrainTypes.Add(new Vector3(t1, t2, t3));
        terrainTypes.Add(new Vector3(t1, t2, t3));
        terrainTypes.Add(new Vector3(t1, t2, t3));

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
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
            case TileSelectionType.Selected:
                {
                    indicatorRenderer.material.color = new Color(1, 0, 1, 1f);
                    break;
                }
        }
    }
}
