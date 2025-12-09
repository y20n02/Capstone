using UnityEngine;

public class EmissionBlink : MonoBehaviour
{
    public Color baseColor = Color.green;
    public float minIntensity = 0f;
    public float maxIntensity = 3f;
    public float speed = 2f;
    public bool hardBlink = false;
    public bool randomStartOffset = true;
    float timeOffset = 0f;

    Renderer rend;
    Material mat;

    void Start()
    {
        // 이 오브젝트 또는 자식들에서 Renderer 찾기
        rend = GetComponent<Renderer>();
        if (rend == null)
            rend = GetComponentInChildren<Renderer>();

        if (rend == null)
        {
            Debug.LogError("EmissionBlink: Renderer를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        mat = rend.material;
        mat.EnableKeyword("_EMISSION");

        if (randomStartOffset)
            timeOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        float t = Mathf.Sin((Time.time + timeOffset) * speed) * 0.5f + 0.5f;
        if (hardBlink)
            t = t > 0.5f ? 1f : 0f;

        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        Color emissionColor = baseColor * intensity;
        mat.SetColor("_EmissionColor", emissionColor);
    }
}
