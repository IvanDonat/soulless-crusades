using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class SetPlayerName : MonoBehaviour
{
    public InputField inputField;
    static string playerNamePrefKey = "PlayerName";

    void Start()
    {
        string defaultName = ""; //ovdje mozemo napravit ako oces tipa guest + random(0,9999)
        inputField = GetComponent<InputField>();

        if (PlayerPrefs.HasKey(playerNamePrefKey))
        {
            defaultName = PlayerPrefs.GetString(playerNamePrefKey);
            inputField.text = defaultName;
        }

        PhotonNetwork.playerName = defaultName;
    }

    public void SetName(string value)
    {
        PhotonNetwork.playerName = value;
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
