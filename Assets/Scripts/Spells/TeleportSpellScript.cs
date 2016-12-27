using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSpellScript : SpellScript {
    void Start()
    {
        if (photonView.isMine)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (GameObject.FindWithTag("Terrain").GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                foreach(GameObject p in GameObject.FindGameObjectsWithTag("Player"))
                {
                    if (p.GetPhotonView().isMine)
                    {
                        Vector3 pos = hit.point;
                        pos.y += 1.2f;

                        p.transform.position = hit.point;
                        break;
                    }
                }
            }
        }

        Destroy(gameObject);
    }

}
