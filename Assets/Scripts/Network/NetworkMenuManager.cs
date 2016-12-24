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
                labelPlayerNumber;
    public InputField roomInputField;
    public Toggle privateToggle;
    public Slider playerNumberSlider;
    public GameObject selectedRoomPrefab;
    public GameObject loadingPanel, errorPanel;

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
        labelPlayerNumber.text += " " + PhotonNetwork.room.PlayerCount;
    }

    public override void OnLeftRoom()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
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
