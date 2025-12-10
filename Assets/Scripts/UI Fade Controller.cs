using System.Collections;
using UnityEngine;

public class UIFadeController : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup uiGroup;

    [Header("Fade 시간")]
    public float fadeInDuration = 1.5f;
    public float fadeOutDuration = 1.0f;

    [Header("모션 스크립트 활성화")]
    public MonoBehaviour[] scriptsToEnable;   // 예: MotionTrigger
    public float delayAfterFadeIn = 3f;       // UI 다 뜬 뒤 모션 켜기까지 대기 시간

    float timer = 0f;

    enum State { Idle, FadingIn, FadingOut }
    State state = State.Idle;

    bool enableCoroutineStarted = false;

    void Start()
    {
        if (uiGroup != null)
        {
            uiGroup.alpha = 0f;           // 처음엔 안 보이게
            uiGroup.interactable = false;
            uiGroup.blocksRaycasts = false;
        }

        // 처음에는 모션 스크립트 전부 꺼두기
        if (scriptsToEnable != null)
        {
            foreach (var s in scriptsToEnable)
            {
                if (s != null) s.enabled = false;
            }
        }
    }

    // 🔵 불 꺼지고 나서 LightFader에서 이걸 호출해주면 됨
    public void FadeIn()
    {
        if (uiGroup == null) return;

        timer = 0f;
        state = State.FadingIn;
        enableCoroutineStarted = false;

        uiGroup.gameObject.SetActive(true);
        uiGroup.interactable = false;
        uiGroup.blocksRaycasts = false;
    }

    // 🔴 모션 인식 성공했을 때 호출 → 1초 동안 페이드아웃
    public void FadeOutAndDisable()
    {
        if (uiGroup == null) return;

        timer = 0f;
        state = State.FadingOut;
        uiGroup.interactable = false;
        uiGroup.blocksRaycasts = false;
    }

    void Update()
    {
        if (uiGroup == null) return;

        switch (state)
        {
            case State.FadingIn:
                UpdateFadeIn();
                break;
            case State.FadingOut:
                UpdateFadeOut();
                break;
        }
    }

    void UpdateFadeIn()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / fadeInDuration);

        uiGroup.alpha = Mathf.Lerp(0f, 1f, t);

        if (t >= 1f)
        {
            state = State.Idle;
            uiGroup.interactable = true;
            uiGroup.blocksRaycasts = true;

            // UI가 다 뜬 뒤 3초 후에 모션 스크립트 켜기
            if (!enableCoroutineStarted && scriptsToEnable != null && scriptsToEnable.Length > 0)
            {
                enableCoroutineStarted = true;
                StartCoroutine(EnableScriptsAfterDelay());
            }
        }
    }

    void UpdateFadeOut()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / fadeOutDuration);

        uiGroup.alpha = Mathf.Lerp(1f, 0f, t);

        if (t >= 1f)
        {
            state = State.Idle;
            uiGroup.alpha = 0f;
            uiGroup.interactable = false;
            uiGroup.blocksRaycasts = false;
            // 필요하면 완전 끄고 싶으면:
            // uiGroup.gameObject.SetActive(false);
        }
    }

    IEnumerator EnableScriptsAfterDelay()
    {
        // UI 다 뜬 뒤 3초 기다렸다가 모션 스크립트 켜기
        yield return new WaitForSeconds(delayAfterFadeIn);

        if (scriptsToEnable != null)
        {
            foreach (var s in scriptsToEnable)
            {
                if (s != null) s.enabled = true;
            }
        }

        Debug.Log("[UIFadeController] 모션 스크립트 활성화 완료");
    }

    // 필요하면 여전히 즉시 끄는 버전도 남겨둘 수 있음
    public void HideInstant()
    {
        if (uiGroup == null) return;

        state = State.Idle;
        uiGroup.alpha = 0f;
        uiGroup.interactable = false;
        uiGroup.blocksRaycasts = false;
    }
}
