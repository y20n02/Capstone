using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoSequenceController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip firstClip;   // 첫 번째 영상 (금 가는 연출까지)
    public VideoClip secondClip;  // 두 번째 영상 (탭한 뒤 재생할 영상)

    public string nextSceneName;  // 필요 없으면 비워둬도 됨

    // 영상 끝난 뒤 MotionTrigger의 단계를 바꾸고 싶을 때 연결
    public MotionTrigger motionTrigger;
    public MotionTrigger.Phase phaseAfterSecond = MotionTrigger.Phase.Purify;

    private bool waitingForTap = false;  // 첫 영상 끝난 뒤 탭 기다리는 중인지
    private bool playingSecond = false;

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.isLooping = false;

        // 첫 번째 영상 세팅 후 바로 재생
        videoPlayer.clip = firstClip;
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
        // ① 첫 번째 영상이 끝난 경우 → 탭 기다리기
        if (!playingSecond)
        {
            waitingForTap = true;
            // 이 상태에서 화면은 첫 영상의 마지막 프레임(금 간 상태)에 멈춰 있음
            Debug.Log("첫 영상 종료, 탭 입력 기다리는 중...");
        }
        // ② 두 번째 영상이 끝난 경우 → Purify 또는 다음 씬
        else
        {
            Debug.Log("두 번째 영상 종료");

            // 정화 단계로 넘기고 싶으면
            if (motionTrigger != null)
            {
                motionTrigger.GoToPurify();
            }

            // 씬 전환도 하고 싶으면 nextSceneName에 이름 넣기
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    /// <summary>
    /// 금 간 화면을 탭했을 때(트리거 인식 시) 호출해 줄 함수
    /// </summary>
    public void OnTapTrigger()
    {
        if (!waitingForTap) return;   // 아직 준비 안됐으면 무시

        waitingForTap = false;
        playingSecond = true;

        // 두 번째 영상으로 교체하고, 먼저 Prepare 해서 끊김 줄이기
        videoPlayer.clip = secondClip;
        videoPlayer.isLooping = false;

        videoPlayer.prepareCompleted += OnSecondPrepared;
        videoPlayer.Prepare();    // 이 동안은 첫 영상 마지막 프레임이 유지됨
    }

    private void OnSecondPrepared(VideoPlayer vp)
    {
        videoPlayer.prepareCompleted -= OnSecondPrepared;

        videoPlayer.Play();
        Debug.Log("탭 인식 → 두 번째 영상 재생 시작");
    }
}
