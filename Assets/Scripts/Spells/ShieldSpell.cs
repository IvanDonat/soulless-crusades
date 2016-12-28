using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSpell : SpellScript
{
    public float shieldTime = 5f;

    void Start()
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetPhotonView().owner == photonView.owner)
            {
                gameObject.transform.parent = p.transform;
                gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

                if (photonView.isMine)
                    p.GetComponent<PlayerScript>().shieldTimeLeft += shieldTime;
                break;
            }
        }
        StartCoroutine(DestroySpell());
    }

    private IEnumerator DestroySpell()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}
