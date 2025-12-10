using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 필수

public class FloatingTextAnimator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TMP_Text textComponent;
    [SerializeField] private float floatSpeed = 2.0f;     // 글자가 올라오는 속도
    [SerializeField] private float moveDistance = 20.0f;  // 아래에서 올라올 거리
    [SerializeField] private float timeBetweenWords = 0.1f; // 다음 단어 나올 때까지의 딜레이

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        // 시작 시 애니메이션 실행 (원하는 시점에 이 함수를 호출하면 됩니다)
        StartCoroutine(AnimateText());
    }

    public IEnumerator AnimateText()
    {
        // 1. 텍스트 메쉬 데이터 강제 업데이트 (필수)
        textComponent.ForceMeshUpdate();

        TMP_TextInfo textInfo = textComponent.textInfo;
        int wordCount = textInfo.wordCount;

        // 2. 초기화: 모든 글자를 투명하게 만들고 위치를 아래로 내림
        // (원본 위치 데이터를 보존하기 위해 캐싱해두는 것이 좋지만, 
        // 간단한 구현을 위해 즉시 값을 변경하며 애니메이션 합니다.)
        
        // 색상 초기화 (투명)
        Color32[] newVertexColors;
        Color32 c0 = textComponent.color;
        c0.a = 0;
        
        // 모든 글자를 일단 투명하게 설정
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;
            
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            newVertexColors = textInfo.meshInfo[materialIndex].colors32;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

            newVertexColors[vertexIndex + 0] = c0;
            newVertexColors[vertexIndex + 1] = c0;
            newVertexColors[vertexIndex + 2] = c0;
            newVertexColors[vertexIndex + 3] = c0;
        }
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // 3. 단어 단위로 루프를 돌며 애니메이션 코루틴 실행
        for (int i = 0; i < wordCount; i++)
        {
            // 각 단어별로 별도의 애니메이션 시작
            StartCoroutine(AnimateSingleWord(i));
            
            // 다음 단어 애니메이션 시작 전 대기
            yield return new WaitForSeconds(timeBetweenWords);
        }
    }

    private IEnumerator AnimateSingleWord(int wordIndex)
    {
        TMP_TextInfo textInfo = textComponent.textInfo;
        TMP_WordInfo wordInfo = textInfo.wordInfo[wordIndex];

        // 원본 위치들을 저장 (애니메이션 목표지점)
        Vector3[][] originalPositions = new Vector3[wordInfo.characterCount][];
        
        for (int j = 0; j < wordInfo.characterCount; j++)
        {
            int charIndex = wordInfo.firstCharacterIndex + j;
            if (!textInfo.characterInfo[charIndex].isVisible) continue;

            int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;
            
            // 현재 글자의 4개 버텍스 원본 위치 저장
            originalPositions[j] = new Vector3[4];
            var sourceVertices = textInfo.meshInfo[materialIndex].vertices;
            
            for(int v=0; v<4; v++) originalPositions[j][v] = sourceVertices[vertexIndex + v];
        }

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * floatSpeed;
            float t = Mathf.Clamp01(timer);
            // 부드러운 움직임을 위한 Easing (선택사항)
            float curveT = t * t * (3f - 2f * t); // SmoothStep

            for (int j = 0; j < wordInfo.characterCount; j++)
            {
                int charIndex = wordInfo.firstCharacterIndex + j;
                if (!textInfo.characterInfo[charIndex].isVisible) continue;

                int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
                Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

                // 목표 위치(원본)와 시작 위치(아래쪽) 사이를 보간
                Vector3 offset = Vector3.up * moveDistance * (1 - curveT);
                
                // 위치 업데이트 (원본 위치에서 offset만큼 뺀 곳에서 시작해서 원본으로 이동)
                for (int v = 0; v < 4; v++)
                {
                    vertices[vertexIndex + v] = originalPositions[j][v] - offset;
                }

                // 알파값(투명도) 업데이트
                byte alpha = (byte)(255 * curveT);
                colors[vertexIndex + 0].a = alpha;
                colors[vertexIndex + 1].a = alpha;
                colors[vertexIndex + 2].a = alpha;
                colors[vertexIndex + 3].a = alpha;
            }

            // 변경된 메쉬 데이터를 적용
            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            yield return null;
        }
    }
}