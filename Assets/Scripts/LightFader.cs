using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class LightFader : MonoBehaviour
{
    [Header("Lights Fade")]
    public Light[] lights;
    public float duration = 2f;

    private float timer = 0f;
    private bool fading = false;
    private float[] initialIntensity;

    [Header("UI")]
    public UIFadeController uiFadeController; // <-- UI 연결

    [Header("Bloom (Global Volume)")]
    public Volume globalVolume;             // 🔹 Global Volume 드래그
    public float bloomThresholdOn = 1f;     // 기본값
    public float bloomIntensityOn = 1.5f;

    public float bloomThresholdOff = 0.5f;  // 불 꺼졌을 때 값
    public float bloomIntensityOff = 50f;
    public float bloomFadeDuration = 1f;    // Bloom 바뀌는 시간

    private Bloom bloom;

    void Start()
    {
        // Bloom 가져오기
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out bloom);
        }

        // 시작 상태를 "불 켜진 값"으로 맞춰주고 싶으면:
        if (bloom != null)
        {
            bloom.threshold.value = bloomThresholdOn;
            bloom.intensity.value = bloomIntensityOn;
        }

        StartFade();
    }

    public void StartFade()
    {
        fading = true;
        timer = 0f;

        initialIntensity = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                initialIntensity[i] = lights[i].intensity;
        }
    }

    void Update()
    {
        if (!fading) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        float fadeValue = 1f - t;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                lights[i].intensity = initialIntensity[i] * fadeValue;
        }

        if (t >= 1f)
        {
            fading = false;

            // 🔥 라이트 다 꺼진 뒤 Bloom 변경 코루틴 시작
            if (bloom != null)
                StartCoroutine(FadeBloom());

            // 🔥 2초 뒤 UI 페이드인
            if (uiFadeController != null)
                StartCoroutine(ShowUIDelayed());
        }
    }

    private IEnumerator ShowUIDelayed()
    {
        yield return new WaitForSeconds(2f); // 2초 기다리고
        uiFadeController.FadeIn();           // UI 등장
    }

    private IEnumerator FadeBloom()
    {
        float startThreshold = bloom.threshold.value;
        float startIntensity = bloom.intensity.value;

        float time = 0f;

        while (time < bloomFadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / bloomFadeDuration);

            bloom.threshold.value = Mathf.Lerp(startThreshold, bloomThresholdOff, t);
            bloom.intensity.value = Mathf.Lerp(startIntensity, bloomIntensityOff, t);

            yield return null;
        }

        bloom.threshold.value = bloomThresholdOff;
        bloom.intensity.value = bloomIntensityOff;
    }
}
