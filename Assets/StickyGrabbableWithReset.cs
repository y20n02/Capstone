using UnityEngine;
using Leap.PhysicalHands;   // ContactHand Ÿ���� ���� �ʿ�

public class StickyGrabbableWithReset : MonoBehaviour
{
    [Header("Reset Settings")]
    public float outOfViewTimeToReset = 1.5f;
    public Camera targetCamera;     // ����θ� �ڵ����� Main Camera ���

    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;
    private float outOfViewTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Update()
    {
        if (targetCamera == null) return;

        // ����Ʈ ��ǥ ��� (0~1 ������ ȭ�� ��)
        Vector3 vp = targetCamera.WorldToViewportPoint(transform.position);

        bool offScreen =
            vp.z < 0f ||            // ī�޶� �ڷ� ���� ��
            vp.x < 0f || vp.x > 1f ||
            vp.y < 0f || vp.y > 1f;

        if (offScreen)
        {
            outOfViewTimer += Time.deltaTime;
            if (outOfViewTimer >= outOfViewTimeToReset)
            {
                ResetObject();
            }
        }
        else
        {
            // ȭ�� ������ ���ƿ��� Ÿ�̸� �ʱ�ȭ
            outOfViewTimer = 0f;
        }
    }

    // Physical Hand Events �� OnGrabEnter �� ����
    public void OnGrabEnter(ContactHand hand)
    {
        // �տ� �� �ٵ��� ���� ��ױ�
        if (rb == null) return;

        rb.isKinematic = true;
        rb.useGravity = false;
        outOfViewTimer = 0f;
    }

    // Physical Hand Events �� OnGrabExit �� ����
    public void OnGrabExit(ContactHand hand)
    {
        // �ٽ� ���� ����
        if (rb == null) return;

        rb.isKinematic = false;
        rb.useGravity = true;
    }

    private void ResetObject()
    {
        outOfViewTimer = 0f;

        // Ȥ�ó� �տ� �پ� �־����� �и�
        transform.SetParent(null);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = startPos;
        transform.rotation = startRot;
    }
}
