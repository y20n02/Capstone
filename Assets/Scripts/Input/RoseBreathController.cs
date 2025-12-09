using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Formats.Alembic.Importer;

public class RoseBreathController : MonoBehaviour
{
    [Header("Alembic 플레이어")]
    public AlembicStreamPlayer alembicPlayer;

    [Tooltip("Alembic 전체 길이(초) — Alembic 컴포넌트에서 Duration 보고 적기")]
    public float alembicDuration = 5f;

    [Header("호흡당 개화량 (0~1)")]
    [Range(0f, 1f)] public float inhaleStep = 0.05f;  // 들숨마다
    [Range(0f, 1f)] public float exhaleStep = 0.10f;  // 날숨마다

    [Header("한 번 움직일 때 걸리는 시간(초)")]
    public float lerpDuration = 1.0f;   // 부드럽게 열리는 시간

    [Header("이벤트")]
    public UnityEvent OnFullyBloomed;
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

        // 🌸 완전 개화 체크
        if (!hasNotifiedFull && currentProgress >= 0.9f)
        {
            hasNotifiedFull = true;
            Debug.Log("[Rose] 완전 개화!");
            OnFullyBloomed?.Invoke();
        }
    }

    /// <summary>
    /// currentProgress(0~1)를 Alembic 시간으로 변환해서 적용
    /// </summary>
    void ApplyToAlembic()
    {
        if (alembicPlayer == null) return;

        float time = Mathf.Clamp(currentProgress * alembicDuration, 0f, alembicDuration);
        alembicPlayer.CurrentTime = time;

        // 필요하면 즉시 업데이트가 보이도록 (버전에 따라 옵션명 다를 수 있음)
        // alembicPlayer.UpdateImmediately();  // 이 함수 있으면 켜고, 없으면 주석 유지
    }

    /// <summary>
    /// 현재 상태에서 delta 만큼 개화 진행 (부드럽게)
    /// </summary>
    void AnimateAdd(float delta)
    {
        startProgress  = currentProgress;
        targetProgress = Mathf.Clamp01(currentProgress + delta);
        lerpTimer      = 0f;
        isAnimating    = true;
    }

    // ==== MotionTrigger 이벤트에서 호출할 함수들 ====

    /// <summary>
    /// 들숨 때 살짝 개화
    /// </summary>
    public void PlayInhaleStep()
    {
        AnimateAdd(inhaleStep);
    }

    /// <summary>
    /// 날숨 때 조금 더 개화
    /// </summary>
    public void PlayExhaleStep()
    {
        AnimateAdd(exhaleStep);
    }

    /// <summary>
    /// 한 번에 만개시키고 싶을 때용 (선택)
    /// </summary>
    public void PlayFullBloom()
    {
        startProgress  = currentProgress;
        targetProgress = 1f;
        lerpTimer      = 0f;
        isAnimating    = true;
    }

    /// <summary>
    /// 외부에서 0~1로 직접 개화 단계 세팅하고 싶을 때
    /// </summary>
    public void SetBreathProgress(float normalized)
    {
        normalized     = Mathf.Clamp01(normalized);

        startProgress  = currentProgress;
        targetProgress = normalized;
        lerpTimer      = 0f;
        isAnimating    = true;
    }

    /// <summary>
    /// 완전 닫힌 상태로 리셋하고 싶을 때
    /// </summary>
    public void ResetToClosed()
    {
        currentProgress = 0f;
        startProgress   = 0f;
        targetProgress  = 0f;
        lerpTimer       = 0f;
        isAnimating     = false;
        hasNotifiedFull = false;
        ApplyToAlembic();
    }
}
