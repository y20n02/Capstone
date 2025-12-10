using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SequentialLoopEffect : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("1. 위로 올라가며 사라질 이미지")]
    public RectTransform firstTarget; 
    
    [Tooltip("2. 서서히 나타나며 내려올 이미지")]
    public RectTransform secondTarget;

    [Header("Settings")]
    public float moveDistance = 50f; // 움직일 거리
    public float duration = 0.8f;    // 애니메이션 시간
    public float delayBetween = 0.2f; // 1번 사라짐 -> 2번 나타남 사이 대기
    
    [Header("Loop Settings")]
    [Tooltip("무한 반복 할지 여부")]
    public bool loop = true;
    [Tooltip("한 사이클(2번 등장 완료)이 끝나고 다시 처음으로 돌아갈 때까지 대기 시간")]
    public float loopDelay = 2.0f;

    [Header("Debug")]
    public bool playOnStart = true;

    private Vector3 originPos1;
    private Vector3 originPos2;
    private CanvasGroup group1;
    private CanvasGroup group2;

    private void Awake()
    {
        // 초기화 및 원본 위치 저장
        PrepareTarget(firstTarget, ref group1, out originPos1);
        PrepareTarget(secondTarget, ref group2, out originPos2);
    }

    private void Start()
    {
        if (playOnStart) PlaySequence();
    }

    private void PrepareTarget(RectTransform target, ref CanvasGroup group, out Vector3 origin)
    {
        origin = Vector3.zero;
        if (target == null) return;

        origin = target.anchoredPosition;
        
        group = target.GetComponent<CanvasGroup>();
        if (group == null) group = target.gameObject.AddComponent<CanvasGroup>();

        // 레이아웃 강제 무시 (Vertical Layout Group 등에 있어도 움직이게)
        LayoutElement le = target.GetComponent<LayoutElement>();
        if (le == null) le = target.gameObject.AddComponent<LayoutElement>();
        le.ignoreLayout = true;
    }

    public void PlaySequence()
    {
        StopAllCoroutines();
        StartCoroutine(ProcessLoopSequence());
    }

    private IEnumerator ProcessLoopSequence()
    {
        do // 최소 한 번은 실행, loop가 true면 계속 실행
        {
            // ------------------------------------------------
            // 0. 시작 전 상태 리셋 (1번 보임, 2번 숨김)
            // ------------------------------------------------
            if (firstTarget)
            {
                firstTarget.anchoredPosition = originPos1;
                group1.alpha = 1f;
            }
            if (secondTarget)
            {
                secondTarget.anchoredPosition = originPos2;
                group2.alpha = 0f;
            }

            // ------------------------------------------------
            // 1단계: 1번 이미지 위로 이동 & 사라짐
            // ------------------------------------------------
            if (firstTarget != null)
            {
                float timer = 0f;
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float t = timer / duration;
                    float curve = Mathf.Sin(t * Mathf.PI * 0.5f);

                    firstTarget.anchoredPosition = originPos1 + (Vector3.up * moveDistance * curve);
                    group1.alpha = 1f - curve; // Fade Out

                    yield return null;
                }
                group1.alpha = 0f;
            }

            // 사이 대기
            if (delayBetween > 0) yield return new WaitForSeconds(delayBetween);

            // ------------------------------------------------
            // 2단계: 2번 이미지 내려오며 나타남
            // ------------------------------------------------
            if (secondTarget != null)
            {
                float timer = 0f;
                while (timer < duration)
                {
                    timer += Time.deltaTime;
                    float t = timer / duration;
                    float curve = Mathf.Sin(t * Mathf.PI * 0.5f);

                    secondTarget.anchoredPosition = originPos2 + (Vector3.down * moveDistance * curve);
                    group2.alpha = curve; // Fade In

                    yield return null;
                }
                group2.alpha = 1f;
            }

            // ------------------------------------------------
            // 3단계: 다음 반복 전 대기 (Loop Delay)
            // ------------------------------------------------
            if (loop)
            {
                yield return new WaitForSeconds(loopDelay);
            }

        } while (loop);
    }
}