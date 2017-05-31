using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockScript : MonoBehaviour 
{
    private float triggerSink = 76f;
    private float defaultY = -1.5f;
    private float speed = 60f;

    void Update()
    {
        if (transform.position.y > defaultY)
            transform.Translate(Vector3.down * Time.deltaTime * speed);
        if (transform.position.y < defaultY)
            transform.position = new Vector3(transform.position.x, defaultY, transform.position.z);
        if (NetworkGameManager.GetRoundTime() >= triggerSink && transform.position.z != 0)
        {
            defaultY = -6f;
            speed = 0.5f;
        }
    }
}
