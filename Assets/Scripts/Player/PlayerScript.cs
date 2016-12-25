using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : Photon.PunBehaviour {
    private PlayerMovement movementScript;
    private Transform terrain;

    private float maxHealth = 100;
    private float health;

    // currently supports one spell, @TODO multiple spells support
    public Transform currentSpellPrefab;
    private SpellScript currentSpellScript;
    private float lastCastedTimestamp;
    private IEnumerator castCoroutine;

    // GUI
    private Slider healthBar;

    void Awake()
    {
        movementScript = transform.GetComponent<PlayerMovement>();
        terrain = GameObject.FindGameObjectWithTag("Terrain").transform;
        SetSpell(currentSpellPrefab);

        healthBar = GameObject.Find("Health Bar").GetComponent<Slider>();
        health = maxHealth;
    }

    void Update()
    {
        if (!photonView.isMine)
            return;

        if (Input.GetMouseButtonDown(0) && Time.time - lastCastedTimestamp >= currentSpellScript.castInterval && movementScript.GetState() != PlayerState.CASTING)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (terrain.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 aimPos = hit.point;
                aimPos.y = transform.position.y;

                Vector3 aimDir = aimPos - transform.position;
                aimDir.Normalize();

                if(castCoroutine != null)
                    CancelCast();
                castCoroutine = CastWithDelay(currentSpellScript.GetCastTime(), aimPos, aimDir);
                StartCoroutine(castCoroutine);

                movementScript.CastSpell(currentSpellScript.GetCastTime(), aimPos);
            }
        }

        healthBar.value = health / maxHealth;
    }

    private IEnumerator CastWithDelay(float time, Vector3 aimPos, Vector3 aimDir)
    {
        yield return new WaitForSeconds(time);
        lastCastedTimestamp = Time.time;

        GameObject spell = PhotonNetwork.Instantiate("Spells/" + currentSpellPrefab.name, transform.position + aimDir*2, Quaternion.LookRotation(aimDir, Vector3.up), 0) as GameObject;
        spell.GetComponent<SpellScript>().SetRPCView(photonView);
    }

    public void SetSpell(Transform spell)
    {
        currentSpellPrefab = spell;
        currentSpellScript = spell.GetComponent<SpellScript>();
    }

    [PunRPC]
    public void TakeDamage(float dmg)
    {
        health -= dmg;
        CancelCast();
        movementScript.Stun(dmg / 5f);
    }

    public void CancelCast()
    {
        if(castCoroutine != null)
            StopCoroutine(castCoroutine);
        movementScript.CancelCast();
    }
}
