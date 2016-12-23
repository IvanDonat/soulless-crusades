using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAnimation : MonoBehaviour {
    public float speed = 360f;

    void Update()
    {
        transform.Rotate(0, 0, -speed * Time.deltaTime);
    }
}
