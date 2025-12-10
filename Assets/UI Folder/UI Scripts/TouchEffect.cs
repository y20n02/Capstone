using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TouchEffect : MonoBehaviour
{
    [Header("Animation Settings")]
    public float fadeDuration = 0.2f; // 나타나고 사라지는 데 걸리는 시간
    public Vector3 startScale = new Vector3(0.5f, 0.5f, 1f); // 시작 크기
    public Vector3 endScale = Vector3.one; // 최종 크기

    private RectTransform rectTransform;
    private Image targetImage;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetImage = GetComponent<Image>();
    }

    // 나타나기 (외부에서 호출)
    public void Show(Vector2 screenPosition)
    {
        rectTransform.position = screenPosition;
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateEffect(true));
    }

    // 위치 업데이트 (터치 유지 중 호출)
    public void Move(Vector2 screenPosition)
    {
        rectTransform.position = screenPosition;
    }

    // 사라지기 (외부에서 호출)
    public void Hide()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(AnimateEffect(false));
    }

    private IEnumerator AnimateEffect(bool show)
    {
        float timer = 0f;
        Vector3 initialScale = show ? startScale : endScale;
        Vector3 finalScale = show ? endScale : startScale;
        float initialAlpha = show ? 0f : targetImage.color.a;
        float finalAlpha = show ? 1f : 0f;

        Color color = targetImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            // 크기 및 알파값 보간
            rectTransform.localScale = Vector3.Lerp(initialScale, finalScale, t);
            color.a = Mathf.Lerp(initialAlpha, finalAlpha, t);
            targetImage.color = color;

            yield return null;
        }

        // 최종 상태 보장
        rectTransform.localScale = finalScale;
        color.a = finalAlpha;
        targetImage.color = color;

        // 사라지는 애니메이션이 끝나면 자신 삭제
        if (!show)
        {
            Destroy(gameObject);
        }
    }
}