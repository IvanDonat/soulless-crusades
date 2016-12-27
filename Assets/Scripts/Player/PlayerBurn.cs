using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBurn : Photon.PunBehaviour {
    public GameObject burn;
    private PlayerScript playerScript;
    private bool onFire = false;

    void Start ()
    {
        if (photonView.isMine)
            playerScript = transform.GetComponent<PlayerScript>();
    }
	

    void Update () 
    {
        if (onFire && photonView.isMine)
        {
            playerScript.TakeDamage(null, 10f * Time.deltaTime, 0);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Lava")
        {
            burn.SetActive(true);
            onFire = true;
        }
        else
        {
            burn.SetActive(false);
            onFire = false;
        }
    }
}
