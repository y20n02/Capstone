using System.Collections;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light[] lights;              // 깜빡이게 할 포인트 라이트들
    public float minIntensity = 0.2f;   // 가장 어두울 때
    public float maxIntensity = 2.0f;   // 가장 밝을 때
    public int flickerSteps = 6;        // 몇 번 깜빡일지
    public float minInterval = 0.02f;   // 깜빡이는 간격 최소
    public float maxInterval = 0.07f;   // 깜빡이는 간격 최대

    float[] baseIntensities;

    void Awake()
    {
        if (lights == null || lights.Length == 0) return;

        // 원래 밝기 저장
        baseIntensities = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                baseIntensities[i] = lights[i].intensity;
        }
    }

    public void PlayFlicker()
    {
        if (!gameObject.activeInHierarchy) return;
        StopAllCoroutines();
        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        if (lights == null || lights.Length == 0) yield break;

        for (int i = 0; i < flickerSteps; i++)
        {
            float intensity = Random.Range(minIntensity, maxIntensity);
            float wait = Random.Range(minInterval, maxInterval);

            foreach (var l in lights)
            {
                if (l != null) l.intensity = intensity;
            }

            yield return new WaitForSeconds(wait);
        }

        // 다 끝나면 원래 밝기로 복귀
        if (baseIntensities != null)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null)
                    lights[i].intensity = baseIntensities[i];
            }
        }
    }
}
