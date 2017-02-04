using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopAnimation : MonoBehaviour 
{
    public Animation anim;
    public string clipName;

    void Update()
    {
        anim.CrossFade(clipName, 0f);
    }
}
