using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScript : Photon.PunBehaviour {
    // inherited by spell types
    // use those as components

    protected NetworkGameManager gameManager;

    public float castTime = 0.3f;
    public float castInterval = 1f;

    public string tooltipText = "Generic Spell.";

    void Awake()
    {
        gameManager = GameObject.FindWithTag("GameController").GetComponent<NetworkGameManager>();
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
