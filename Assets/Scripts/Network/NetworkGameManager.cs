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

public class NetworkGameManager : Photon.PunBehaviour 
{
    public Transform playerPrefab;
    public Transform gameOverDummyPlayer;
    public Transform soulPrefab;

    public Text gameTimeText;
    public Text roundTimeText;
    private static float gameTime = 0;
    private static float roundTime = 0;

    private TerrainManager terrainManager;

    public GameObject sharedUI;
    public GameObject playingUI;
    public GameObject spectatorUI;
    public GameObject betweenRoundsUI;
    public GameObject gameOverUI;

    public GameObject scorePanel;
    public Transform scoreContent;
    public GameObject scoreItem;

    public GameObject tooltipParent;
    public Text tooltipName;
    public Text tooltipDescription;

    public Text roundOverText;
    public Text roundOverTime;

    public Text miniScoresNames, miniScoresRounds;

    public GameObject infoPanel;
    public Text information;

    public Slider castingBar;

    private List<GameObject> scoreList = new List<GameObject>();
    private RectTransform scorePanelRect;

    public const string GAME_STATE = "gamestate";
    private GameState gameState = GameState.WARMUP;

    // between rounds
    private const float timeBetweenRounds = 10f;
    private float timeBetweenRoundsCounter;

    // tracks wins for each player, UNSYNCED FOR EVERYONE ELSE!!
    private Dictionary<PhotonPlayer, int> masterClientWinTracker = new Dictionary<PhotonPlayer, int>();
    private int winsForGameOver = -1;

    private bool androidShowScore = false;
    public GameObject showScore, hideScore;

    public Text winner;

    public GameObject deathParticles;

    public GameObject lensFlare;
 
    // chat
    public GameObject chatMsgPrefab;
    public InputField chatInput;
    public Scrollbar chatScroll;
    public AudioSource chatTickSound;

    public AudioSource startingMusic;
    public AudioSource fightingMusic;
    public AudioSource endMusic;

    void Start()
    {
        Events.Clear();
        terrainManager = GameObject.FindWithTag("Terrain").GetComponent<TerrainManager>();
        StartCoroutine(terrainManager.ProjectArcaneCircle());

        playingUI.SetActive(false);
        spectatorUI.SetActive(false);
        betweenRoundsUI.SetActive(false);
        gameOverUI.SetActive(false);

        tooltipParent.SetActive(false);

        gameTime = 0f;
        roundTime = 0f;

        StartCoroutine(Wait(8.5f));

        scorePanelRect = scorePanel.GetComponent<RectTransform>();

        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            bool mine = false;
            if (PhotonNetwork.player == p)
                mine = true;

            GameObject go = Instantiate(scoreItem, scoreContent) as GameObject;
            go.name = p.NickName;
            go.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);

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

                if (mine)
                    r.GetComponent<Text>().color = Color.Lerp(r.GetComponent<Text>().color, Color.red, 0.5f);
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

        if (gameState == GameState.IN_ROUND)
            roundTime += Time.deltaTime;

        if (!PhotonNetwork.isMasterClient)
            gameState = GetState();

        int minutes = (int)gameTime / 60;
        int seconds = (int)gameTime % 60;
        gameTimeText.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");

        minutes = (int)roundTime / 60;
        seconds = (int)roundTime % 60;
        roundTimeText.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");

        if (Input.GetKey(KeyCode.Tab) || GetState() == GameState.BETWEEN_ROUNDS || androidShowScore == true)
        {
            scorePanel.transform.Translate(0, -500f * Time.deltaTime, 0);
            hideScore.SetActive(true);
            showScore.SetActive(false);
        }
        else
        {
            scorePanel.transform.Translate(0, 500f * Time.deltaTime, 0);
            hideScore.SetActive(false);
            showScore.SetActive(true);
        }

        scorePanelRect.anchoredPosition = new Vector2(0, Mathf.Clamp(scorePanelRect.anchoredPosition.y, -160f, 99f));


        miniScoresNames.text = "";
        miniScoresRounds.text = "";

        if (GetState() != GameState.GAME_OVER)
        {
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                // major scoreboard
                foreach (GameObject go in scoreList)
                {
                    if (go.name != p.NickName)
                        continue;

                    foreach (RectTransform r in go.GetComponentsInChildren<RectTransform>())
                    {
                        if (p.CustomProperties[PlayerProperties.KILLS] == null)
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

                // mini scoreboard
                miniScoresNames.text += p.NickName.Substring(0, Mathf.Min(9, p.NickName.Length)) + '\n';
                miniScoresRounds.text += p.CustomProperties[PlayerProperties.WINS] + "/" + winsForGameOver + '\n';
            }
        }

        if (GetState() == GameState.BETWEEN_ROUNDS)
        {
            timeBetweenRoundsCounter -= Time.deltaTime;

            int time = (int)timeBetweenRoundsCounter;
            if (time < 0)
                time = 0;
            
            roundOverTime.text = "New round starts in " + (time+1) + " seconds.";

            if (PhotonNetwork.isMasterClient && timeBetweenRoundsCounter <= 0)
            {
                int pointIndex = Random.Range(0, 7);
                foreach (PhotonPlayer p in PhotonNetwork.playerList)
                {
                    photonView.RPC("StartNewRound", p, pointIndex % 8);
                    pointIndex++;
                }
            }
        }

        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && chatInput.gameObject.activeInHierarchy)
        {
            SendMsg();
            chatInput.gameObject.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            chatInput.gameObject.SetActive(true);
            chatInput.text = "";
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            chatInput.gameObject.SetActive(false);
            chatInput.text = "";
        }

        if (chatInput.gameObject.activeInHierarchy)
        {
            chatInput.ActivateInputField();
        }
    }

    [PunRPC]
    public void StartNewRound(int spawnpointIndex)
    {
        playingUI.SetActive(true);
        spectatorUI.SetActive(false);
        betweenRoundsUI.SetActive(false);
        terrainManager.StartRound();
        if(PhotonNetwork.isMasterClient)
            SetState(GameState.IN_ROUND);
        
        roundTime = 0f;
        timeBetweenRoundsCounter = timeBetweenRounds;

        Vector3 spawn = GameObject.Find("Spawnpoint" + spawnpointIndex).transform.position;
        PhotonNetwork.Instantiate(playerPrefab.name, spawn, Quaternion.identity, 0);

        Events.Add("Round started.");
    }

    private IEnumerator Wait(float sec)
    {
        yield return new WaitForSeconds(sec);

        if (PhotonNetwork.isMasterClient)
        {
            int pointIndex = Random.Range(0, 7);
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                photonView.RPC("StartNewRound", p, pointIndex % 8);
                pointIndex++;
            }
        }

        fightingMusic.Play();
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
    public void OnPlayerDeath(PhotonPlayer player, Vector3 deathPos, PhotonPlayer killer)
    { // is called for everyone by dying player
        
        if (killer == PhotonNetwork.player)
            PlayerProperties.IncrementProperty(PlayerProperties.KILLS);

        if (killer != null)
            Events.Add("<color=green>" + killer.NickName + "</color>" + " killed " + "<color=green>" + player.NickName + "</color>" + "!");
        else
            Events.Add("<color=green>" + player.NickName + "</color>" + " died");

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

        Instantiate(deathParticles, deathPos, new Quaternion(0,0,0,0));

    }

    [PunRPC]
    public void RoundOver(PhotonPlayer winner)
    {
        roundOverText.text = "<color=green>" + winner.NickName + "</color>" + "\nwon this round!";
        Events.Add("<color=green>" + winner.NickName + "</color>" + " won this round!");

        if (PhotonNetwork.player == winner)
            PlayerProperties.IncrementProperty(PlayerProperties.WINS); 

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (player.GetComponent<PhotonView>().isMine)
            {
                player.GetComponent<PlayerScript>().Die(true);
            }
        }

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
        {
            if (player.GetComponent<PhotonView>().isMine)
            {
                player.GetComponent<PlayerScript>().Die(true);
            }
        }

        Transform dummy = Instantiate(gameOverDummyPlayer, new Vector3(0f, 4.76f, 0f), Quaternion.identity) as Transform;
        dummy.Rotate(new Vector3(0, 180, 0));
        dummy.GetComponentInChildren<Text>().text = lastRoundWinner.NickName;

        Transform soul = Instantiate(soulPrefab);
        soul.GetComponent<SoulScript>().goToPos = dummy.transform.position;

        Camera.main.GetComponent<GameCamera>().isFrozen = true;
        fightingMusic.Stop();
        endMusic.Play();

        betweenRoundsUI.SetActive(false);
        spectatorUI.SetActive(false);
        playingUI.SetActive(false);
        gameOverUI.SetActive(true);

        winner.text = "<color=green>" + lastRoundWinner.NickName + "</color>" + " is the winner!";
        Events.Add("<color=green>" + lastRoundWinner.NickName + "</color>" + " is the winner!");
        AndroidShowScore();
    }

    [PunRPC]
    public void RpcSendText(string nick, string msg)
    {
        if (msg != "")
        {
            Transform parent = GameObject.Find("Message List Parent").transform;
            GameObject go = Instantiate(chatMsgPrefab, parent);
            go.transform.localScale = new Vector3(1f, 1f, 1f);
            go.GetComponentInChildren<Text>().text = string.Format("<color=#FFE798B4>[{0}]</color>  <color=orange>{1}</color>: {2}",
                System.DateTime.Now.ToString("HH:mm:ss"), nick, msg);
            chatTickSound.Play();
        }

        StartCoroutine(ScrollChat());
    }

    private IEnumerator ScrollChat()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        chatScroll.value = 0;
    }

    public void SendMsg()
    {
        if (chatInput.text.Trim() == "")
        {
            chatInput.ActivateInputField();
            return;
        }

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("RpcSendText", PhotonTargets.All, PhotonNetwork.player.NickName, chatInput.text);
        chatInput.text = "";
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

    private GameState cachedGameState = GameState.WARMUP;
    public GameState GetState()
    {
        if (PhotonNetwork.isMasterClient)
        {
            return gameState;
        }
        else
        {
            GameState gs = (GameState)PhotonNetwork.room.CustomProperties[GAME_STATE];

            if (gs != null)
                cachedGameState = gs;

            return cachedGameState;
        }
    }

    public static float GetGameTime()
    {
        return gameTime;
    }

    public static float GetRoundTime()
    {
        return roundTime;
    }

    public float GetDragScalar()
    {
        // 1 at 0 sec
        // 0.1 at 2 minutes
        float timeRatio = roundTime / 180f;
        timeRatio = Mathf.Clamp(timeRatio, 0f, .9f);
        return 1 - timeRatio;
    }

    public bool IsChatOpen()
    {
        return chatInput.gameObject.activeInHierarchy;
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        if (PhotonNetwork.playerList.Length == 1 && GetState() != GameState.GAME_OVER)
        {
            infoPanel.SetActive(true);
            information.text = "Everyone left. Exiting to main menu...";
            StartCoroutine(DisconnectDelay());
        }
        Events.Add("<color=green>" + other.NickName + "</color>" + " has left.");
    }

    private IEnumerator DisconnectDelay()
    {
        yield return new WaitForSeconds(3f);
        Disconnect();
    }

    public void Disconnect()
    {
        Events.Add("Disconnecting.");
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Menu");
    }

    public void AndroidShowScore()
    {
        androidShowScore = true;
    }

    public void AndroidHideScore()
    {
        androidShowScore = false;
    }

    void OnGUI()
    {
        GUI.color = Color.white;

        if (PhotonNetwork.GetPing() >= 200)
            GUI.color = Color.red;
        else if (PhotonNetwork.GetPing() >= 100)
            GUI.color = Color.yellow;
        else
            GUI.color = Color.white;

        GUI.Label(new Rect(3, 0, 200, 20), PhotonNetwork.connectionState.ToString() + " - " + PhotonNetwork.GetPing() + " ms");
    }
}
