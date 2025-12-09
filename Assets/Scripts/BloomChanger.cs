using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloomChanger : MonoBehaviour
{
    public Volume volume;

    private Bloom bloom;

    void Start()
    {
        volume.profile.TryGet(out bloom);
    }

    public void ApplyBurstBloom()
    {
        if (bloom != null)
        {
            bloom.threshold.value = 0.6f;
            bloom.intensity.value = 10f;
        }
    }

    public void ResetBloom()
    {
        if (bloom != null)
        {
            bloom.threshold.value = 1f;
            bloom.intensity.value = 1.5f;
        }
    }
}
