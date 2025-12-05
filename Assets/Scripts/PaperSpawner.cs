using UnityEngine;

public class PaperSpawner : MonoBehaviour
{
    [Header("참조")]
    public RectTransform container;   // PaperContainer
    public GameObject paperPrefab;    // PaperPrefab (Image + PaperFall)

    [Header("Accumulate 한 번당 생성 수")]
    public int countPerTrigger = 6;

    [Header("자동 스폰 설정 (씬 전환 연출용)")]
    public bool autoSpawn = false;
    public float autoSpawnInterval = 0.15f;

    private float autoSpawnTimer = 0f;

    private void Update()
    {
        if (!autoSpawn) return;

        autoSpawnTimer -= Time.deltaTime;
        if (autoSpawnTimer <= 0f)
        {
            SpawnOne();
            autoSpawnTimer = autoSpawnInterval;
        }
    }

    // === Accumulate 이벤트에서 호출 (손 한번 쌓을 때마다) ===
    public void SpawnByAccumulate()
    {
        for (int i = 0; i < countPerTrigger; i++)
        {
            SpawnOne();
        }
    }

    // === Accumulate 끝나고 자동으로 계속 떨어뜨릴 때 ===
    public void StartAutoSpawn()
    {
        autoSpawn = true;
        autoSpawnTimer = 0f;
    }

    public void StopAutoSpawn()
    {
        autoSpawn = false;
    }

    // 실제 종이 1장 생성 함수
    private void SpawnOne()
    {
        if (paperPrefab == null || container == null) return;

        GameObject go = Instantiate(paperPrefab, container);
        RectTransform rect = go.GetComponent<RectTransform>();

        float width  = container.rect.width;
        float height = container.rect.height;

        // 화면 위쪽 바깥에서 랜덤 위치로 시작
        float x = Random.Range(-width * 0.5f, width * 0.5f);
        float y = height * 0.5f + Random.Range(50f, 200f);

        rect.anchoredPosition = new Vector2(x, y);
        rect.localScale = Vector3.one * Random.Range(0.7f, 1.2f);

        PaperFall fall = go.GetComponent<PaperFall>();
        if (fall != null)
        {
            // 각 종이마다 다 다른 값 주기 (불규칙 느낌)
            float duration    = Random.Range(3.5f, 6.5f);
            float amplitude   = Random.Range(60f, 140f);
            float frequency   = Random.Range(1.0f, 3.0f);
            float rotSpeed    = Random.Range(-40f, 40f);
            float startDelay  = Random.Range(0f, 0.4f);

            fall.Init(container, duration, amplitude, frequency, rotSpeed, startDelay);
        }
    }
}
