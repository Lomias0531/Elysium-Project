using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
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
        int[] resCount = new int[3];
        resCount[0] = Random.Range(2, 5);
        resCount[1] = Random.Range(1, 4);
        resCount[2] = Random.Range(1, 3);

        for(int i = 0;i<resCount.Length;i++)
        {
            for(int j = 0; j < resCount[i];j++)
            {
                BaseTile origin;
                bool isOriginSet;
                do
                {
                    isOriginSet = true;
                    var index = Random.Range(0,mapTiles.Count);
                    origin = mapTiles.ToList()[index].Value;
                    if(resourcesDic.ContainsKey(origin.Pos))
                    {
                        isOriginSet = false;
                        continue;
                    }
                    if(j == 0)
                    {
                        if (origin.terrainType == BaseTile.TerrainType.Water)
                        {
                            isOriginSet = false;
                            continue;
                        }
                    }
                }while(!isOriginSet);

                BaseResource res = GameObject.Instantiate(resourceTemplate, tsf_ResContainer);
                res.transform.position = origin.transform.position;
                res.InitResource(origin.Pos, (BaseResource.ResourceType)i);

                entityDic.Add(res.ID, res);
                resourcesDic.Add(res.Pos, res);

                List<BaseTile> ResToGen = new List<BaseTile>();
                List<BaseTile> ResGenerated = new List<BaseTile>
                {
                    origin
                };

                do
                {
                    ResToGen.Clear();
                    foreach (var tile in ResGenerated)
                    {
                        foreach (var adjTile in tile.adjacentTiles)
                        {
                            if (!resourcesDic.ContainsKey(adjTile.Value.Pos) && !ResToGen.Contains(adjTile.Value))
                            {
                                float rnd = Random.Range(0f, 1f);

                                float para = 0f;
                                switch(adjTile.Value.terrainType)
                                {
                                    case BaseTile.TerrainType.Plain:
                                        {
                                            para = 0.7f;
                                            break;
                                        }
                                    default:
                                        {
                                            para = 0.9f;
                                            break;
                                        }
                                }

                                float threshold = Mathf.Exp(-para * Tools.GetDistance(adjTile.Value.Pos, origin.Pos));
                                if (rnd <= threshold)
                                {
                                    ResToGen.Add(adjTile.Value);
                                }
                            }
                        }
                    }

                    foreach (var item in ResToGen)
                    {
                        BaseResource res1 = GameObject.Instantiate(resourceTemplate, tsf_ResContainer);
                        res1.transform.position = item.transform.position;
                        res1.InitResource(item.Pos, (BaseResource.ResourceType)i);

                        entityDic.Add(res1.ID, res1);
                        resourcesDic.Add(res1.Pos, res1);

                        ResGenerated.Add(item);
                    }
                } while (ResToGen.Count > 0);
            }
        }
    }
    #endregion
}
