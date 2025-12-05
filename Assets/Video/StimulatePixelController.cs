using UnityEngine;

public class PixelEffectController : MonoBehaviour
{
    [Header("픽셀 셰이더 머티리얼 (FullScreenPass에 쓰이는거)")]
    public Material pixelMaterial;

    [Header("픽셀 크기 변화 범위")]
    public float startPixelSize = 400f;  // 처음 값 (거의 안깨진 상태)
    public float endPixelSize   = 40f;   // 많이 작아진 값 (훨씬 픽셀 느낌)

    [Header("최대 카운트 (몇 번 휘둘렀을 때 완전히 깨질지)")]
    public int maxHits = 20;  // 적당히 조절

    private int currentHits = 0;

    // MotionTrigger에서 OnStimulateMotion에 연결할 함수
    public void AddHit()
    {
        if (pixelMaterial == null) return;

        currentHits++;
        if (currentHits > maxHits) currentHits = maxHits;

        float t = (float)currentHits / maxHits;   // 0 → 1
        // 휘두를수록 PixelSize가 작아지도록 Lerp (start → end)
        float newSize = Mathf.Lerp(startPixelSize, endPixelSize, t);

        pixelMaterial.SetFloat("_PixelSize", newSize);
        // Debug.Log($"PixelSize = {newSize}");
    }

    // 필요하면 리셋용
    public void ResetPixel()
    {
        currentHits = 0;
        if (pixelMaterial != null)
            pixelMaterial.SetFloat("_PixelSize", startPixelSize);
    }
}
