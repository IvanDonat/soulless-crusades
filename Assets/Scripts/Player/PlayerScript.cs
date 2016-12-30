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

    private string currentSpellName;
    private Spell currentSpellScript;
    private IEnumerator castCoroutine;

    public Slider healthBar3D;
    private Slider healthBar;
    private Text healthBarNum;

    private Slider castingBar;

    public Transform spawnParticles;

    public Texture2D defaultCursor;
    public Texture2D castCursor;

    [NonSerialized]
    public float shieldTimeLeft = 0f;

    public AudioSource audioSpellSelect;

    public Text nameBar;

    void Start()
    {
        Instantiate(spawnParticles, transform.position, Quaternion.identity);

        nameBar.text = photonView.owner.NickName;

        if(!photonView.isMine)
        {
            Destroy(this); // remove this component if not mine
            return;
        }

        movementScript = transform.GetComponent<PlayerMovement>();
        terrain = GameObject.FindGameObjectWithTag("Terrain").transform;

        gameManager = GameObject.FindWithTag("GameController").GetComponent<NetworkGameManager>();

        healthBar = GameObject.Find("Health Bar").GetComponent<Slider>();
        healthBarNum = healthBar.GetComponentInChildren<Text>();
        health = maxHealth;

        castingBar = gameManager.castingBar;
        castingBar.gameObject.SetActive(false);

        gameManager.GetSpectatorUI().SetActive(false);

        PlayerProperties.SetProperty(PlayerProperties.ALIVE, true);

        LinkSpellButtons();
    }

    void Update()
    {
        if (!photonView.isMine)
            return;

        UpdateSpells();
        PlayerProperties.SetProperty(PlayerProperties.HEALTH, (int) health);

        if (Input.GetKeyDown(KeyCode.Escape))
            SetSpell(null);

        healthBar.value = Mathf.Lerp(healthBar.value, health / maxHealth, Time.deltaTime * 5f);
        //healthBar3D.value = Mathf.Lerp(healthBar3D.value, (int)PhotonNetwork.player.CustomProperties[PlayerProperties.HEALTH] / (float)maxHealth, Time.deltaTime * 5f);
        healthBarNum.text = (Convert.ToInt32(healthBar.value * 100)).ToString().Aggregate(string.Empty, (c, i) => c + i + ' ') 
            + "/ " + maxHealth.ToString().Aggregate(string.Empty, (c, i) => c + i + ' ');
        if (healthBar.value < 1 / 100f)
            Die(false);

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
                aimPos -= Camera.main.transform.forward * (transform.position.y - aimPos.y); // move towards camera by a bit
                aimPos.y = transform.position.y;

                Vector3 aimDir = aimPos - transform.position;
                aimDir.Normalize();

                if(castCoroutine != null)
                    CancelCast();
                castCoroutine = CastWithDelay(currentSpellName, indexSpellSelected, currentSpellScript.GetCooldown(), currentSpellScript.GetCastTime(), hit.point, aimPos, aimDir);
                StartCoroutine(castCoroutine);

                movementScript.CastSpell(currentSpellScript.GetCastTime(), aimPos);

                SetSpell(null);
            }
        }

        if (shieldTimeLeft >= 0f)
            shieldTimeLeft -= Time.deltaTime;
    }

    private IEnumerator CastWithDelay(string spell, int spellIndex, float cooldown, float time, Vector3 mousePos, Vector3 aimPos, Vector3 aimDir)
    {
        float timePassed = 0f;
        if(time > 0.1f)
            castingBar.gameObject.SetActive(true);
        
        while (timePassed <= time)
        {
            castingBar.value = timePassed / time;
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        castingBar.gameObject.SetActive(false);
        spellCooldown[spellIndex] = cooldown;
        GameObject spellGO = (GameObject) PhotonNetwork.Instantiate("Spells/" + spell, transform.position + aimDir*2, Quaternion.LookRotation(aimDir, Vector3.up), 0);
        Spell spellScript = spellGO.GetComponent<Spell>();
        spellScript.SetParams(transform, mousePos);
    }

    public void SetSpell(string spellName)
    {
        currentSpellName = spellName;
        if (spellName != null)
        {
            GameObject spellGO = (GameObject) Resources.Load("Spells/" + spellName);
            currentSpellScript = spellGO.GetComponent<Spell>();
            Cursor.SetCursor(castCursor, Vector2.zero, CursorMode.Auto);
            audioSpellSelect.Play();
        }
        else
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }
    }

    public string GetSpell()
    {
        return currentSpellName;
    }

    [PunRPC]
    public void TakeDamage(PhotonPlayer dmgDealer, float dmg, float stunTime)
    {
        if (shieldTimeLeft >= 0f)
            return;

        health -= dmg;

        if (dmgDealer != null)
        { // null in case of lava
            lastDamageDealer = dmgDealer;
            movementScript.Stun(stunTime);
            CancelCast();
        }
    }

    [PunRPC]
    public void DoKnockback(Vector3 dir, float force, float dragResetTime)
    {
        if (shieldTimeLeft >= 0f)
            return;

        dir.y = 0;
        dir.Normalize();

        float dragDropTo = 2f * gameManager.GetDragScalar();

        movementScript.GetRigidbody().velocity += dir * force;
        movementScript.SetDrag(dragDropTo, dragResetTime);
    }

    [PunRPC]
    public void Heal(float amount)
    {
        health += amount;
        health = Mathf.Clamp(health, 0, maxHealth);
    }
    
    public void Die(bool isGameOver)
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        
        PlayerProperties.SetProperty(PlayerProperties.ALIVE, false);
        
        gameManager.GetPlayingUI().SetActive(false);
        gameManager.GetSpectatorUI().SetActive(true);
        
        if(!isGameOver)
        {
            gameManager.GetComponent<PhotonView>().RPC("OnPlayerDeath", PhotonTargets.All, photonView.owner);
            PlayerProperties.IncrementProperty(PlayerProperties.DEATHS);

            if (lastDamageDealer != null)
                gameManager.GetComponent<PhotonView>().RPC("GotKill", lastDamageDealer, photonView.owner);
        }

        PhotonNetwork.Destroy(this.photonView);
    }

    public void CancelCast()
    {
        castingBar.gameObject.SetActive(false);
        if(castCoroutine != null)
            StopCoroutine(castCoroutine);
        movementScript.CancelCast();
    }
}
