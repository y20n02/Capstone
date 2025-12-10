using System.Collections;
using UnityEngine;

public class LabRippleFadeOut : MonoBehaviour
{
    [Header("Target Material (랩 씬 전체에 쓰는 울렁 머티리얼)")]
    public Material rippleMaterial;

    [Header("Shader Property Names")]
    public string rippleCountProp = "_RippleCount";
    public string rippleSpeedProp = "_RippleSpeed";
    public string rippleContrastProp = "_RippleContrast";

    [Header("초기값 (시작할 때 강하게 적용될 값)")]
    public float startRippleCount = 35.6f;
    public float startRippleSpeed = 0.31f;
    public float startRippleContrast = 3.2f;

    [Header("페이드아웃 시간 (초)")]
    public float fadeDuration = 5f;

    void Start()
    {
        if (rippleMaterial == null)
        {
            Debug.LogWarning("[LabRippleFadeOut] rippleMaterial이 비어있습니다.");
            return;
        }

        // 처음부터 강한 값 적용
        SetRipple(startRippleCount, startRippleSpeed, startRippleContrast);

        // 페이드 아웃 시작
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            float r = Mathf.Clamp01(t / fadeDuration);  // 0→1
            float count = Mathf.Lerp(startRippleCount, 0f, r);
            float speed = Mathf.Lerp(startRippleSpeed, 0f, r);
            float contrast = Mathf.Lerp(startRippleContrast, 0f, r);

            SetRipple(count, speed, contrast);

            yield return null;
        }

        // 완전 정지
        SetRipple(0f, 0f, 0f);
        gameObject.SetActive(false);
    }

    private void SetRipple(float count, float speed, float contrast)
    {
        rippleMaterial.SetFloat(rippleCountProp, count);
        rippleMaterial.SetFloat(rippleSpeedProp, speed);
        rippleMaterial.SetFloat(rippleContrastProp, contrast);
    }
}
