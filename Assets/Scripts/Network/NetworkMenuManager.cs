using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMenuManager : Photon.PunBehaviour {

    public string gameVersion = "";
    public bool autoJoinLobby = true;
    public bool autoSyncScene = true;

    public Text labelVersion, labelError, labelStatus, labelPlayerInt, labelRoomName,
                labelPlayerNumber;
    public GameObject loadingPanel, errorPanel;

    //Default room options
    private string roomName = "";
    private byte numberOfPlayers = 2;
    private bool isPrivate = false;
    //private mapa

    private const string strConnecting      = "CONNECTING TO SERVER...";
    private const string strConnected       = "CONNECTED!";
    private const string strFailedToConnect = "SOULLESS CRUSADES FAILED TO ESTABLISH A CONNECTION TO SERVER!";
    private const string strDisconnected    = "SOULLESS CRUSADES DISCONNECTED FROM SERVER!";

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = autoJoinLobby;
        PhotonNetwork.automaticallySyncScene = autoSyncScene;
    }

    void Start()
    {
        Connect();
    }

    void Update()
    {
        
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

    public void OnSliderChangeValue (Slider slider)
    {
        labelPlayerInt.text = slider.value.ToString();
        numberOfPlayers = (byte) slider.value;
    }

    public void OnPrivateChangeValue (Toggle toggle)
    {
        if (toggle.isOn)
            isPrivate = true;
        else
            isPrivate = false;
    }

    public void OnRoomNameChangeValue (InputField inputField)
    {
        roomName = inputField.text;
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(roomName, new RoomOptions { MaxPlayers = numberOfPlayers, IsVisible = isPrivate}, null);
    }

    public override void OnJoinedRoom()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToLobby();
        labelRoomName.text = PhotonNetwork.room.Name;
        labelPlayerNumber.text += " " + PhotonNetwork.room.PlayerCount;
    }

    public override void OnConnectedToMaster()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
        labelVersion.text = "Development build v" + gameVersion;
        labelStatus.text = strConnected;
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
