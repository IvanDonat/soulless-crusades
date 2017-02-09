using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour
{
    private Vector2 largeSize;
    private Vector2 normalSize;
    private bool isLarge = false;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        normalSize = rectTransform.sizeDelta;
    }

    void Update()
    {
        float ratio = normalSize.y / normalSize.x;
        float largeHeight = normalSize.y * 1.4f;
        largeSize = new Vector2(largeHeight / ratio, largeHeight);

        if (isLarge)
        {
            rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, largeSize, Time.deltaTime * 2);
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0f, 0f, 0f), Time.deltaTime * 5);
        }
        else
        {
            rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, normalSize, Time.deltaTime * 2);
            transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(0f, -20f, 0f), Time.deltaTime * 5);
        }
    }

    public void ImageSize()
    {
        isLarge = !isLarge;
    }
}