using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Fader : MonoBehaviour
{
    //This script is used to fade in and out a RawImage -> it's used for the black panel that covers the screen when a cutscene is played or scene is loaded
    //Two ways to use this script:
    //1. Use StartCoroutine(FadeInCo()) or FadeOutCo() and wait until the coroutine equals null <- looks like this will not work and i dont know why
    //2. Subscribe to the OnFadeInComplete or OnFadeOutComplete events

    public RawImage fadeImage;

    public float fadeSpeed = 0f;
    public float fadeMaxSpeed = 1f;
    public float fadeTime = 1f;
    public float fadeDelay = 1f;
    [SerializeField]
    private float fadeAlpha = 1f;

    public delegate void FadeInComplete();
    public event FadeInComplete OnFadeInComplete;

    public delegate void FadeOutComplete();
    public event FadeOutComplete OnFadeOutComplete;

    private void Start()
    {
        fadeImage = GetComponent<RawImage>();
        ChangeAlpha();
    }

    private void ChangeAlpha()
    {
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, fadeAlpha);
    }

    public float FadeAlpha
    {
        get { return fadeAlpha; }
    }

    public void FadeIn()
    {
        StartCoroutine(FadeInCo());
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutCo());
    }

    public IEnumerator FadeOutCo()
    {
        yield return new WaitForSeconds(fadeDelay);
        fadeImage.raycastTarget = true;
        while (fadeAlpha > 0.09)
        {
            fadeAlpha = Mathf.SmoothDamp(fadeAlpha, 0f, ref fadeSpeed, fadeTime, fadeMaxSpeed, Time.unscaledDeltaTime);

            ChangeAlpha();
            yield return new WaitForEndOfFrame();
        }
        fadeAlpha = 0f;
        ChangeAlpha();
        fadeImage.raycastTarget = false;
        OnFadeOutComplete?.Invoke();
    }

    public IEnumerator FadeInCo()
    {
        yield return new WaitForSeconds(fadeDelay);
        fadeImage.raycastTarget = true;
        while (fadeAlpha < 0.91)
        {
            fadeAlpha = Mathf.SmoothDamp(fadeAlpha, 1f, ref fadeSpeed, fadeTime, fadeMaxSpeed, Time.unscaledDeltaTime);
            ChangeAlpha();
            yield return null;
        }
        fadeAlpha = 1f;
        ChangeAlpha();
        OnFadeInComplete?.Invoke();
    }
}
