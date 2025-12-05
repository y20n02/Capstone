using UnityEngine;

public class PaperFall : MonoBehaviour
{
    private RectTransform rect;
    private RectTransform container;

    // 움직임 파라미터
    private float duration;        // 전체 떨어지는 시간
    private float amplitude;       // 좌우 흔들림 크기
    private float frequency;       // 좌우 흔들림 속도
    private float rotationSpeed;   // 회전 속도 (deg/sec)
    private float delay;           // 시작 딜레이

    private float startX;
    private float startY;
    private float endY;
    private float startTime;
    private float phase;           // 사인 웨이브 위상 (불규칙 느낌용)

    // 스포너에서 호출해 줄 초기화 함수
    public void Init(
        RectTransform container,
        float duration,
        float amplitude,
        float frequency,
        float rotationSpeed,
        float delay)
    {
        this.container     = container;
        this.duration      = duration;
        this.amplitude     = amplitude;
        this.frequency     = frequency;
        this.rotationSpeed = rotationSpeed;
        this.delay         = delay;

        rect    = (RectTransform)transform;
        Vector2 pos = rect.anchoredPosition;

        startX = pos.x;
        startY = pos.y;
        // 화면 아래 조금 더 내려가도록
        endY   = -container.rect.height * 0.5f - 200f;

        phase     = Random.Range(0f, Mathf.PI * 2f);
        startTime = Time.time;
    }

    private void Update()
    {
        if (rect == null || container == null) return;

        float t = Time.time - startTime;

        // 시작 딜레이 동안은 가만히
        if (t < delay) return;
        t -= delay;

        float norm = t / duration;
        if (norm >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // 위에서 아래로 천천히
        float y = Mathf.Lerp(startY, endY, norm);

        // 좌우 팔랑팔랑 (사인 웨이브)
        float x = startX + Mathf.Sin(t * frequency + phase) * amplitude;

        rect.anchoredPosition = new Vector2(x, y);

        // 살짝 회전
        float z = rotationSpeed * t;
        rect.localRotation = Quaternion.Euler(0f, 0f, z);
    }
}
