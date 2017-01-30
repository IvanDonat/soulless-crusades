using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public partial class PlayerScript : Photon.PunBehaviour
{
    // mostly controlled by the main PlayerScript.cs

    private const int maxSpells = 6;

    private Button[] spellSelectButtons = new Button[maxSpells];
    private Text[] spellSelectCooldownText = new Text[maxSpells];
    private Text[] spellSelectNameText = new Text[maxSpells];

    private static string[] spellName = new string[maxSpells];
    private float[] spellCooldown = new float[maxSpells];
    private int indexSpellSelected;

    private Dictionary<Button, string> buttonToSpell = new Dictionary<Button, string>();

    void LinkSpellButtons()
    {
        Sprite[] allIcons = Resources.LoadAll<Sprite>("Spells");

        for (int i = 0; i < maxSpells; i++)
        {
            spellSelectButtons[i] = GameObject.Find("Spell" + i.ToString()).GetComponent<Button>();
            buttonToSpell[spellSelectButtons[i]] = spellName[i];

            spellSelectCooldownText[i] = spellSelectButtons[i].transform.FindChild("Cooldown").GetComponent<Text>();

            spellSelectNameText[i] = spellSelectButtons[i].transform.FindChild("Text").GetComponent<Text>();
            spellSelectNameText[i].text = spellName[i];

            Image icon = spellSelectButtons[i].transform.FindChild("Icon").GetComponent<Image>();
            icon.enabled = false;
            foreach (Sprite s in allIcons)
            {
                if (s.name == spellName[i])
                {
                    icon.sprite = s;
                    icon.enabled = true;
                    break;
                }
            }

            spellSelectButtons[i].onClick.AddListener(delegate{SpellButtonClicked();});
        }
    }

    void UpdateSpells()
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
                if (spellCooldown[i] < 10)
                    spellSelectCooldownText[i].text = spellCooldown[i].ToString("F1");
                else
                    spellSelectCooldownText[i].text = ((int)spellCooldown[i]).ToString();
            }
        }

        if (SpellSelectScript.currentlyHoveredButton != null)
        { // buttons have the SpellSelectItemScript.cs so this works
            gameManager.tooltipParent.SetActive(true);
            Text tooltipName = gameManager.tooltipName;
            Text tooltipDescription = gameManager.tooltipDescription;

            string name = buttonToSpell[SpellSelectScript.currentlyHoveredButton];
            GameObject spellGO = Resources.Load<GameObject>("Spells/" + name);
            Spell s = spellGO.GetComponent<Spell>();
            tooltipName.text = name;
            tooltipDescription.text = SpellSelectScript.GetTooltipText(s);
        }
        else
            gameManager.tooltipParent.SetActive(false);

        foreach (Button b in spellSelectButtons)
            b.GetComponent<Image>().color = Color.white;

        if (currentSpellName != null)
            spellSelectButtons[indexSpellSelected].GetComponent<Image>().color = Color.red;
        
        if (gameManager.IsChatOpen())
            return;

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
        if (Input.GetMouseButton(1) || movementScript.GetState() == PlayerState.CASTING || movementScript.GetState() == PlayerState.STUNNED)
        {
            audioAccessDenied.Play();
            return;
        }

        if (currentSpellName != null && indexSpellSelected == index)
        {
            StartCastingSpell();
            return;
        }

        if (spellName[index] != "")
        {
            if (spellCooldown[index] > 0)
            {
                audioAccessDenied.Play();
                return;
            }

            SetSpell(spellName[index]);
            indexSpellSelected = index;
        }
    }

    private void SpellButtonClicked()
    {
        int index = int.Parse(EventSystem.current.currentSelectedGameObject.name.Substring("Spell".Length));
        SpellButtonClicked(index);
    }

    public static void SetSpells(string[] list)
    {
        spellName = list;
    }
}
