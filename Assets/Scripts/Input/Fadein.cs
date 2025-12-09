using UnityEngine;

public class FadeInOnStart : MonoBehaviour
{
    public float fadeDuration = 1.5f;  // 페이드 시간

    private CanvasGroup cg;
    private float timer = 0f;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 1f;  // 처음엔 완전 검정
    }

    void Update()
    {
        if (cg.alpha <= 0f) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / fadeDuration);
        cg.alpha = 1f - t;   // 1 → 0 으로 줄어듦
    }
}
