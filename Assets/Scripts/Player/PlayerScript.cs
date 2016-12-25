using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : Photon.PunBehaviour {
    private NetworkGameManager gameManager;
    private PlayerMovement movementScript;
    private Transform terrain;

    private float maxHealth = 100;
    private float health;

    // currently supports one spell, @TODO multiple spells support
    public Transform currentSpellPrefab;
    private SpellScript currentSpellScript;
    private float lastCastedTimestamp;
    private IEnumerator castCoroutine;

    private Slider healthBar;
    private Text healthBarNum;

    void Start()
    {
        if(!photonView.isMine)
            Destroy(this); // remove this component if not mine

        movementScript = transform.GetComponent<PlayerMovement>();
        terrain = GameObject.FindGameObjectWithTag("Terrain").transform;
        SetSpell(currentSpellPrefab);

        gameManager = GameObject.FindWithTag("GameController").GetComponent<NetworkGameManager>();

        healthBar = GameObject.Find("Health Bar").GetComponent<Slider>();
        health = maxHealth;

        healthBarNum = healthBar.GetComponentInChildren<Text>();

        gameManager.GetSpectatorUI().SetActive(false);
    }

    void Update()
    {
        if (!photonView.isMine)
            return;

        if (Input.GetMouseButtonDown(0) && Time.time - lastCastedTimestamp >= currentSpellScript.castInterval 
            && movementScript.GetState() != PlayerState.CASTING && movementScript.GetState() != PlayerState.STUNNED)
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

        healthBar.value = Mathf.Lerp(healthBar.value, health / maxHealth, Time.deltaTime * 5f);
        healthBarNum.text = (Convert.ToInt32(healthBar.value * 100)).ToString().Aggregate(string.Empty, (c, i) => c + i + ' ') 
            + "/ " + maxHealth.ToString().Aggregate(string.Empty, (c, i) => c + i + ' ');
        if (healthBar.value < 1 / 100f)
            Die();
    }

    private IEnumerator CastWithDelay(float time, Vector3 aimPos, Vector3 aimDir)
    {
        yield return new WaitForSeconds(time);
        lastCastedTimestamp = Time.time;

        PhotonNetwork.Instantiate("Spells/" + currentSpellPrefab.name, transform.position + aimDir*2, Quaternion.LookRotation(aimDir, Vector3.up), 0);
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
        movementScript.Stun(dmg / 20f);
    }

    private void Die()
    {
        gameManager.GetPlayingUI().SetActive(false);
        gameManager.GetSpectatorUI().SetActive(true);

        gameManager.GetComponent<PhotonView>().RPC("OnPlayerDeath", PhotonTargets.All, photonView.viewID);

        PhotonNetwork.Destroy(this.photonView);
    }

    public void CancelCast()
    {
        if(castCoroutine != null)
            StopCoroutine(castCoroutine);
        movementScript.CancelCast();
    }
}
