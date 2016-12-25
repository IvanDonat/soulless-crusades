using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScript : Photon.PunBehaviour {
    public float speed = 3f;
    public float damage = 20;
    public float castTime = 0.3f;
    public float castInterval = 1f;

    private NetworkGameManager gameManager;

    void Start()
    {
        gameManager = GameObject.FindWithTag("GameController").GetComponent<NetworkGameManager>();;
    }

    void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider c)
    {
        if (photonView.isMine)
        {
            if (c.tag == "Player" && !c.GetComponent<PhotonView>().isMine)
            {
                c.GetComponent<PhotonView>().RPC("TakeDamage", c.GetComponent<PhotonView>().owner, damage);
            }
            else if (c.tag == "Player" && c.GetComponent<PhotonView>().isMine)
            {
                return;
            }
            else if (c.tag == "Spell")
            {
                gameManager.GetComponent<PhotonView>().RPC("DestroySpell", c.GetComponent<PhotonView>().owner, c.GetComponent<PhotonView>().viewID);
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }

    public float GetCastTime()
    {
        return castTime;
    }
}
