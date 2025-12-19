using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class OutroSceneController : MonoBehaviour
{
    [Header("흰색 패널(CanvasGroup)")]
    public CanvasGroup whitePanel;

    [Header("타이밍")]
    public float whiteHoldDuration = 0.3f; // 영상 준비 후, 흰색으로 살짝 더 유지
    public float fadeDuration = 1.5f; // 흰색 → 투명

    [Header("아웃트로 영상")]
    public VideoPlayer videoPlayer;

    private IEnumerator Start()
    {
        // 1) 흰색 패널 완전 불투명으로 시작
        if (whitePanel != null)
        {
            whitePanel.alpha = 1f;
            whitePanel.gameObject.SetActive(true);
        }

        // 2) 비디오 준비
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.Stop();

            // ▶ 영상 준비 (첫 프레임 로드)
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
            {
                // 준비되는 동안 계속 흰색으로 가려두기
                yield return null;
            }

            // 준비가 되면 바로 재생 시작 (아직은 흰 패널이 가리고 있음)
            videoPlayer.Play();
        }

        // 3) 영상이 뒤에서 돌기 시작한 상태로 흰 화면 잠깐 유지
        if (whiteHoldDuration > 0f)
            yield return new WaitForSeconds(whiteHoldDuration);

        // 4) 흰색 패널 페이드아웃 (밑에선 이미 영상이 재생 중)
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / fadeDuration);

            if (whitePanel != null)
                whitePanel.alpha = 1f - lerp; // 1 → 0

            yield return null;
        }

        // 5) 완전히 사라졌으면 비활성화
        if (whitePanel != null)
        {
            whitePanel.alpha = 0f;
            whitePanel.gameObject.SetActive(false);
        }
    }
}
