using UnityEngine;
using UnityEngine.Video;

public class VideoSequenceController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip firstClip;   // 첫 번째 영상
    public VideoClip secondClip;  // 두 번째 영상

    public MotionTrigger motionTrigger;   // Purify로 넘길 대상
    public GameObject tapInstructionUI;   // 사이에 띄울 UI 패널
    public SceneLoader sceneLoader;
    private bool waitingForTap = false;   // 첫 영상 끝난 뒤 탭 기다리는 중인지
    private bool playingSecond = false;

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        // 처음에는 UI 끄기
        if (tapInstructionUI != null)
            tapInstructionUI.SetActive(false);

        videoPlayer.clip = firstClip;
        videoPlayer.isLooping = false;
        videoPlayer.Play();

        videoPlayer.loopPointReached += OnVideoEnd;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        // ① 첫 번째 영상 끝난 경우 → 탭 대기
        if (!playingSecond)
        {
            waitingForTap = true;

            if (tapInstructionUI != null)
                tapInstructionUI.SetActive(true);

            Debug.Log("[VideoSequence] 첫 영상 종료, 탭 입력 기다리는 중...");
        }
        // ② 두 번째 영상 끝난 경우 → Purify로 전환
        else
        {
            Debug.Log("[VideoSequence] 두 번째 영상 종료");

            if (tapInstructionUI != null)
                tapInstructionUI.SetActive(false);

            if (sceneLoader != null)
                SceneLoader.LoadPurify();
                
            else
            {
                Debug.LogWarning("[VideoSequence] motionTrigger가 안 물려 있음!");
            }
        }
    }

    /// <summary>
    /// 금 간 화면을 탭했을 때(버스트 제스처 인식 시) MotionTrigger 에서 호출
    /// </summary>
    public void OnTapTrigger()
    {
        if (!waitingForTap)
        {
            Debug.Log("[VideoSequence] 아직 탭 받을 준비가 안 됨 (waitingForTap=false)");
            return;
        }

        waitingForTap = false;
        playingSecond = true;

        if (tapInstructionUI != null)
            tapInstructionUI.SetActive(false);

        videoPlayer.clip = secondClip;
        videoPlayer.isLooping = false;
        videoPlayer.Play();

        Debug.Log("[VideoSequence] 탭 인식 → 두 번째 영상 재생 시작");
    }
}
