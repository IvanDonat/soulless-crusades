using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerProperties {
    public const string KILLS   = "kills";
    public const string DEATHS  = "deaths";
    public const string ALIVE   = "alive";
    public const string WINS    = "wins";
    public const string HEALTH  = "health";
    
    public static void SetProperty(string key, int value)
    {
        var props = new ExitGames.Client.Photon.Hashtable();
        props.Add(key, value);
        PhotonNetwork.player.SetCustomProperties(props, null);
    }

    public static void SetProperty(string key, bool value)
    {
        var props = new ExitGames.Client.Photon.Hashtable();
        props.Add(key, value);
        PhotonNetwork.player.SetCustomProperties(props, null);
    }

    public static void IncrementProperty(string key)
    {
        var props = new ExitGames.Client.Photon.Hashtable();
        props.Add(key, (int) PhotonNetwork.player.CustomProperties[key] + 1);
        PhotonNetwork.player.SetCustomProperties(props, null);
    }

    public static string GetPropertyStr(string key)
    {
        return (string) PhotonNetwork.player.CustomProperties[key];
    }

    public static bool GetPropertyBool(string key)
    {
        return (bool) PhotonNetwork.player.CustomProperties[key];
    }

    public static int GetPropertyInt(string key)
    {
        return (int) PhotonNetwork.player.CustomProperties[key];
    }
}

