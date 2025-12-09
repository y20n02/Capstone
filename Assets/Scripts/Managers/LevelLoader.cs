using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;

    public void LoadNextLevel()
    {
        int current = SceneManager.GetActiveScene().buildIndex;
        int next = current + 1;

        // 마지막 씬이면 더 이상 넘어가지 않도록 방어
        if (next >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[LevelLoader] 다음 씬 인덱스 {next} 가 Build Settings에 없음. (마지막 씬입니다)");
            return;
        }

        StartCoroutine(LoadLevelRoutine(next));
    }

    IEnumerator LoadLevelRoutine(int index)
    {
        if (transition != null)
        {
            transition.SetTrigger("Start");          // FadeOut 트리거
            yield return new WaitForSeconds(transitionTime);
        }

        SceneManager.LoadScene(index);
    }
}
