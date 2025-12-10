using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public const string IntroScene = "00_Intro";
    public const string LabScene = "01_Lab";
    public const string AccumulateScene = "02_Accumulate";
    public const string StimulateScene = "03_Stimulate";
    public const string BurstScene = "04_Burst";
    public const string PurifyScene = "05_Purify";
    public const string OutroScene = "06_Outro";

    // ───────────────── 공통 내부 함수들 ─────────────────

    // 그냥 즉시 로드
    public static void LoadSceneDirect(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 울렁 효과 + 로드
    public static void LoadSceneWithRipple(string sceneName)
    {
        var ripple = RippleDistortionController.Instance;

        if (ripple != null)
        {
            ripple.Play(() =>
            {
                SceneManager.LoadScene(sceneName);
            });
        }
        else
        {
            // 이 씬에 RippleDistortionController가 없으면 그냥 바로 로드
            SceneManager.LoadScene(sceneName);
        }
    }

    // ───────────────── 외부에서 쓸 함수들 ─────────────────
    // ★ 기본 정책: 전부 "효과 있는 버전"을 기본으로 쓴다

    public static void LoadIntro() => LoadSceneWithRipple(IntroScene);
    public static void LoadLab() => LoadSceneWithRipple(LabScene);
    public static void LoadAccumulate() => LoadSceneWithRipple(AccumulateScene);
    public static void LoadStimulate() => LoadSceneWithRipple(StimulateScene);
    public static void LoadBurst() => LoadSceneWithRipple(BurstScene);
    public static void LoadPurify() => LoadSceneWithRipple(PurifyScene);
    public static void LoadOutro() => LoadSceneWithRipple(OutroScene);

    // ───── 예외: 효과 없이 바로 가는 버전 (자극→표출, 표출→정화에서만 사용) ─────

    public static void LoadBurstDirect() => LoadSceneDirect(BurstScene);
    public static void LoadPurifyDirect() => LoadSceneDirect(PurifyScene);

    // 키보드 디버그용
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) LoadIntro();
        if (Input.GetKeyDown(KeyCode.F2)) LoadLab();
        if (Input.GetKeyDown(KeyCode.F3)) LoadAccumulate();
        if (Input.GetKeyDown(KeyCode.F4)) LoadStimulate();
        if (Input.GetKeyDown(KeyCode.F5)) LoadBurst();
        if (Input.GetKeyDown(KeyCode.F6)) LoadPurify();
        if (Input.GetKeyDown(KeyCode.F7)) LoadOutro();
    }
}
