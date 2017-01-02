using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockScript : MonoBehaviour 
{
    void Update()
    {
        if (transform.position.y > 0f)
            transform.Translate(Vector3.down * Time.deltaTime * 60f);
        if (transform.position.y < 0)
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }
}
