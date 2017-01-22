﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkMenuManager : Photon.PunBehaviour 
{
    public string gameVersion = "";
    public bool autoJoinLobby = true;
    public bool autoSyncScene = true;
    public float roomRefreshInterval = 0f;
    private float roomRefreshTimer;

    public Text labelVersion, labelError, labelPlayerInt, labelRoomName,
                labelPlayerNumber, maxPlayers, labelRoundsToWin, labelRoundsToWinInt, labelAuthStatus, 
                labelRegStatus, labelUser, labelPrivateRoomInfo, labelTotalPlayers;
    public InputField roomInputField, chatInput, usernameInput, pwInput, emailRegInput, usernameRegInput, pwRegInput,
                        privateRoomField;
    public Toggle privateToggle, readyToggle;
    public Slider playerNumberSlider, roundsToWinSlider;
    public Button kickPlayer, startGame, goToLogin, goToRegister, goToVideo, goToSound, goToControls;
    public Scrollbar chatScroll;
    public GameObject loadingPanel, errorPanel, selectedRoomPrefab, listedPlayerPrefab, chatMsgPrefab, infoPanel,
                        selectSpellsPanel, loginPanel, registerPanel, joinPrivateRoomPanel, videoPanel, soundPanel,
                        controlsPanel;

    private Dictionary<PhotonPlayer, Toggle> readyCheckmarks = new Dictionary<PhotonPlayer, Toggle>();

    private CloudRegionCode selectedRegion;
    private bool isAuthError = false; //photon treats auth failiure like a dc

    //Default room options
    private string roomName = "";
    private byte numberOfPlayers = 2;

    //Private lobby vars
    private PhotonPlayer selectedPlayer;
    private bool isCurrentRoomCompetitive = false;

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

        pwInput.onEndEdit.AddListener(delegate{if(Input.GetKey(KeyCode.Return)) Connect();});
        usernameInput.onEndEdit.AddListener(delegate{if(Input.GetKey(KeyCode.Return)) Connect();});

        pwRegInput.onEndEdit.AddListener(delegate{if(Input.GetKey(KeyCode.Return)) Register();});
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

                GameObject go = Instantiate(selectedRoomPrefab, parent) as GameObject;
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

        if (PhotonNetwork.inRoom && Input.GetKey(KeyCode.Return))
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
                startGame.interactable = true;
            else
                startGame.interactable = false;

            if (isCurrentRoomCompetitive)
            {
                if (allReady && readyCount == COMP_MAXPLAYERS)
                {
                    StartGame();
                }
            }
        }
    }

    public void Connect()
    {
        if (PhotonNetwork.connected)
        {
            loadingPanel.SetActive(true);
            errorPanel.SetActive(false);

            Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
        }
        else
        {
            if (usernameInput.text.Length == 0 || pwInput.text.Length == 0)
            {
                labelAuthStatus.text = "Username and password cannot be empty!";
                return;
            }

            loadingPanel.SetActive(true);
            errorPanel.SetActive(false);

            PhotonNetwork.AuthValues = new AuthenticationValues();
            PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
            PhotonNetwork.AuthValues.AddAuthParameter("username", usernameInput.text);
            PhotonNetwork.AuthValues.AddAuthParameter("password", pwInput.text);
            PhotonNetwork.ConnectToRegion(selectedRegion, gameVersion);
        }
    }

    public void ConnectAsGuest()
    {
        Debug.LogError("UNFINISHED ConnectAsGuest\nAdd AuthParameter to Photon Server and webserver bool isguest to allow guest logins");

        loadingPanel.SetActive(true);
        errorPanel.SetActive(false);

        PhotonNetwork.AuthValues = new AuthenticationValues();
        PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Custom;
        PhotonNetwork.AuthValues.AddAuthParameter("username", "Guest" + UnityEngine.Random.Range(1000, 9999));
        PhotonNetwork.AuthValues.AddAuthParameter("password", "");
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
        if (!emailRegInput.text.Contains("@") ||
           !emailRegInput.text.Contains("."))
        {
            labelRegStatus.text = "Invalid email address.";
            return;
        }

        labelRegStatus.text = ""; //user sees a refresh every time he tries...
        loadingPanel.SetActive(true);
        StartCoroutine(Reg());
    }

    private IEnumerator Reg()
    {
        WWWForm form = new WWWForm();
        form.AddField("email", emailRegInput.text);
        form.AddField("username", usernameRegInput.text);
        form.AddField("password", pwRegInput.text);
        WWW w = new WWW("https://soullesscrusades.000webhostapp.com/register.php", form);
        yield return w;
        //Debug.Log(w.error);
        //Debug.Log(w.text);
        if (w.text == "1")
        {
            labelAuthStatus.text = "Registration successful. You can now login!";
            emailRegInput.text = "";
            usernameRegInput.text = "";
            pwRegInput.text = "";
            GoToLogin();
        }
        else
            labelRegStatus.text = "Email or username already in use. Please modify your input.";
        loadingPanel.SetActive(false);
    }

    public void GoToLogin()
    {
        loginPanel.SetActive(true);
        goToLogin.interactable = false;
        registerPanel.SetActive(false);
        goToRegister.interactable = true;
    }

    public void GoToRegister()
    {
        loginPanel.SetActive(false);
        goToLogin.interactable = true;
        registerPanel.SetActive(true);
        goToRegister.interactable = false;
    }

    public void Okay()
    {
        infoPanel.SetActive(false);
    }

    public void SendMsg()
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("RpcSendText", PhotonTargets.All, PhotonNetwork.player.NickName, chatInput.text);
        chatInput.text = "";
        chatInput.ActivateInputField();
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

    public void OnMaxPlayersSliderChangeValue(Slider slider)
    {
        labelPlayerInt.text = slider.value.ToString();
        numberOfPlayers = (byte)slider.value;
    }

    public void OnRoundsToWinSliderChangeValue(Slider slider)
    {
        labelRoundsToWinInt.text = slider.value.ToString();
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
        roomInputField.text = "Room" + UnityEngine.Random.Range(1000, 9999);
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = numberOfPlayers, IsVisible = !privateToggle.isOn }, null);
        privateToggle.isOn = false;
        roomInputField.text = "";
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
        PhotonNetwork.JoinRoom(privateRoomField.text);
        isCurrentRoomCompetitive = false;
    }

    public void ShowPrivateRoomJoin()
    {
        joinPrivateRoomPanel.SetActive(true);
    }

    public void ClosePrivateRoomJoin()
    {
        joinPrivateRoomPanel.SetActive(false);
        privateRoomField.text = "";
        labelPrivateRoomInfo.text = "";
    }

    public void OnModifiedPrivateRoomField(InputField inputField)
    {
        labelPrivateRoomInfo.text = "";
    }

    public void SelectPlayer()
    {
        kickPlayer.interactable = true;
        selectedPlayer = EventSystem.current.currentSelectedGameObject.transform.parent.GetComponent<PhotonPlayerContainer>().Get();
    }

    public void OpenSpellList()
    {
        selectSpellsPanel.SetActive(true);
    }

    public void CloseSpellList()
    {
        selectSpellsPanel.SetActive(false);
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
            GameObject go = Instantiate(chatMsgPrefab, parent);
            go.GetComponentInChildren<Text>().text = string.Format("<color=#FFE798B4>[{0}]</color>  <color=orange>{1}</color>: {2}", 
                DateTime.Now.ToString("HH:mm:ss"), nick, msg);
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

    public void OptionsToVideo()
    {
        videoPanel.SetActive(true);
        soundPanel.SetActive(false);
        controlsPanel.SetActive(false);
        goToVideo.interactable = false;
        goToSound.interactable = true;
        goToControls.interactable = true;
    }

    public void OptionsToSound()
    {
        videoPanel.SetActive(false);
        soundPanel.SetActive(true);
        controlsPanel.SetActive(false);
        goToVideo.interactable = true;
        goToSound.interactable = false;
        goToControls.interactable = true;
    }

    public void OptionsToControls()
    {
        videoPanel.SetActive(false);
        soundPanel.SetActive(false);
        controlsPanel.SetActive(true);
        goToVideo.interactable = true;
        goToSound.interactable = true;
        goToControls.interactable = false;
    }

    public void GoToOptions()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToOptions();
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        isAuthError = true;
        loadingPanel.SetActive(false);
        errorPanel.SetActive(false);
        labelAuthStatus.text = "Username or password is invalid!";
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        if (joinPrivateRoomPanel.activeInHierarchy)
        {
            labelPrivateRoomInfo.text = "Room is full or doesn't exist!";
        }
    }

    public override void OnJoinedRoom()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToLobby();
        maxPlayers.text = "Max Players: " + PhotonNetwork.room.MaxPlayers;
        labelPlayerNumber.text = "Current player number: " + PhotonNetwork.room.PlayerCount;
        Transform parent = GameObject.Find("Player List Parent").transform;

        if (isCurrentRoomCompetitive)
            labelRoomName.text = "Competitive lobby";
        else 
            labelRoomName.text = "Room: " + PhotonNetwork.room.Name;

        if (joinPrivateRoomPanel.activeInHierarchy)
        {
            privateRoomField.text = "";
            ClosePrivateRoomJoin();
        }

        if (PhotonNetwork.isMasterClient)
        {
            startGame.gameObject.SetActive(true);
            AddPlayerListItem(PhotonNetwork.player, parent);
            kickPlayer.onClick.AddListener(() => { KickPlayer(); });

            var props = new ExitGames.Client.Photon.Hashtable();
            props.Add("maxwins", (int) roundsToWinSlider.value);
            PhotonNetwork.room.SetCustomProperties(props, null);
            labelRoundsToWin.text = "Rounds to Win: " + (int) roundsToWinSlider.value;
        }
        else
        {
            labelRoundsToWin.text = "Rounds to Win: " + PhotonNetwork.room.CustomProperties["maxwins"].ToString();

            startGame.gameObject.SetActive(false);
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
                AddPlayerListItem(p, parent);
        }

        // comp override
        if (isCurrentRoomCompetitive && PhotonNetwork.isMasterClient)
        {
            startGame.gameObject.SetActive(false);

            var props = new ExitGames.Client.Photon.Hashtable();
            props.Add("maxwins", COMP_MAXWINS);
            PhotonNetwork.room.SetCustomProperties(props, null);
            labelRoundsToWin.text = "Rounds to Win: " + COMP_MAXWINS;
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
            if (t.gameObject.GetComponent<PhotonPlayerContainer>().Get().ID == other.ID)
                Destroy(t.gameObject);
        }

        labelPlayerNumber.text = "Current player number: " + PhotonNetwork.room.PlayerCount;
    }

    private void AddPlayerListItem(PhotonPlayer player, Transform parent)
    {
        GameObject go = Instantiate(listedPlayerPrefab, parent) as GameObject;
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

        selectSpellsPanel.SetActive(true);
        readyToggle.isOn = false;
    }

    public override void OnConnectedToMaster()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
        labelVersion.text = "Development build v" + gameVersion;
        labelUser.text = "Account settings for " + PhotonNetwork.player.NickName;
        PhotonNetwork.JoinLobby();
        pwInput.text = "";
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
        if (!isAuthError)
        {
            labelError.text = strDisconnected;
            loadingPanel.SetActive(false);
            errorPanel.SetActive(true);
            Camera.main.GetComponent<MenuCamera>().TransitionToLogin();
        }
        isAuthError = false;
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        if (!isCurrentRoomCompetitive)
        {
            LeaveRoom();
            infoPanel.SetActive(true);
            infoPanel.GetComponentInChildren<Text>().text = strHostLeft;
        }
    }

    void OnGUI()
    {
        if(PhotonNetwork.connected)
            GUILayout.Label(PhotonNetwork.connectionState + "\n" + PhotonNetwork.GetPing() + " ms");
    }
}
