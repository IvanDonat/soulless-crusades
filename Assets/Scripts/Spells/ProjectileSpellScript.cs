using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpellScript : SpellScript {
    public float speed = 3f;
    public float damage = 20;
    public float knockbackForce = 15f;
    public float dragDropTo = 1f;
    public float dragResetTime = 1f;
    public float stunTime = 1f;

    public Transform explosionTransform;
    public Transform healTransform;
    public bool isLifeLeach = false;

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
                    healTransform.gameObject.SetActive(true);
                    if (photonView.isMine)
                        p.GetComponent<PlayerScript>().Heal(damage);
                    break;
                }
            }
        }

        if (photonView.isMine)
        {

            if (c.tag == "Player" && !c.GetComponent<PhotonView>().isMine)
            {
                c.GetComponent<PhotonView>().RPC("TakeDamage", c.GetComponent<PhotonView>().owner, photonView.owner, damage, stunTime);
                c.GetComponent<PhotonView>().RPC("DoKnockback", c.GetComponent<PhotonView>().owner, transform.forward, knockbackForce, dragDropTo, dragResetTime);
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

            photonView.RPC("Remove", PhotonTargets.All);
        }
    }

    [PunRPC] // use instead of PhotonNetwork.Destroy(...) to set off explosion on all clients
    public void Remove()
    {
        if (isLifeLeach)
        {
            explosionTransform.parent = victim;
            explosionTransform.gameObject.SetActive(true);

        }
        else
        {
            explosionTransform.parent = null;
            explosionTransform.gameObject.SetActive(true);
        }

        Destroy(gameObject);
    }
}
