using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpellSelectScript : MonoBehaviour
{
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

            if (s is HealSpell)
            {
                tooltipDescription.text += "Heal amount: " + (s as HealSpell).healAmount + '\n';
            }
            else if (s is ProjectileSpell)
            {
                tooltipDescription.text += "Damage: " + (s as ProjectileSpell).damage + '\n';
                tooltipDescription.text += "Knockback: " + (s as ProjectileSpell).knockbackForce + '\n';
                tooltipDescription.text += "Stun time: " + (s as ProjectileSpell).stunTime + " s\n";
            }
            else if (s is ShieldSpell)
            {
                tooltipDescription.text += "Shield time: " + (s as ShieldSpell).shieldTime + " s\n";
            }
            else if (s is SpikeSpell)
            {
                tooltipDescription.text += "Spike lifetime: " + (s as SpikeSpell).duration + " s\n";
                tooltipDescription.text += "Slowdown duration: " + (s as SpikeSpell).slowdownTime + " s\n";
            }
            else if (s is MeteorSpell)
            {
                tooltipDescription.text += "Damage: " + (s as MeteorSpell).damage + '\n';
                tooltipDescription.text += "Knockback: " + (s as MeteorSpell).knockbackForce + '\n';
                tooltipDescription.text += "Stun time: " + (s as MeteorSpell).stunTime + " s\n";
            }
            else if (s is InvisibilitySpell)
            {
                tooltipDescription.text += "Duration: " + (s as InvisibilitySpell).cloakTime + " s\n";
            }
            else if (s is BlindSpell)
            {
                tooltipDescription.text += "Duration: " + (s as BlindSpell).blindTime + " s\n";
            }

            int numLines = tooltipDescription.text.Split('\n').Length - 1;
            tooltipCanvas.sizeDelta = new Vector2(tooltipCanvas.sizeDelta.x, 90 + 65 * numLines);
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
        if (selectedSpellButtons.Count < 6)
        {
            closeButton.interactable = false;
            if (selectedSpellButtons.Count != 5)
                errorText.text = string.Format("You have to select {0} more spells to continue!", 
                    6 - selectedSpellButtons.Count);
            else
                errorText.text = string.Format("You have to select 1 more spell to continue!");
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
