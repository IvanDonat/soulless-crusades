using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkGameManager : MonoBehaviour {
    public Transform playerPrefab;

    public Text gameTimeText;
    private float gameTime = 0;

    private TerrainManager terrainManager;

    public GameObject sharedUI;
    public GameObject playingUI;
    public GameObject spectatorUI;

    public GameObject scorePanel;
    public Transform scoreContent;
    public GameObject scoreItem;

    private List<GameObject> scoreList = new List<GameObject>();
    private RectTransform scorePanelRect;

    void Start()
    {
        terrainManager = GameObject.FindWithTag("Terrain").GetComponent<TerrainManager>();
        StartCoroutine(terrainManager.ProjectArcaneCircle());

        playingUI.SetActive(false);
        spectatorUI.SetActive(false);
        StartCoroutine(Wait(8.5f));

        scorePanelRect = scorePanel.GetComponent<RectTransform>();

        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            GameObject go = Instantiate(scoreItem, scoreContent) as GameObject;

            foreach (RectTransform r in go.GetComponentInChildren<RectTransform>())
            {
                if (r.gameObject.name == "Name")
                    r.GetComponent<Text>().text = p.NickName;
                else if (r.gameObject.name == "Kills")
                    r.GetComponent<Text>().text = "0";
                else if (r.gameObject.name == "Deaths")
                    r.GetComponent<Text>().text = "0";
                else if (r.gameObject.name == "Rounds Won")
                    r.GetComponent<Text>().text = "0";
            }

            scoreList.Add(go);
        }
    }

    void Update()
    {
        gameTime += Time.deltaTime;

        int minutes = (int)gameTime / 60;
        int seconds = (int)gameTime % 60;
        gameTimeText.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");

        if (Input.GetKey(KeyCode.Tab))
            scorePanel.transform.Translate(0, -500f * Time.deltaTime, 0);
        else
            scorePanel.transform.Translate(0, 500f * Time.deltaTime, 0);

        scorePanelRect.anchoredPosition = new Vector2(0, Mathf.Clamp(scorePanelRect.anchoredPosition.y, -144f, 99f));

        // koristiti GetSortedPlayerList() za updejtat listu
        // @TODO @simbaorka101
    }

    private IEnumerator Wait(float sec)
    {
        yield return new WaitForSeconds(sec);
        playingUI.SetActive(true);
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(Random.Range(-15f, 15f), 1, Random.Range(-15f, 15f)), Quaternion.identity, 0);
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
    public void OnPlayerDeath(PhotonPlayer player)
    { // is called for everyone by dying player
        
    }

    [PunRPC]
    public void GotKill(PhotonPlayer victim)
    {
        print("You killed: " + victim.NickName);
        PlayerProperties.IncrementProperty(PlayerProperties.KILLS);
    }

    public List<PhotonPlayer> GetSortedPlayerList()
    {
        return PhotonNetwork.playerList.OrderBy(pl => 
            {if ((bool) pl.CustomProperties[PlayerProperties.ALIVE] == false) return 10000 + pl.GetScore(); 
            else return pl.GetScore();}).ToList();
    }

    public void Disconnect()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    void OnGUI()
    {
        if(PhotonNetwork.connected)
            GUILayout.Label(PhotonNetwork.connectionState + " " + PhotonNetwork.GetPing() + " ms");
    }
}
