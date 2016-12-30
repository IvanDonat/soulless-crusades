using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    WARMUP,
    IN_ROUND,
    BETWEEN_ROUNDS,
    GAME_OVER
}

public class NetworkGameManager : Photon.PunBehaviour {
    public Transform playerPrefab;

    public Text gameTimeText;
    private float gameTime = 0;

    private TerrainManager terrainManager;

    public GameObject sharedUI;
    public GameObject playingUI;
    public GameObject spectatorUI;
    public GameObject betweenRoundsUI;
    public GameObject gameOverUI;

    public GameObject scorePanel;
    public Transform scoreContent;
    public GameObject scoreItem;

    public Text roundOverText;

    public Slider castingBar;

    private List<GameObject> scoreList = new List<GameObject>();
    private RectTransform scorePanelRect;

    public const string GAME_STATE = "gamestate";
    private GameState gameState = GameState.WARMUP;

    // between rounds
    private const float timeBetweenRounds = 5f;
    private float timeBetweenRoundsCounter;

    // tracks wins for each player, UNSYNCED FOR EVERYONE ELSE!!
    private Dictionary<PhotonPlayer, int> masterClientWinTracker = new Dictionary<PhotonPlayer, int>();
    private int winsForGameOver = -1;

    private bool androidShowScore = false;
    public GameObject showScore, hideScore;

    public Text winner;

    void Start()
    {
        terrainManager = GameObject.FindWithTag("Terrain").GetComponent<TerrainManager>();
        StartCoroutine(terrainManager.ProjectArcaneCircle());

        playingUI.SetActive(false);
        spectatorUI.SetActive(false);
        betweenRoundsUI.SetActive(false);
        gameOverUI.SetActive(false);

        StartCoroutine(Wait(8.5f));

        scorePanelRect = scorePanel.GetComponent<RectTransform>();

        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            GameObject go = Instantiate(scoreItem, scoreContent) as GameObject;
            go.name = p.NickName;

            foreach (RectTransform r in go.GetComponentInChildren<RectTransform>())
            {
                if (r.gameObject.name == "Name")
                    r.GetComponent<Text>().text = p.NickName;
                else if (r.gameObject.name == "Kills")
                    r.GetComponent<Text>().text = "0";
                else if (r.gameObject.name == "Deaths")
                    r.GetComponent<Text>().text = "0";
                else if (r.gameObject.name == "Rounds Won")
                    r.GetComponent<Text>().text = "0";
            }

            scoreList.Add(go);
        }

        PlayerProperties.SetProperty(PlayerProperties.KILLS, 0);
        PlayerProperties.SetProperty(PlayerProperties.DEATHS, 0);
        PlayerProperties.SetProperty(PlayerProperties.WINS, 0);
        PlayerProperties.SetProperty(PlayerProperties.ALIVE, false);
        PlayerProperties.SetProperty(PlayerProperties.HEALTH, 100);

        winsForGameOver = (int) PhotonNetwork.room.CustomProperties["maxwins"];

        foreach (var player in PhotonNetwork.playerList)
            masterClientWinTracker[player] = 0;

        if(PhotonNetwork.isMasterClient)
            SetState(GameState.WARMUP); 
        timeBetweenRoundsCounter = timeBetweenRounds;
    }

    void Update()
    {
        gameTime += Time.deltaTime;

        if (!PhotonNetwork.isMasterClient)
            gameState = GetState();

        int minutes = (int)gameTime / 60;
        int seconds = (int)gameTime % 60;
        gameTimeText.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");

        if (Input.GetKey(KeyCode.Tab) || GetState() == GameState.BETWEEN_ROUNDS || androidShowScore == true)
            scorePanel.transform.Translate(0, -500f * Time.deltaTime, 0);
        else
            scorePanel.transform.Translate(0, 500f * Time.deltaTime, 0);

        scorePanelRect.anchoredPosition = new Vector2(0, Mathf.Clamp(scorePanelRect.anchoredPosition.y, -144f, 99f));

        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            foreach (GameObject go in scoreList)
            {
                if (go.name != p.NickName)
                    continue;

                foreach (RectTransform r in go.GetComponentsInChildren<RectTransform>())
                {
                    if(p.CustomProperties[PlayerProperties.KILLS] == null)
                        return;

                    if (r.gameObject.name == "Name")
                        r.GetComponent<Text>().text = p.NickName;
                    else if (r.gameObject.name == "Kills")
                        r.GetComponent<Text>().text = p.CustomProperties[PlayerProperties.KILLS].ToString();
                    else if (r.gameObject.name == "Deaths")
                        r.GetComponent<Text>().text = p.CustomProperties[PlayerProperties.DEATHS].ToString();
                    else if (r.gameObject.name == "Rounds Won")
                        r.GetComponent<Text>().text = p.CustomProperties[PlayerProperties.WINS].ToString();
                }
            }

        }

        if (GetState() == GameState.BETWEEN_ROUNDS && PhotonNetwork.isMasterClient)
        {
            timeBetweenRoundsCounter -= Time.deltaTime;
            if (timeBetweenRoundsCounter <= 0)
            {
                photonView.RPC("StartNewRound", PhotonTargets.All);
                timeBetweenRoundsCounter = timeBetweenRounds;
            }
        }
    }

    [PunRPC]
    public void StartNewRound()
    {
        playingUI.SetActive(true);
        spectatorUI.SetActive(false);
        betweenRoundsUI.SetActive(false);
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(Random.Range(-15f, 15f), 1, Random.Range(-15f, 15f)), Quaternion.identity, 0);
        terrainManager.StartRound();
        if(PhotonNetwork.isMasterClient)
            SetState(GameState.IN_ROUND);
    }

    private IEnumerator Wait(float sec)
    {
        yield return new WaitForSeconds(sec);
        if(PhotonNetwork.isMasterClient)
            photonView.RPC("StartNewRound", PhotonTargets.All);

    }

    public GameObject GetPlayingUI()
    {
        return playingUI;
    }

    public GameObject GetSpectatorUI()
    {
        return spectatorUI;
    }

    [PunRPC]
    public void DestroySpell(int id)
    {
        foreach (GameObject sp in GameObject.FindGameObjectsWithTag("Spell"))
        {
            if (sp.GetComponent<PhotonView>().viewID == id)
            {
                sp.GetComponent<PhotonView>().RPC("Remove", PhotonTargets.All);
                break;
            }
        }
    }

    [PunRPC]
    public void OnPlayerDeath(PhotonPlayer player)
    { // is called for everyone by dying player

        if(PhotonNetwork.isMasterClient && GetState() == GameState.IN_ROUND)
        {
            int aliveCount = 0;
            PhotonPlayer alivePlayer = null;
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                if ((bool)p.CustomProperties[PlayerProperties.ALIVE] == true)
                {
                    alivePlayer = p;
                    aliveCount++;
                }
            }
            
            if (aliveCount <= 1)
            {
                if (alivePlayer != null)
                {
                    masterClientWinTracker[alivePlayer]++;

                    if (masterClientWinTracker[alivePlayer] >= winsForGameOver)
                    {
                        photonView.RPC("GameOver", PhotonTargets.All, alivePlayer);
                        SetState(GameState.GAME_OVER);
                        return;
                    }
                }

                photonView.RPC("RoundOver", PhotonTargets.All, alivePlayer);
                SetState(GameState.BETWEEN_ROUNDS);
            }
        }

    }

    [PunRPC]
    public void RoundOver(PhotonPlayer winner)
    {
        roundOverText.text = winner.NickName + "\n\nWon this round!";
        if (PhotonNetwork.player == winner)
            PlayerProperties.IncrementProperty(PlayerProperties.WINS); 

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
            if (player.GetComponent<PhotonView>().isMine)
                player.GetComponent<PlayerScript>().Die(true);

        betweenRoundsUI.SetActive(true);
        spectatorUI.SetActive(false);
        playingUI.SetActive(false);
    }

    [PunRPC]
    public void GameOver(PhotonPlayer lastRoundWinner)
    {
        if (PhotonNetwork.player == lastRoundWinner)
            PlayerProperties.IncrementProperty(PlayerProperties.WINS); 

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        if (player.GetComponent<PhotonView>().isMine)
            player.GetComponent<PlayerScript>().Die(true);

        betweenRoundsUI.SetActive(false);
        spectatorUI.SetActive(false);
        playingUI.SetActive(false);
        gameOverUI.SetActive(true);

        winner.text = lastRoundWinner.NickName + " is the winner!";
        AndroidShowScore();
        StartCoroutine(DisconnectTimer());
    }


    [PunRPC]
    public void GotKill(PhotonPlayer victim)
    {
        print("You killed: " + victim.NickName);
        PlayerProperties.IncrementProperty(PlayerProperties.KILLS);        
    }

    IEnumerator DisconnectTimer()
    {
        yield return new WaitForSeconds(6f);
        Disconnect();
    }

    public List<PhotonPlayer> GetSortedPlayerList()
    {
        return PhotonNetwork.playerList.OrderBy(pl => 
            {if ((bool) pl.CustomProperties[PlayerProperties.ALIVE] == false) return 10000 + pl.GetScore(); 
            else return pl.GetScore();}).ToList();
    }

    public void SetState(GameState s)
    {
        if (PhotonNetwork.isMasterClient)
        {
            gameState = s;
            var props = new ExitGames.Client.Photon.Hashtable();
            props.Add(GAME_STATE, s);
            PhotonNetwork.room.SetCustomProperties(props, null);
        }
        else
            Debug.LogError("non master client tried to set state");
    }

    public GameState GetState()
    {
        if (PhotonNetwork.isMasterClient)
        {
            return gameState;
        }
        else
        {
            return (GameState)PhotonNetwork.room.CustomProperties[GAME_STATE];
        }
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void AndroidShowScore()
    {
        hideScore.SetActive(true);
        showScore.SetActive(false);
        androidShowScore = true;
    }

    public void AndroidHideScore()
    {
        showScore.SetActive(true);
        hideScore.SetActive(false);
        androidShowScore = false;
    }

    void OnGUI()
    {
        if(PhotonNetwork.connected)
            GUILayout.Label(PhotonNetwork.connectionState + " " + PhotonNetwork.GetPing() + " ms");
    }
}
