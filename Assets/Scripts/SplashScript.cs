using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScript : MonoBehaviour 
{
    public Image logo;
    public AudioSource sound;

    private bool shown = false;

    void Start()
    {
        logo.enabled = false;
        StartCoroutine(ShowLogo());
    }

    void Update()
    {
        if (logo.color.a <= 0.001f || Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Space))
            SceneManager.LoadScene("Menu");

        if (!shown)
            return;

        Color c = logo.color;
        c.a -= .3f * Time.deltaTime;
        logo.color = c;
    }

    private IEnumerator ShowLogo()
    {
        yield return new WaitForSeconds(.15f);
        sound.Play();
        logo.enabled = true;
        shown = true;
    }
}
