using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionPlayer : MonoBehaviour
{
    [Header("참조")]
    public Image blinkImage;                    // 화면 덮는 UI 이미지

    [Header("타이밍")]
    public float transitionDuration = 3f;       // 전체 연출 시간

    [Header("씬 설정")]
    public string nextSceneName;               // 이동할 씬 이름

    bool isPlaying = false;

    public void PlayTransitionAndLoad()
    {
        if (isPlaying) return;
        StartCoroutine(CoPlayTransitionAndLoad());
    }

    IEnumerator CoPlayTransitionAndLoad()
    {
        isPlaying = true;

        // 화면 덮는 이미지 색
        Color c = blinkImage.color;
        c.a = 0f;
        blinkImage.color = c;

        int blinkCount = 2;
        float fadeDuration = 0.15f;
        float closedHold = 0.05f;
        float betweenBlinks = 0.1f;

        for (int i = 0; i < blinkCount; i++)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Clamp01(t / fadeDuration);  // 0 → 1
                c.a = alpha;
                blinkImage.color = c;
                yield return null;
            }

            yield return new WaitForSeconds(closedHold);

            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(t / fadeDuration); // 1 → 0
                c.a = alpha;
                blinkImage.color = c;
                yield return null;
            }

            if (i < blinkCount - 1)
                yield return new WaitForSeconds(betweenBlinks);
        }

        yield return new WaitForSeconds(transitionDuration - 0.8f);

        SceneManager.LoadScene(nextSceneName);

        isPlaying = false;
    }
}
