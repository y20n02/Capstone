using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("Fade UI")]
    public CanvasGroup fadeCanvas;   // FadeImage의 CanvasGroup
    public float fadeTime = 1f;      // 페이드 속도

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);                  // 매니저 유지
            DontDestroyOnLoad(fadeCanvas.transform.root.gameObject); // Canvas 전체 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 외부에서 씬 전환 호출할 때 사용
    /// </summary>
    public void StartTransition(string nextSceneName)
    {
        StartCoroutine(TransitionRoutine(nextSceneName));
    }

    private IEnumerator TransitionRoutine(string nextSceneName)
    {
        // 필요한 경우: 여기서 VFX 커지는 시간 기다려도 됨
        // yield return new WaitForSeconds(2f);

        // 1) 화면 흰색으로 페이드 인
        float a = 0f;
        while (a < 1f)
        {
            a += Time.deltaTime / fadeTime;
            fadeCanvas.alpha = a;
            yield return null;
        }

        // 2) 씬 로드
        SceneManager.LoadScene(nextSceneName);

        // 씬이 바뀐 뒤 1프레임 기다리기
        yield return null;

        // 3) 다시 페이드 아웃
        a = 1f;
        while (a > 0f)
        {
            a -= Time.deltaTime / fadeTime;
            fadeCanvas.alpha = a;
            yield return null;
        }
    }

    // ★ 테스트용 (원하면 나중에 지워도 됨)
    private void Update()
    {
        // T 키 누르면 강제로 다음 씬으로 전환 테스트
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartTransition("다음씬이름"); // 여기에 실제 씬 이름 넣어서 테스트
        }
    }
}
