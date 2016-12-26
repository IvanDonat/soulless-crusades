using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerScript : Photon.PunBehaviour {
    // mostly controlled by the main PlayerScript.cs

    private const int maxSpells = 6;
    private Button[] spellSelectButtons = new Button[maxSpells];
    private string[] spellName = new string[maxSpells];

    void LinkSpellButtons()
    {
        for (int i = 0; i < maxSpells; i++)
        {
            spellSelectButtons[i] = GameObject.Find("Spell" + i.ToString()).GetComponent<Button>();
            spellSelectButtons[i].onClick.AddListener( () => {SpellButtonClicked();} );
        }
    }

    private void SpellButtonClicked()
    {
        int index = int.Parse(EventSystem.current.currentSelectedGameObject.name.Substring("Spell".Length));

        if (spellName[index] != "")
        {
            SetSpell(spellName[index]);
        }

        if (index == 0)
        {
            SetSpell("Spell Fireball");
        }
        else
        {
            SetSpell(null);
        }
    }
}
