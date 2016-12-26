using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class PlayerScript : Photon.PunBehaviour {
    private NetworkGameManager gameManager;
    private PlayerMovement movementScript;
    private Transform terrain;

    private float maxHealth = 100;
    private float health;
    private PhotonPlayer lastDamageDealer;

    // currently supports one spell, @TODO multiple spells support
    private string currentSpellName;
    private SpellScript currentSpellScript;
    private IEnumerator castCoroutine;

    private Slider healthBar;
    private Text healthBarNum;

    public Texture2D defaultCursor;
    public Texture2D castCursor;

    void Start()
    {
        if(!photonView.isMine)
            Destroy(this); // remove this component if not mine

        movementScript = transform.GetComponent<PlayerMovement>();
        terrain = GameObject.FindGameObjectWithTag("Terrain").transform;

        gameManager = GameObject.FindWithTag("GameController").GetComponent<NetworkGameManager>();

        healthBar = GameObject.Find("Health Bar").GetComponent<Slider>();
        healthBarNum = healthBar.GetComponentInChildren<Text>();
        health = maxHealth;

        gameManager.GetSpectatorUI().SetActive(false);

        LinkSpellButtons();
    }

    void Update()
    {
        if (!photonView.isMine)
            return;

        UpdateSpells();

        if (Input.GetKeyDown(KeyCode.Escape))
            SetSpell(null);

        healthBar.value = Mathf.Lerp(healthBar.value, health / maxHealth, Time.deltaTime * 5f);
        healthBarNum.text = (Convert.ToInt32(healthBar.value * 100)).ToString().Aggregate(string.Empty, (c, i) => c + i + ' ') 
            + "/ " + maxHealth.ToString().Aggregate(string.Empty, (c, i) => c + i + ' ');
        if (healthBar.value < 1 / 100f)
            Die();

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (currentSpellName != null && Input.GetMouseButtonDown(0) && spellCooldown[indexSpellSelected] <= 0
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
                castCoroutine = CastWithDelay(currentSpellName, indexSpellSelected, currentSpellScript.GetCooldown(), currentSpellScript.GetCastTime(), aimPos, aimDir);
                StartCoroutine(castCoroutine);

                movementScript.CastSpell(currentSpellScript.GetCastTime(), aimPos);

                SetSpell(null);
            }
        }
    }

    private IEnumerator CastWithDelay(string spell, int spellIndex, float cooldown, float time, Vector3 aimPos, Vector3 aimDir)
    {
        yield return new WaitForSeconds(time);
        spellCooldown[indexSpellSelected] = cooldown;
        PhotonNetwork.Instantiate("Spells/" + spell, transform.position + aimDir*2, Quaternion.LookRotation(aimDir, Vector3.up), 0);
    }

    public void SetSpell(string spellName)
    {
        currentSpellName = spellName;
        if (spellName != null)
        {
            GameObject spellGO = (GameObject) Resources.Load("Spells/" + spellName);
            currentSpellScript = spellGO.GetComponent<SpellScript>();
            Cursor.SetCursor(castCursor, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }
    }

    [PunRPC]
    public void TakeDamage(PhotonPlayer dmgDealer, float dmg)
    {
        health -= dmg;
        CancelCast();
        movementScript.Stun(dmg / 20f);

        if (dmgDealer != null)
        { // null in case of lava
            lastDamageDealer = dmgDealer;
        }
    }

    private void Die()
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        
        if (lastDamageDealer != null)
        {
            gameManager.GetComponent<PhotonView>().RPC("GotKill", lastDamageDealer, photonView.owner);
        }

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
