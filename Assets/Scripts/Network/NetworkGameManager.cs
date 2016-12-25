using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkGameManager : MonoBehaviour {
    public Text gameTimeText;
    private float gameTime = 0;

    void Start()
    {

    }

    void Update()
    {
        gameTime += Time.deltaTime;

        int minutes = (int) gameTime / 60;
        int seconds = (int) gameTime % 60;
        gameTimeText.text = minutes.ToString("D2") + ":" + seconds.ToString("D2");
    }
}
