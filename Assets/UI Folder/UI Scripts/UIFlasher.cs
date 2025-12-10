using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Shadow))] // Shadow 컴포넌트가 꼭 있어야 작동합니다.
public class ShadowGlowAnimator : MonoBehaviour
{
    [Header("Glow Settings")]
    [Tooltip("반짝일 때의 그림자 색상 (형광색 추천)")]
    public Color glowColor = Color.cyan;

    [Tooltip("평소(꺼졌을 때)의 그림자 색상 (보통 투명하거나 검은색)")]
    public Color normalColor = new Color(0, 0, 0, 0.5f);

    [Tooltip("깜빡이는 속도")]
    public float glowSpeed = 3.0f;

    private Shadow shadowComponent;

    private void Awake()
    {
        shadowComponent = GetComponent<Shadow>();
        // 시작할 때 색상 초기화
        if (shadowComponent != null)
            shadowComponent.effectColor = normalColor;
    }

    private void OnEnable()
    {
        StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        while (true)
        {
            // 0 ~ 1 사이를 왔다갔다 하는 값 (PingPong)
            // Time.time * 속도 -> 계속 증가하는 시간 값
            float t = Mathf.PingPong(Time.time * glowSpeed, 1f);

            // 두 색상 사이를 부드럽게 섞음 (Lerp)
            if (shadowComponent != null)
            {
                shadowComponent.effectColor = Color.Lerp(normalColor, glowColor, t);
            }

            yield return null;
        }
    }
}