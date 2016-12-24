using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {
    private PlayerMovement movementScript;
    private Transform terrain;

    private float health = 100;

    // currently supports one spell, @TODO multiple spells support
    public Transform spellPrefab;

    void Awake()
    {
        movementScript = transform.GetComponent<PlayerMovement>();
        terrain = GameObject.FindGameObjectWithTag("Terrain").transform;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (terrain.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 aimPos = hit.point;
                Vector3 spawnPos = transform.position;
                aimPos.y = spawnPos.y;

                Vector3 aimDir = (aimPos - transform.position * 1.2f).normalized;

                Instantiate(spellPrefab, transform.position + aimDir*2, Quaternion.LookRotation(aimDir, Vector3.up));

                movementScript.CastSpell(spellPrefab.GetComponent<SpellScript>().GetCastTime(), aimPos);
            }
        }
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        movementScript.Stun(dmg / 5f);
    }
}
