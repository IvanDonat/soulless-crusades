using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        int minutes = (int)gameTime / 60;
        int seconds = (int)gameTime % 60;
        gameTimeText.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");

        if (Input.GetKey(KeyCode.Escape))
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene(0);
        }
    }

    [PunRPC]
    public void DestroySpell(int id)
    {
        foreach (GameObject sp in GameObject.FindGameObjectsWithTag("Spell"))
        {
            if (sp.GetComponent<PhotonView>().viewID == id)
            {
                PhotonNetwork.Destroy(sp);
                break;
            }
        }
    }

    void OnGUI()
    {
        if(PhotonNetwork.connected)
            GUILayout.Label(PhotonNetwork.connectionState + "\n" + PhotonNetwork.GetPing() + " ms");
    }
}
