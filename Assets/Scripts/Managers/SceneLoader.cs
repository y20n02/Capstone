using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public const string IntroScene      = "00_Intro";
    public const string LabScene        = "01_Lab";
    public const string AccumulateScene = "02_Accumulate";
    public const string StimulateScene  = "03_Stimulate";
    public const string BurstScene      = "04_Burst";
    public const string PurifyScene     = "05_Purify";
    public const string OutroScene      = "06_Outro";

    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static void LoadIntro()      => LoadScene(IntroScene);
    public static void LoadLab()        => LoadScene(LabScene);
    public static void LoadAccumulate() => LoadScene(AccumulateScene);
    public static void LoadStimulate()  => LoadScene(StimulateScene);
    public static void LoadBurst()      => LoadScene(BurstScene);
    public static void LoadPurify()     => LoadScene(PurifyScene);
    public static void LoadOutro()      => LoadScene(OutroScene);

    // 키보드로 테스트할 때만 사용
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
