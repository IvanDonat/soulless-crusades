using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SpellSelectItemScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private Button b;

    void Awake()
    {
        b = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SpellSelectScript.currentlyHoveredButton = b;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SpellSelectScript.currentlyHoveredButton == b)
            SpellSelectScript.currentlyHoveredButton = null;
    }
}
