using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellScript : MonoBehaviour {
    public float speed = 3f;
    public float damage = 20;
    public float castTime = 0.3f;
    public float castInterval = 1f;


    // @TODO 

    void Start()
    {

    }

    void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }


    void OnTriggerEnter(Collider c)
    {
        // @TODO when networked:
        // if tag == Player and isMine, do nothing


        if (c.tag == "Player")
        {
            c.GetComponent<PlayerScript>().TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (c.tag == "Spell")
        {
            Destroy(this);
            Destroy(c.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetCastTime()
    {
        return castTime;
    }

}
