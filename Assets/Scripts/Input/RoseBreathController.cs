using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Formats.Alembic.Importer;
using System.Collections;

public class RoseBreathController : MonoBehaviour
{
    [Header("Alembic 플레이어")]
    public AlembicStreamPlayer alembicPlayer;

    [Tooltip("Alembic 전체 길이(초) — Alembic 컴포넌트에서 Duration 보고 적기")]
    public float alembicDuration = 5f;

    [Header("연출 오브젝트")]
    [Tooltip("완전 개화 후 켜질 Splash 이펙트 오브젝트")]
    public GameObject splashObject;   // Splash 오브젝트

    [Header("호흡당 개화량 (0~1)")]
    [Range(0f, 1f)] public float inhaleStep = 0.05f;  // 들숨마다
    [Range(0f, 1f)] public float exhaleStep = 0.10f;  // 날숨마다

    [Header("한 번 움직일 때 걸리는 시간(초)")]
    public float lerpDuration = 1.0f;   // 부드럽게 열리는 시간

    [Header("이벤트")]
    public UnityEvent OnFullyBloomed;

    
    [Header("연출 타이밍 설정")]
    public float splashDuration = 2.5f;   // ⭐ 스플래시 유지 시간
    public float fadeOutDuration = 2.5f;  // ⭐ 페이드아웃 시간

    // 내부 상태
    float currentProgress = 0f;   // 0=완전 봉우리, 1=완전 개화
    float startProgress;
    float targetProgress;
    float lerpTimer;
    bool isAnimating;

    bool hasNotifiedFull = false;

    void Awake()
    {
        if (alembicPlayer == null)
            alembicPlayer = GetComponent<AlembicStreamPlayer>();
    }

    void Start()
    {
        // 처음에는 완전 닫힌 상태에서 시작
        currentProgress = 0f;
        startProgress   = 0f;
        targetProgress  = 0f;
        lerpTimer       = 0f;
        isAnimating     = false;
        hasNotifiedFull = false;

        // Splash는 처음에 꺼진 상태로 시작
        if (splashObject != null)
        {
            splashObject.SetActive(false);

            var ps = splashObject.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        ApplyToAlembic();
    }

    void Update()
    {
        if (!isAnimating) return;

        lerpTimer += Time.deltaTime;
        float t = Mathf.Clamp01(lerpTimer / lerpDuration);

        float p = Mathf.Lerp(startProgress, targetProgress, t);
        currentProgress = p;
        ApplyToAlembic();

        if (t >= 1f)
            isAnimating = false;

        // 🌸 완전 개화 체크 (0.9 이상이면 한 번만)
        if (!hasNotifiedFull && currentProgress >= 0.8f)
        {
            hasNotifiedFull = true;
            Debug.Log("[Rose] 완전 개화!");

            StartCoroutine(PlayFinalSequence());

            // 외부 이벤트도 같이 호출 (필요하면)
            OnFullyBloomed?.Invoke();
        }
    }

    IEnumerator PlayFinalSequence()
    {   
        Debug.Log("[Rose] 스플래시 시작");
        // 1) 스플래시 켜기
        if (splashObject != null)
        {
            splashObject.SetActive(true);
            var ps = splashObject.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Clear();
                ps.Play();
            }
        }

        // ⭐ Splash 오래 보여주기
        yield return new WaitForSeconds(splashDuration);

        Debug.Log("[Rose] 스플래시 종료");

        // ⭐ Splash 끄기
        if (splashObject != null)
        {
            var ps = splashObject.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            splashObject.SetActive(false);
        }

        // 3) 검은 화면 페이드아웃
        if (UIOutroFader.Instance != null)
        {
            Debug.Log("[Rose] 페이드아웃 시작");
            UIOutroFader.Instance.FadeOut(fadeOutDuration);
        }
        else
        {
            Debug.LogWarning("[Rose] UIOutroFader.Instance 가 null 이라 페이드 못함");
        }

        yield return new WaitForSeconds(fadeOutDuration);

        // 4) 영상 재생
        Debug.Log("[Rose] Outro 씬 로드");
        SceneLoader.LoadOutro();
        
    }
    
    void ApplyToAlembic()
    {
        if (alembicPlayer == null) return;

        float time = Mathf.Clamp(currentProgress * alembicDuration, 0f, alembicDuration);
        alembicPlayer.CurrentTime = time;
    }

    void AnimateAdd(float delta)
    {
        startProgress  = currentProgress;
        targetProgress = Mathf.Clamp01(currentProgress + delta);
        lerpTimer      = 0f;
        isAnimating    = true;
    }

    // ==== MotionTrigger에서 호출 ====
    public void PlayInhaleStep()  => AnimateAdd(inhaleStep);
    public void PlayExhaleStep()  => AnimateAdd(exhaleStep);

    public void PlayFullBloom()
    {
        startProgress  = currentProgress;
        targetProgress = 1f;
        lerpTimer      = 0f;
        isAnimating    = true;
    }

    public void SetBreathProgress(float normalized)
    {
        normalized     = Mathf.Clamp01(normalized);
        startProgress  = currentProgress;
        targetProgress = normalized;
        lerpTimer      = 0f;
        isAnimating    = true;
    }

    public void ResetToClosed()
    {
        currentProgress = 0f;
        startProgress   = 0f;
        targetProgress  = 0f;
        lerpTimer       = 0f;
        isAnimating     = false;
        hasNotifiedFull = false;

        if (splashObject != null)
        {
            var ps = splashObject.GetComponent<ParticleSystem>();
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            splashObject.SetActive(false);
        }

        ApplyToAlembic();
    }
}
