using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlindSpell : Spell
{
    public float speed = 3f;
    public float blindTime = 4f;

    public Transform explosionTransform;

    void Start()
    {

    }

    void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.tag == "Player" && c.GetComponent<PhotonView>().isMine)
            return;

        if (photonView.isMine)
        {

            if (c.tag == "Player")
            {
                c.GetComponent<PhotonView>().RPC("Blind", c.GetComponent<PhotonView>().owner, blindTime);
            }
            else if (c.tag == "Spell" && c.GetComponent<ProjectileSpell>() != null)
            {
                // tell owner to find his own spell and destroy it as well
                gameManager.GetComponent<PhotonView>().RPC("DestroySpell", c.GetComponent<PhotonView>().owner, c.GetComponent<PhotonView>().viewID);
            }

            photonView.RPC("Remove", PhotonTargets.All);
        }
    }

    [PunRPC]
    public void Remove()
    {
        explosionTransform.parent = null;
        explosionTransform.gameObject.SetActive(true);
        Destroy(gameObject);
    }
}
