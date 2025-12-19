using UnityEngine;
using UnityEngine.UI;

public class CircleLoading : MonoBehaviour
{
    public Image loadingBar;   // 원형 이미지

    [Header("로딩 시작 효과음")]
    public AudioClip startSfx;
    public float startVolume = 1f;

    // 이번 '유지 시도(손 고정)' 동안 이미 소리 재생했는지 체크
    private bool soundPlayedThisHold = false;

    // 0~1 사이 값으로 채우기
    public void SetProgress(float ratio)
    {
        if (loadingBar == null) return;
        loadingBar.fillAmount = Mathf.Clamp01(ratio);
    }

    public void ResetProgress()
    {
        SetProgress(0f);
        // 손이 흔들려서 로딩이 끊기면, 다음에 다시 시작할 때 소리 다시 나도록 초기화
        soundPlayedThisHold = false;
    }

    /// <summary>
    /// 로딩이 "처음 시작될 때" 한 번만 호출해줄 함수
    /// </summary>
    public void PlayStartSoundOnce()
    {
        if (soundPlayedThisHold) return;   // 이미 이번 홀드에서 재생했으면 패스
        soundPlayedThisHold = true;

        if (startSfx != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(startSfx, Camera.main.transform.position, startVolume);
        }
    }
}
