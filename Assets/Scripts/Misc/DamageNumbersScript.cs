using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageNumbersScript : Photon.PunBehaviour 
{
    private Text text;

    private float time = 0f;
    private float lifetime = 1f;

    private float speed = 1f;

    void Awake()
    {
        text = gameObject.GetComponentInChildren<Text>();
    }

    void Update()
    {
        time += Time.deltaTime;

        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.World);

        text.color = new Color(text.color.r, text.color.g, text.color.b, 
            Mathf.Lerp(1, 0, time / lifetime));

        if (time > lifetime)
            Destroy(gameObject);
    }

    [PunRPC]
    public void RpcSetText(int dmg)
    {
        if (dmg == 0)
            Destroy(gameObject);
        else if (dmg > 0)
        {
            text.text = dmg.ToString("D2");
            text.color = Color.red;
        }
        else
        {
            dmg = -dmg; // negative amounts will mean healing, swap to positive
            text.text = dmg.ToString("D2");
            text.color = Color.green;
        }
    }
}
