using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaScript : MonoBehaviour {
    private Terrain terrain;
    private TerrainData data;
    private int width, height;
    float[,] heights;

    private float terrainUpdateInterval = 1 / 30f;
    private float lastUpdatedTimeStamp;

    void Awake()
    {
        terrain = GetComponent<Terrain>();
        data = terrain.terrainData;

        width = data.heightmapWidth;
        height = data.heightmapHeight;
        heights = data.GetHeights(0, 0, width, height);
    }

    void Update()
    {
        if (Time.time - lastUpdatedTimeStamp > terrainUpdateInterval)
        {
            Recalculate();
            lastUpdatedTimeStamp = Time.time;
        }
    }

    private void Recalculate()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                heights[i, j] = Mathf.Sin(Time.time + i / 10 + j / 10) / 15f;;
            }
        }

        ReloadTerrain();
    }

    void ReloadTerrain()
    {
        data.SetHeights(0, 0, heights);
    }

    void OnApplicationQuit()
    { // inside unity editor, reset terrain
        int width = data.heightmapWidth;
        int height = data.heightmapHeight;
        float[,] heights = data.GetHeights(0, 0, width, height);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                heights[i, j] = 0;
            }
        }

        data.SetHeights(0, 0, heights);
    }
}
