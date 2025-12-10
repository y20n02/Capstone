using UnityEngine;

public class UIMoverHorizontal : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("이동 속도입니다. 높을수록 빨라집니다.")]
    public float moveSpeed = 2.0f;

    [Tooltip("좌우로 이동할 최대 거리(픽셀)입니다.")]
    public float moveDistance = 50.0f;

    private RectTransform rectTransform;
    private Vector3 startPosition;

    private void Start()
    {
        // UI 컴포넌트의 위치를 제어하는 RectTransform을 가져옵니다.
        rectTransform = GetComponent<RectTransform>();
        
        // 시작할 때의 원래 위치를 기억해둡니다. (기준점)
        startPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        // Mathf.Sin(Time.time * 속도) -> 시간이 지남에 따라 -1 ~ 1 사이 값을 반환
        float sinValue = Mathf.Sin(Time.time * moveSpeed);

        // -1~1 값에 이동 거리를 곱해서 실제 이동할 오프셋(간격) 계산
        float offsetX = sinValue * moveDistance;

        // 원래 시작 위치(startPosition) 기준으로 X축으로만 오프셋을 더해서 위치 갱신
        // anchoredPosition을 써야 UI 좌표계에서 올바르게 작동합니다.
        rectTransform.anchoredPosition = startPosition + new Vector3(offsetX, 0f, 0f);
    }
}