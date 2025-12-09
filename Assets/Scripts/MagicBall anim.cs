using UnityEngine;
using UnityEngine.VFX;

public class SceneTransitionBall : MonoBehaviour
{
    public VisualEffect vfx;
    public float duration = 2f;      // 커지는 데 걸리는 시간
    public float startSize = 0.1f;
    public float endSize = 15f;

    private float time;
    private bool isPlaying = false;

    // 👇 축적 단계에서 "이제 대기 들어간다" 순간에 이 함수를 호출해주면 됨
    public void PlayExpand()
    {
        time = 0f;
        isPlaying = true;
    }

    void Update()
    {
        if (!isPlaying) return;

        time += Time.deltaTime;
        float t = Mathf.Clamp01(time / duration);

        float size = Mathf.Lerp(startSize, endSize, t);
        vfx.SetFloat("SphereSize", size);

        // 끝까지 다 찼으면 멈추기 (원하면 생략 가능)
        if (t >= 1f)
        {
            isPlaying = false;
        }
    }
}
