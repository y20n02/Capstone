using UnityEngine;
using UnityEngine.UI;

public class CircleLoading : MonoBehaviour
{
    public Image loadingBar;   // 원형 이미지

    // 0~1 사이 값으로 채우기
    public void SetProgress(float ratio)
    {
        if (loadingBar == null) return;
        loadingBar.fillAmount = Mathf.Clamp01(ratio);
    }

    public void ResetProgress()
    {
        SetProgress(0f);
    }
}
