﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour {
    /* what the terrain asset should be like (in case it gets changed)
     * 
     * relatively low Terrain Width and Length, depending on arena size
     * low Terrain Height, single digit
     * 
     * Heightmap Resolution impacts performance and the way it looks
     * preferably around 65, 129, 257?
     */
    public GameObject arcaneCircle;
    public GameObject lavaEffect;
    private bool groundRaised;

    private Terrain terrain;
    private TerrainData data;
    private int width, height;
    private float[,] heights;
    private float timePassedSinceTerrainUpdate = -1f;

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
        timePassedSinceTerrainUpdate -= Time.deltaTime;
        if (timePassedSinceTerrainUpdate < 0)
        {
            SetTerrainToCircle(75f);

            ReloadTerrain();

            timePassedSinceTerrainUpdate = 1 / 10f;
        }
    }

    public IEnumerator ProjectArcaneCircle()
    {
        bool arcaneCircleProjected = false;
        groundRaised = false;

        arcaneCircle.SetActive(true);
        lavaEffect.SetActive(false);
        arcaneCircle.transform.localScale = new Vector3(0, 0, 1);
        terrain.transform.position = new Vector3(-data.size.x / 2, -1.5f, -data.size.z / 2);

        while (!arcaneCircleProjected)
        {
            if (arcaneCircle.transform.localScale.x <= 7f && arcaneCircle.transform.localScale.y <= 7f)
                arcaneCircle.transform.localScale += new Vector3(3f * Time.deltaTime, 3f * Time.deltaTime, 0f);
            else
                arcaneCircleProjected = true;
            
            yield return new WaitForEndOfFrame();
        }

        float timePassedSinceGroundRaised = 0f;
        while (!groundRaised && timePassedSinceGroundRaised < 2f)
        {
            if (terrain.transform.position.y <= -0.6f)
            {
                terrain.transform.Translate(new Vector3(0, 0.2f * Time.deltaTime, 0));
                lavaEffect.SetActive(true);
            }
            else if (terrain.transform.position.y >= -0.6f)
                timePassedSinceGroundRaised += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }

        arcaneCircle.SetActive(false);
        lavaEffect.SetActive(false);
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
                heights[i, j] = 1;
            }
        }

        data.SetHeights(0, 0, heights);
    }

    public void SetTerrainToCircle(float radius)
    {
        float terrainStartDescentDist = radius;
        float descentLength = 10;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float h = 0;

                float offsetX = Mathf.Abs(i - width / 2);
                float offsetY = Mathf.Abs(j - height / 2);
                float dist = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);

                if (dist < terrainStartDescentDist)
                    h = 1;
                else if (dist >= terrainStartDescentDist + descentLength)
                    h = 0;
                else
                {
                    h = Mathf.Lerp(1, 0, (dist - terrainStartDescentDist) / (dist - terrainStartDescentDist + descentLength));
                }

                heights[i, j] = h;
            }
        }
    }
}
