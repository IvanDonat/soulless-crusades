using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobilitySpell : Spell 
{
    public float speedTime = 5f;
    public float moveForceMultiplier = 1.5f;

    void Start()
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetPhotonView().owner == photonView.owner)
            {
                gameObject.transform.parent = p.transform;
                gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

                if (photonView.isMine)
                    p.GetComponent<PlayerMovement>().moveForceMultiplier = moveForceMultiplier;
                break;
            }
        }

        StartCoroutine(DestroySpell());
    }

    private IEnumerator DestroySpell()
    {
        yield return new WaitForSeconds(speedTime);

        if (photonView.isMine)
        {
            PlayerMovement player = myPlayer.GetComponent<PlayerMovement>();
            player.moveForceMultiplier = 1f;
        }

        Destroy(gameObject);
    }
}
