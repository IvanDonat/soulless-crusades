using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibilitySpell : Spell
{
    public float cloakTime = 5f;

    void Start()
    {
        if (photonView.isMine)
            myPlayer.GetComponent<PhotonView>().RPC("Cloak", PhotonTargets.All, cloakTime);
    }
}
