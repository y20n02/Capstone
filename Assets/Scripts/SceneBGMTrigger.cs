using UnityEngine;

public class SceneBGMTrigger : MonoBehaviour
{
    [Header("이 씬에서 사용할 BGM")]
    public AudioClip bgmClip;

    // 가이드 Canvas 끝난 뒤 호출 → 이 씬 BGM 페이드인
    public void PlaySceneBGM()
    {
        if (SoundManager.Instance != null && bgmClip != null)
        {
            SoundManager.Instance.PlayBGM(bgmClip);
        }
    }

    // 씬이 끝날 때 호출 → BGM 페이드아웃
    public void FadeOutSceneBGM()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.FadeOutAndStop();
        }
    }
}
