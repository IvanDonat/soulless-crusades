using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibilitySpell : Spell
{
    public float cloakTime = 5f;
    public Transform cloakParticles;

    void Start()
    {
        if (photonView.isMine)
            myPlayer.GetComponent<PhotonView>().RPC("Cloak", PhotonTargets.All, cloakTime);

        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (p.GetComponent<PhotonView>().owner == photonView.owner)
            {
                myPlayer = p.transform;
                Instantiate(cloakParticles, p.transform.position, Quaternion.identity);
                break;
            }
        }

        StartCoroutine(UncloakEffect());
    }

    private IEnumerator UncloakEffect()
    {
        yield return new WaitForSeconds(cloakTime); // myPlayer is set for everyone in Start()
        if(myPlayer) // hasn't died
            Instantiate(cloakParticles, myPlayer.transform.position, Quaternion.identity);
    }
}
