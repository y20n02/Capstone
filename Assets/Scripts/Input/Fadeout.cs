using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIOutroFader : MonoBehaviour
{
    public static UIOutroFader Instance;

    public CanvasGroup fadeCanvas; // CanvasGroup 연결

    void Awake()
    {
        Instance = this;

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 0f;
            fadeCanvas.interactable = false;
            fadeCanvas.blocksRaycasts = false;
        }
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(FadeRoutine(duration));
    }

    IEnumerator FadeRoutine(float duration)
    {
        float t = 0;
        fadeCanvas.interactable = true;
        fadeCanvas.blocksRaycasts = true;

        while (t < duration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }

        fadeCanvas.alpha = 1f;
    }
}
