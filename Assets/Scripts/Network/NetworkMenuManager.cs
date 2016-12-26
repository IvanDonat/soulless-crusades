using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NetworkMenuManager : Photon.PunBehaviour {

    public string gameVersion = "";
    public bool autoJoinLobby = true;
    public bool autoSyncScene = true;
    public float roomRefreshInterval = 0f;
    private float roomRefreshTimer;

    public Text labelVersion, labelError, labelStatus, labelPlayerInt, labelRoomName,
                labelPlayerNumber, maxPlayers;
    public InputField roomInputField, chatInput;
    public Toggle privateToggle, readyToggle;
    public Slider playerNumberSlider;
    public Button kickPlayer, startGame;
    public Scrollbar chatScroll;
    public GameObject loadingPanel, errorPanel, selectedRoomPrefab, listedPlayerPrefab, chatMsgPrefab, infoPanel;

    private Dictionary<PhotonPlayer, Toggle> readyCheckmarks = new Dictionary<PhotonPlayer, Toggle>();

    //Default room options
    private string roomName = "";
    private byte numberOfPlayers = 2;

    //Private lobby vars
    private PhotonPlayer selectedPlayer;

    private const string strConnecting = "CONNECTING TO SERVER...";
    private const string strConnected = "CONNECTED!";
    private const string strFailedToConnect = "SOULLESS CRUSADES FAILED TO ESTABLISH A CONNECTION TO SERVER!";
    private const string strDisconnected = "SOULLESS CRUSADES DISCONNECTED FROM SERVER!";

    //Info msgs
    private const string strKicked = "You have been kicked from this session!";

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = autoJoinLobby;
        PhotonNetwork.automaticallySyncScene = autoSyncScene;
        PhotonNetwork.sendRate = 40;
        PhotonNetwork.sendRateOnSerialize = 40;

        PhotonNetwork.networkingPeer.DebugOut = ExitGames.Client.Photon.DebugLevel.WARNING;
        PhotonNetwork.logLevel = PhotonLogLevel.ErrorsOnly;
    }

    void Start()
    {
        roomRefreshTimer = roomRefreshInterval;
        Connect();
    }

    void Update()
    {
        roomRefreshTimer -= Time.deltaTime;
        if (roomRefreshTimer <= 0f)
        {
            Transform parent = GameObject.Find("Room List Parent").transform;

            foreach (RectTransform t in parent.GetComponentsInChildren<RectTransform>())
            {
                if (t.transform != parent.transform)
                    Destroy(t.gameObject);
            }

            foreach (RoomInfo ri in PhotonNetwork.GetRoomList())
            {
                GameObject go = Instantiate(selectedRoomPrefab, parent) as GameObject;
                go.name = "RoomListItem " + ri.Name;

                go.transform.FindChild("RoomNameText").GetComponent<Text>().text = string.Format(ri.Name);
                go.transform.FindChild("RoomPlayersText").GetComponent<Text>().text = string.Format(ri.PlayerCount + "/" + ri.MaxPlayers);

                go.GetComponent<Button>().onClick.AddListener(() => { JoinRoom(); });
            }

            roomRefreshTimer = roomRefreshInterval;
        }

        if (PhotonNetwork.inRoom && Input.GetKey(KeyCode.Return))
        {
            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("RpcSendText", PhotonTargets.All, PhotonNetwork.player.NickName, chatInput.text);
            chatInput.text = "";
            chatInput.ActivateInputField();
        }

        if (PhotonNetwork.inRoom)
        {
            int readyCount = 0;
            bool allReady = true;
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                bool isReady = false; // treba biti ovako jer property može bit null
                if(p.CustomProperties["ready"] != null)
                    isReady = (bool) p.CustomProperties["ready"];

                if (isReady)
                    readyCount++;
                else
                    allReady = false;

                readyCheckmarks[p].isOn = isReady;
            }

            if (allReady && readyCount >= 1)
                startGame.interactable = true;
            else
                startGame.interactable = false;
        }
    }

    public void Connect()
    {
        loadingPanel.SetActive(true);
        errorPanel.SetActive(false);

        if (PhotonNetwork.connected)
        {
            Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
            labelStatus.text = strConnected;
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings(gameVersion);
            labelStatus.text = strConnecting;
        }
    }

    public void Okay()
    {
        infoPanel.SetActive(false);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void KickedFromRoom()
    {
        PhotonNetwork.LeaveRoom();
        infoPanel.SetActive(true);
        infoPanel.GetComponentInChildren<Text>().text = strKicked;
    }

    public void OnSliderChangeValue(Slider slider)
    {
        labelPlayerInt.text = slider.value.ToString();
        numberOfPlayers = (byte)slider.value;
    }

    public void OnRoomNameChangeValue(InputField inputField)
    {
        roomName = inputField.text;
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = numberOfPlayers, IsVisible = !privateToggle.isOn }, null);
        playerNumberSlider.value = 2;
        privateToggle.isOn = false;
        roomInputField.text = "";
    }

    public void JoinRoom()
    {
        string roomName = EventSystem.current.currentSelectedGameObject.name.Substring("RoomListItem ".Length);
        PhotonNetwork.JoinRoom(roomName);
    }

    public void SelectPlayer()
    {
        kickPlayer.interactable = true;
        selectedPlayer = EventSystem.current.currentSelectedGameObject.transform.parent.GetComponent<PhotonPlayerContainer>().Get();
    }

    public void StartGame()
    {
        PhotonNetwork.room.IsVisible = false;
        PhotonNetwork.LoadLevel(1);
    }

    [PunRPC]
    public void RpcSendText(string nick, string msg)
    {
        if (msg != "")
        {
            Transform parent = GameObject.Find("Message List Parent").transform;
            GameObject go = Instantiate(chatMsgPrefab, parent);
            go.GetComponentInChildren<Text>().text = string.Format("{0}: {1}", nick, msg);
        }

        chatScroll.value = 0;
    }

    public void KickPlayer()
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("RpcKick", selectedPlayer);
    }

    [PunRPC]
    private void RpcKick()
    {
        KickedFromRoom();
    }

    public void OnReadyChangedValue(Toggle ready)
    {
        SetReady(ready.isOn);
    }

    private void SetReady(bool ready)
    {
        var props = new ExitGames.Client.Photon.Hashtable();
        props.Add("ready", ready);
        PhotonNetwork.player.SetCustomProperties(props, null);
    }

    public override void OnJoinedRoom()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToLobby();
        labelRoomName.text = PhotonNetwork.room.Name;
        maxPlayers.text = "Max Players: " + PhotonNetwork.room.MaxPlayers;
        labelPlayerNumber.text = "Current player number: " + PhotonNetwork.room.PlayerCount;
        Transform parent = GameObject.Find("Player List Parent").transform;
        if (PhotonNetwork.isMasterClient)
        {
            startGame.gameObject.SetActive(true);
            AddPlayerListItem(PhotonNetwork.player, parent);
            kickPlayer.onClick.AddListener(() => { KickPlayer(); });
        }
        else
        {
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
                AddPlayerListItem(p, parent);
        }

        SetReady(false);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        labelPlayerNumber.text = "Current player number: " + PhotonNetwork.room.PlayerCount;
        Transform parent = GameObject.Find("Player List Parent").transform;
        AddPlayerListItem(other, parent);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        labelPlayerNumber.text = "Current player number: " + PhotonNetwork.room.PlayerCount;
        Transform parent = GameObject.Find("Player List Parent").transform;

        foreach (RectTransform t in parent.GetComponentInChildren<RectTransform> ())
        {
            if (t.gameObject.GetComponent<PhotonPlayerContainer>().Get().NickName == other.NickName)
                Destroy(t.gameObject);
        }

        labelPlayerNumber.text = "Current player number: " + PhotonNetwork.room.PlayerCount;
    }

    private void AddPlayerListItem(PhotonPlayer player, Transform parent)
    {
        GameObject go = Instantiate(listedPlayerPrefab, parent) as GameObject;
        go.name = "PlayerListItem " + player.NickName;
        go.GetComponentInChildren<Text>().text = player.NickName;
        if(player.IsMasterClient)
            go.GetComponentInChildren<Text>().color = Color.Lerp(go.GetComponentInChildren<Text>().color, Color.red, 0.3f);
        go.GetComponent<PhotonPlayerContainer>().Set(player);

        if (PhotonNetwork.isMasterClient)
        {
            go.GetComponentInChildren<Button>().interactable = true;
            go.GetComponentInChildren<Button>().onClick.AddListener(() => { SelectPlayer(); });
        }

        readyCheckmarks[player] = go.GetComponentInChildren<Toggle>();
    }

    public override void OnLeftRoom()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();

        Transform parent = GameObject.Find("Player List Parent").transform;

        foreach (RectTransform t in parent.GetComponentInChildren<RectTransform>())
        {
            if (t.transform != parent.transform)
                Destroy(t.gameObject);
        }

        Transform parentChat = GameObject.Find("Message List Parent").transform;

        foreach (RectTransform t in parentChat.GetComponentInChildren<RectTransform>())
        {
            if (t.transform != parentChat.transform)
                Destroy(t.gameObject);
        }

        readyToggle.isOn = false;
    }

    public override void OnConnectedToMaster()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
        labelVersion.text = "Development build v" + gameVersion;
        labelStatus.text = strConnected;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        // refresh now
        roomRefreshTimer = -1f;
    }

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        labelError.text = strFailedToConnect;
        loadingPanel.SetActive(false);
        errorPanel.SetActive(true);
    }

    public override void OnDisconnectedFromPhoton()
    {
        labelError.text = strDisconnected;
        loadingPanel.SetActive(false);
        errorPanel.SetActive(true);
        Camera.main.GetComponent<MenuCamera>().TransitionToLoadingGame();
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        LeaveRoom();
    }

    void OnGUI()
    {
        if(PhotonNetwork.connected)
            GUILayout.Label(PhotonNetwork.connectionState + "\n" + PhotonNetwork.GetPing() + " ms");
    }
}
