using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : Photon.PunBehaviour
{
	public GameObject[] terrainParts;

    public GameObject arcaneCircle;
   	public GameObject lavaEffect;
    private bool groundRaised;

    public AudioSource soundCrumble;
    public AudioSource soundCrumbleLong;

    private float stageInterval = 10; // how long is each stage
    private float roundTimeElapsed = 0f;
	private int currentScalingIndex = -1;

	private NetworkGameManager gameManager;


    void Awake()
    {
		gameManager = GameObject.FindWithTag("GameController").GetComponent<NetworkGameManager>();
		transform.position = new Vector3(transform.position.x, -8, transform.position.z);
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
    }

    void Update()
    {
        if(gameManager.GetState() == GameState.IN_ROUND)
            roundTimeElapsed += Time.deltaTime;
        
        int index = (int)(roundTimeElapsed / stageInterval);
        if (index != currentScalingIndex)
        {
            StartCoroutine(Camera.main.GetComponent<GameCamera>().Shake(.3f, 2f));
            //soundCrumble.Play();

			for (int i = 0; i < terrainParts.Length; i++)
			{
				if (i <= index)
					terrainParts[i].gameObject.SetActive(true);
				else
					terrainParts[i].gameObject.SetActive(false);
			}

            currentScalingIndex = index;
        }

		if (transform.position.y > 2f -index * 4f) // treba podesiti ovo, dakle da se spušta ovisno na kojem je stageu (index)
			transform.Translate(Vector3.down * Time.deltaTime * 2f);
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
        //soundCrumbleLong.Play();
        StartCoroutine(Camera.main.GetComponent<GameCamera>().Shake(.3f, 4.2f));

        while (!groundRaised && timePassedSinceGroundRaised < 2f)
        {
            if (transform.position.y <= 0f)
            {
                transform.Translate(new Vector3(0, 2f * Time.deltaTime, 0));
                lavaEffect.SetActive(true);
            }
            else if (transform.position.y >= 0f)
                timePassedSinceGroundRaised += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }

        arcaneCircle.SetActive(false);
        lavaEffect.SetActive(false);
    }

	public void ReloadTerrain()
	{
		transform.position = new Vector3(transform.position.x, -8, transform.position.z);
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
