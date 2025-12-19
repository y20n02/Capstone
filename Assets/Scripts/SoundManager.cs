using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM 페이드 시간(초)")]
    public float bgmFadeTime = 1.5f;

    private AudioSource bgmSource;
    private float defaultVolume = 1f;
    private Coroutine bgmRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = GetComponent<AudioSource>();
        bgmSource.loop = true;
        defaultVolume = bgmSource.volume;
    }

    // 새 BGM으로 부드럽게 갈아타기
    public void PlayBGM(AudioClip clip)
    {
        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);

        if (clip == null)
            bgmRoutine = StartCoroutine(FadeOutAndStopRoutine());
        else
            bgmRoutine = StartCoroutine(FadeToNewClipRoutine(clip));
    }

    // 그냥 현재 BGM만 페이드아웃하고 멈추기
    public void FadeOutAndStop()
    {
        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);

        bgmRoutine = StartCoroutine(FadeOutAndStopRoutine());
    }

    IEnumerator FadeToNewClipRoutine(AudioClip newClip)
    {
        float startVol = bgmSource.volume;

        // 1) 기존 곡 페이드아웃
        if (bgmSource.isPlaying)
        {
            for (float t = 0f; t < bgmFadeTime; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeTime);
                yield return null;
            }
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();

        // 2) 새 곡 재생
        bgmSource.clip = newClip;
        bgmSource.Play();

        // 3) 페이드인
        for (float t = 0f; t < bgmFadeTime; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, defaultVolume, t / bgmFadeTime);
            yield return null;
        }

        bgmSource.volume = defaultVolume;
        bgmRoutine = null;
    }

    IEnumerator FadeOutAndStopRoutine()
    {
        if (!bgmSource.isPlaying)
        {
            bgmSource.clip = null;
            bgmRoutine = null;
            yield break;
        }

        float startVol = bgmSource.volume;

        for (float t = 0f; t < bgmFadeTime; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeTime);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();
        bgmSource.clip = null;
        bgmRoutine = null;
    }
}
