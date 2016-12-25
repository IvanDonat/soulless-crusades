using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkGameManager : MonoBehaviour {
    public Transform playerPrefab;

    public Text gameTimeText;
    private float gameTime = 0;

    void Start()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, Vector3.up * 2, Quaternion.identity, 0);
    }

    void Update()
    {
        gameTime += Time.deltaTime;

        int minutes = (int) gameTime / 60;
        int seconds = (int) gameTime % 60;
        gameTimeText.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");
    }

    void OnGUI()
    {
        if(PhotonNetwork.connected)
            GUILayout.Label(PhotonNetwork.connectionState + "\n" + PhotonNetwork.GetPing() + " ms");
    }
}
