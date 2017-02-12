using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkMenuManager : Photon.PunBehaviour 
{
    [Header("Game Values")]
    public string gameVersion = "";
    public bool autoJoinLobby = true;
    public bool autoSyncScene = true;
    public float roomRefreshInterval = 0f;

    [Header("Main Menu Canvas")]
    public Button buttonAccountSettings;
    public GameObject prefabRoomItem;
    public GameObject panelMenuInfo;
    public GameObject panelJoinPrivateRoom;
    public InputField inputFieldPrivateRoom;
    public Text labelVersion;
    public Text labelUser;
    public Text labelTotalPlayers;
    public Text labelPrivateRoomInfo;

    [Header("Options Canvas")]
    public Dropdown dropdownResolution;
    public Dropdown dropdownQuality;
    public Slider sliderGlobalVolume;
    public Slider sliderMusicVolume;
    public Toggle toggleWindowed;

    [Header("Lobby Canvas")]
    public Button buttonKickPlayer;
    public Button buttonStartGame;
    public GameObject prefabChatMsg;
    public GameObject prefabLobbyPlayer;
    public GameObject panelSelectSpells;
    public InputField inputFieldChat;
    public Scrollbar scrollChat;
    public Text labelRoomName;
    public Text labelPlayerCount;
    public Text labelMaxCount;
    public Text labelRoundsToWin;
    public Toggle toggleReady;

    [Header("Create Room Canvas")]
    public InputField inputFieldRoom;
    public Slider sliderRoundsToWin;
    public Text labelPlayerCountSet;
    public Text labelRoundsToWinSet;
    public Toggle togglePrivate;

    [Header("Login Canvas")]
    public Button buttonGoLogin;
    public Button buttonGoReg;
    public GameObject panelLoading;
    public GameObject panelAuthError;
    public GameObject panelLogin;
    public GameObject panelRegister;
    public GameObject panelRecovery;
    public InputField inputFieldUsername;
    public InputField inputFieldPw;
    public InputField inputFieldEmailReg;
    public InputField inputFieldUsernameReg;
    public InputField inputFieldPwReg;
    public InputField inputFieldRecoveryEmail;
    public Text labelLoginError;
    public Text labelAuthStatus;
    public Text labelRegStatus;
    public Text labelRecoveryInfo;

    [Header("Misc")]
    public AudioSource chatTickSound;

    //Private Variables

    //Main Menu Canvas
    private float roomRefreshTimer;

    //Options Canvas
    private bool loadedResolutions = false;
    private bool loadedQuality = false;

    //Lobby Canvas
    private Dictionary<PhotonPlayer, Toggle> readyCheckmarks = new Dictionary<PhotonPlayer, Toggle>();

    //Login Canvas
    private CloudRegionCode selectedRegion;
    private bool isAuthError = false; //photon treats auth failiure like a dc

    //Default room options
    private string roomName = "";
    private byte numberOfPlayers = 4;

    //Private lobby vars
    private PhotonPlayer selectedPlayer;
    private bool isCurrentRoomCompetitive = false;

    //Auth info msgs
    private const string strFailedToConnect = "SOULLESS CRUSADES FAILED TO ESTABLISH A CONNECTION TO SERVER!";
    private const string strDisconnected = "SOULLESS CRUSADES DISCONNECTED FROM SERVER!";

    //Info msgs
    private const string strKicked = "You have been kicked from this session!";
    private const string strHostLeft = "The host has abandoned the session.";

    //Competitive constants
    private const string COMP_PREFIX = "competitive_room";
    private const int COMP_MAXPLAYERS = 4;
    private const int COMP_MAXWINS = 3;

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
        selectedRegion = CloudRegionCode.eu;
        roomRefreshTimer = roomRefreshInterval;

        int i = 0, savedResolution = -1;
        Resolution curr = Screen.currentResolution;
        foreach (Resolution res in Screen.resolutions) //for loop will slow this down much more than adding i
        {
            dropdownResolution.options.Add(new Dropdown.OptionData() {
                text = res.width + " x " + res.height + "       " + res.refreshRate + "Hz"
            });

            if (curr.width == res.width && curr.height == res.height && curr.refreshRate == res.refreshRate)
                savedResolution = i;
            i++;
        }
        if (savedResolution >= 0)
            dropdownResolution.value = savedResolution;
        loadedResolutions = true;

        int r = 0, savedQuality = -1;
        string currentQuality = QualitySettings.names[QualitySettings.GetQualityLevel()];
        foreach(string s in QualitySettings.names)
        {
            dropdownQuality.options.Add(new Dropdown.OptionData() {
                text = s
            });

            if (currentQuality == s)
                savedQuality = r;
            r++;
        }
        if (savedQuality>= 0)
            dropdownQuality.value = savedQuality;
        loadedQuality = true;

        toggleWindowed.isOn = Screen.fullScreen;
        if (PlayerPrefs.HasKey("GlobalVolume"))
            sliderGlobalVolume.value = PlayerPrefs.GetFloat("GlobalVolume");
        if (PlayerPrefs.HasKey("MusicVolume"))
            sliderMusicVolume.value = PlayerPrefs.GetFloat("MusicVolume");

        inputFieldPw.onEndEdit.AddListener(delegate{if(Input.GetKey(KeyCode.Return)) Connect();});
        inputFieldUsername.onEndEdit.AddListener(delegate{if(Input.GetKey(KeyCode.Return)) Connect();});

        inputFieldPwReg.onEndEdit.AddListener(delegate{if(Input.GetKey(KeyCode.Return)) Register();});
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
                if (ri.Name.StartsWith(COMP_PREFIX))
                    continue;

                GameObject go = Instantiate(prefabRoomItem, parent) as GameObject;
                go.name = "RoomListItem " + ri.Name;

                go.transform.FindChild("RoomNameText").GetComponent<Text>().text = string.Format(ri.Name);
                go.transform.FindChild("RoomPlayersText").GetComponent<Text>().text = string.Format(ri.PlayerCount + "/" + ri.MaxPlayers);

                go.GetComponent<Button>().onClick.AddListener(() => { JoinRoom(); });
            }

            roomRefreshTimer = roomRefreshInterval;
        }

        // ^^
        if (PhotonNetwork.countOfPlayers > 2)
            labelTotalPlayers.text = "Total Players: " + PhotonNetwork.countOfPlayers;
        else
            labelTotalPlayers.text = "";

        if (PhotonNetwork.inRoom && (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter)))
            SendMsg();

        if (PhotonNetwork.inRoom)
        {
            int readyCount = 0;
            bool allReady = true;
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
            {
                bool isReady = false; // treba biti ovako jer property može bit null
                if (p.CustomProperties["ready"] != null)
                    isReady = (bool)p.CustomProperties["ready"];

                if (isReady)
                    readyCount++;
                else
                    allReady = false;

                readyCheckmarks[p].isOn = isReady;
            }

            if (allReady && readyCount >= 2)
                buttonStartGame.interactable = true;
            else
                buttonStartGame.interactable = false;

            if (isCurrentRoomCompetitive)
            {
                if (allReady && readyCount == COMP_MAXPLAYERS)
                {
                    StartGame();
                }
            }
        }

        Screen.fullScreen = toggleWindowed.isOn;
    }

    public void Connect()
    {
        if (PhotonNetwork.connected)
        {
            panelLoading.SetActive(true);
            panelAuthError.SetActive(false);

            Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
        }
        else
        {
            if (inputFieldUsername.text.Length == 0 || inputFieldPw.text.Length == 0)
            {
                labelAuthStatus.text = "Username and password cannot be empty!";
                return;
            }

            panelLoading.SetActive(true);
            panelAuthError.SetActive(false);

            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
            PhotonNetwork.AuthValues.AddAuthParameter("username", inputFieldUsername.text);
            PhotonNetwork.AuthValues.AddAuthParameter("password", inputFieldPw.text);
            PhotonNetwork.ConnectToRegion(selectedRegion, gameVersion);
        }
    }

    public void ConnectAsGuest()
    {
        panelLoading.SetActive(true);
        panelAuthError.SetActive(false);
        //acSettings.interactable = false;
        //acSettings.GetComponentInChildren<Text>().color = new Color32(136, 125, 89, 255);
        inputFieldUsername.text = "Guest" + UnityEngine.Random.Range(1000, 9999);

        PhotonNetwork.AuthValues = new AuthenticationValues();
        PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.None;
        PhotonNetwork.ConnectToRegion(selectedRegion, gameVersion);
    }

    public void SelectRegion(Dropdown target)
    {
        if (target.value == 0)
        {
            selectedRegion = CloudRegionCode.eu;
        }
        else if (target.value == 1)
        {
            selectedRegion = CloudRegionCode.us;
        }
        else if (target.value == 2)
        {
            selectedRegion = CloudRegionCode.asia;
        }
        else
        {
            selectedRegion = CloudRegionCode.sa;
        }
    }

    public void SelectResolution(Dropdown target)
    {
        if (!loadedResolutions)
            return;
        Resolution res = Screen.resolutions[target.value];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void SelectQuality(Dropdown target)
    {
        if (!loadedQuality)
            return;
        QualitySettings.SetQualityLevel(target.value);
    }

    //operations - "add", "substract", "get"
    //if using operation get put null for ammount
    public static IEnumerator ModifyElo(string op, string amount)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", PhotonNetwork.player.NickName);
        form.AddField("operation", op);
        if (amount != null)
            form.AddField("amount", amount);
        WWW w = new WWW("https://soullesscrusades.000webhostapp.com/elo.php", form);
        yield return w;
        //Debug.Log(w.error);
        //Debug.Log(w.text);
        if (w.text == "updated")
        {
            Debug.Log("Elo update success");
        }
        else if (w.text == "-1")
        {
            Debug.Log("Elo update failed");
        }
        else
        {
            Debug.Log("Current elo: " + w.text);
        }
    }

    public void Register()
    {
        if (!inputFieldEmailReg.text.Contains("@") ||
           !inputFieldEmailReg.text.Contains("."))
        {
            labelRegStatus.text = "Invalid email address.";
            return;
        }

        if (inputFieldUsernameReg.text.ToLower().StartsWith("guest"))
        {
            labelRegStatus.text = "Username can't start with Guest";
            return;
        }

        if (inputFieldUsernameReg.text.Length < 3)
        {
            labelRegStatus.text = "Username too short";
            return;
        }

        labelRegStatus.text = ""; //user sees a refresh every time he tries...
        panelLoading.SetActive(true);
        StartCoroutine(Reg());
    }

    private IEnumerator Reg()
    {
        WWWForm form = new WWWForm();
        form.AddField("email", inputFieldEmailReg.text);
        form.AddField("username", inputFieldUsernameReg.text);
        form.AddField("password", inputFieldPwReg.text);
        WWW w = new WWW("https://soullesscrusades.000webhostapp.com/register.php", form);
        yield return w;
        //Debug.Log(w.error);
        //Debug.Log(w.text);
        if (w.text == "1")
        {
            labelAuthStatus.text = "Before logging in you must confirm your email address.";
            inputFieldEmailReg.text = "";
            inputFieldUsernameReg.text = "";
            inputFieldPwReg.text = "";
            GoToLogin();
        }
        else
            labelRegStatus.text = "Email or username already in use. Please modify your input.";
        panelLoading.SetActive(false);
    }

    public void GoToLogin()
    {
        panelLogin.SetActive(true);
        buttonGoLogin.interactable = false;
        panelRegister.SetActive(false);
        buttonGoReg.interactable = true;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void GoToRegister()
    {
        panelLogin.SetActive(false);
        buttonGoLogin.interactable = true;
        panelRegister.SetActive(true);
        buttonGoReg.interactable = false;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void Okay()
    {
        panelMenuInfo.SetActive(false);
    }

    public void SendMsg()
    {
        if (inputFieldChat.text.Trim() == "")
        {
            inputFieldChat.ActivateInputField();
            return;
        }

        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("RpcSendText", PhotonTargets.All, PhotonNetwork.player.NickName, inputFieldChat.text);
        inputFieldChat.text = "";
        inputFieldChat.ActivateInputField();
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
        panelMenuInfo.SetActive(true);
        panelMenuInfo.GetComponentInChildren<Text>().text = strKicked;
    }

    public void OnGlobalVolumeChangeValue(Slider slider)
    {
        AudioListener.volume = slider.value;
        PlayerPrefs.SetFloat("GlobalVolume", slider.value);
    }

    public void OnMusicVolumeChangeValue(Slider slider)
    {
        MusicScript.volume = slider.value;
        PlayerPrefs.SetFloat("MusicVolume", slider.value);
    }

    public void OnMaxPlayersSliderChangeValue(Slider slider)
    {
        labelPlayerCountSet.text = slider.value.ToString();
        numberOfPlayers = (byte)slider.value;
    }

    public void OnRoundsToWinSliderChangeValue(Slider slider)
    {
        labelRoundsToWinSet.text = slider.value.ToString();
    }

    public void OnRoomNameChangeValue(InputField inputField)
    {
        roomName = inputField.text;
    }
        
    public void OnCompetitiveClicked()
    {
        List<RoomInfo> compRooms = new List<RoomInfo>();
        foreach (RoomInfo room in PhotonNetwork.GetRoomList())
        {
            if (room.Name.StartsWith(COMP_PREFIX))
            {
                compRooms.Add(room);
            }
        }

        if (compRooms.Count > 0)
        {
            // @TODO rooms should contain rank group in name once ranking is done
            PhotonNetwork.JoinRoom(compRooms[0].Name);
        }
        else
        {
            // @TODO rooms should contain rank group in name once ranking is done
            string name = COMP_PREFIX + UnityEngine.Random.Range(1, 10000);

            PhotonNetwork.CreateRoom(name, new RoomOptions { MaxPlayers = COMP_MAXPLAYERS }, null);
        }

        isCurrentRoomCompetitive = true;
    }

    public void OnTransitionToCreateRoom()
    {
        inputFieldRoom.text = "Room" + UnityEngine.Random.Range(1000, 9999);
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = numberOfPlayers, IsVisible = !togglePrivate.isOn }, null);
        togglePrivate.isOn = false;
        isCurrentRoomCompetitive = false;
    }

    public void JoinRoom()
    {
        string roomName = EventSystem.current.currentSelectedGameObject.name.Substring("RoomListItem ".Length);
        PhotonNetwork.JoinRoom(roomName);
        isCurrentRoomCompetitive = false;
    }

    public void JoinPrivateRoom()
    {
        PhotonNetwork.JoinRoom(inputFieldPrivateRoom.text);
        isCurrentRoomCompetitive = false;
    }

    public void ShowPrivateRoomJoin()
    {
        panelJoinPrivateRoom.SetActive(true);
    }

    public void ClosePrivateRoomJoin()
    {
        panelJoinPrivateRoom.SetActive(false);
        inputFieldPrivateRoom.text = "";
        labelPrivateRoomInfo.text = "";
    }

    public void OnModifiedPrivateRoomField(InputField inputField)
    {
        labelPrivateRoomInfo.text = "";
    }

    public void SelectPlayer()
    {
        buttonKickPlayer.interactable = true;
        selectedPlayer = EventSystem.current.currentSelectedGameObject.transform.parent.GetComponent<PhotonPlayerContainer>().Get();
    }

    public void OpenSpellList()
    {
        panelSelectSpells.SetActive(true);
    }

    public void CloseSpellList()
    {
        panelSelectSpells.SetActive(false);
    }

    public void StartGame()
    {
        PhotonNetwork.room.IsVisible = false;
        //sadly can't use PhotonNetwork.LoadLevel and auto sync scenes due to
        //it breaking more important things...
        photonView.RPC("RpcStartGame", PhotonTargets.All); 
    }

    [PunRPC]
    public void RpcStartGame()
    {
        SceneManager.LoadScene("Game");
    }

    [PunRPC]
    public void RpcSendText(string nick, string msg)
    {
        if (msg != "")
        {
            Transform parent = GameObject.Find("Message List Parent").transform;
            GameObject go = Instantiate(prefabChatMsg, parent);
            go.GetComponentInChildren<Text>().text = string.Format("<color=#FFE798B4>[{0}]</color>  <color=orange>{1}</color>: {2}", 
                DateTime.Now.ToString("HH:mm:ss"), nick, msg);
            chatTickSound.Play();
        }

        StartCoroutine(ScrollChat());
    }

    private IEnumerator ScrollChat()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        scrollChat.value = 0;
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

    public void GoToOptions()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToOptions();
    }

    public void GoToCredits()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToCredits();
    }

    public void GoToTutorial()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToTutorial();
    }

    public void OpenRecovery()
    {
        panelRecovery.SetActive(true);
        inputFieldRecoveryEmail.ActivateInputField();
    }

    public void CloseRecovery()
    {
        panelRecovery.SetActive(false);
        inputFieldRecoveryEmail.text = "";
        labelRecoveryInfo.text = "Enter the email address associated with your account. We will send you a password reset link shortly.";
    }

    public void SendRecoveryMail()
    {
        StartCoroutine(SendRecovery(inputFieldRecoveryEmail.text));
        inputFieldRecoveryEmail.text = "";
        inputFieldRecoveryEmail.ActivateInputField();
    }

    private IEnumerator SendRecovery(string mail)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", mail);
        WWW w = new WWW("https://soullesscrusades.000webhostapp.com/lost_info.php", form);
        yield return w;
        //Debug.Log(w.error);
        //Debug.Log(w.text);
        if (w.text == "1")
        {
            labelRecoveryInfo.text = "Further instructions have been sent to your email address.";
        }
        else
        {
            labelRecoveryInfo.text = "There is no account associated with that email address.";
        }
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        isAuthError = true;
        panelLoading.SetActive(false);
        panelAuthError.SetActive(false);
        labelAuthStatus.text = debugMessage;
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        if (panelJoinPrivateRoom.activeInHierarchy)
        {
            labelPrivateRoomInfo.text = "Room is full or doesn't exist!";
        }
    }

    public override void OnJoinedRoom()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToLobby();
        labelMaxCount.text = "Max Players: " + PhotonNetwork.room.MaxPlayers;
        labelPlayerCount.text = "Player count: " + PhotonNetwork.room.PlayerCount;
        Transform parent = GameObject.Find("Player List Parent").transform;

        if (isCurrentRoomCompetitive)
            labelRoomName.text = "Quickplay";
        else 
            labelRoomName.text = "Room: " + PhotonNetwork.room.Name;

        if (panelJoinPrivateRoom.activeInHierarchy)
        {
            inputFieldPrivateRoom.text = "";
            ClosePrivateRoomJoin();
        }

        if (PhotonNetwork.isMasterClient)
        {
            buttonStartGame.gameObject.SetActive(true);
            AddPlayerListItem(PhotonNetwork.player, parent);
            buttonKickPlayer.onClick.AddListener(() => { KickPlayer(); });

            var props = new ExitGames.Client.Photon.Hashtable();
            props.Add("maxwins", (int) sliderRoundsToWin.value);
            PhotonNetwork.room.SetCustomProperties(props, null);
            labelRoundsToWin.text = "Rounds to Win: " + (int) sliderRoundsToWin.value;
        }
        else
        {
            labelRoundsToWin.text = "Rounds to Win: " + PhotonNetwork.room.CustomProperties["maxwins"].ToString();

            buttonStartGame.gameObject.SetActive(false);
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
                AddPlayerListItem(p, parent);
        }

        // comp override
        if (isCurrentRoomCompetitive && PhotonNetwork.isMasterClient)
        {
            buttonStartGame.gameObject.SetActive(false);

            var props = new ExitGames.Client.Photon.Hashtable();
            props.Add("maxwins", COMP_MAXWINS);
            PhotonNetwork.room.SetCustomProperties(props, null);
            labelRoundsToWin.text = "Rounds to Win: " + COMP_MAXWINS;
        }

        SetReady(false);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        labelPlayerCount.text = "Player count: " + PhotonNetwork.room.PlayerCount;
        Transform parent = GameObject.Find("Player List Parent").transform;
        AddPlayerListItem(other, parent);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        Transform parent = GameObject.Find("Player List Parent").transform;

        foreach (RectTransform t in parent.GetComponentInChildren<RectTransform> ())
        {
            if (t.gameObject.GetComponent<PhotonPlayerContainer>().Get().ID == other.ID)
                Destroy(t.gameObject);
        }

        labelPlayerCount.text = "Player count: " + PhotonNetwork.room.PlayerCount;
    }

    private void AddPlayerListItem(PhotonPlayer player, Transform parent)
    {
        GameObject go = Instantiate(prefabLobbyPlayer, parent) as GameObject;
        go.name = "PlayerListItem " + player.NickName;
        go.GetComponentInChildren<Text>().text = player.NickName;
        if(player.IsMasterClient && !isCurrentRoomCompetitive)
            go.GetComponentInChildren<Text>().color = Color.Lerp(go.GetComponentInChildren<Text>().color, Color.red, 0.3f);
        go.GetComponent<PhotonPlayerContainer>().Set(player);

        if (PhotonNetwork.isMasterClient && !isCurrentRoomCompetitive)
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

        GetComponent<SpellSelectScript>().DeselectAll();

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

        panelSelectSpells.SetActive(true);
        toggleReady.isOn = false;
    }

    public override void OnConnectedToMaster()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
        labelVersion.text = "beta v" + gameVersion;
        labelUser.text = "Welcome " + PhotonNetwork.player.NickName;
        PhotonNetwork.JoinLobby();
        inputFieldPw.text = "";
    }

    public override void OnJoinedLobby()
    {
        // refresh now
        roomRefreshTimer = -1f;
    }

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        labelLoginError.text = strFailedToConnect;
        panelLoading.SetActive(false);
        panelAuthError.SetActive(true);
    }

    public override void OnDisconnectedFromPhoton()
    {
        if (!isAuthError)
        {
            labelLoginError.text = strDisconnected;
            panelLoading.SetActive(false);
            panelAuthError.SetActive(true);
            Camera.main.GetComponent<MenuCamera>().TransitionToLogin();
        }
        isAuthError = false;
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        if (!isCurrentRoomCompetitive)
        {
            LeaveRoom();
            panelMenuInfo.SetActive(true);
            panelMenuInfo.GetComponentInChildren<Text>().text = strHostLeft;
        }
    }

    public void SocialDiscord()
    {
        Application.OpenURL("https://discord.gg/DZ2UkbC");
    }

    void OnGUI()
    {
        GUI.color = Color.white;
        GUI.Label(new Rect(3, 0, 100, 20), PhotonNetwork.connectionState.ToString());

        if (PhotonNetwork.GetPing() >= 200)
            GUI.color = Color.red;
        else if (PhotonNetwork.GetPing() >= 100)
            GUI.color = Color.yellow;
        else
            GUI.color = Color.white;

        GUI.Label(new Rect(3, 15, 100, 20), PhotonNetwork.GetPing() + " ms");

        GUI.color = Color.white;
        GUI.Label(new Rect(3, Screen.height - 18, 250, 20), "©2017 IDP, RM, KB");
    }
}
