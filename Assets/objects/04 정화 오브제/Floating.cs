using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    [Header("둥실거리기 설정")]
    public float floatSpeed = 1f;    // 위아래 움직이는 속도
    public float floatHeight = 0.5f; // 위아래 이동 범위

    [Header("회전 설정")]
    public float rotateSpeed = 50f;  // 회전 속도 (클수록 빨리 돔)

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position; // 시작 위치 저장
    }

    void Update()
    {
        // 1. 위아래로 둥실거리기 (기존 기능)
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = startPos + new Vector3(0, newY, 0);

        // 2. 제자리 회전하기 (추가된 기능)
        // Vector3.up은 Y축(초록색 화살표)을 기준으로 돈다는 뜻입니다.
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}