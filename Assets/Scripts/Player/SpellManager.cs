using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerScript : Photon.PunBehaviour {
    // mostly controlled by the main PlayerScript.cs

    private const int maxSpells = 6;

    private Button[] spellSelectButtons = new Button[maxSpells];
    private Text[] spellSelectCooldownText = new Text[maxSpells];
    private Text[] spellSelectNameText = new Text[maxSpells];

    private string[] spellName = new string[maxSpells];
    private float[] spellCooldown = new float[maxSpells];
    private int indexSpellSelected;

    void LinkSpellButtons()
    {
        for (int i = 0; i < maxSpells; i++)
        {
            spellSelectButtons[i] = GameObject.Find("Spell" + i.ToString()).GetComponent<Button>();
            spellSelectCooldownText[i] = spellSelectButtons[i].transform.FindChild("Cooldown").GetComponent<Text>();
            spellSelectNameText[i] = spellSelectButtons[i].transform.FindChild("Text").GetComponent<Text>();
            spellSelectButtons[i].onClick.AddListener( delegate{ SpellButtonClicked(); } );
        }


        // TEMPORARY
        spellName[0] = "Fireball";
        spellName[1] = "Teleport";
    }

    void UpdateSpells()
    {
        for (int i = 0; i < maxSpells; i++)
        {
            spellCooldown[i] -= Time.deltaTime;

            spellSelectNameText[i].text = spellName[i];

            if (spellCooldown[i] <= 0)
            {
                spellSelectCooldownText[i].gameObject.SetActive(false);
            }
            else
            {
                spellSelectCooldownText[i].gameObject.SetActive(true);
                spellSelectCooldownText[i].text = spellCooldown[i].ToString("F1");
            }
        }

        foreach (Button b in spellSelectButtons)
            b.GetComponent<Image>().color = Color.white;

        if (currentSpellName != null)
            spellSelectButtons[indexSpellSelected].GetComponent<Image>().color = Color.red;

        if (Input.GetKeyDown(KeyCode.Q))
            SpellButtonClicked(0);
        if (Input.GetKeyDown(KeyCode.W))
            SpellButtonClicked(1);
        if (Input.GetKeyDown(KeyCode.E))
            SpellButtonClicked(2);
        
        if (Input.GetKeyDown(KeyCode.A))
            SpellButtonClicked(3);
        if (Input.GetKeyDown(KeyCode.S))
            SpellButtonClicked(4);
        if (Input.GetKeyDown(KeyCode.D))
            SpellButtonClicked(5);
    }

    private void SpellButtonClicked(int index)
    {
        if (spellName[index] != "" && spellCooldown[index] <= 0)
        {
            SetSpell(spellName[index]);
        }

        indexSpellSelected = index;
    }

    private void SpellButtonClicked()
    {
        int index = int.Parse(EventSystem.current.currentSelectedGameObject.name.Substring("Spell".Length));
        SpellButtonClicked(index);
    }
}
