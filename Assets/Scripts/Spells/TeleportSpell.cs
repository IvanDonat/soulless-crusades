using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSpell : Spell
{
    [Header("Spell specific")]

    public Transform teleportParticles;

    void Start()
    {
        if (photonView.isMine)
        {
            if(castMousePos != null && myPlayer != null)
            {
                Vector3 pos = castMousePos;
                pos.y += 1.2f;
                
                photonView.RPC("TeleportPlayer", PhotonTargets.All, PhotonNetwork.player, pos);
                myPlayer.GetComponent<PlayerScript>().shieldTimeLeft += .3f;
            }
            else
                photonView.RPC("TeleportPlayer", PhotonTargets.All, PhotonNetwork.player, transform.position);   // called by TerrainManager to tp             
        }
    }

    [PunRPC]
    public void TeleportPlayer(PhotonPlayer owner, Vector3 pos)
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetPhotonView().owner == owner)
            {
                Instantiate(teleportParticles, p.transform.position, Quaternion.identity);
                Instantiate(teleportParticles, pos, Quaternion.identity);

                p.transform.position = pos;
                break;
            }
        }

        Destroy(gameObject);
    }

}
