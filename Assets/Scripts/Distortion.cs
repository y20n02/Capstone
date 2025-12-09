using System.Collections;
using UnityEngine;

public class SphereRippleController : MonoBehaviour
{
    [Header("Ripple Target")]
    Renderer rend;
    Material mat;

    [Header("Shader Property Names")]
    public string rippleCountProp = "_RippleCount";
    public string rippleSpeedProp = "_RippleSpeed";
    public string rippleContrastProp = "_RippleContrast";

    [Header("Target Values")]
    public float targetRippleCount = 35.6f;
    public float targetRippleSpeed = 0.31f;
    public float targetRippleContrast = 3.2f;

    [Header("Tween Settings")]
    public float duration = 5f;   // 5초 동안 0 → 타겟값

    bool playing = false;

    void Start()
    {
        // 시작할 때는 꺼진 상태로 두고 싶으면
        gameObject.SetActive(false);
    }

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // 이 오브젝트 전용 인스턴스
            mat = rend.material;
        }
    }

    public void PlayAndGoPurify()
    {
        if (playing || mat == null) return;

        // 꺼져있던 울렁 오브젝트 켜기
        gameObject.SetActive(true);

        // 시작값 0으로 초기화 (안전하게)
        mat.SetFloat(rippleCountProp, 0f);
        mat.SetFloat(rippleSpeedProp, 0f);
        mat.SetFloat(rippleContrastProp, 0f);

        StartCoroutine(CoRipple());
    }

    public void Play()
    {
        // 왜곡 구체 오브젝트 켜기
        gameObject.SetActive(true);

        // 만약 머테리얼 파라미터 초기값 세팅하고 싶으면 여기서 해도 됨
        // 예시:
        // var mat = GetComponent<Renderer>().material;
        // mat.SetFloat("_Distortion", 0f);
    }

    IEnumerator CoRipple()
    {
        playing = true;

        float t = 0f;

        // 시작값은 0으로 가정 (필요하면 mat.GetFloat 로 현재값 읽어와도 됨)
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);

            float count = Mathf.Lerp(0f, targetRippleCount, k);
            float speed = Mathf.Lerp(0f, targetRippleSpeed, k);
            float contrast = Mathf.Lerp(0f, targetRippleContrast, k);

            mat.SetFloat(rippleCountProp, count);
            mat.SetFloat(rippleSpeedProp, speed);
            mat.SetFloat(rippleContrastProp, contrast);

            yield return null;
        }

        // 마지막에 정확히 타겟값 고정
        mat.SetFloat(rippleCountProp, targetRippleCount);
        mat.SetFloat(rippleSpeedProp, targetRippleSpeed);
        mat.SetFloat(rippleContrastProp, targetRippleContrast);

        // 다 끝나면 Purify 씬으로
        SceneLoader.LoadPurify();

        playing = false;
    }
}
