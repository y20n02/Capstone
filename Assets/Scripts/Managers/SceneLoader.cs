using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 🔹 인트로 / 랩 / 카타르시스 공간 / 아웃트로 씬 이름 상수
    public const string IntroScene = "00_Intro";
    public const string LabScene = "01_Lab";
    public const string EmotionScene = "02_EmotionSpace";
    public const string OutroScene = "03_Outro";

    // 🔹 공통 씬 로드 함수 (씬 이름으로 불러오기)
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 🔹 단계별로 편하게 부를 수 있는 함수들
    public static void LoadIntro()
    {
        LoadScene(IntroScene);
    }

    public static void LoadLab()
    {
        LoadScene(LabScene);
    }

    public static void LoadEmotionSpace()
    {
        LoadScene(EmotionScene);
    }

    public static void LoadOutro()
    {
        LoadScene(OutroScene);
    }

    // 🔹 개발 중에 키보드로 씬 전환 테스트 (원하면 삭제해도 됨)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            LoadIntro();            // F1 = 인트로
        if (Input.GetKeyDown(KeyCode.F2))
            LoadLab();              // F2 = 실험실
        if (Input.GetKeyDown(KeyCode.F3))
            LoadEmotionSpace();     // F3 = 카타르시스 공간
        if (Input.GetKeyDown(KeyCode.F4))
            LoadOutro();            // F4 = 아웃트로
    }
}
