using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformManager : Photon.PunBehaviour
{

    public GameObject arcaneCircle;
    public GameObject lavaEffect;
    public GameObject platform;
    private bool groundRaised;

    public AudioSource soundCrumble;
    public AudioSource soundCrumbleLong;

    private NetworkGameManager gameManager;

    private float startingHeight = -0.63f;
    private float[] heights = { -7.28f, -14.34f, -21.6f, -28.5f };
    private float stageInterval = 30f; // how long is each stage
    private float roundTimeElapsed = 0f;
    private int currentHeightIndex = 0;

    void Awake()
    {
        gameManager = GameObject.FindWithTag("GameController").GetComponent<NetworkGameManager>();
    }

    void Start()
    {
        //StartCoroutine(SetTerrainToCircle(startingHeight, 0f));
    }

    public void StartRound()
    {
        currentHeightIndex = 0;
        roundTimeElapsed = 0f;
        //StartCoroutine(SetTerrainToCircle(startingHeight, 0f));

        //if (PhotonNetwork.isMasterClient)
            //photonView.RPC("SpawnRocks", PhotonTargets.All, Random.Range(1, 10000));
    }

    void Update()
    {
        if (gameManager.GetState() == GameState.IN_ROUND)
            roundTimeElapsed += Time.deltaTime;

        int index = (int)(roundTimeElapsed / stageInterval);
        if (index >= heights.Length)
            index = heights.Length - 1;
        if (index != currentHeightIndex)
        {
            StartCoroutine(SetTerrainToCircle(heights[index], 1f));
            StartCoroutine(Camera.main.GetComponent<GameCamera>().Shake(.3f, 2f));
            soundCrumble.Play();
            currentHeightIndex = index;
        }
    }

    public IEnumerator ProjectArcaneCircle()
    {
        bool arcaneCircleProjected = false;
        groundRaised = false;

        arcaneCircle.SetActive(true);
        lavaEffect.SetActive(false);
        arcaneCircle.transform.localScale = new Vector3(0, 0, 1);
        //terrain.transform.position = new Vector3(-data.size.x / 2, -1.5f, -data.size.z / 2);

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
            if (platform.transform.position.y <= startingHeight)
            {
                platform.transform.Translate(new Vector3(0, 5f * Time.deltaTime, 0));
                lavaEffect.SetActive(true);
            }
            else if (platform.transform.position.y >= startingHeight)
                timePassedSinceGroundRaised += Time.deltaTime;

            yield return new WaitForEndOfFrame();
        }

        arcaneCircle.SetActive(false);
        lavaEffect.SetActive(false);
    }


    public IEnumerator SetTerrainToCircle(float height, float time)
    {
        yield return new WaitForSeconds(time);

        platform.transform.position = new Vector3(0, height, 0);
    }
}
