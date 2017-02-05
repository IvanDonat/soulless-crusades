using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulScript : MonoBehaviour {

    public Vector3 goToPos;
    private float distance;
    public GameObject soulExplosion;

	void Start () {
        soulExplosion.SetActive(false);
        transform.position = new Vector3(Random.Range(50f, 100f), Random.Range(10f, 20f), Random.Range(50f, 100f));
        distance = (goToPos - transform.position).magnitude;
	}
	
	void Update () {
        transform.position = Vector3.Lerp(transform.position, goToPos, Time.deltaTime);

        if (distance < 5f)
        {
            gameObject.GetComponent<ParticleSystem>().Stop();
            soulExplosion.SetActive(true);
        }
	}
}
