using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaScript : MonoBehaviour {
    private MeshRenderer rend;
    private Vector2 offset = Vector2.zero;

    void Start()
    {
        rend = GetComponent<MeshRenderer>();
    }

    void LateUpdate()
    {
        offset = Vector2.up/2f + (Vector2.up + Vector2.right) * Mathf.Sin(Time.time / 2f) / 75f;

        rend.material.SetTextureOffset("_MainTex", offset);
        rend.material.SetTextureOffset("_BumpMap", offset);
    }
}
