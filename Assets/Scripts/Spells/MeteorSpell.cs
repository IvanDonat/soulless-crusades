using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSpell : Spell 
{
    public float speed = 3f;
    public float damage = 20;
    public float knockbackForce = 15f;
    public float dragResetTime = 1f;
    public float stunTime = 1f;

    private bool inAir = true;

    void Start () 
    {
        if (photonView.isMine)
        {
            castMousePos += Vector3.up * 60f;
            photonView.RPC("SetPosition", PhotonTargets.All, castMousePos);
        }
    }

    void Update()
    {
        transform.Translate(Vector3.down * Time.deltaTime * speed, Space.World);

        if (transform.position.y <= 1f && inAir && photonView.isMine)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                Transform t = go.GetComponent<Transform>();
                Vector3 knockBackDir = - transform.position + t.position;
                if (knockBackDir.magnitude < 10f && !go.GetComponent<PhotonView>().isMine)
                {
                    knockBackDir.y = go.transform.position.y;
                    knockBackDir.Normalize();

                    go.GetComponent<PhotonView>().RPC("TakeDamage", go.GetComponent<PhotonView>().owner, photonView.owner, damage, stunTime);
                    go.GetComponent<PhotonView>().RPC("DoKnockback", go.GetComponent<PhotonView>().owner, knockBackDir, knockbackForce, dragResetTime);
                }
            }

            inAir = false;
        }
    }

    [PunRPC]
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    [PunRPC]
    public void Remove()
    {
        //explosionTransform.parent = null;
        //explosionTransform.gameObject.SetActive(true);
        Destroy(gameObject);
    }
}
