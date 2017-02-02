using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityWellSpell : Spell 
{
    public float pullForce = 40f;
    public float radius = 3f;
    public float lifetime = 8f;

    private float timeAlive = 0f;

    private PlayerMovement localPlayer;

    void Start () 
    {
        if (photonView.isMine)
        {   
            castMousePos.y = myPlayer.position.y - 0.2f;
            photonView.RPC("SetPosition", PhotonTargets.All, castMousePos);
        }

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (go.GetComponent<PhotonView>().isMine)
            {
                localPlayer = go.GetComponent<PlayerMovement>();
                break;
            }
        }
    }

    void FixedUpdate()
    {
        if (!localPlayer)
            return; //died

        timeAlive += Time.deltaTime;
        if (timeAlive >= lifetime)
            Destroy(gameObject);

        Vector3 playerPos = localPlayer.GetRigidbody().position;
        Vector3 pullDir = transform.position - playerPos;
        if (pullDir.magnitude <= radius)
        {
            pullDir.Normalize();
            pullDir *= pullForce;

            float dist = (transform.position - playerPos).magnitude;
            if (dist < 2f) // dont overshoot
                pullDir *= dist;

            localPlayer.CancelMovementOrder();
            localPlayer.GetRigidbody().AddForce(pullDir, ForceMode.Acceleration);
        }
    }

    [PunRPC]
    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }
}
