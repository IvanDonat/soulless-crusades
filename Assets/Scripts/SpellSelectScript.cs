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

    private Dictionary<Button, Spell> buttonToSpell = new Dictionary<Button, Spell>();

    public static Button currentlyHoveredButton;
    public Button closeButton;

    public RectTransform tooltipCanvas;
    public Text tooltipName;
    public Text tooltipDescription;
    public Text errorText;

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

            buttonToSpell[button] = obj.GetComponent<Spell>();

            button.onClick.AddListener(() => ClickedButtonSelectSpell());
        }
        CheckSpellNumber();
    }

    void Update()
    {
        if (currentlyHoveredButton != null)
        {
            tooltipCanvas.gameObject.SetActive(true);

            Vector3 mpos = Input.mousePosition;
            mpos.z = 700f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mpos);
            worldPos += Vector3.right * tooltipCanvas.sizeDelta.x / 2 * tooltipCanvas.localScale.x * 1.1f;
            worldPos += Vector3.down * tooltipCanvas.sizeDelta.y / 2 * tooltipCanvas.localScale.y * 1.1f;
            tooltipCanvas.position = worldPos;

            Spell s = buttonToSpell[currentlyHoveredButton];
            tooltipName.text = s.gameObject.name;

            tooltipDescription.text = "";
            tooltipDescription.text += s.tooltipText + "\n\n";
            tooltipDescription.text += "Cast Time: " + s.castTime + " s\n";
            tooltipDescription.text += "Cooldown: " + s.castInterval + " s\n";
        }
        else
            tooltipCanvas.gameObject.SetActive(false);
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

        CheckSpellNumber();
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

        CheckSpellNumber();
        UpdateSpellManagerSpells();
    }

    private void CheckSpellNumber()
    {
        if (selectedSpellButtons.Count < 5) //6 when available
        {
            closeButton.interactable = false;
            errorText.text = string.Format("You have to select {0} more spell(s) to continue!", 
                5 - selectedSpellButtons.Count);
        }
        else
        {
            closeButton.interactable = true;
            errorText.text = "";
        }
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
