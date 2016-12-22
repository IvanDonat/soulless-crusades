using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour {
    public float lifetimeInSeconds = 2f;

    void Update()
    {
        lifetimeInSeconds -= Time.deltaTime;

        if (lifetimeInSeconds <= 0)
        {
            Destroy(gameObject);
        }
    }
}
