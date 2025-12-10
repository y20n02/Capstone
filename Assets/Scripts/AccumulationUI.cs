using System.Collections;
using UnityEngine;

public class AccumulationIntroUI : MonoBehaviour
{
    [Header("UI 참조")]
    public CanvasGroup introGroup;          // AccumulationCanvas에 있는 CanvasGroup

    [Header("타이밍 설정")]
    public float delayBeforeShow = 2.5f;    // 씬 들어오고 나서 대기 시간 (2~3초)
    public float fadeTime = 1.0f;           // 페이드 인/아웃 시간
    public float visibleTime = 5.0f;        // 화면에 유지되는 시간

    [Header("인트로 끝난 뒤 켤 스크립트들")]
    public MonoBehaviour[] scriptsToEnable; // 모션 인식 스크립트들 드래그해서 넣기

    public bool IntroFinished { get; private set; } = false;

    void Start()
    {
        // 1) 인트로 UI는 처음에 "보이지 않게"만 만든다 (GameObject는 끄지 X)
        if (introGroup != null)
        {
            introGroup.gameObject.SetActive(true);   // 항상 켜둔 상태에서
            introGroup.alpha = 0f;                   // 투명하게 숨기기
            introGroup.interactable = false;
            introGroup.blocksRaycasts = false;
        }

        // 2) 모션 관련 스크립트들은 일단 꺼두기
        if (scriptsToEnable != null)
        {
            foreach (var s in scriptsToEnable)
            {
                if (s != null) s.enabled = false;
            }
        }

        // 3) 인트로 연출 시작
        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        // 1) 씬 들어오고 잠깐 기다렸다가
        yield return new WaitForSeconds(delayBeforeShow);

        // 2) 인트로 UI 페이드 인
        if (introGroup != null)
        {
            // 보이도록 켜둔 뒤 서서히 나타나게
            introGroup.gameObject.SetActive(true);
            yield return Fade(0f, 1f, fadeTime);

            introGroup.interactable = true;
            introGroup.blocksRaycasts = true;
        }

        // 3) UI를 visibleTime 만큼 유지
        yield return new WaitForSeconds(visibleTime);

        // 4) 페이드 아웃
        if (introGroup != null)
        {
            introGroup.interactable = false;
            introGroup.blocksRaycasts = false;

            yield return Fade(1f, 0f, fadeTime);

            // 필요하면 완전히 끄고 싶을 때만 사용 (여긴 코루틴 끝나는 시점이라 괜찮음)
            introGroup.gameObject.SetActive(false);
        }

        // 5) 이제부터 모션 인식 시작!
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
