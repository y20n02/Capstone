using UnityEngine;

public class RespawnWhenOffscreen : MonoBehaviour
{
    [Header("Camera that defines the Game View")]
    public Camera targetCamera;   // ���⿡ Main Camera �ֱ�

    [Header("Optional Respawn Point (����θ� ���� ��ġ�� ����)")]
    public Transform respawnPoint;

    [Header("Respawn Delay (seconds)")]
    public float respawnDelay = 1.5f;

    private Rigidbody rb;

    private Vector3 startPos;
    private Quaternion startRot;

    private float offscreenTimer = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // ī�޶� �ڵ� �Ҵ�
        if (targetCamera == null)
            targetCamera = Camera.main;

        // ������ ��ġ ����: ���� ����Ʈ > ���� ��ġ
        if (respawnPoint != null)
        {
            startPos = respawnPoint.position;
            startRot = respawnPoint.rotation;
        }
        else
        {
            startPos = transform.position;
            startRot = transform.rotation;
        }
    }

    void Update()
    {
        if (targetCamera == null) return;

        // ī�޶� ���� ��ǥ (0~1 ������ ȭ�� ��)
        Vector3 vp = targetCamera.WorldToViewportPoint(transform.position);

        bool offScreen =
            vp.z < 0f ||     // ī�޶� �ڷ� ������ ���
            vp.x < 0f || vp.x > 1f ||
            vp.y < 0f || vp.y > 1f;

        if (offScreen)
        {
            offscreenTimer += Time.deltaTime;

            if (offscreenTimer >= respawnDelay)
            {
                Respawn();
            }
        }
        else
        {
            offscreenTimer = 0f;
        }
    }

    void Respawn()
    {
        //<Rigidbody> ���� ���� �� ����
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = startPos;
        transform.rotation = startRot;

        offscreenTimer = 0f;
    }
}
