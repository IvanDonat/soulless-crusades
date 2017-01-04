using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScript : MonoBehaviour 
{
    public Image logo;
    public AudioSource sound;

    void Start()
    {
        logo.enabled = false;

        StartCoroutine(ShowLogo());
    }

    private IEnumerator ShowLogo()
    {
        yield return new WaitForSeconds(1.5f);
        sound.Play();
        logo.enabled = true;

        StartCoroutine(FadeLogo());
    }

    private IEnumerator FadeLogo()
    {
        while (logo.color.a > 0f)
        {
            Color c = logo.color;
            c.a -= 0.002f;
            logo.color = c;

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.45f);
        SceneManager.LoadScene("Menu");
    }
}
