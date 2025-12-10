using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("위아래로 움직이는 속도입니다.")]
    public float speed = 2f; // 움직이는 속도
    
    [Tooltip("위아래로 움직이는 범위(높이)입니다.")]
    public float height = 10f; // 움직이는 높이 범위 (픽셀 단위)

    private Vector3 startPos;

    void Start()
    {
        // 게임이 시작될 때의 원래 위치를 기억해둡니다.
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Mathf.Sin(Time.time * speed) -> -1 ~ 1 사이를 부드럽게 오가는 값 생성
        // 거기에 height를 곱해서 움직임의 폭을 결정
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * height;

        // 계산된 새로운 Y 위치를 적용 (X와 Z는 원래 위치 그대로 유지)
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
    }
}