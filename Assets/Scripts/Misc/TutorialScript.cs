using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour
{
    private Vector2 newSize;
    private Vector2 oldSize;
    private bool isLarge = false;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        oldSize = rectTransform.sizeDelta;
        newSize = new Vector2(Screen.width - 60, Screen.height - 40);
        print(isLarge);
    }

    void Update()
    {
        if (isLarge)
        {
            rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, newSize, Time.deltaTime * 2);
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0f, 0f, 0f), Time.deltaTime * 5);
        }
        else
        {
            rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, oldSize, Time.deltaTime * 2);
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0f, -20f, 0f), Time.deltaTime * 5);
        }
    }

    public void ImageSize()
    {
        if (isLarge)
            isLarge = false;
        else
            isLarge = true;
    }
}