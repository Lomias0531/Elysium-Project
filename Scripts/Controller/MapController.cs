using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MapController : Singletion<MapController>
{
    public BaseTile tileTemplate;
    public Transform tileContainer;

    public int mapWidth = 50;
    public int mapHeight = 50;

    int curSeed;
    public Dictionary<Vector3Int,BaseTile> mapTiles = new Dictionary<Vector3Int, BaseTile> ();
    public Dictionary<Vector3Int, BaseResource> resourcesDic = new Dictionary<Vector3Int, BaseResource> ();
    public Dictionary<string, BaseObj> entityDic = new Dictionary<string, BaseObj> ();

    public List<GameObject> treesTemplate = new List<GameObject> ();
    public List<GameObject> rocksTemplate = new List<GameObject> ();
    public List<GameObject> metalTemplate = new List<GameObject> ();
    public GameObject waterTemplate;
    GameObject water;
    float waveTime;
    public BaseResource resourceTemplate;
    public Transform tsf_ResContainer;
    // Start is called before the first frame update
    void Start()
    {
        curSeed = UnityEngine.Random.Range(0, 3000);
        GenerateMap(mapWidth, mapHeight, curSeed, 15f, 1, 0.3f, 8f, new Vector2(0, 0));
    }

    // Update is called once per frame
    async void Update()   
    {
        if(water != null)
        {
            waveTime += Time.deltaTime * 0.4f;
            if (waveTime > 360) waveTime = 0;
            var value = 0.25f + (Mathf.Sin(Mathf.PI * 0.5f * waveTime) * 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * waveTime)) * 0.05f;
            water.transform.position = new Vector3(25, value, 25);
        }
    }
    #region Generation
    public void GenerateMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float minHeight = Mathf.Infinity;
        float maxHeight = Mathf.Infinity * -1;

        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
            }
        }

        List<BaseTile> tileToAlign = new List<BaseTile>();

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);

                if (noiseMap[x, y] > maxHeight) maxHeight = noiseMap[x, y];
                if (noiseMap[x, y] < minHeight) minHeight = noiseMap[x, y];

                var tile = Instantiate(tileTemplate, tileContainer);
                tile.originalPos = new Vector2Int(x, y);

                noiseMap[x, y] = Mathf.Ceil(noiseMap[x, y] / 0.1f) * 0.1f;

                tile.InitHex();

                if (noiseMap[x, y] < 0.2)
                {
                    tile.gameObject.transform.localPosition = new Vector3(x + (y % 2 == 0 ? 0 : 0.5f), -noiseMap[x, y], 0.866025404f * y);
                    tile.PaintTile(Color.white, 4f);
                    tile.terrainType = BaseTile.TerrainType.Water;
                }
                if (noiseMap[x, y] >= 0.2 && noiseMap[x, y] < 0.3)
                {
                    tile.gameObject.transform.localPosition = new Vector3(x + (y % 2 == 0 ? 0 : 0.5f), noiseMap[x, y] * 0.5f, 0.866025404f * y);
                    tile.PaintTile(Color.white, 2f);
                    tile.terrainType = BaseTile.TerrainType.Water;
                }
                if (noiseMap[x, y] >= 0.3 && noiseMap[x, y] < 0.7)
                {
                    tile.gameObject.transform.localPosition = new Vector3(x + (y % 2 == 0 ? 0 : 0.5f), noiseMap[x, y], 0.866025404f * y);
                    tile.PaintTile(Color.white, 0f);
                    tile.terrainType = BaseTile.TerrainType.Plain;
                }
                if (noiseMap[x, y] >= 0.7)
                {
                    tile.gameObject.transform.localPosition = new Vector3(x + (y % 2 == 0 ? 0 : 0.5f), noiseMap[x, y] * 2f, 0.866025404f * y);
                    tile.PaintTile(Color.white, 3f);
                    tile.terrainType = BaseTile.TerrainType.Rocks;
                }

                tileToAlign.Add(tile);
            }
        }

        RefreshTileCoord(tileToAlign);

        GenerateWater();
        GenerateResources();
    }
    void RefreshTileCoord(List<BaseTile> tiles)
    {
        foreach (var tile in tiles)
        {
            tile.Pos = new Vector3Int(0, 0, 0);

            tile.Pos.x = tile.originalPos.x - tile.originalPos.y / 2;
            tile.Pos.z = tile.originalPos.y;
            tile.Pos.y = 0 - tile.Pos.x - tile.Pos.z;

            mapTiles.Add(tile.Pos, tile);
        }

        foreach (var tile in mapTiles)
        {
            tile.Value.GetAdjacentTiles();
        }
        foreach (var tile in mapTiles)
        {
            tile.Value.BlendAdjTileColor();
        }
    }
    void GenerateWater()
    {
        water = GameObject.Instantiate(waterTemplate, tileContainer);
        water.transform.localPosition = new Vector3(25, 0.2f, 25);
    }
    void GenerateResources()
    {
        int treeCount = 50 + Random.Range(-5, 5);
        for(int i = 0;i< treeCount; i++)
        {
            bool resGenerated = false;
            do
            {
                var index = Random.Range(0, mapTiles.Count);
                var tile = mapTiles.ToList()[index].Value;
                if (tile.terrainType == BaseTile.TerrainType.Plain)
                {
                    if (resourcesDic.ContainsKey(tile.Pos))
                    {
                        resGenerated = false;
                        continue;
                    }
                    var res = GameObject.Instantiate(resourceTemplate, tsf_ResContainer);
                    res.transform.position = tile.transform.position;
                    var resIndex = Random.Range(0, 3);
                    res.InitResource(tile.Pos, (BaseResource.ResourceType)resIndex);

                    entityDic.Add(res.ID, res);
                    resourcesDic.Add(res.Pos, res);
                    resGenerated = true;
                }
            } while (!resGenerated);
        }
    }
    #endregion
}
