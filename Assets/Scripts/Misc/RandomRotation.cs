using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotation : MonoBehaviour 
{
    public float speed = 270f;
    
    void Start()
    {
        transform.rotation = Random.rotation;
    }
    
    void Update()
    {
        transform.Rotate (speed * Time.deltaTime, 0, 0, Space.Self);
    }
}
