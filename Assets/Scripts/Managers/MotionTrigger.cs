using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Leap;

public class MotionTrigger : MonoBehaviour
{
    public enum Phase
    {
        Intro,          // 인트로 : 손 올려두기
        Accumulate,     // 1단계 : 축적 (쌓다 제스처)
        Stimulate,      // 2단계 : 자극 (막 휘두르는 제스처들)
        Burst,          // 3단계 : 표출 (한 번 세게 내리찍기 / 손가락 탭)
        Purify          // 4단계 : 정화 (올렸다 내리기 3회)
    }

    // 자극 단계에서 얼마나 흔들었는지 결과 (표출 연출에서 사용)
    public enum StimulateLevel
    {
        None,   // 거의 자극 안 함
        Low,    // 조금만 한 상태
        High    // 충분히 자극한 상태
    }

    [Header("Leap")]
    public LeapServiceProvider provider;

    [Header("현재 단계 (씬 별로 설정)")]
    public Phase currentPhase = Phase.Intro;

    // ========== Stimulate 결과 (Burst에서 사용) ==========
    [Header("Stimulate Result (for Burst)")]
    public StimulateLevel lastStimulateLevel = StimulateLevel.None;
    [Range(0f, 1f)]
    public float lastStimulateRatio = 0f;   // 0~1 비율 (stimulateCount / stimulateTargetCount)

    // ========== Intro (손 올려두기) ==========
    [Header("Intro Settings")]
    public float introRequiredSteadyTime = 5f;  // 5초 유지
    public float introSpeedThreshold = 0.3f;    // 거의 안 움직이는 기준 (mm/s)
    public UnityEvent OnIntroComplete;

    private float introSteadyTimer = 0f;
    private bool introDone = false;   // ★ 여러 번 실행 방지

    [Header("Intro UI / Flow")]
    public CircleLoading introLoading;       // 원형 로딩 UI
    public GameObject introCheckCanvas;      // 체크 화면 캔버스
    public GameObject introDistortionObj;    // 왜곡 효과 오브젝트
    public float introDistortionDuration = 5f;


    // ========== Accumulate (축적 : 쌓다 제스처) ==========
    [Header("Accumulate Settings")]
    [Tooltip("손바닥이 아래를 향했다고 볼 기준 (palmNormal.y < 이 값이면 손바닥 아래)")]
    public float accumulatePalmNormalDown = -0.3f;

    [Tooltip("손을 위로 올릴 때 필요한 최소 속도 (mm/s, +Y 방향)")]
    public float accumulateDownSpeedThreshold = 0.8f;

    [Tooltip("맥시멈(최대로 쌓아야 하는 횟수)")]
    public int accumulateMaxCount = 10;

    [Header("Accumulate Timing")]
    [Tooltip("첫 카운트 이후, 이 시간 동안 맥시멈을 못 채우면 자동으로 다음 단계로 이동 (카운트 0일 때는 시간 안 감)")]
    public float accumulatePlayWindow = 30f;   // 예: 30초

    [Tooltip("한 번 인식된 뒤 다음 인식까지 최소 대기시간")]
    public float accumulateCooldown = 0.35f;

    public UnityEvent OnAccumulateTrigger;    // 한 번 "쌓았다!" 할 때
    public UnityEvent OnAccumulateComplete;   // Accumulate 단계 종료 시 (맥시멈 or 타임아웃)

    private int accumulateCount = 0;
    private float accumulateCooldownTimer = 0f;
    private bool wasUpGesture = false;       // 바로 이전 프레임에서 위로 올리는 중이었는지

    // "카운트 1 이상"부터 흐르는 시간
    private float accumulateSinceFirstCount = 0f;
    private bool accumulateHasAnyCount = false;      // 카운트를 한 번이라도 했는가

    // === 축적 연출용 파티클 & 라이트 ===
    [Header("축적 파티클")]
    public ParticleSystem paperParticle;          // 한 번 쌓을 때마다 툭툭 떨어지는 종이
    public ParticleSystem accumulateLoopParticle; // 축적 단계 동안 계속 도는 루프 파티클

    [Header("축적 반짝 파티클 (새로 추가되는 효과)")]
    public ParticleSystem accumulateSparkParticle;  // ✨ 반짝이 파티클
    public float sparkRateMin = 2f;                 // 모션 1번쯤일 때 Rate
    public float sparkRateMax = 14f;                // 맥시멈일 때 Rate

    // ========== Stimulate (자극: 4방향 스와이프) ==========
    [Header("Stimulate Settings")]
    public int stimulateTargetCount = 15;

    [Tooltip("좌우 스와이프 속도 임계값 (mm/s)")]
    public float stimulateSwipeHorizontalThreshold = 1.5f;

    [Tooltip("위아래 스와이프 속도 임계값 (mm/s)")]
    public float stimulateSwipeVerticalThreshold = 1.5f;

    [Header("Stimulate Timing")]
    [Tooltip("첫 스와이프 이후, 이 시간 동안 목표치 미달이면 자동으로 다음 단계로 이동 (스와이프 0일 때는 시간 안 감)")]
    public float stimulatePlayWindow = 30f;

    [Tooltip("연속 스와이프 사이 최소 간격")]
    public float stimulateCooldown = 0.2f;

    public UnityEvent OnStimulateMotion;        // 한 번 휘두를 때마다
    public UnityEvent OnStimulateComplete;      // Stimulate 단계 종료 시 (목표 달성 or 타임아웃)

    private int stimulateCount = 0;
    private float stimulateCooldownTimer = 0f;

    // "스와이프 1 이상"부터 흐르는 시간
    private float stimulateSinceFirstSwipe = 0f;
    private bool stimulateHasAnySwipe = false;      // 한 번이라도 스와이프 했는가

    [Header("Glitch")]
    public GlitchController glitchController;

    // ========== Burst (표출: 손바닥 기준 콕 탭 + 자극량에 따른 연출 분기) ==========
    [Header("Burst Settings")]
    [Tooltip("손바닥이 아래로 내려가는 최소 속도 (mm/s, -Y 방향)")]
    public float burstTapDownSpeedThreshold = -0.8f;

    [Tooltip("탭 동작일 때 허용되는 최대 좌우/앞뒤 속도 (너무 대각선이면 제외)")]
    public float burstMaxHorizontalSpeed = 1.5f;

    [Tooltip("손바닥 전체가 같이 세게 움직이면 제외하고 싶을 때 기준 속도")]
    public float burstMaxPalmSpeed = 2.5f;

    [Tooltip("약간 쥔 상태(검지+엄지) 조건을 줄 경우 핀치 최소값")]
    public float burstMinPinch = 0.3f;

    [Header("Burst Sequence Timing")]
    [Tooltip("불꽃놀이 파티클을 보여주는 시간 (초)")]
    public float fireworkDuration = 14f;

    [Header("Burst Fireworks")]
    public FireworkActivator fireworkActivator;

    [Header("Burst Event (단일 연출)")]
    public UnityEvent OnBurst;

    private bool burstFired = false;

    [Header("Burst UI 숨기기")]
    public UIFadeController burstUI;   // ← Burst 안내 UI

    [Header("Burst Distortion Sphere")]
    public SphereRippleController rippleSphere;

    // ========== Purify (정화: 올렸다 내리기 3회) ==========
    [Header("Purify Settings")]
    public int purifyTargetBreathCount = 3;

    // 천천히 올리고 내리는 동작도 인식되도록 (Inspector에서 1 / -1로 세팅해도 됨)
    public float purifyUpSpeedThreshold = 1f;    // 위로 +Y
    public float purifyDownSpeedThreshold = -1f;   // 아래로 -Y

    public float purifyPalmUp = 0.3f;   // 손바닥이 위를 향할 때 palmNormal.y 기준 (참고용)
    public float purifyPalmDown = -0.3f;  // 손등이 위를 향할 때 palmNormal.y 기준 (참고용)

    [Tooltip("호흡 동작이 끝났다고 보는 속도 (거의 정지, velY 기준)")]
    public float purifyIdleSpeedThreshold = 0.1f;

    [Tooltip("한 호흡이 끝난 후 다음 호흡까지 최소 대기 시간(초)")]
    public float purifyBetweenCycleCooldown = 0.4f;

    [Tooltip("정화 단계에서 양손이 모두 보여야만 인식할지 여부")]
    public bool requireBothHandsForPurify = true;

    public UnityEvent OnPurifyOneCycle;   // 1회 호흡 완료 시
    public UnityEvent OnPurifyComplete;   // 3회 완료 시

    public UnityEvent OnPurifyInhale;     // ★ 들숨 시작 시
    public UnityEvent OnPurifyExhale;     // ★ 날숨 시작 시

    [Header("Purify → Rose")]
    public RoseBreathController roseBreathController;
    private int purifyBreathCount = 0;
    private enum PurifyState { Idle, Inhaling, Exhaling }
    private PurifyState purifyState = PurifyState.Idle;
    private float purifyCooldownTimer = 0f;

    private float purifyExhaleTimer = 0f;  // 날숨 멈춤 시간 체크용

    // =========================================================
    // Update
    // =========================================================
    void Update()
    {
        if (provider == null)
        {
            Debug.LogWarning("[MotionDetector] provider가 비어있어요. Service Provider(Desktop)를 Inspector에 연결해줘.");
            return;
        }

        Frame frame = provider.CurrentFrame;

        // ★ 손이 하나도 안 잡히는 경우 처리
        if (frame == null || frame.Hands == null || frame.Hands.Count == 0)
        {
            // Accumulate : 이미 한 번이라도 쌓았으면(카운트 ≥ 1) 시간 진행
            if (currentPhase == Phase.Accumulate && accumulateHasAnyCount)
            {
                accumulateSinceFirstCount += Time.deltaTime;
                CheckAccumulateTimeout();
            }

            // Stimulate : 이미 한 번이라도 스와이프 했으면(카운트 ≥ 1) 시간 진행
            if (currentPhase == Phase.Stimulate && stimulateHasAnySwipe)
            {
                stimulateSinceFirstSwipe += Time.deltaTime;
                CheckStimulateTimeout();
            }

            // Purify / Intro 같은 건 기존처럼 상태만 리셋
            ResetPerPhaseState();
            return;
        }

        // ★ 정화 단계에서 양손이 필요하다면, 손이 2개 미만일 때는 정화 제스처 무시
        if (currentPhase == Phase.Purify && requireBothHandsForPurify && frame.Hands.Count < 2)
        {
            purifyState = PurifyState.Idle;
            return;
        }

        // 한 손 기준 (첫 번째 손)
        Hand hand = frame.Hands[0];
        Vector3 palmVel = hand.PalmVelocity;
        Vector3 palmNormal = hand.PalmNormal;

        float speed = palmVel.magnitude;            // 총 속도 크기
        float grab = hand.GrabStrength;            // 0~1
        float pinch = hand.PinchStrength;           // 0~1

        // 공용 타이머 업데이트
        if (accumulateCooldownTimer > 0f) accumulateCooldownTimer -= Time.deltaTime;
        if (stimulateCooldownTimer > 0f) stimulateCooldownTimer -= Time.deltaTime;
        if (purifyCooldownTimer > 0f) purifyCooldownTimer -= Time.deltaTime;

        switch (currentPhase)
        {
            case Phase.Intro:
                UpdateIntro(speed);
                break;
            case Phase.Accumulate:
                UpdateAccumulate(palmVel, palmNormal);
                break;
            case Phase.Stimulate:
                UpdateStimulate(palmVel, grab, pinch);
                break;
            case Phase.Burst:
                UpdateBurst(palmVel, pinch);
                break;
            case Phase.Purify:
                UpdatePurify(palmVel, palmNormal);
                break;
        }
    }

    // =========================================================
    // Intro: 손 올려두고 5초 유지
    // =========================================================
    void UpdateIntro(float speed)
{
    if (introDone) return;   // 이미 끝났으면 무시

    if (speed < introSpeedThreshold)
    {
        // 🔥 로딩이 "처음 시작되는 순간" : 타이머가 0에서 증가하기 시작할 때
        if (introSteadyTimer == 0f && introLoading != null)
        {
            introLoading.PlayStartSoundOnce();
        }

        // 손이 거의 안 움직이면 타이머 증가
        introSteadyTimer += Time.deltaTime;

        // 0~1 비율 계산해서 로딩 UI 채우기
        float ratio = Mathf.Clamp01(introSteadyTimer / introRequiredSteadyTime);
        if (introLoading != null)
            introLoading.SetProgress(ratio);

        // 5초 채워졌으면 완료 처리
        if (introSteadyTimer >= introRequiredSteadyTime)
        {
            introDone = true;
            Debug.Log("[Intro] 5초 유지 + 로딩 100% 완료!");

            OnIntroComplete?.Invoke();

            StartCoroutine(IntroFlow());
        }
    }
    else
    {
        // 손이 흔들리면 타이머/로딩 리셋
        introSteadyTimer = 0f;
        if (introLoading != null)
            introLoading.ResetProgress();
    }
}


    // =========================================================
    // Accumulate: 손바닥 아래 + 위로 빠르게 올릴 때 "쌓았다" 한 번
    //  - 카운트 0일 때는 시간 안 감
    //  - 카운트 1 이상이면 accumulatePlayWindow 만큼 시간 흐름
    //  - 맥시멈 도달 시 즉시 다음 단계
    // =========================================================
    void UpdateAccumulate(Vector3 palmVel, Vector3 palmNormal)
    {
        // 카운트 1 이상일 때만 시간 흐르게
        if (accumulateHasAnyCount)
            accumulateSinceFirstCount += Time.deltaTime;

        bool palmFacingDown = (palmNormal.y < accumulatePalmNormalDown);
        bool movingUpFast = (palmVel.y > accumulateDownSpeedThreshold);

        bool upGestureNow = palmFacingDown && movingUpFast;
        bool pressedNow = upGestureNow && !wasUpGesture;

        if (accumulateCooldownTimer <= 0f && pressedNow)
        {
            accumulateCount++;
            accumulateHasAnyCount = true;
            accumulateSinceFirstCount = 0f;
            accumulateCooldownTimer = accumulateCooldown;

            Debug.Log($"[Accumulate] 쌓았다! count = {accumulateCount}");
            OnAccumulateTrigger?.Invoke();

            // 🔵 여기서 루프 파티클 세기 업데이트!!
            UpdateAccumulateLoopRate();

            // 1회째부터 루프 파티클 켜기
            if (accumulateCount == 1 &&
                accumulateLoopParticle != null &&
                !accumulateLoopParticle.isPlaying)
            {
                accumulateLoopParticle.Play();
            }

            // 종이 파티클
            OnAccumulateMotion();

            // ⭐ 맥시멈 도달 시 바로 다음 단계
            if (accumulateCount >= accumulateMaxCount)
            {
                Debug.Log("[Accumulate] 맥시멈 달성! 다음 단계(Stimulate) 이동");
                OnAccumulateComplete?.Invoke();
                return;
            }
        }

        wasUpGesture = upGestureNow;

        CheckAccumulateTimeout();
    }


    // =========================================================
    // Stimulate: 4방향 스와이프 (좌 / 우 / 위 / 아래)
    //  - 스와이프 0일 때는 시간 안 감
    //  - 스와이프 1 이상이면 stimulatePlayWindow 만큼 시간 흐름
    //  - 목표 카운트 도달 시 즉시 Burst로
    // =========================================================
    void UpdateStimulate(Vector3 palmVel, float grab, float pinch)
    {
        // 스와이프를 한 번이라도 했을 때만(카운트 ≥ 1) 시간 흐르게
        if (stimulateHasAnySwipe)
            stimulateSinceFirstSwipe += Time.deltaTime;

        // 쿨다운
        if (stimulateCooldownTimer > 0f)
            stimulateCooldownTimer -= Time.deltaTime;

        // 1) 4방향 스와이프 인식
        float vx = palmVel.x;
        float vy = palmVel.y;

        bool swipeLeft = vx < -stimulateSwipeHorizontalThreshold;
        bool swipeRight = vx > stimulateSwipeHorizontalThreshold;
        bool swipeUp = vy > stimulateSwipeVerticalThreshold;
        bool swipeDown = vy < -stimulateSwipeVerticalThreshold;

        bool swipeNow = (swipeLeft || swipeRight || swipeUp || swipeDown);

        if (stimulateCooldownTimer <= 0f && swipeNow)
        {
            stimulateCount++;
            stimulateHasAnySwipe = true;
            stimulateSinceFirstSwipe = 0f;
            stimulateCooldownTimer = stimulateCooldown;

            string dir = "Unknown";
            if (swipeLeft) dir = "Left";
            else if (swipeRight) dir = "Right";
            else if (swipeUp) dir = "Up";
            else if (swipeDown) dir = "Down";

            Debug.Log($"[Stimulate] Swipe {dir}! count = {stimulateCount}");
            OnStimulateMotion?.Invoke();

            // ★ 글리치 트리거
            if (glitchController != null)
            {
                glitchController.PulseGlitch(0.6f);
            }

            // 목표 카운트 채우면 → 시간 상관없이 바로 Burst로
            if (stimulateCount >= stimulateTargetCount)
            {
                if (stimulateCount > stimulateTargetCount)
                    stimulateCount = stimulateTargetCount;

                ComputeStimulateResult();
                Debug.Log("[Stimulate] 목표치 달성! 표출 단계(Burst)로 이동");
                OnStimulateComplete?.Invoke();
                return;
            }
        }

        // 타임아웃 체크 (스와이프 ≥ 1일 때만)
        CheckStimulateTimeout();
    }

    void CheckStimulateTimeout()
    {
        if (!stimulateHasAnySwipe)
            return;

        if (stimulateCount >= stimulateTargetCount)
            return;

        if (stimulateSinceFirstSwipe >= stimulatePlayWindow)
        {
            ComputeStimulateResult();
            Debug.Log("[Stimulate] 첫 스와이프 이후 제한 시간 초과 → 다음 단계(Burst) 이동");
            OnStimulateComplete?.Invoke();
        }
    }

    // =========================================================
    // Stimulate 결과 계산 (Burst 연출에서 사용)
    // =========================================================
    void ComputeStimulateResult()
    {
        // 안전장치: 목표 카운트가 0 이하로 셋팅된 이상한 상황 방지
        if (stimulateTargetCount <= 0)
        {
            lastStimulateRatio = 1f;

            if (stimulateCount == 0)
            {
                lastStimulateLevel = StimulateLevel.None;
                Debug.Log("[Stimulate] 결과: 자극 거의 없음 (target<=0, count=0)");
            }
            else
            {
                lastStimulateLevel = StimulateLevel.High;
                Debug.Log("[Stimulate] 결과: target<=0 이라 강한 자극으로 처리 (count>0)");
            }
            return;
        }

        // 0~1 비율은 참고용
        lastStimulateRatio = Mathf.Clamp01((float)stimulateCount / (float)stimulateTargetCount);

        if (stimulateCount == 0)
        {
            lastStimulateLevel = StimulateLevel.None;
            Debug.Log($"[Stimulate] 결과: 자극 거의 없음 / count=0, ratio={lastStimulateRatio:F2}, level={lastStimulateLevel}");
        }
        else if (stimulateCount < stimulateTargetCount)
        {
            // 목표 미달
            lastStimulateLevel = StimulateLevel.Low;
            Debug.Log($"[Stimulate] 결과: 일부 자극 (목표 미달) / count={stimulateCount}, target={stimulateTargetCount}, ratio={lastStimulateRatio:F2}, level={lastStimulateLevel}");
        }
        else
        {
            // 목표 이상 (맥시멈 or 초과)
            lastStimulateLevel = StimulateLevel.High;
            Debug.Log($"[Stimulate] 결과: 충분한 자극 (목표 도달) / count={stimulateCount}, target={stimulateTargetCount}, ratio={lastStimulateRatio:F2}, level={lastStimulateLevel}");
        }
    }

    // =========================================================
    // Burst: 손바닥 기준으로 "콕" 찍는 동작 감지
    // =========================================================
    void UpdateBurst(Vector3 palmVel, float pinch)
    {
        if (burstFired) return;

        bool fastTapDown = palmVel.y < burstTapDownSpeedThreshold;
        bool smallHorizontal = Mathf.Abs(palmVel.x) < burstMaxHorizontalSpeed &&
                               Mathf.Abs(palmVel.z) < burstMaxHorizontalSpeed;
        bool palmNotTooFast = palmVel.magnitude < burstMaxPalmSpeed;
        bool pinchOn = pinch > burstMinPinch;

        if (fastTapDown && smallHorizontal && palmNotTooFast && pinchOn)
        {
            burstFired = true;

            // 1) Burst 안내 UI 숨기기
            if (burstUI != null)
            burstUI.FadeOutAndDisable();

            Debug.Log($"[Burst] 표출 연출 실행 (단일) / count={stimulateCount}, ratio={lastStimulateRatio:F2}");

            // 2) 불꽃놀이 전부 발사
            if (fireworkActivator != null)
                fireworkActivator.ActivateAll();

            // 3) 기존 Burst 이벤트 (사운드, 카메라 등)
            OnBurst?.Invoke();

            // 🔥 4) "불꽃 7초 + 울렁 5초" 시퀀스 시작
            StartCoroutine(BurstFlow());
        }
    }

    // =========================================================
    // Purify: 올렸다(들숨) → 내렸다(날숨) 3회
    // =========================================================
    void UpdatePurify(Vector3 palmVel, Vector3 palmNormal)
    {
        float velY = palmVel.y;
        float normalY = palmNormal.y;

        // 조건 기준값
        float inhaleSpeed = 0.3f;     // 들숨 속도
        float exhaleSpeed = -0.3f;    // 날숨 속도
        float stopSpeed = -0.1f;      // 멈췄다고 보는 속도
        float inhalePalm = 0.3f;      // 손바닥 위
        float exhalePalm = -0.3f;     // 손등 위

        // 2초 정지 타이머
        const float exhaleStopTime = 2.0f;

        // 한 호흡 후 쿨다운 적용
        if (purifyCooldownTimer > 0f)
        {
            purifyCooldownTimer -= Time.deltaTime;
            return;
        }

        switch (purifyState)
        {
            case PurifyState.Idle:
                // 들숨 시작 (손바닥 위 + 위로 이동)
                if (velY > inhaleSpeed && normalY > inhalePalm)
                {
                    purifyState = PurifyState.Inhaling;
                    Debug.Log("[Purify] 들숨 시작");

                    if (roseBreathController != null)
                        roseBreathController.PlayInhaleStep();
                }
                break;

            case PurifyState.Inhaling:
                // 날숨 시작 (손등 위 + 아래로 이동)
                if (velY < exhaleSpeed && normalY < exhalePalm)
                {
                    purifyState = PurifyState.Exhaling;
                    purifyExhaleTimer = 0f;
                    Debug.Log("[Purify] 날숨 시작");

                    if (roseBreathController != null)
                        roseBreathController.PlayExhaleStep();
                }
                break;

            case PurifyState.Exhaling:
                // 아직 내려가는 중이면 타이머 초기화
                if (velY < stopSpeed)
                {
                    purifyExhaleTimer = 0f;
                }
                else
                {
                    // 거의 멈춘 상태 유지 시간 증가
                    purifyExhaleTimer += Time.deltaTime;

                    if (purifyExhaleTimer >= exhaleStopTime)
                    {
                        purifyBreathCount++;
                        purifyCooldownTimer = 0.5f;

                        Debug.Log($"[Purify] 1회 호흡 완료! count={purifyBreathCount}");
                        OnPurifyOneCycle?.Invoke();

                        // 꽃 개화 진행도 업데이트
                        if (roseBreathController != null && purifyTargetBreathCount > 0)
                        {
                            float progress = Mathf.Clamp01(
                                (float)purifyBreathCount / (float)purifyTargetBreathCount
                            );
                            roseBreathController.SetBreathProgress(progress);
                        }

                        // 다음 사이클 준비
                        purifyState = PurifyState.Idle;
                    }
                }
                break;
        }
    }


    // =========================================================
    // 손이 안 보일 때 상태 리셋
    // =========================================================
    void ResetPerPhaseState()
    {
        if (currentPhase == Phase.Intro)
        {
            introSteadyTimer = 0f;
            introDone = false;
            if (introLoading != null)
                introLoading.ResetProgress();
        }
        else if (currentPhase == Phase.Purify)
        {
            purifyState = PurifyState.Idle;
            purifyCooldownTimer = 0f;
        }
        // Accumulate / Stimulate / Burst는 카운트/플래그 유지하는 쪽이 체험상 더 자연스러워서 그대로 둠
    }

    // 디버깅용 리셋 함수 (Inspector에서 버튼으로 호출해도 됨)
    public void ResetAll()
    {
        introSteadyTimer = 0f;

        // Accumulate 관련
        accumulateCount = 0;
        accumulateSinceFirstCount = 0f;
        accumulateHasAnyCount = false;
        wasUpGesture = false;
        accumulateCooldownTimer = 0f;

        // Stimulate 관련
        stimulateCount = 0;
        stimulateCooldownTimer = 0f;
        stimulateSinceFirstSwipe = 0f;
        stimulateHasAnySwipe = false;
        lastStimulateLevel = StimulateLevel.None;
        lastStimulateRatio = 0f;

        // Burst 관련
        burstFired = false;

        // Purify 관련
        purifyBreathCount = 0;
        purifyState = PurifyState.Idle;
        purifyCooldownTimer = 0f;
        purifyExhaleTimer = 0f;
    }

    private System.Collections.IEnumerator IntroFlow()
    {
        // 1) 체크 화면 켜기
        if (introCheckCanvas != null)
            introCheckCanvas.SetActive(true);

        // 2) 왜곡 효과 켜기
        if (introDistortionObj != null)
            introDistortionObj.SetActive(true);

        // 3) 5초 동안 유지
        yield return new WaitForSeconds(introDistortionDuration);

        // 4) 왜곡 효과 끄기 (필요하면)
        if (introDistortionObj != null)
            introDistortionObj.SetActive(false);

        // 5) 다음 씬으로 이동 (원하는 방식 사용)
        // SceneLoader.LoadAccumulate();  // 네가 쓰는 SceneLoader에 맞게 변경
        // 혹은
        // UnityEngine.SceneManagement.SceneManager.LoadScene("AccumulateScene");
    }

    // =========================================================
    // Accumulate 모션 시 파티클 연출
    // =========================================================
    public void OnAccumulateMotion()
    {
        if (paperParticle == null) return;

        // 쌓을수록 더 많이 떨어지도록 (최대 40장)
        int totalEmit = Mathf.Clamp(5 + accumulateCount * 3, 5, 40);

        StartCoroutine(EmitPaperBurst(totalEmit));
    }

    private IEnumerator EmitPaperBurst(int totalEmit)
    {
        int emitted = 0;

        while (emitted < totalEmit)
        {
            // 한 번에 나갈 양 (3장씩 뿌리기)
            int batch = Mathf.Min(3, totalEmit - emitted);

            paperParticle.Emit(batch);
            emitted += batch;

            // 0.03~0.05 정도 간격으로 나눠서 떨어지게
            yield return new WaitForSeconds(0.05f);
        }
    }

    void CheckAccumulateTimeout()
    {
        // 아직 한 번도 쌓지 않았으면(카운트 0) 시간 안 감 → 타임아웃 없음
        if (!accumulateHasAnyCount)
            return;

        // 이미 맥시멈 채웠으면 여기 들어오지 않게 처리됨
        if (accumulateCount >= accumulateMaxCount)
            return;

        // 첫 카운트 이후 PlayWindow 초가 지나면 자동으로 다음 단계
        if (accumulateSinceFirstCount >= accumulatePlayWindow)
        {
            Debug.Log("[Accumulate] 첫 제스처 이후 제한 시간 초과 → 다음 단계(Stimulate) 이동");
            OnAccumulateComplete?.Invoke();
        }
    }

    private void UpdateAccumulateLoopRate()
    {
        if (accumulateLoopParticle == null)
            return;

        float t = 0f;
        if (accumulateMaxCount > 0)
            t = Mathf.Clamp01((float)accumulateCount / (float)accumulateMaxCount);

        float newRate = Mathf.Lerp(sparkRateMin, sparkRateMax, t);

        var emission = accumulateLoopParticle.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(newRate);
    }
    // =========================================================
    // 정화
    // =========================================================

    public void OnRoseFullyBloomed()
    {
        Debug.Log("[Purify] 꽃이 완전히 피었습니다 → 정화 완료");
        OnPurifyComplete?.Invoke();
    }

    // 외부에서 직접 Purify 씬으로 보내고 싶을 때 쓸 수 있는 헬퍼
    public void GoToPurify()
    {
        SceneLoader.LoadPurify();
    }

    public void GoToStimulate()
    {
        SceneLoader.LoadStimulate();   // 🔵 여기서 Ripple + 씬 전환 한 번에 처리
    }


    // =========================================================
    // Burst Flow: 불꽃 7초 → 울렁 5초(별도 스크립트) → Purify 씬
    // =========================================================
    private IEnumerator BurstFlow()
    {
        // 1) 불꽃놀이 fireworkDuration초 동안 보여주기
        if (fireworkDuration > 0f)
        {
            Debug.Log($"[Burst] 불꽃 연출 {fireworkDuration}초 동안 재생");
            yield return new WaitForSeconds(fireworkDuration);
        }

        // 2) 불꽃놀이 5초 동안 서서히 꺼지게 만들기
        if (fireworkActivator != null)
        {
            Debug.Log("[Burst] 불꽃 페이드아웃 시작 (5초)");
            yield return StartCoroutine(fireworkActivator.FadeOutAll(5f));
            // ⭐ 5초 동안 천천히 페이드
        }

        // 3) 페이드아웃이 끝난 뒤 울렁효과 시작
        if (rippleSphere != null)
        {
            Debug.Log("[Burst] 울렁 효과 시작");
            rippleSphere.PlayAndGoPurify();
        }
    }



}
