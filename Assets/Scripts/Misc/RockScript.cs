using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockScript : MonoBehaviour 
{
    private float defaultY = -1.5f;

    void Update()
    {
        if (transform.position.y > defaultY)
            transform.Translate(Vector3.down * Time.deltaTime * 60f);
        if (transform.position.y < defaultY)
            transform.position = new Vector3(transform.position.x, defaultY, transform.position.z);
    }
}
