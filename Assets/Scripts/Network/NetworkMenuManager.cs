using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMenuManager : Photon.PunBehaviour {

    public string gameVersion = "";
    public bool autoJoinLobby = true;
    public bool autoSyncScene = true;

    public Text labelVersion, labelError;
    public GameObject loadingPanel, errorPanel;

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = autoJoinLobby;
        PhotonNetwork.automaticallySyncScene = autoSyncScene;
    }

    void Start () {
        Connect();
	}
	
	void Update () {
		
	}

    public void Connect()
    {
        loadingPanel.SetActive(true);
        errorPanel.SetActive(false);

        if (PhotonNetwork.connected)
        {
            Debug.Log("Client already connected, transitioning to menu...");
            Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
        }
        else
        {
            Debug.Log("Connecting...");
            PhotonNetwork.ConnectUsingSettings(gameVersion);
        }
    }

    public void Exit()
    {
        Application.Quit();
    } 

    public override void OnConnectedToMaster()
    {
        Camera.main.GetComponent<MenuCamera>().TransitionToMainMenu();
        labelVersion.text = "Development build v" + gameVersion;
    }

    public override void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.Log("Disconnected because " + cause.ToString());
        labelError.text = "SOULLESS CRUSADES FAILED TO ESTABLISH A CONNECTION TO THE SERVER!";
        loadingPanel.SetActive(false);
        errorPanel.SetActive(true);
    }

    public override void OnDisconnectedFromPhoton()
    {
        labelError.text = "SOULESS CRUSADES DISCONNECTED FROM THE SERVER!";
        loadingPanel.SetActive(false);
        errorPanel.SetActive(true);
        Camera.main.GetComponent<MenuCamera>().TransitionToLoadingGame();
    }
}
