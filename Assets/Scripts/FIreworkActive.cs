using System.Collections;
using UnityEngine;

public class FireworkActivator : MonoBehaviour
{
    public ParticleSystem[] fireworks;
    public float fadeOutTime = 3f; // 페이드 시간

    private bool activated = false;

    public void ActivateAll()
    {
        if (activated) return;
        activated = true;

        for (int i = 0; i < fireworks.Length; i++)
        {
            var fx = fireworks[i];
            if (fx == null) continue;

            fx.gameObject.SetActive(true);
            fx.Play();
            Debug.Log($"🔥 Firework {i + 1} 발사!");
        }
    }

    public IEnumerator FadeOutAll(float fadeDuration)
    {
        // 파티클 재생 중지(완전 Stop 아님!)
        foreach (var fx in fireworks)
        {
            if (fx == null) continue;

            var main = fx.main;
            main.loop = false; // 루프 제거해서 자연스럽게 꺼짐
        }

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);

            foreach (var fx in fireworks)
            {
                if (fx == null) continue;

                var renderer = fx.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    Color startColor = renderer.material.GetColor("_Color");
                    startColor.a = alpha;
                    renderer.material.SetColor("_Color", startColor);
                }
            }

            yield return null;
        }

        // 완전 제거
        foreach (var fx in fireworks)
        {
            if (fx == null) continue;
            fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Debug.Log("🔥 불꽃놀이 페이드아웃 완료!");
    }

    private IEnumerator FadeRoutine()
    {
        float t = 0f;

        // 렌더러 및 머테리얼 캐싱
        var rends = new Renderer[fireworks.Length];
        var mats = new Material[fireworks.Length];

        for (int i = 0; i < fireworks.Length; i++)
        {
            rends[i] = fireworks[i].GetComponent<Renderer>();
            mats[i] = rends[i].material;
        }

        // 파티클 루프 중지
        for (int i = 0; i < fireworks.Length; i++)
            fireworks[i].loop = false;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);

            for (int i = 0; i < fireworks.Length; i++)
            {
                if (mats[i].HasProperty("_Color"))
                {
                    Color c = mats[i].color;
                    c.a = alpha;
                    mats[i].color = c;
                }
            }

            yield return null;
        }

        // 완전히 꺼진 후
        for (int i = 0; i < fireworks.Length; i++)
        {
            fireworks[i].Stop();
            fireworks[i].gameObject.SetActive(false);
        }

        Debug.Log("🔥 Fireworks faded out!");
        activated = false;
    }
}
