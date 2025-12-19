using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class IntroVideoController : MonoBehaviour
{
    [Header("영상")]
    public VideoPlayer introVideo;
    public GameObject introRoot;         // 영상만 켜고 끌 때 사용할 오브젝트 (Intro)
    
    [Header("메인 씬 루트(필요하면)")]
    public GameObject mainRoot;          // Main 오브젝트 (이미 켜져 있으면 null로 놔둬도 됨)

    [Header("UI 인트로 컨트롤러")]
    public AccumulationIntroUI accumulationIntroUI;  // 👈 여기다 드래그!

    public float delayBeforeUI = 0.5f;   // 영상 끝 → UI 시작 전 살짝 텀

    bool isPlaying = true;

    void Start()
    {
        if (introVideo != null)
        {
            introVideo.loopPointReached += OnVideoFinished;
            introVideo.Play();
        }

        // 메인은 항상 켜두고, MotionManager는 AccumulationIntroUI가 마지막에 켜줄 거면
        // mainRoot는 굳이 안 건드려도 됨.
    }

    public SceneBGMStarter sceneBgmStarter;

    void OnVideoFinished(VideoPlayer vp)
    {
        // 인트로 영상 끝 → 이제 BGM 시작
        if (sceneBgmStarter != null)
            sceneBgmStarter.PlayManually();
        
        if (!isPlaying) return;
        isPlaying = false;


        StartCoroutine(AfterVideoRoutine());

    }



    IEnumerator AfterVideoRoutine()
    {
        // 1) 영상 화면 끄기
        if (introRoot != null)
            introRoot.SetActive(false);

        // 2) 살짝 텀
        yield return new WaitForSeconds(delayBeforeUI);

        // 3) 이제 UI 인트로 실행!
        if (accumulationIntroUI != null)
            accumulationIntroUI.StartIntro();

            
        else
            Debug.LogWarning("AccumulationIntroUI 안 연결됨!");
    }
}
