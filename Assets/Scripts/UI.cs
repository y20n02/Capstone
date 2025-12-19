using System.Collections;
using UnityEngine;

public class UI : MonoBehaviour
{
    [Header("UI ����")]
    public CanvasGroup introGroup;          // AccumulationCanvas�� �ִ� CanvasGroup

    [Header("Ÿ�̹� ����")]
    public float delayBeforeShow = 2.5f;    // �� ������ ���� ��� �ð� (2~3��)
    public float fadeTime = 1.0f;           // ���̵� ��/�ƿ� �ð�
    public float visibleTime = 5.0f;        // ȭ�鿡 �����Ǵ� �ð�

    [Header("��Ʈ�� ���� �� �� ��ũ��Ʈ��")]
    public MonoBehaviour[] scriptsToEnable; // ��� �ν� ��ũ��Ʈ�� �巡���ؼ� �ֱ�

    public bool IntroFinished { get; private set; } = false;

    void Start()
    {
        // 1) ��Ʈ�� UI�� ó���� "������ �ʰ�"�� ����� (GameObject�� ���� X)
        if (introGroup != null)
        {
            introGroup.gameObject.SetActive(true);   // �׻� �ѵ� ���¿���
            introGroup.alpha = 0f;                   // �����ϰ� �����
            introGroup.interactable = false;
            introGroup.blocksRaycasts = false;
        }

        // 2) ��� ���� ��ũ��Ʈ���� �ϴ� ���α�
        if (scriptsToEnable != null)
        {
            foreach (var s in scriptsToEnable)
            {
                if (s != null) s.enabled = false;
            }
        }

        // 3) ��Ʈ�� ���� ����
        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        // 1) �� ������ ��� ��ٷȴٰ�
        yield return new WaitForSeconds(delayBeforeShow);

        // 2) ��Ʈ�� UI ���̵� ��
        if (introGroup != null)
        {
            // ���̵��� �ѵ� �� ������ ��Ÿ����
            introGroup.gameObject.SetActive(true);
            yield return Fade(0f, 1f, fadeTime);

            introGroup.interactable = true;
            introGroup.blocksRaycasts = true;
        }

        // 3) UI�� visibleTime ��ŭ ����
        yield return new WaitForSeconds(visibleTime);

        // 4) ���̵� �ƿ�
        if (introGroup != null)
        {
            introGroup.interactable = false;
            introGroup.blocksRaycasts = false;

            yield return Fade(1f, 0f, fadeTime);

            // �ʿ��ϸ� ������ ���� ���� ���� ��� (���� �ڷ�ƾ ������ �����̶� ������)
            introGroup.gameObject.SetActive(false);
        }

        // 5) �������� ��� �ν� ����!
        IntroFinished = true;

        if (scriptsToEnable != null)
        {
            foreach (var s in scriptsToEnable)
            {
                if (s != null) s.enabled = true;
            }
        }
    }

    IEnumerator Fade(float from, float to, float time)
    {
        if (introGroup == null) yield break;

        float t = 0f;
        introGroup.alpha = from;

        while (t < time)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Clamp01(t / time);
            introGroup.alpha = Mathf.Lerp(from, to, lerp);
            yield return null;
        }

        introGroup.alpha = to;
    }
}
