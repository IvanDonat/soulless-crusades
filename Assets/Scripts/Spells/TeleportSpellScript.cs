using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSpellScript : SpellScript {
    void Start()
    {
        if (photonView.isMine)
        {
            Vector3 pos = castMousePos;
            pos.y += 1.2f;
            photonView.RPC("TeleportPlayer", PhotonTargets.All, PhotonNetwork.player, pos);
        }
    }

    [PunRPC]
    public void TeleportPlayer(PhotonPlayer owner, Vector3 pos)
    {
        foreach(GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetPhotonView().owner == owner)
            {
                p.transform.position = pos;
                break;
            }
        }

        Destroy(gameObject);
    }

}
