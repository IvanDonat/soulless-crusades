using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulScript : MonoBehaviour 
{
    
    public Vector3 goToPos;
    public GameObject soulExplosion;
    
    void Start () 
    {
        soulExplosion.SetActive(false);
        transform.position = new Vector3(Random.Range(50f, 100f), Random.Range(10f, 20f), Random.Range(50f, 100f));
    }
    
    void Update () 
    {
        transform.position = Vector3.Lerp(transform.position, goToPos, Time.deltaTime);

        if ((goToPos - transform.position).magnitude < .5f)
        {
            gameObject.GetComponent<ParticleSystem>().Stop();
            soulExplosion.SetActive(true);
        }
    }
}
