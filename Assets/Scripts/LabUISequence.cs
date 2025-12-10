using System.Collections;
using UnityEngine;

public class IntroUISequence : MonoBehaviour
{
    [Header("CanvasGroups")]
    public CanvasGroup grabText;        // 01Grab_text
    public CanvasGroup shakeText;       // 02Shake_text
    public CanvasGroup putText;         // 03Put_text
    public CanvasGroup putDirection;    // 03Direction
    public CanvasGroup greatText;       // 04Great_text

    [Header("Fade")]
    public float fadeTime = 0.5f;

    private enum UIState { None, Grab, Shake, Put, Great }
    private UIState _state = UIState.None;

    void Start()
    {
        // 처음 상태: Grab만 보이게
        SetAllInvisible();

        if (grabText != null)
        {
            grabText.gameObject.SetActive(true);
            grabText.alpha = 0f;
            grabText.interactable = true;
            grabText.blocksRaycasts = true;

            StartCoroutine(FadeIn(grabText));
            _state = UIState.Grab;
        }

        Debug.Log("[IntroUI] Start - Grab 단계 시작");
    }

    // ① Grab 완료 시 호출 (포션 손에 붙었을 때)
    public void OnGrabDone()
    {
        if (_state != UIState.Grab)
        {
            Debug.Log($"[IntroUI] OnGrabDone 호출됐지만 현재 상태는 {_state}, 무시");
            return;
        }

        Debug.Log("[IntroUI] OnGrabDone → Grab → Shake 전환");

        StopAllCoroutines();
        StartCoroutine(TransitionGrabToShake());
    }

    // ② Shake 완료 시 호출
    public void OnShakeDone()
    {
        if (_state != UIState.Shake)
        {
            Debug.Log($"[IntroUI] OnShakeDone 호출됐지만 현재 상태는 {_state}, 무시");
            return;
        }

        Debug.Log("[IntroUI] OnShakeDone → Shake → Put 전환");

        StopAllCoroutines();
        StartCoroutine(TransitionShakeToPut());
    }

    // ③ Put 완료 시 호출
    public void OnPutDone()
    {
        if (_state != UIState.Put)
        {
            Debug.Log($"[IntroUI] OnPutDone 호출됐지만 현재 상태는 {_state}, 무시");
            return;
        }

        Debug.Log("[IntroUI] OnPutDone → Put → Great 전환");

        StopAllCoroutines();
        StartCoroutine(TransitionPutToGreat());
    }

    // ---------------- 전환 코루틴 ----------------

    IEnumerator TransitionGrabToShake()
    {
        // Shake 준비
        if (shakeText != null)
        {
            shakeText.gameObject.SetActive(true);
            shakeText.alpha = 0f;
            shakeText.interactable = true;
            shakeText.blocksRaycasts = true;
        }

        // Grab 페이드 아웃 → Shake 페이드 인
        if (grabText != null)
            yield return FadeOut(grabText);

        if (shakeText != null)
            yield return FadeIn(shakeText);

        _state = UIState.Shake;
        Debug.Log("[IntroUI] 상태 변경: Shake");
    }

    IEnumerator TransitionShakeToPut()
    {
        // Shake 사라지고 → Put + Direction 등장
        if (shakeText != null)
            yield return FadeOut(shakeText);

        if (putDirection != null)
        {
            putDirection.gameObject.SetActive(true);
            putDirection.alpha = 0f;
        }

        if (putText != null)
        {
            putText.gameObject.SetActive(true);
            putText.alpha = 0f;
        }

        // Direction / Put 동시에 서서히 등장
        if (putDirection != null)
            StartCoroutine(FadeIn(putDirection));

        if (putText != null)
            yield return FadeIn(putText);

        _state = UIState.Put;
        Debug.Log("[IntroUI] 상태 변경: Put");
    }

    IEnumerator TransitionPutToGreat()
    {
        // Put 단계 UI 전부 사라짐
        if (putDirection != null)
            StartCoroutine(FadeOut(putDirection));

        if (putText != null)
            yield return FadeOut(putText);

        if (greatText != null)
        {
            greatText.gameObject.SetActive(true);
            greatText.alpha = 0f;
            yield return FadeIn(greatText);
        }

        _state = UIState.Great;
        Debug.Log("[IntroUI] 상태 변경: Great");
    }

    // ---------------- 공통 유틸 ----------------

    void SetAllInvisible()
    {
        SetVisible(grabText, false);
        SetVisible(shakeText, false);
        SetVisible(putText, false);
        SetVisible(putDirection, false);
        SetVisible(greatText, false);
    }

    void SetVisible(CanvasGroup cg, bool visible)
    {
        if (cg == null) return;

        cg.gameObject.SetActive(visible);
        cg.alpha = visible ? 1f : 0f;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
    }

    IEnumerator FadeIn(CanvasGroup cg)
    {
        if (cg == null) yield break;

        cg.gameObject.SetActive(true);
        cg.interactable = true;
        cg.blocksRaycasts = true;

        float t = 0f;
        cg.alpha = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }
        cg.alpha = 1f;
    }

    IEnumerator FadeOut(CanvasGroup cg)
    {
        if (cg == null) yield break;

        cg.interactable = false;
        cg.blocksRaycasts = false;

        float t = 0f;
        float startAlpha = cg.alpha;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeTime);
            yield return null;
        }
        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }
}
