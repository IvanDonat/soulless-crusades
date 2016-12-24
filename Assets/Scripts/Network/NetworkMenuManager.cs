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
    public InputField roomInputField;
    public Toggle privateToggle;
    public Slider playerNumberSlider;
    public GameObject loadingPanel, errorPanel, selectedRoomPrefab, listedPlayerPrefab;

    //Default room options
    private string roomName = "";
    private byte numberOfPlayers = 2;

    private const string strConnecting = "CONNECTING TO SERVER...";
    private const string strConnected = "CONNECTED!";
    private const string strFailedToConnect = "SOULLESS CRUSADES FAILED TO ESTABLISH A CONNECTION TO SERVER!";
    private const string strDisconnected = "SOULLESS CRUSADES DISCONNECTED FROM SERVER!";

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = autoJoinLobby;
        PhotonNetwork.automaticallySyncScene = autoSyncScene;
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
                if(t.transform != parent.transform)
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
    }

    public void Connect()
    {
        loadingPanel.SetActive(true);
        errorPanel.SetActive(false);

        if (PhotonNetwork.connected)
        {
            Debug.Log("Client already connected, transitioning to menu...");
            Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
            labelStatus.text = strConnected;
        }
        else
        {
            Debug.Log("Connecting...");
            PhotonNetwork.ConnectUsingSettings(gameVersion);
            labelStatus.text = strConnecting;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
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
        string roomName = EventSystem.current.currentSelectedGameObject.name.Split(' ')[1];
        PhotonNetwork.JoinRoom(roomName);
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
            AddPlayerListItem(PhotonNetwork.player.NickName, parent);
        }
        else
        {
            foreach (PhotonPlayer p in PhotonNetwork.playerList)
                AddPlayerListItem(p.NickName, parent);
        }
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer other)
    {
        labelPlayerNumber.text = "Current player number: " + PhotonNetwork.room.PlayerCount;
        Transform parent = GameObject.Find("Player List Parent").transform;
        AddPlayerListItem(other.NickName, parent);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
    {
        labelPlayerNumber.text = "Current player number: " + PhotonNetwork.room.PlayerCount;
        Transform parent = GameObject.Find("Player List Parent").transform;

        foreach (RectTransform t in parent.GetComponentInChildren<RectTransform> ())
        {
            if (t.gameObject.name.Split(' ')[1] == other.NickName)
                Destroy(t.gameObject);
        }

        labelPlayerNumber.text = "Current player number: " + PhotonNetwork.room.PlayerCount;
    }

    private void AddPlayerListItem(string playerName, Transform parent)
    {
        GameObject go = Instantiate(listedPlayerPrefab, parent) as GameObject;
        go.name = "PlayerListItem " + playerName;
        go.GetComponentInChildren<Text>().text = playerName;
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
        Debug.Log("Disconnected because " + cause.ToString());
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
}
