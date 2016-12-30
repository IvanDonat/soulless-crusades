using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpell : Spell {
    public float speed = 3f;
    public float damage = 20;
    public float knockbackForce = 15f;
    public float dragResetTime = 1f;
    public float stunTime = 1f;

    public Transform explosionTransform;
    public Transform healTransform;
    public bool isLifeLeach = false;
    public bool parentToVictim = false;

    private Transform victim;

    void Start()
    {
        
    }

    void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider c)
    {
        if (isLifeLeach)
        {
            foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (p.GetPhotonView().owner == photonView.owner)
                {
                    healTransform.parent = p.transform;
                    healTransform.localPosition = new Vector3(0f, -1f, 0f);
                    break;
                }
            }
        }

        if (photonView.isMine)
        {

            if (c.tag == "Player" && !c.GetComponent<PhotonView>().isMine)
            {
                c.GetComponent<PhotonView>().RPC("TakeDamage", c.GetComponent<PhotonView>().owner, photonView.owner, damage, stunTime);
                c.GetComponent<PhotonView>().RPC("DoKnockback", c.GetComponent<PhotonView>().owner, transform.forward, knockbackForce, dragResetTime);

                if (isLifeLeach)
                {
                    myPlayer.GetComponent<PlayerScript>().Heal(damage);
                    healTransform.gameObject.SetActive(true);
                }

                victim = c.transform;
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

            if (victim != null)
                photonView.RPC("Remove", PhotonTargets.All, victim.GetComponent<PhotonView>().owner);
            else
                photonView.RPC("Remove", PhotonTargets.All);
        }
    }

    [PunRPC] // use instead of PhotonNetwork.Destroy(...) to set off explosion on all clients
    public void Remove(PhotonPlayer victim)
    {
        if (parentToVictim)
        {
            foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (p.GetPhotonView().owner == victim)
                {
                    explosionTransform.parent = p.transform;
                    explosionTransform.localPosition = Vector3.zero;
                    break;
                }
            }
        }
        else
            explosionTransform.parent = null;

        explosionTransform.gameObject.SetActive(true);

        Destroy(gameObject);
    }

    [PunRPC]
    public void Remove()
    {
        explosionTransform.parent = null;
        explosionTransform.gameObject.SetActive(true);
        Destroy(gameObject);
    }
}
