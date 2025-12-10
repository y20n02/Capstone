using UnityEngine;
using Leap.PhysicalHands;
using UnityEngine.Events;

public class AutoStickToHand : MonoBehaviour
{
    public Rigidbody rb;
    public bool detachOnSecondTouch = false; // OnContactExit로 떨어뜨릴 때 사용

    // ★ 처음 잡혔을 때 한 번만 호출할 이벤트 (원래 있던 거)
    public UnityEvent OnFirstGrab;

    // ★ Intro UI에 직접 알려주기 위해 추가
    [Header("UI Sequence (선택)")]
    public IntroUISequence uiSequence;

    ContactHand currentHand;
    Vector3 localOffset;
    Quaternion localRotOffset;

    // 흔들기용 속도 계산
    Vector3 _lastPos;
    public Vector3 CurrentVelocity { get; private set; } = Vector3.zero;

    public bool IsAttached => currentHand != null;

    // ★ 중복 호출 방지
    bool _firstGrabFired = false;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    // PhysicalHandEvents → OnContactEnter 에 연결
    public void AttachToHand(ContactHand hand)
    {
        // 이미 다른 손에 붙어있으면 무시
        if (currentHand != null) return;

        currentHand = hand;

        Transform palm = currentHand.palmBone.transform;
        localOffset = palm.InverseTransformPoint(transform.position);
        localRotOffset = Quaternion.Inverse(palm.rotation) * transform.rotation;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        _lastPos = transform.position;
        CurrentVelocity = Vector3.zero;

        // ★ 처음 한 번만 UI 이벤트 쏘기
        if (!_firstGrabFired)
        {
            _firstGrabFired = true;

            // 1) 기존 UnityEvent 호출 (원하면 유지)
            OnFirstGrab?.Invoke();

            // 2)  IntroUISequence에 직접 알리기
            if (uiSequence != null)
            {
                Debug.Log("[AutoStickToHand] 첫 Grab → IntroUISequence.OnGrabDone 호출");
                uiSequence.OnGrabDone();
            }
            else
            {
                Debug.LogWarning("[AutoStickToHand] uiSequence가 비어있어서 Grab UI 전환을 못 함!");
            }
        }
    }

    // PhysicalHandEvents → OnContactExit 에 연결 (옵션)
    public void DetachFromHand(ContactHand hand)
    {
        if (!detachOnSecondTouch) return;
        if (hand != currentHand) return;

        ForceDetach(true);
    }

    // HotplateMission 같은 데서 강제로 떼고 싶을 때 호출
    public void ForceDetach(bool restorePhysics)
    {
        currentHand = null;

        if (rb != null && restorePhysics)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    void LateUpdate()
    {
        if (currentHand == null) return;

        Transform palm = currentHand.palmBone.transform;

        Vector3 newPos = palm.TransformPoint(localOffset);
        Quaternion newRot = palm.rotation * localRotOffset;

        // 속도 계산 (손에 붙어 있는 동안)
        CurrentVelocity = (newPos - _lastPos) / Mathf.Max(Time.deltaTime, 0.0001f);
        _lastPos = newPos;

        transform.position = newPos;
        transform.rotation = newRot;
    }
}
