using UnityEngine;

public class WaterPourController : MonoBehaviour
{
    public ParticleSystem waterParticles;
    public float pourAngle = 50f;  // 이 각도 이상 기울어지면 물 나옴

    private ParticleSystem.EmissionModule emission;

    void Start()
    {
        emission = waterParticles.emission;
        emission.enabled = false; // 기본 OFF
    }

    void Update()
    {
        // Z축 회전을 기준으로 기울기 계산
        float angle = Vector3.Angle(transform.up, Vector3.up);

        if (angle > pourAngle)
            emission.enabled = true;
        else
            emission.enabled = false;
    }
}
