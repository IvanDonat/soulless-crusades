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
        string defaultName = "";
        inputField = GetComponent<InputField>();

        if (PlayerPrefs.HasKey(playerNamePrefKey))
        {
            if (!PlayerPrefs.GetString(playerNamePrefKey).StartsWith("Guest"))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                inputField.text = defaultName;
            }
        }

        PhotonNetwork.playerName = defaultName;
    }

    public void SetName(string value)
    {
        PhotonNetwork.playerName = value;
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
