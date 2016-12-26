using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkGameManager : MonoBehaviour {
    public Transform playerPrefab;

    public Text gameTimeText;
    private float gameTime = 0;

    public GameObject sharedUI;
    public GameObject playingUI;
    public GameObject spectatorUI;

    // stats
    private int kills = 0;

    void Start()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(Random.Range(-15f, 15f), 1, Random.Range(-15f, 15f)), Quaternion.identity, 0);
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
    public void OnPlayerDeath(int playerID)
    { // is called for everyone by dying player

    }

    [PunRPC]
    public void GotKill(PhotonPlayer victim)
    {
        print("You killed: " + victim.NickName);
        kills++;
        PhotonNetwork.player.SetScore(kills);
    }

    void OnGUI()
    {
        if(PhotonNetwork.connected)
            GUILayout.Label(PhotonNetwork.connectionState + " " + PhotonNetwork.GetPing() + " ms");
    }
}
