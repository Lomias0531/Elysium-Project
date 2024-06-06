using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : Singletion<MapController>
{
    public BaseTile tileTemplate;
    public Transform tileContainer;

    public int mapWidth = 50;
    public int mapHeight = 50;

    int curSeed;
    public Dictionary<Vector3Int,BaseTile> mapTiles = new Dictionary<Vector3Int, BaseTile> ();
    // Start is called before the first frame update
    void Start()
    {
        curSeed = UnityEngine.Random.Range(0, 3000);
        GenerateMap(mapWidth, mapHeight, curSeed, 10f, 1, 0.3f, 8f, new Vector2(0, 0));
    }

    // Update is called once per frame
    void Update()   
    {
        
    }
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
                tile.gameObject.transform.localPosition = new Vector3(x + (y % 2 == 0 ? 0 : 0.5f), 0, 0.866025404f * y);


                tile.InitHex();

                if (noiseMap[x, y] < 0.3) tile.PaintTile(Color.blue);
                if (noiseMap[x, y] >= 0.3 && noiseMap[x, y] < 0.6) tile.PaintTile(Color.green);
                if (noiseMap[x, y] >= 0.6) tile.PaintTile(Color.yellow);

                tileToAlign.Add(tile);
            }
        }

        RefreshTileCoord(tileToAlign);
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
}
