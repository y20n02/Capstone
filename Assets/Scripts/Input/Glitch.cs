using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlitchController : MonoBehaviour
{
    public Camera targetCamera;
    public int normalRendererIndex = 0;   // PC_Renderer (원래 렌더러)
    public int glitchRendererIndex = 1;   // PC_Renderer 1 (Interferences 들어간 렌더러)

    private UniversalAdditionalCameraData camData;
    private Coroutine pulseRoutine;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        camData = targetCamera.GetComponent<UniversalAdditionalCameraData>();

        // 시작은 항상 정상 화면
        SetNormal();
    }

    public void SetGlitch()
    {
        if (camData == null) return;
        camData.SetRenderer(glitchRendererIndex);
    }

    public void SetNormal()
    {
        if (camData == null) return;
        camData.SetRenderer(normalRendererIndex);
    }

    /// <summary>
    /// duration 동안만 글리치 렌더러 사용 후 자동으로 복구.
    /// </summary>
    public void PulseGlitch(float duration)
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        pulseRoutine = StartCoroutine(PulseCoroutine(duration));
    }

    private System.Collections.IEnumerator PulseCoroutine(float duration)
    {
        SetGlitch();
        yield return new WaitForSeconds(duration);
        SetNormal();
        pulseRoutine = null;
    }
}
