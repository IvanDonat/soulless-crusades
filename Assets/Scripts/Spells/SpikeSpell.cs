using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeSpell : Spell {
    public float duration = 10f;
    public float slowdownTime = 5f;
    public Collider spikeCollider;
    public Renderer spikeRenderer;

    private bool isActive = false;

    void Start()
    {
        if (!isActive)
        { 
            // this is necessary because somehow Photon Engine can have the RPC happen before Start()
            spikeCollider.enabled = false;
            spikeRenderer.enabled = false;
        }

        if (photonView.isMine)
        {
            Vector3 newPos = castMousePos + Vector3.down * 3f;
            photonView.RPC("Activate", PhotonTargets.All, newPos);
        }
    }

    void Update()
    {
        if (isActive && transform.position.y < .8f)
        {
            transform.Translate(Vector3.up * Time.deltaTime * 10f, Space.World);
        }
    }

    [PunRPC]
    public void Activate(Vector3 pos)
    {
        transform.position = pos;
        spikeCollider.enabled = true;
        spikeRenderer.enabled = true;
        isActive = true;

        StartCoroutine(DestroySpell());
    }

    private IEnumerator DestroySpell()
    {
        yield return new WaitForSeconds(duration);

        float timePassed = 0;
        isActive = false;
        while (timePassed < 2f)
        {
            transform.Translate(Vector3.down * Time.deltaTime * 2f, Space.World);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider c)
    {
        if (photonView.isMine && c.tag == "Player")
        {
            c.GetComponent<PhotonView>().RPC("SetSlowdown", c.GetComponent<PhotonView>().owner, slowdownTime);
        }
    }
}
