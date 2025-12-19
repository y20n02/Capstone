using System.Collections;
using UnityEngine;

public class AccumulationIntroUI : MonoBehaviour
{
    [Header("UI 그룹")]
    public CanvasGroup introGroup;          // AccumulationCanvas에 있는 CanvasGroup

    [Header("타이밍 설정")]
    public float delayBeforeShow = 2.5f;    // (영상 끝난 뒤) UI 나오기 전 대기 시간
    public float fadeTime = 1.0f;           // 페이드 인/아웃 시간
    public float visibleTime = 5.0f;        // 화면에 떠 있는 시간

    [Header("인트로 끝난 뒤 켤 스크립트들")]
    public MonoBehaviour[] scriptsToEnable; // 예: MotionTrigger 같은 것들

    public bool IntroFinished { get; private set; } = false;

    bool introStarted = false; // 중복 실행 방지

    void Start()
    {
        // 1) 인트로 UI를 "보이지 않게" 준비
        if (introGroup != null)
        {
            introGroup.gameObject.SetActive(true);   // 항상 씬 안에 살아있게
            introGroup.alpha = 0f;                   // 투명
            introGroup.interactable = false;
            introGroup.blocksRaycasts = false;
        }

        // 2) 나중에 켤 스크립트들 일단 비활성화
        if (scriptsToEnable != null)
        {
            foreach (var s in scriptsToEnable)
            {
                if (s != null) s.enabled = false;
            }
        }

        // ❌ 여기서는 코루틴 시작 안 함!
        // StartCoroutine(IntroRoutine());  <-- 삭제
    }

    /// <summary>
    /// 🔹 영상이 다 끝난 후, 외부에서 이 함수를 한 번 호출해주면
    ///    delay → 페이드인 → visibleTime → 페이드아웃 → 스크립트 enable 순서로 진행됨
    /// </summary>
    public void StartIntro()
    {
        if (introStarted) return;
        introStarted = true;

        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        // 1) 영상 끝난 뒤, 약간의 여유 시간
        yield return new WaitForSeconds(delayBeforeShow);

        // 2) 인트로 UI 페이드 인
        if (introGroup != null)
        {
            introGroup.gameObject.SetActive(true);
            yield return Fade(0f, 1f, fadeTime);

            introGroup.interactable = true;
            introGroup.blocksRaycasts = true;
        }

        // 3) UI가 visibleTime 만큼 떠 있도록
        yield return new WaitForSeconds(visibleTime);

        // 4) 페이드 아웃
        if (introGroup != null)
        {
            introGroup.interactable = false;
            introGroup.blocksRaycasts = false;

            yield return Fade(1f, 0f, fadeTime);

            introGroup.gameObject.SetActive(false);
        }

        // 5) 인트로 완전 종료 → 스크립트들 활성화
        IntroFinished = true;

        if (scriptsToEnable != null)
        {
            foreach (var s in scriptsToEnable)
            {
                if (s != null) s.enabled = true;
            }
        }
    }

    IEnumerator Fade(float from, float to, float time)
    {
        if (introGroup == null) yield break;

        float t = 0f;
        introGroup.alpha = from;

        while (t < time)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / time);
            introGroup.alpha = Mathf.Lerp(from, to, lerp);
            yield return null;
        }

        introGroup.alpha = to;
    }
}
