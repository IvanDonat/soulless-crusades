using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpellSelectScript : MonoBehaviour {
    public Transform selectedSpellsParent;
    public Transform allSpellsParent;

    public Transform spellSelectButtonPrefab;

    private List<Button> selectedSpellButtons = new List<Button>();
    private List<Button> allSpellButtons = new List<Button>();

    void Start()
    {
        GameObject[] allSpells = Resources.LoadAll<GameObject>("Spells");
        foreach (var obj in allSpells)
        {
            Transform buttonTransform = (Transform) Instantiate(spellSelectButtonPrefab);
            buttonTransform.name = obj.name;
            buttonTransform.SetParent(allSpellsParent, false);

            Button button = buttonTransform.GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = obj.name;
            allSpellButtons.Add(button);

            button.onClick.AddListener(() => ClickedButtonSelectSpell());
        }
    }

    private void ClickedButtonSelectSpell()
    {
        if (selectedSpellButtons.Count >= 6)
            return;

        Button buttonClicked = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        EventSystem.current.SetSelectedGameObject(null);

        selectedSpellButtons.Add(buttonClicked);
        allSpellButtons.Remove(buttonClicked);
        buttonClicked.transform.SetParent(selectedSpellsParent, false);

        buttonClicked.onClick.RemoveAllListeners();
        buttonClicked.onClick.AddListener(() => ClickButtonDeselectSpell());

        UpdateSpellManagerSpells();
    }

    private void ClickButtonDeselectSpell()
    {
        Button buttonClicked = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        EventSystem.current.SetSelectedGameObject(null);

        allSpellButtons.Add(buttonClicked);
        selectedSpellButtons.Remove(buttonClicked);
        buttonClicked.transform.SetParent(allSpellsParent, false);

        buttonClicked.onClick.RemoveAllListeners();
        buttonClicked.onClick.AddListener(() => ClickedButtonSelectSpell());

        UpdateSpellManagerSpells();
    }

    private void UpdateSpellManagerSpells()
    {
        string[] list = new string[6];

        int index = 0;
        foreach (Button b in selectedSpellButtons)
        { 
            list[index] = b.transform.name;
            index++;
        }

        PlayerScript.SetSpells(list);
    }
}
