using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScript : Photon.PunBehaviour {
    public float speed = 3f;
    public float damage = 20;
    public float castTime = 0.3f;
    public float castInterval = 1f;

    // use this to send damage RPCs to others
    private PhotonView ownersPhotonView;

    void Start()
    {

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
                ownersPhotonView.RPC("TakeDamage", c.GetComponent<PhotonView>().owner, damage);
            }
            else if (c.GetComponent<PhotonView>().isMine)
            {
                return;
            }
            else if (c.tag == "Spell")
            {
                ownersPhotonView.RPC("DestroySpell", c.GetComponent<PhotonView>().owner, c.GetComponent<PhotonView>().viewID);
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    private void DestroySpell(int id)
    {
        if (photonView.viewID == id)
            PhotonNetwork.Destroy(gameObject);
    }

    public float GetCastTime()
    {
        return castTime;
    }

    public void SetRPCView(PhotonView pw)
    {
        ownersPhotonView = pw;
    }

}
