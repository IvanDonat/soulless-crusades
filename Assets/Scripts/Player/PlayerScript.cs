using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {
    private PlayerMovement movementScript;
    private Transform terrain;

    private float health = 100;

    // currently supports one spell, @TODO multiple spells support
    public Transform currentSpellPrefab;
    private SpellScript currentSpellScript;
    private float lastCastedTimestamp;
    private IEnumerator castCoroutine;

    void Awake()
    {
        movementScript = transform.GetComponent<PlayerMovement>();
        terrain = GameObject.FindGameObjectWithTag("Terrain").transform;
        SetSpell(currentSpellPrefab);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time - lastCastedTimestamp >= currentSpellScript.castInterval && movementScript.GetState() != PlayerState.CASTING)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (terrain.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 aimPos = hit.point;
                Vector3 spawnPos = transform.position;
                aimPos.y = spawnPos.y;
                Vector3 aimDir = (aimPos - transform.position * 1.2f).normalized;

                if(castCoroutine != null)
                    CancelCast();
                castCoroutine = CastWithDelay(currentSpellScript.GetCastTime(), aimPos, aimDir);
                StartCoroutine(castCoroutine);

                movementScript.CastSpell(currentSpellScript.GetCastTime(), aimPos);
            }
        }
    }

    private IEnumerator CastWithDelay(float time, Vector3 aimPos, Vector3 aimDir)
    {
        yield return new WaitForSeconds(time);
        lastCastedTimestamp = Time.time;
        Instantiate(currentSpellPrefab, transform.position + aimDir*2, Quaternion.LookRotation(aimDir, Vector3.up));
    }

    public void SetSpell(Transform spell)
    {
        currentSpellPrefab = spell;
        currentSpellScript = spell.GetComponent<SpellScript>();
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        CancelCast();
        movementScript.Stun(dmg / 5f);
    }

    public void CancelCast()
    {
        StopCoroutine(castCoroutine);
        movementScript.CancelCast();
    }
}
