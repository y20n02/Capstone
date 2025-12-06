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

    // ========== Accumulate (축적 : 쌓다 제스처) ==========
    [Header("Accumulate Settings")]
    [Tooltip("손바닥이 아래를 향했다고 볼 기준 (palmNormal.y < 이 값이면 손바닥 아래)")]
    public float accumulatePalmNormalDown = -0.3f;

    [Tooltip("손을 위로 올릴 때 필요한 최소 속도 (mm/s, +Y 방향)")]
    public float accumulateDownSpeedThreshold = 0.8f;

    [Tooltip("맥시멈(최대로 쌓아야 하는 횟수)")]
    public int accumulateMaxCount = 10;

    [Header("Accumulate Timing")]
    [Tooltip("Accumulate 단계 들어온 뒤, 이 시간 동안 카운트가 전혀 없으면 다음단게로 넘어가는 타이머 시작")]
    public float accumulateNoCountTimeout = 10f;      // 10초

    [Tooltip("첫 카운트 후 / 또는 NoCountTimeout 이후, 이 시간 동안 맥시멈 못 채우면 자동 이동 준비")]
    public float accumulatePlayWindow = 25f;          // 20~30초 중간값

    [Tooltip("타임아웃 판단된 뒤, 이 딜레이가 지나면 실제로 다음 단계로 이동")]
    public float accumulateAutoNextDelay = 5f;        // 5초 후 자동 이동

    [Tooltip("한 번 인식된 뒤 다음 인식까지 최소 대기시간")]
    public float accumulateCooldown = 0.35f;

    public UnityEvent OnAccumulateTrigger;    // 한 번 "쌓았다!" 할 때
    public UnityEvent OnAccumulateComplete;   // Accumulate 단계 종료 시 (맥시멈 또는 타임아웃)

    private int   accumulateCount = 0;
    private float accumulateCooldownTimer = 0f;
    private bool  wasUpGesture = false;       // 바로 이전 프레임에서 위로 올리는 중이었는지

    private float accumulatePhaseTimer = 0f;          // Accumulate에 들어온 뒤 경과 시간
    private float accumulateSinceFirstCount = 0f;     // 첫 카운트 이후 경과 시간
    private bool  accumulateHasAnyCount = false;      // 카운트를 한 번이라도 했는가
    private float accumulateAutoNextTimer = -1f;      // 0보다 크면 자동 이동 카운트다운 중

    // ========== Stimulate (자극: 4방향 스와이프) ==========
    [Header("Stimulate Settings")]
    public int stimulateTargetCount = 15;

    [Tooltip("좌우 스와이프 속도 임계값 (mm/s)")]
    public float stimulateSwipeHorizontalThreshold = 1.5f;

    [Tooltip("위아래 스와이프 속도 임계값 (mm/s)")]
    public float stimulateSwipeVerticalThreshold = 1.5f;

    [Header("Stimulate Timing")]
    [Tooltip("Stimulate 단계 들어온 뒤, 이 시간 동안 스와이프가 전혀 없으면 자동 이동 카운트다운 시작")]
    public float stimulateNoSwipeTimeout = 10f;

    [Tooltip("첫 스와이프 이후, 이 시간 동안 목표치 미달이면 자동 이동 카운트다운 시작")]
    public float stimulatePlayWindow = 20f;

    [Tooltip("타임아웃 판정 난 뒤, 이 딜레이가 지나면 실제로 다음 단계(Burst)로 이동")]
    public float stimulateAutoNextDelay = 5f;

    [Tooltip("연속 스와이프 사이 최소 간격")]
    public float stimulateCooldown = 0.2f;

    public UnityEvent OnStimulateMotion;        // 한 번 휘두를 때마다
    public UnityEvent OnStimulateComplete;      // Stimulate 단계 종료 시 (목표 달성 or 타임아웃)

    private int   stimulateCount = 0;
    private float stimulateCooldownTimer = 0f;

    // 타이밍/상태
    private float stimulatePhaseTimer = 0f;          // Stimulate에 들어온 뒤 경과 시간
    private float stimulateSinceFirstSwipe = 0f;     // 첫 스와이프 이후 경과 시간
    private bool  stimulateHasAnySwipe = false;      // 한 번이라도 스와이프 했는가
    private float stimulateAutoNextTimer = -1f;      // 0보다 크면 자동 이동 카운트다운 중

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

    [Header("Burst → Purify Timing")]
    [Tooltip("표출 연출이 끝난 뒤 정화 단계로 넘어가기까지 딜레이 (초)")]
    public float burstToPurifyDelay = 5f;

    [Header("Burst Events By Stimulate Level")]
    public UnityEvent OnBurstWeak;    // 자극 거의 없음
    public UnityEvent OnBurstNormal;  // 중간
    public UnityEvent OnBurstStrong;  // 충분히 자극함

    private bool  burstFired = false;
    private float burstToPurifyTimer = -1f;   // 0 이상이면 카운트다운 중

    // ========== Purify (정화: 올렸다 내리기 3회) ==========
    [Header("Purify Settings")]
    public int   purifyTargetBreathCount      = 3;

    // 천천히 올리고 내리는 동작도 인식되도록 (Inspector에서 1 / -1로 세팅해도 됨)
    public float purifyUpSpeedThreshold       = 1f;    // 위로 +Y
    public float purifyDownSpeedThreshold     = -1f;   // 아래로 -Y

    public float purifyPalmUp                 = 0.3f;   // 손바닥이 위를 향할 때 palmNormal.y 기준 (참고용)
    public float purifyPalmDown               = -0.3f;  // 손등이 위를 향할 때 palmNormal.y 기준 (참고용)

    [Tooltip("호흡 동작이 끝났다고 보는 속도 (거의 정지, velY 기준)")]
    public float purifyIdleSpeedThreshold     = 0.2f;

    [Tooltip("한 호흡이 끝난 후 다음 호흡까지 최소 대기 시간(초)")]
    public float purifyBetweenCycleCooldown   = 0.4f;

    [Tooltip("정화 단계에서 양손이 모두 보여야만 인식할지 여부")]
    public bool  requireBothHandsForPurify    = true;

    public UnityEvent OnPurifyOneCycle;   // 1회 호흡 완료 시
    public UnityEvent OnPurifyComplete;   // 3회 완료 시

    private int  purifyBreathCount = 0;
    private enum PurifyState { Idle, Inhaling, Exhaling }
    private PurifyState purifyState = PurifyState.Idle;
    private float       purifyCooldownTimer = 0f;

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

        // ★ Burst → Purify 딜레이 타이머 전역 처리
        if (currentPhase == Phase.Burst && burstFired && burstToPurifyTimer > 0f)
        {
            burstToPurifyTimer -= Time.deltaTime;
            if (burstToPurifyTimer <= 0f)
            {
                Debug.Log("[Burst] 표출 연출 종료 → Purify 단계로 이동");
            }
        }

        Frame frame = provider.CurrentFrame;

        // ★ 손이 하나도 안 잡히는 경우 처리
        if (frame == null || frame.Hands == null || frame.Hands.Count == 0)
        {
            // === Accumulate 단계일 때는 '반응 없음' 타임아웃 로직도 같이 돌려주기 ===
            if (currentPhase == Phase.Accumulate)
            {
                // 전체 경과시간 / 첫 카운트 이후 시간 증가
                accumulatePhaseTimer += Time.deltaTime;
                if (accumulateHasAnyCount)
                    accumulateSinceFirstCount += Time.deltaTime;

                // 이미 자동 이동 카운트다운 중이면, 타이머만 깎다가 끝나면 넘어가기
                if (accumulateAutoNextTimer > 0f)
                {
                    accumulateAutoNextTimer -= Time.deltaTime;
                    if (accumulateAutoNextTimer <= 0f)
                    {
                        Debug.Log("[Accumulate][NoHand] 손이 계속 안 보임 → 타임아웃으로 다음 단계(Stimulate) 이동");
                        OnAccumulateComplete?.Invoke();
                    }
                }
                else
                {
                    // ① 아예 카운트 한 번도 안 했고, 오래 지남
                    if (!accumulateHasAnyCount &&
                        accumulatePhaseTimer >= accumulateNoCountTimeout + accumulatePlayWindow)
                    {
                        Debug.Log("[Accumulate][NoHand] 반응 없음 + 시간 초과 → 자동 이동 카운트다운 시작");
                        accumulateAutoNextTimer = accumulateAutoNextDelay; // 예: 5초 뒤 진짜 이동
                    }
                    // ② 카운트는 있었지만 맥시멈 못 채우고 오래 지남
                    else if (accumulateHasAnyCount &&
                             accumulateSinceFirstCount >= accumulatePlayWindow &&
                             accumulateCount < accumulateMaxCount)
                    {
                        Debug.Log("[Accumulate][NoHand] 맥시멈 미달 + 시간 초과 → 자동 이동 카운트다운 시작");
                        accumulateAutoNextTimer = accumulateAutoNextDelay;
                    }
                }
            }

            // === Stimulate 단계일 때도 비슷한 타임아웃 로직 적용 ===
            if (currentPhase == Phase.Stimulate)
            {
                stimulatePhaseTimer += Time.deltaTime;
                if (stimulateHasAnySwipe)
                    stimulateSinceFirstSwipe += Time.deltaTime;

                if (stimulateAutoNextTimer > 0f)
                {
                    stimulateAutoNextTimer -= Time.deltaTime;
                    if (stimulateAutoNextTimer <= 0f)
                    {
                        // 타이머 끝 → Burst로 이동
                        ComputeStimulateResult();
                        Debug.Log("[Stimulate][NoHand] 손 안 보임 + 타임아웃 → 다음 단계(Burst) 이동");
                        OnStimulateComplete?.Invoke();
                    }
                }
                else
                {
                    // ① 한 번도 스와이프 안 했고, 오래 지남
                    if (!stimulateHasAnySwipe &&
                        stimulatePhaseTimer >= stimulateNoSwipeTimeout + stimulatePlayWindow)
                    {
                        Debug.Log("[Stimulate] 오랫동안 반응 없음 → 타임아웃 카운트다운 시작");
                        stimulateAutoNextTimer = stimulateAutoNextDelay;
                    }
                    // ② 스와이프는 했지만 목표치 못 채우고 오래 지남
                    else if (stimulateHasAnySwipe &&
                             stimulateSinceFirstSwipe >= stimulatePlayWindow &&
                             stimulateCount < stimulateTargetCount)
                    {
                        Debug.Log("[Stimulate] 목표 미달 + 시간 초과 → 타임아웃 카운트다운 시작");
                        stimulateAutoNextTimer = stimulateAutoNextDelay;
                    }
                }
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
        Vector3 palmVel    = hand.PalmVelocity;
        Vector3 palmNormal = hand.PalmNormal;

        float speed = palmVel.magnitude;            // 총 속도 크기
        float grab  = hand.GrabStrength;            // 0~1
        float pinch = hand.PinchStrength;           // 0~1

        // 공용 타이머 업데이트
        if (accumulateCooldownTimer > 0f) accumulateCooldownTimer -= Time.deltaTime;
        if (stimulateCooldownTimer  > 0f) stimulateCooldownTimer  -= Time.deltaTime;
        if (purifyCooldownTimer     > 0f) purifyCooldownTimer     -= Time.deltaTime;

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
        if (speed < introSpeedThreshold)
        {
            introSteadyTimer += Time.deltaTime;

            if (introSteadyTimer >= introRequiredSteadyTime)
            {
                Debug.Log("[Intro] 5초 유지 완료! Accumulate 단계로 이동");

                OnIntroComplete?.Invoke();          // 필요하면 UI용 이벤트
                introSteadyTimer = 0f;

            }
        }
        else
        {
            introSteadyTimer = 0f;
        }
    }

    // =========================================================
    // Accumulate: 손바닥 아래 + 위로 빠르게 올릴 때 "쌓았다" 한 번
    // =========================================================
    void UpdateAccumulate(Vector3 palmVel, Vector3 palmNormal)
    {
        // 0) 전역 타이머 업데이트
        accumulatePhaseTimer += Time.deltaTime;
        if (accumulateHasAnyCount)
            accumulateSinceFirstCount += Time.deltaTime;

        // 0-1) 이미 자동 이동 카운트다운 중이면, 타이머만 깎다가 끝나면 다음 단계
        if (accumulateAutoNextTimer > 0f)
        {
            accumulateAutoNextTimer -= Time.deltaTime;
            if (accumulateAutoNextTimer <= 0f)
            {
                Debug.Log("[Accumulate] 타임아웃으로 다음 단계(Stimulate) 이동");
                OnAccumulateComplete?.Invoke();
            }
            return;
        }

        // 1) “쌓았다” 제스처 인식 (손바닥 아래 + 위로 올리기)
        bool palmFacingDown = (palmNormal.y < accumulatePalmNormalDown);
        bool movingUpFast   = (palmVel.y > accumulateDownSpeedThreshold);

        bool upGestureNow = palmFacingDown && movingUpFast;
        bool pressedNow   = upGestureNow && !wasUpGesture;

        if (accumulateCooldownTimer <= 0f && pressedNow)
        {
            accumulateCount++;
            accumulateHasAnyCount       = true;
            accumulateSinceFirstCount   = 0f;      // 첫 카운트이거나, 다시 "활동 중"이란 의미로 리셋
            accumulateCooldownTimer     = accumulateCooldown;

            Debug.Log($"[Accumulate] 쌓았다! count = {accumulateCount}");
            OnAccumulateTrigger?.Invoke();

            // 맥시멈 채웠으면 바로 다음 단계
            if (accumulateCount >= accumulateMaxCount)
            {
                Debug.Log("[Accumulate] 맥시멈 달성! 다음 단계(Stimulate) 이동");
                OnAccumulateComplete?.Invoke();
                return;
            }
        }

        wasUpGesture = upGestureNow;

        // 2) 타임아웃 조건들 ---------------------------

        // 2-1) 아예 카운트가 한 번도 안 된 사람:
        if (!accumulateHasAnyCount &&
            accumulatePhaseTimer >= accumulateNoCountTimeout + accumulatePlayWindow)
        {
            Debug.Log("[Accumulate] 오랫동안 반응 없음 → 타임아웃 카운트다운 시작");
            accumulateAutoNextTimer = accumulateAutoNextDelay;  // 5초 후 실제 이동
            return;
        }

        // 2-2) 카운트는 있었지만 맥시멈을 못 채운 사람:
        if (accumulateHasAnyCount &&
            accumulateSinceFirstCount >= accumulatePlayWindow &&
            accumulateCount < accumulateMaxCount)
        {
            Debug.Log("[Accumulate] 맥시멈 미달 + 시간 초과 → 타임아웃 카운트다운 시작");
            accumulateAutoNextTimer = accumulateAutoNextDelay;
            return;
        }
    }

    // =========================================================
    // Stimulate: 4방향 스와이프 (좌 / 우 / 위 / 아래)
    // =========================================================
    void UpdateStimulate(Vector3 palmVel, float grab, float pinch)
    {
        // 공용 타이머 업데이트
        stimulatePhaseTimer += Time.deltaTime;
        if (stimulateHasAnySwipe)
            stimulateSinceFirstSwipe += Time.deltaTime;

        // 쿨다운
        if (stimulateCooldownTimer > 0f)
            stimulateCooldownTimer -= Time.deltaTime;

        // 이미 자동 이동 카운트다운 중이면, 타이머만 깎다가 끝나면 Burst 이동
        if (stimulateAutoNextTimer > 0f)
        {
            stimulateAutoNextTimer -= Time.deltaTime;
            if (stimulateAutoNextTimer <= 0f)
            {
                ComputeStimulateResult();
                Debug.Log("[Stimulate] 타임아웃으로 다음 단계(Burst) 이동");
                OnStimulateComplete?.Invoke();
            }
            return;
        }

        // 1) 4방향 스와이프 인식
        float vx = palmVel.x;
        float vy = palmVel.y;

        bool swipeLeft  = vx < -stimulateSwipeHorizontalThreshold;
        bool swipeRight = vx >  stimulateSwipeHorizontalThreshold;
        bool swipeUp    = vy >  stimulateSwipeVerticalThreshold;
        bool swipeDown  = vy < -stimulateSwipeVerticalThreshold;

        bool swipeNow = (swipeLeft || swipeRight || swipeUp || swipeDown);

        if (stimulateCooldownTimer <= 0f && swipeNow)
        {
            stimulateCount++;
            stimulateHasAnySwipe      = true;
            stimulateSinceFirstSwipe  = 0f;   // 활동 중이니 타이머 리셋
            stimulateCooldownTimer    = stimulateCooldown;

            string dir = "Unknown";
            if      (swipeLeft)  dir = "Left";
            else if (swipeRight) dir = "Right";
            else if (swipeUp)    dir = "Up";
            else if (swipeDown)  dir = "Down";

            Debug.Log($"[Stimulate] Swipe {dir}! count = {stimulateCount}");
            OnStimulateMotion?.Invoke();

            // 목표 카운트 채우면 → 5초 뒤 Burst로
            if (stimulateCount >= stimulateTargetCount)
            {
                if (stimulateCount > stimulateTargetCount)
                    stimulateCount = stimulateTargetCount;   // 혹시 넘쳐도 클램프

                // 자극 결과 미리 계산 (로그용)
                ComputeStimulateResult();
                Debug.Log("[Stimulate] 목표치 달성! 5초 뒤 표출 단계(Burst)로 이동 예정");

                // 5초 뒤 자동으로 Burst로 넘어가도록 타이머 시작
                stimulateAutoNextTimer = stimulateAutoNextDelay;  // Inspector에서 5로 셋
                return;
            }
        }

        // 2) 타임아웃 조건 ---------------------------

        // 2-1) 한 번도 스와이프 안 하고 오래 지남
        if (!stimulateHasAnySwipe &&
            stimulatePhaseTimer >= stimulateNoSwipeTimeout + stimulatePlayWindow)
        {
            Debug.Log("[Stimulate] 오랫동안 반응 없음 → 타임아웃 카운트다운 시작");
            stimulateAutoNextTimer = stimulateAutoNextDelay;  // 5초 후 실제 이동
            return;
        }

        // 2-2) 스와이프는 했지만 목표치 못 채우고 오래 지남
        if (stimulateHasAnySwipe &&
            stimulateSinceFirstSwipe >= stimulatePlayWindow &&
            stimulateCount < stimulateTargetCount)
        {
            Debug.Log("[Stimulate] 목표 미달 + 시간 초과 → 타임아웃 카운트다운 시작");
            stimulateAutoNextTimer = stimulateAutoNextDelay;
            return;
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
    //  - 자극 레벨에 따라 다른 연출 이벤트 호출
    //  - 연출 후 burstToPurifyDelay 만큼 기다렸다가 Purify 단계로 이동
    // =========================================================
    void UpdateBurst(Vector3 palmVel, float pinch)
    {
        // 이미 콕 제스처가 한 번 발생했다면 여기서는 아무 것도 안 함
        // (실제 Purify 이동은 Update() 상단의 타이머에서 처리)
        if (burstFired) return;

        // 아래로 빠르게 찍는 동작인지 확인
        bool fastTapDown     = palmVel.y < burstTapDownSpeedThreshold;
        bool smallHorizontal = Mathf.Abs(palmVel.x) < burstMaxHorizontalSpeed &&
                               Mathf.Abs(palmVel.z) < burstMaxHorizontalSpeed;
        bool palmNotTooFast  = palmVel.magnitude < burstMaxPalmSpeed;
        bool pinchOn         = pinch > burstMinPinch;   // 살짝 쥔 상태(검지+엄지) 정도

        if (fastTapDown && smallHorizontal && palmNotTooFast && pinchOn)
        {
            burstFired          = true;
            burstToPurifyTimer  = burstToPurifyDelay;   // ★ 표출 → 정화 카운트다운 시작

            string burstDesc = "";
            switch (lastStimulateLevel)
            {
                case StimulateLevel.None:
                    burstDesc = "약한 표출 (자극 거의 없음)";
                    Debug.Log($"[Burst] {burstDesc} 실행 / count={stimulateCount}, ratio={lastStimulateRatio:F2}");
                    OnBurstWeak?.Invoke();
                    break;

                case StimulateLevel.Low:
                    burstDesc = "보통 표출 (자극 일부)";
                    Debug.Log($"[Burst] {burstDesc} 실행 / count={stimulateCount}, ratio={lastStimulateRatio:F2}");
                    OnBurstNormal?.Invoke();
                    break;

                case StimulateLevel.High:
                    burstDesc = "강한 표출 (자극 충분, 맥스 근접 또는 달성)";
                    Debug.Log($"[Burst] {burstDesc} 실행 / count={stimulateCount}, ratio={lastStimulateRatio:F2}");
                    OnBurstStrong?.Invoke();
                    break;
            }
        }
    }

    // =========================================================
    // Purify: 올렸다(들숨) → 내렸다(날숨) 3회
    //  - 들숨 → 날숨 → 거의 멈춤 = 1회로 카운트
    // =========================================================
    void UpdatePurify(Vector3 palmVel, Vector3 palmNormal)
{
    float velY = palmVel.y;
    float normalY = palmNormal.y;
    float speed = palmVel.magnitude;

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
            }
            break;

        case PurifyState.Inhaling:
            // 날숨 시작 (손등 위 + 아래로 이동)
            if (velY < exhaleSpeed && normalY < exhalePalm)
            {
                purifyState = PurifyState.Exhaling;
                purifyExhaleTimer = 0f;   // 타이머 초기화
                Debug.Log("[Purify] 날숨 시작");
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

                    if (purifyBreathCount >= purifyTargetBreathCount)
                    {
                        Debug.Log("[Purify] 정화 완료!");
                        OnPurifyComplete?.Invoke();
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
        }
        else if (currentPhase == Phase.Purify)
        {
            purifyState         = PurifyState.Idle;
            purifyCooldownTimer = 0f;
        }
        // Accumulate / Stimulate / Burst는 카운트/플래그 유지하는 쪽이 체험상 더 자연스러워서 그대로 둠
    }

    // 디버깅용 리셋 함수 (Inspector에서 버튼으로 호출해도 됨)
    public void ResetAll()
    {
        introSteadyTimer = 0f;

        // Accumulate 관련
        accumulateCount           = 0;
        accumulatePhaseTimer      = 0f;
        accumulateSinceFirstCount = 0f;
        accumulateHasAnyCount     = false;
        accumulateAutoNextTimer   = -1f;
        wasUpGesture              = false;
        accumulateCooldownTimer   = 0f;

        // Stimulate 관련
        stimulateCount           = 0;
        stimulateCooldownTimer   = 0f;
        stimulatePhaseTimer      = 0f;
        stimulateSinceFirstSwipe = 0f;
        stimulateHasAnySwipe     = false;
        stimulateAutoNextTimer   = -1f;
        lastStimulateLevel       = StimulateLevel.None;
        lastStimulateRatio       = 0f;

        // Burst 관련
        burstFired         = false;
        burstToPurifyTimer = -1f;

        // Purify 관련
        purifyBreathCount = 0;
        purifyState = PurifyState.Idle;
        purifyCooldownTimer = 0f;
        purifyExhaleTimer = 0f;
    }
}
