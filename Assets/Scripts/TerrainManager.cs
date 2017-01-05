using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : Photon.PunBehaviour
{
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

    public AudioSource soundCrumble;
    public AudioSource soundCrumbleLong;

    private NetworkGameManager gameManager;
    private Terrain terrain;
    private TerrainData data;
    private int width, height;
    private float[,] heights;

    private float startRadius = 75f;
    private float[] radiusScaling = {1.0f, 0.9f, 0.8f, 0.75f, 0.7f, 0.65f, 0.60f, 0.5f, 0.45f, 0.4f, 0.35f, 0.3f};
    private float stageInterval = 15f; // how long is each stage
    private float roundTimeElapsed = 0f;
    private int currentScalingIndex = 0;


    void Awake()
    {
        gameManager = GameObject.FindWithTag("GameController").GetComponent<NetworkGameManager>();

        terrain = GetComponent<Terrain>();
        data = terrain.terrainData;

        width = data.heightmapWidth;
        height = data.heightmapHeight;
        heights = data.GetHeights(0, 0, width, height);
    }

    void Start()
    {
        StartCoroutine(SetTerrainToCircle(startRadius, 0f));
    }

    public void StartRound()
    {
        currentScalingIndex = 0;
        roundTimeElapsed = 0f;
        StartCoroutine(SetTerrainToCircle(startRadius, 0f));

        if(PhotonNetwork.isMasterClient)
            photonView.RPC("SpawnRocks", PhotonTargets.All, Random.Range(1, 10000));
    }

    void Update()
    {
        if(gameManager.GetState() == GameState.IN_ROUND)
            roundTimeElapsed += Time.deltaTime;
        
        int index = (int)(roundTimeElapsed / stageInterval);
        if (index >= radiusScaling.Length)
            index = radiusScaling.Length - 1;
        if (index != currentScalingIndex)
        {
            StartCoroutine(SetTerrainToCircle(radiusScaling[index] * startRadius, 1f));
            StartCoroutine(Camera.main.GetComponent<GameCamera>().Shake(.3f, 2f));
            soundCrumble.Play();
            currentScalingIndex = index;
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
        soundCrumbleLong.Play();
        StartCoroutine(Camera.main.GetComponent<GameCamera>().Shake(.3f, 4.2f));

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

    public IEnumerator SetTerrainToCircle(float radius, float time)
    {
        yield return new WaitForSeconds(time);

        float descentLength = 10;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float h = 0;

                float offsetX = Mathf.Abs(i - width / 2);
                float offsetY = Mathf.Abs(j - height / 2);
                float dist = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);

                if (dist < radius)
                    h = 1;
                else if (dist >= radius + descentLength)
                    h = 0;
                else
                {
                    h = Mathf.Lerp(1, 0, (dist - radius) / (dist - radius + descentLength));
                }

                heights[i, j] = h;
            }
        }

        ReloadTerrain();
    }

    public IEnumerator SetTerrainToHexagon(float radius, float time) // @TODO FIX SLOPE
    {
        yield return new WaitForSeconds(time);

        float descentLength = 10;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float h = 0;

                float offsetX = Mathf.Abs(i - width / 2);
                float offsetY = Mathf.Abs(j - height / 2);
                float dist = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);
    
                if (radius * Mathf.Sqrt(2) - offsetX > offsetY)
                    h = 1f;
                else if (offsetX < radius && offsetY < radius)
                    h = 1f;
                else if (radius + descentLength < dist)
                    h = Mathf.Lerp(1, 0, (dist - radius) / (dist - radius + descentLength));
                else
                    h = 0f;

                heights[i, j] = h;
            }
        }

        ReloadTerrain();
    }

    [PunRPC]
    public void SpawnRocks(int seed)
    { // set up rocks, clear last ones
        foreach (var g in GameObject.FindGameObjectsWithTag("Rock"))
            Destroy(g);

        Random.InitState(seed);

        GameObject[] rocks = Resources.LoadAll<GameObject>("Rocks");
        foreach (var spawnpos in GameObject.FindGameObjectsWithTag("RockSpawnpoint"))
        {
            Transform rock = rocks[Random.Range(0, rocks.Length - 1)].transform;
            
            Vector3 pos = spawnpos.transform.position;
            pos.y = 40f;

            Quaternion rot = Quaternion.Euler(new Vector3(0, Random.Range(0, 359), 0));

            Instantiate(rock, pos, rot);
        }
    }
}
