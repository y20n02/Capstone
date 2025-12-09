using System.Collections;
using UnityEngine;

public class FireworkFader : MonoBehaviour
{
    public ParticleSystem[] fireworks;
    public float fadeDuration = 3f;

    public void FadeOut()
    {
        StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        float t = 0f;

        // renderers ±∏«œ±‚
        var rends = new Renderer[fireworks.Length];
        var mats = new Material[fireworks.Length];

        for (int i = 0; i < fireworks.Length; i++)
        {
            rends[i] = fireworks[i].GetComponent<Renderer>();
            mats[i] = rends[i].material; // ¿ŒΩ∫≈œΩÃµ 
        }

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);

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

        gameObject.SetActive(false);
    }
}
