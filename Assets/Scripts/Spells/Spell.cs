using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spell : Photon.PunBehaviour {
    // inherited by spell types
    // use those as components

    protected NetworkGameManager gameManager;

    public float castTime = 0.3f;
    public float castInterval = 1f;

    public string tooltipText = "Generic Spell.";

    protected Transform myPlayer;
    protected Vector3 castMousePos;

    void Awake()
    {
        gameManager = GameObject.FindWithTag("GameController").GetComponent<NetworkGameManager>();
    }

    public void SetParams(Transform myPlayer, Vector3 castMousePos)
    {
        this.myPlayer = myPlayer;
        this.castMousePos = castMousePos;
    }

    public float GetCastTime()
    {
        return castTime;
    }

    public float GetCooldown()
    {
        return castInterval;
    }
}
