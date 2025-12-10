using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SlideLoopAnimator : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("이동할 거리 (X축)")]
    public float moveDistance = 100.0f; 
    
    [Tooltip("이동 속도")]
    public float moveSpeed = 200.0f;

    [Tooltip("다음 반복까지 대기 시간")]
    public float loopDelay = 0.2f;

    [Header("Fade Settings")]
    [Tooltip("이동 시작 후 언제까지 서서히 나타날지 (0~1). 예: 0.2면 처음 20% 구간동안 페이드 인")]
    [Range(0f, 0.5f)]
    public float fadeInPoint = 0.2f; // [추가됨] 페이드 인 구간

    [Tooltip("이동 중 언제부터 사라지기 시작할지 (0~1). 예: 0.7이면 70% 구간부터 페이드 아웃")]
    [Range(0.5f, 1f)]
    public float fadeStartPoint = 0.7f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        startPos = rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        StartCoroutine(AnimateLoop());
    }

    private IEnumerator AnimateLoop()
    {
        while (true)
        {
            // 1. 초기화 (위치는 원복하되, 투명하게 시작)
            rectTransform.anchoredPosition = startPos;
            canvasGroup.alpha = 0f; // [변경] 처음엔 안 보이게 시작

            float timer = 0f;
            float calculatedDuration = Mathf.Abs(moveDistance) / Mathf.Max(moveSpeed, 0.1f);

            // 2. 이동 애니메이션
            while (timer < calculatedDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / calculatedDuration; // 0.0 ~ 1.0
                
                // 위치 이동 (부드럽게)
                float moveProgress = Mathf.Sin(progress * Mathf.PI * 0.5f);
                rectTransform.anchoredPosition = startPos + new Vector3(moveDistance * moveProgress, 0f, 0f);

                // --- 투명도 조절 로직 ---
                if (progress < fadeInPoint)
                {
                    // [구간 1] 페이드 인: 0 -> 1
                    // 현재 진행도가 fadeInPoint의 몇 퍼센트인지 계산
                    canvasGroup.alpha = progress / fadeInPoint;
                }
                else if (progress > fadeStartPoint)
                {
                    // [구간 3] 페이드 아웃: 1 -> 0
                    // 남은 구간 비율 계산
                    float fadeOutProgress = (progress - fadeStartPoint) / (1f - fadeStartPoint);
                    canvasGroup.alpha = 1f - fadeOutProgress;
                }
                else
                {
                    // [구간 2] 중간 유지: 1 (완전 선명)
                    canvasGroup.alpha = 1f;
                }
                // ---------------------

                yield return null;
            }

            // 3. 끝 처리 (완전 투명)
            canvasGroup.alpha = 0f;
            rectTransform.anchoredPosition = startPos + new Vector3(moveDistance, 0f, 0f);

            // 4. 대기
            yield return new WaitForSeconds(loopDelay);
        }
    }
}