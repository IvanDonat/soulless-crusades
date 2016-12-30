using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISoundScript : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler {
    // add this script to each UI object that should make sound fx

    //private AudioSource hoverSound;
    private AudioSource clickSound;

    void Start()
    {
        //hoverSound = GameObject.Find("Hover Sound").GetComponent<AudioSource>();
        clickSound = GameObject.Find("Click Sound").GetComponent<AudioSource>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //hoverSound.Play();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        clickSound.Play();
    }
}
