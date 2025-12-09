using UnityEngine;

public class UIFadeController : MonoBehaviour
{
    public CanvasGroup uiGroup;
    public float fadeDuration = 1.5f;

    float timer = 0f;
    bool fadingIn = false;

    void Start()
    {
        if (uiGroup != null)
        {
            uiGroup.alpha = 0f;
            uiGroup.interactable = false;
            uiGroup.blocksRaycasts = false;
        }
    }

    public void FadeIn()
    {
        if (uiGroup == null) return;

        timer = 0f;
        fadingIn = true;
    }

    void Update()
    {
        if (!fadingIn || uiGroup == null) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / fadeDuration);

        uiGroup.alpha = Mathf.Lerp(0f, 1f, t);

        if (t >= 1f)
        {
            fadingIn = false;
            uiGroup.interactable = true;
            uiGroup.blocksRaycasts = true;
        }
    }

    // 🔻 모션 인식됐을 때 UI 바로 숨기기
    public void HideInstant()
    {
        if (uiGroup == null) return;

        uiGroup.alpha = 0f;
        uiGroup.interactable = false;
        uiGroup.blocksRaycasts = false;
    }
}
