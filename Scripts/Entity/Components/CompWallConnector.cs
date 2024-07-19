using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BaseTile;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class CompWallConnector : BaseComponent
{
    public GameObject BottomPoint;
    public GameObject TopPoint;
    public Dictionary<BaseTile.HexDirection, GameObject> wallDir = new Dictionary<BaseTile.HexDirection, GameObject>();
    public override void OnApply(int index)
    {
        
    }

    public override void OnDestroyThis()
    {
        foreach (var item in wallDir)
        {
            item.Value.gameObject.SetActive(false);
        }
    }

    public override void OnTriggerFunction(params object[] obj)
    {
        
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        for(int i = 0;i<6;i++)
        {
            wallDir.Add((BaseTile.HexDirection)i, null);
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        foreach (var tile in thisObj.GetTileWhereUnitIs().adjacentTiles)
        {
            var unit = tile.Value.GetEntitynThisTile();
            if(unit != null)
            {
                var wall = unit.GetDesiredComponent<CompWallConnector>();
                if (wall != null)
                {
                    if (wallDir[tile.Key] == null)
                    {
                        GenerateWall(wall, tile.Key);
                    }
                }
                else
                {
                    if (wallDir[tile.Key] != null)
                    {
                        HideWall(tile.Key);
                    }
                }
            }else
            {
                if (wallDir[tile.Key] != null)
                {
                    HideWall(tile.Key);
                }
            }
        }
    }
    void GenerateWall(CompWallConnector targetWall, BaseTile.HexDirection dir)
    {
        if(wallDir.ContainsKey(dir))
        {
            GameObject wallObj = new GameObject("Wall" + dir.ToString());
            wallObj.transform.SetParent(thisObj.gameObject.transform);
            Mesh wallMesh = wallObj.AddComponent<MeshFilter>().mesh = new Mesh();
            wallMesh.name = "Wall";
            MeshRenderer renderer = wallObj.AddComponent<MeshRenderer>();

            int vertexIndex = 0;
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            vertices.Add(this.TopPoint.transform.position);
            vertices.Add(this.BottomPoint.transform.position);
            vertices.Add(targetWall.TopPoint.transform.position);
            vertices.Add(targetWall.BottomPoint.transform.position);

            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);

            wallMesh.SetVertices(vertices.ToArray());
            wallMesh.SetTriangles(triangles.ToArray(), 0);

            wallMesh.RecalculateBounds();
            wallMesh.RecalculateNormals();

            renderer.material = (Material)Resources.Load("Materials/LaserWall");

            wallDir[dir] = wallObj;
        }
    }
    void HideWall(BaseTile.HexDirection dir)
    {
        if(wallDir.ContainsKey(dir))
        {
            Destroy(wallDir[dir].gameObject);
            wallDir[dir] = null;
        }
    }
}
