using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSpell : Spell
{
    [Header("Spell specific")]

    public float healAmount = 10f;
    public float healAnimLen = 2f;

    void Start()
    {
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetPhotonView().owner == photonView.owner)
            {
                gameObject.transform.parent = p.transform;
                gameObject.transform.localPosition = new Vector3(0f, -1f, 0f);

                if (photonView.isMine)
                    p.GetComponent<PlayerScript>().Heal(healAmount);
                break;
            }
        }
        StartCoroutine(DestroySpell());
    }

    private IEnumerator DestroySpell()
    {
        yield return new WaitForSeconds(healAnimLen);
        Destroy(gameObject);
    }
}
