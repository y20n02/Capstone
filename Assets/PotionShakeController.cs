using UnityEngine;

public class PotionShakeController : MonoBehaviour
{
    [Header("References")]
    public Renderer liquidRenderer;        // 안쪽 Liquid
    public AutoStickToHand autoStick;      // 손에 붙이는 스크립트

    [Header("UI Sequence (선택)")]
    public IntroUISequence uiSequence;     // ⭐ 흔들기 완료 시 UI 전환용

    [Header("Shake Settings")]
    public int requiredShakes = 5;         // 몇 번 “흔든” 걸로 볼지 (Inspector에서 8로 바꿔도 됨)
    public float shakeSpeedThreshold = 0.15f; // 이 이상으로 움직이면 1회 흔들기
    public float minTimeBetweenShakes = 0.25f;

    [Header("Fresnel Color 변화")]
    public Color fresnelBase = new Color(0.1f, 0.1f, 0.5f);   // 시작 색
    public Color fresnelBright = new Color(1.0f, 1.0f, 1.0f);   // 거의 하얀색으로

    [Header("Top Color 변화")]
    public Color topBase = new Color(0.2f, 0.0f, 0.4f);
    public Color topBright = new Color(1.0f, 0.6f, 1.0f);

    public bool debugLog = true;

    public bool IsFullyShaken => currentShakes >= requiredShakes;

    int currentShakes = 0;
    float lastShakeTime = 0f;

    // ⭐ UI에 한 번만 알려주기 위한 플래그
    bool shakeUIDone = false;

    Material liquidMat;
    Vector3 lastPos;

    readonly string fresnelProp = "_FresnelColor";
    readonly string topProp = "_TopColor";

    void Start()
    {
        if (autoStick == null)
            autoStick = GetComponent<AutoStickToHand>();

        if (liquidRenderer != null)
        {
            liquidMat = liquidRenderer.material;
        }

        lastPos = transform.position;
        SetLiquidColors(0f); // 처음 색 세팅
    }

    void Update()
    {
        if (liquidMat == null) return;
        if (autoStick != null && !autoStick.IsAttached)
        {
            // 손에서 떨어져 있을 땐 흔들기 카운트 안 함
            lastPos = transform.position;
            return;
        }

        // 그냥 포션의 위치 변화를 이용해서 "속도" 추정
        Vector3 currentPos = transform.position;
        float distance = (currentPos - lastPos).magnitude;
        float speed = distance / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPos = currentPos;

        if (speed > shakeSpeedThreshold && Time.time - lastShakeTime > minTimeBetweenShakes)
        {
            currentShakes = Mathf.Min(currentShakes + 1, requiredShakes);
            lastShakeTime = Time.time;

            float raw = currentShakes / (float)requiredShakes;

            // ① 처음 1~2번은 거의 안 변하게 오프셋 주고
            raw = Mathf.Clamp01((raw - 0.2f) / 0.8f);  // 0.2 이전은 0, 그 이후부터 서서히

            // ② 끝으로 갈수록 급격히 바뀌도록 지수 곡선
            float t = Mathf.Pow(raw, 2.5f);            // 2.0~3.0 사이에서 취향대로 조절

            SetLiquidColors(t);

            if (debugLog)
                Debug.Log($"[PotionShake] Shake #{currentShakes}, raw={raw}, t={t}");

            // ⭐ 여기서 흔들기 UI 전환 처리
            if (!shakeUIDone && currentShakes >= requiredShakes && uiSequence != null)
            {
                shakeUIDone = true;
                uiSequence.OnShakeDone();   // 02Shake_text → 03Put_text로 전환
            }
        }
    }

    void SetLiquidColors(float t)
    {
        t = Mathf.Clamp01(t);

        Color fresnel = Color.Lerp(fresnelBase, fresnelBright, t);
        Color top = Color.Lerp(topBase, topBright, t);

        // Fresnel / Top 색 확실하게 바꾸기
        if (liquidMat.HasProperty(fresnelProp))
            liquidMat.SetColor(fresnelProp, fresnel);
        if (liquidMat.HasProperty(topProp))
            liquidMat.SetColor(topProp, top);

        // Emission도 아주 강하게 변화 주기
        if (liquidMat.HasProperty("_EmissionColor"))
        {
            // t=0일 때 거의 어둡고, t=1일 때 엄청 밝게
            Color emission = Color.Lerp(Color.black, Color.white, t) * 8f;
            liquidMat.SetColor("_EmissionColor", emission);
        }
        if (liquidMat.HasProperty("_Emission"))
        {
            liquidMat.SetFloat("_Emission", Mathf.Lerp(0.5f, 10f, t));
        }
    }
}
