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

    private string[] spellName = new string[maxSpells];
    private float[] spellCooldown = new float[maxSpells];
    private int indexSpellSelected;

    void LinkSpellButtons()
    {
        for (int i = 0; i < maxSpells; i++)
        {
            spellSelectButtons[i] = GameObject.Find("Spell" + i.ToString()).GetComponent<Button>();
            spellSelectCooldownText[i] = spellSelectButtons[i].transform.FindChild("Cooldown").GetComponent<Text>();
            spellSelectButtons[i].onClick.AddListener( () => {SpellButtonClicked();} );
        }
    }

    void UpdateSpellCooldowns()
    {
        for (int i = 0; i < maxSpells; i++)
        {
            spellCooldown[i] -= Time.deltaTime;

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
    }

    private void SpellButtonClicked()
    {
        int index = int.Parse(EventSystem.current.currentSelectedGameObject.name.Substring("Spell".Length));

        if (spellName[index] != "" && spellCooldown[index] <= 0)
        {
            SetSpell(spellName[index]);
        }

        indexSpellSelected = index;

        if (index == 0 && spellCooldown[index] <= 0)
        {
            SetSpell("Spell Fireball");
        }
        else
        {
            SetSpell(null);
        }
    }
}
