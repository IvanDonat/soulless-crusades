using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScript : MonoBehaviour 
{
    public Image logo;
    public AudioSource sound;
    public Text storyIntro;
    public AudioSource storySound;

    private bool shown = false;
    private bool storyShown = false;

    private float storySpeed = .045f;

    void Start()
    {
        logo.enabled = false;
        storyIntro.enabled = false;
        StartCoroutine(ShowStoryIntro());
    }

    void Update()
    {
        if (logo.color.a <= 0.001f || Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Space))
            SceneManager.LoadScene("Menu");

        if (!storyShown)
            return;

        Color ca = storyIntro.color;
        ca.a -= storySpeed * Time.deltaTime;
        storyIntro.color = ca;

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

    private IEnumerator ShowStoryIntro()
    {
        yield return new WaitForSeconds(.15f);
        storySound.Play();
        storyIntro.enabled = true;
        storyShown = true;
        yield return new WaitForSeconds(22f);
        storySpeed = 1f;
        StartCoroutine(ShowLogo());
    }
}
