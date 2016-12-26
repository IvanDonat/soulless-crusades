using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScript : Photon.PunBehaviour {
    public float speed = 3f;
    public float damage = 20;
    public float knockbackForce = 15f;
    public float dragDropTo = 1f;
    public float dragResetTime = 1f;
    public float stunTime = 1f;
    public float castTime = 0.3f;
    public float castInterval = 1f;

    private NetworkGameManager gameManager;
    public Transform explosionTransform;

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
                c.GetComponent<PhotonView>().RPC("TakeDamage", c.GetComponent<PhotonView>().owner, photonView.owner, damage, stunTime);
                c.GetComponent<PhotonView>().RPC("DoKnockback", c.GetComponent<PhotonView>().owner, transform.forward, knockbackForce, dragDropTo, dragResetTime);
            }
            else if (c.tag == "Player" && c.GetComponent<PhotonView>().isMine)
            {
                return;
            }
            else if (c.tag == "Spell")
            {
                // tell owner to find his own spell and destroy it as well
                gameManager.GetComponent<PhotonView>().RPC("DestroySpell", c.GetComponent<PhotonView>().owner, c.GetComponent<PhotonView>().viewID);
            }

            photonView.RPC("Remove", PhotonTargets.All);
        }
    }

    [PunRPC] // use instead of PhotonNetwork.Destroy(...) to set off explosion on all clients
    public void Remove()
    {
        explosionTransform.parent = null;
        explosionTransform.gameObject.SetActive(true);

        Destroy(gameObject);
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
