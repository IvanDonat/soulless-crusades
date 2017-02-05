using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsScroll : MonoBehaviour
{

    private Scrollbar scroll;

	void Start ()
    {
        scroll = gameObject.GetComponent<Scrollbar>();

    }
	
	void Update ()
    {
        scroll.value -= 0.1f * Time.deltaTime;

        if (scroll.value == 0f)
            scroll.value = 1f;
	}
}
