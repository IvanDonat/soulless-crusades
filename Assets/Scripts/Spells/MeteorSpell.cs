using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorSpell : Spell 
{
    public float speed = 3f;
    public float damage = 20;
    public float radius = 10f;
    public float knockbackForce = 15f;
    public float dragResetTime = 1f;
    public float stunTime = 1f;

    public Transform explosionTransform;

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

        if (transform.position.y <= 1f)
            Explode();
    }

    private void Explode()
    {
        if (photonView.isMine)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
            {
                Transform t = go.GetComponent<Transform>();
                Vector3 knockBackDir = - transform.position + t.position;
                knockBackDir.y = go.transform.position.y;
                float dist = knockBackDir.magnitude;
                if (dist < radius && !go.GetComponent<PhotonView>().isMine)
                {
                    knockBackDir.Normalize();

                    float scale = Mathf.Lerp(1, 0, dist / radius);

                    go.GetComponent<PhotonView>().RPC("TakeDamage", go.GetComponent<PhotonView>().owner, photonView.owner, damage * scale, stunTime * scale);
                    go.GetComponent<PhotonView>().RPC("DoKnockback", go.GetComponent<PhotonView>().owner, knockBackDir, knockbackForce * scale, dragResetTime * scale);
                }
            }
        }

        explosionTransform.parent = null;
        explosionTransform.gameObject.SetActive(true);
        Destroy(gameObject);
    }

    [PunRPC]
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    void OnTriggerEnter(Collider c)
    {
        // @TODO fix?
        if (c.tag == "Rock")
            Explode();
    }
}
