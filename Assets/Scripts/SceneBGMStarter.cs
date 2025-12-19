using UnityEngine;

public class SceneBGMStarter : MonoBehaviour
{
    [Header("이 씬에서 사용할 BGM (없으면 비워두기)")]
    public AudioClip bgmClip;

    [Header("씬 시작할 때 자동 재생 여부")]
    public bool playOnStart = true;   // 대부분 씬은 true

    [Header("씬 시작 시 이전 BGM만 끄고 싶을 때 체크")]
    public bool fadeOutOnly = false;  // BGM 없는 씬용

    void Start()
    {
        if (SoundManager.Instance == null) return;

        if (fadeOutOnly)
        {
            // 남아있는 BGM만 서서히 끄기 (새 곡 없음)
            SoundManager.Instance.FadeOutAndStop();
        }
        else if (playOnStart && bgmClip != null)
        {
            // 이전 씬 BGM → 이 씬 BGM으로 부드럽게 전환
            SoundManager.Instance.PlayBGM(bgmClip);
        }
    }

    // 인트로 영상 끝난 뒤 등, 외부에서 수동으로 BGM 시작하고 싶을 때 호출
    public void PlayManually()
    {
        if (SoundManager.Instance != null && bgmClip != null)
        {
            SoundManager.Instance.PlayBGM(bgmClip);
        }
    }
}
