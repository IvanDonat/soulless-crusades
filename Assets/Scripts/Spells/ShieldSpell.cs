using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldSpell : Spell
{
    public float shieldTime = 5f;

    Quaternion rot = Quaternion.identity;

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

    void Update()
    {
        rot = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y + 270 * Time.deltaTime, rot.eulerAngles.z);
        transform.rotation = rot;
    }

    private IEnumerator DestroySpell()
    {
        yield return new WaitForSeconds(shieldTime);
        Destroy(gameObject);
    }
}
