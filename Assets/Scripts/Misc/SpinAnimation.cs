using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAnimation : MonoBehaviour {

    public float speed = 360f;
    public float radius = 0.5f;

    public GameObject particles;

    Vector3 offset;
    Transform transform;
    Transform particleTransform;

    bool isAnimating;

    void Awake()
    {
        particleTransform = particles.GetComponent<Transform>();
        transform = GetComponent<Transform>();
    }

    void Start()
    {
        StartLoaderAnimation();
    }

    void Update()
    {
        if (isAnimating)
        {
            transform.Rotate(0f, 0f, speed * Time.deltaTime);
            particleTransform.localPosition = Vector3.MoveTowards(particleTransform.localPosition, offset, 0.5f * Time.deltaTime);
        }
    }

    public void StartLoaderAnimation()
    {
        isAnimating = true;
        offset = new Vector3(radius, 0f, 0f);
        particles.SetActive(true);
    }

    public void StopLoaderAnimation()
    {
        particles.SetActive(false);
    }
}
