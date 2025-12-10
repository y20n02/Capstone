using System;
using System.Collections;
using UnityEngine;

public class RippleDistortionController : MonoBehaviour
{
    public static RippleDistortionController Instance { get; private set; }

    [Header("디스토션 오브젝트 (실제 이펙트)")]
    public GameObject distortionObject;      // ← Distortion (2)를 드래그해서 연결

    [Header("Target Material")]
    public Material targetMaterial;

    [Header("Shader Property Names")]
    public string rippleCountProp = "_RippleCount";
    public string rippleSpeedProp = "_RippleSpeed";
    public string rippleContrastProp = "_RippleContrast";

    [Header("Target Values")]
    public float targetRippleCount = 35.6f;
    public float targetRippleSpeed = 0.31f;
    public float targetRippleContrast = 3.2f;

    [Header("Tween Settings")]
    public float duration = 5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 씬 시작할 땐 Distortion (2) 완전 꺼둔 상태
        if (distortionObject != null)
            distortionObject.SetActive(false);

        ResetRippleValues();
    }

    void ResetRippleValues()
    {
        if (targetMaterial == null) return;

        targetMaterial.SetFloat(rippleCountProp, 0f);
        targetMaterial.SetFloat(rippleSpeedProp, 0f);
        targetMaterial.SetFloat(rippleContrastProp, 0f);
    }

    public void Play(Action onFinished)
    {
        if (targetMaterial == null)
        {
            Debug.LogWarning("[RippleDistortion] targetMaterial 없음");
            onFinished?.Invoke();
            return;
        }

        // 이 컨트롤러 자신은 반드시 활성 상태여야 함
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[RippleDistortion] Controller GameObject가 비활성 상태입니다.");
            onFinished?.Invoke();
            return;
        }

        // 🔵 전환 직전에 Distortion (2) 켜기
        if (distortionObject != null && !distortionObject.activeSelf)
            distortionObject.SetActive(true);

        StartCoroutine(PlayCoroutine(onFinished));
    }

    private IEnumerator PlayCoroutine(Action onFinished)
    {
        ResetRippleValues();

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / duration);

            targetMaterial.SetFloat(rippleCountProp, Mathf.Lerp(0f, targetRippleCount, r));
            targetMaterial.SetFloat(rippleSpeedProp, Mathf.Lerp(0f, targetRippleSpeed, r));
            targetMaterial.SetFloat(rippleContrastProp, Mathf.Lerp(0f, targetRippleContrast, r));

            yield return null;
        }

        targetMaterial.SetFloat(rippleCountProp, targetRippleCount);
        targetMaterial.SetFloat(rippleSpeedProp, targetRippleSpeed);
        targetMaterial.SetFloat(rippleContrastProp, targetRippleContrast);

        // 🔥 씬 넘어가기 직전에 다시 꺼줘도 됨 (어차피 곧 씬 로드지만)
        if (distortionObject != null)
            distortionObject.SetActive(false);

        onFinished?.Invoke();
    }
}
