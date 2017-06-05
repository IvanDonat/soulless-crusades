using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : Photon.PunBehaviour
{
	public GameObject[] terrainParts;
    public GameObject obolisk;

    public GameObject arcaneCircle;
   	public GameObject lavaEffect;
    private bool groundRaised;

    public AudioSource soundCrumble;
    public AudioSource soundCrumbleLong;

    private float stageInterval = 30; // how long is each stage
    private float roundTimeElapsed = 0f;
	private int currentScalingIndex = -1;
    private bool moveCamera = false;

	private NetworkGameManager gameManager;
    private PlayerScript player;

    void Awake()
    {
		gameManager = GameObject.FindWithTag("GameController").GetComponent<NetworkGameManager>();
		transform.position = new Vector3(transform.position.x, -28.3f, transform.position.z);
    }

    void Start()
    {
		StartRound();
		//StartCoroutine(ProjectArcaneCircle());
    }

    public void StartRound()
	{
        currentScalingIndex = -1;
        roundTimeElapsed = 0f;
        StartCoroutine(DelayCamBlock());
    }

    private IEnumerator DelayCamBlock()
    {
        yield return new WaitForSeconds(3f);
        moveCamera = false;
    }

    void Update()
    {
        if (gameManager.GetState() == GameState.IN_ROUND)
            roundTimeElapsed += Time.deltaTime;

        int index = (int)(roundTimeElapsed / stageInterval);
        if (index != currentScalingIndex)
        {
            if (index != 0)
            {
                StartCoroutine(Camera.main.GetComponent<GameCamera>().Shake(.3f, 2f));
                soundCrumble.Play();

                PhotonNetwork.Instantiate("Spells/Teleport", new Vector3(Random.Range(-10f, 10f), 15f, Random.Range(-10f, 10f)), Quaternion.identity, 0);
            }

            currentScalingIndex = index;
        }

        if (index == 0)
        {
            if (transform.position.y > 2f - index * 9f)
                transform.Translate(Vector3.down * Time.deltaTime * 2f);

            terrainParts[index].gameObject.SetActive(true);
        }
        if (index == 1)
        {
            if (transform.position.y > 2f - index * 8.75f)
                transform.Translate(Vector3.down * Time.deltaTime * 2f);

            terrainParts[index].gameObject.SetActive(true);
        }
        if (index == 2)
        {
            if (transform.position.y > 2f - index * 7.8f)
                transform.Translate(Vector3.down * Time.deltaTime * 2f);

            terrainParts[index].gameObject.SetActive(true);
        }
        if (index == 3)
        {
            if (transform.position.y > 2f - index * 7.6f)
                transform.Translate(Vector3.down * Time.deltaTime * 2f);

            terrainParts[index].gameObject.SetActive(true);
        }
        if (index == 4)
        {
            if (transform.position.y > 2f - index * 7.45f)
                transform.Translate(Vector3.down * Time.deltaTime * 2f);

            obolisk.SetActive(true);
            terrainParts[index].gameObject.SetActive(true);
        }
    }

    void LateUpdate()
    {
        if (moveCamera)
        {
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, Quaternion.Euler(60, 0, 0), Time.deltaTime);
            Camera.main.GetComponent<GameCamera>().camPos = Vector3.Lerp(Camera.main.GetComponent<GameCamera>().camPos, new Vector3(0f, 25f, -15f), Time.deltaTime);
            Camera.main.GetComponent<GameCamera>().offset = Vector3.Lerp(Camera.main.GetComponent<GameCamera>().offset, new Vector3(0f, 25f, -15f), Time.deltaTime);
        }
    }

    public IEnumerator ProjectArcaneCircle()
    {
        bool arcaneCircleProjected = false;
        groundRaised = false;

        arcaneCircle.SetActive(true);
        lavaEffect.SetActive(false);
        arcaneCircle.transform.localScale = new Vector3(0, 0, 1);

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
            if (transform.position.y <= 0f)
            {
                transform.Translate(new Vector3(0, 6f * Time.deltaTime, 0));
                lavaEffect.SetActive(true);
            }
            else if (transform.position.y >= 0f)
                timePassedSinceGroundRaised += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }

        arcaneCircle.SetActive(false);
        lavaEffect.SetActive(false);
        obolisk.SetActive(false);

        for (int i = 0; i < terrainParts.Length; i++)
            terrainParts[i].gameObject.SetActive(false);

        moveCamera = true;
    }

	public void ReloadTerrain()
	{
		transform.position = new Vector3(transform.position.x, 0.013f, transform.position.z);
		currentScalingIndex = -1;
		roundTimeElapsed = 0f;

		for (int i = 0; i < terrainParts.Length; i++)
		{
			if (i <= currentScalingIndex)
				terrainParts[i].gameObject.SetActive(true);
			else
				terrainParts[i].gameObject.SetActive(false);
		}
	}
}
