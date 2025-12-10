using System.Collections;
using UnityEngine;
using TMPro;

public class MotionText : MonoBehaviour
{
    [Header("Settings")]
    public float floatSpeed = 3.0f;
    public float moveDistance = 30.0f;
    public float timeBetweenWords = 0.1f;

    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        
        // [수정된 부분] 텍스트 메쉬를 강제로 업데이트하여 정보를 갱신합니다.
        textComponent.ForceMeshUpdate();
        
        // 시작 시 투명하게 설정
        textComponent.alpha = 0; 
    }

    public void PlayIn()
    {
        gameObject.SetActive(true);
        textComponent.alpha = 1; 
        StartCoroutine(AnimateText(true));
    }

    public void PlayOut()
    {
        StartCoroutine(AnimateText(false));
    }

    private IEnumerator AnimateText(bool isShowing)
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;
        int wordCount = textInfo.wordCount;

        for (int i = 0; i < wordCount; i++)
        {
            StartCoroutine(AnimateSingleWord(i, isShowing));
            yield return new WaitForSeconds(timeBetweenWords);
        }
    }

    private IEnumerator AnimateSingleWord(int wordIndex, bool isShowing)
    {
        TMP_TextInfo textInfo = textComponent.textInfo;
        TMP_WordInfo wordInfo = textInfo.wordInfo[wordIndex];
        
        // 원본 위치 저장
        Vector3[][] originalPositions = new Vector3[wordInfo.characterCount][];
        
        for (int j = 0; j < wordInfo.characterCount; j++)
        {
            int charIndex = wordInfo.firstCharacterIndex + j;
            if (!textInfo.characterInfo[charIndex].isVisible) continue;
            
            int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;
            
            originalPositions[j] = new Vector3[4];
            var sourceVertices = textInfo.meshInfo[materialIndex].vertices;
            
            for(int v=0; v<4; v++) originalPositions[j][v] = sourceVertices[vertexIndex + v];
        }

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * floatSpeed;
            float t = Mathf.Clamp01(timer);
            float curveT = t * t * (3f - 2f * t); 

            float alphaValue = isShowing ? curveT : (1 - curveT);
            float currentOffsetFactor = isShowing ? (1 - curveT) : curveT;
            Vector3 offset = Vector3.up * moveDistance * currentOffsetFactor;

            for (int j = 0; j < wordInfo.characterCount; j++)
            {
                int charIndex = wordInfo.firstCharacterIndex + j;
                if (!textInfo.characterInfo[charIndex].isVisible) continue;

                int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;
                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
                Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

                for (int v = 0; v < 4; v++)
                {
                    vertices[vertexIndex + v] = originalPositions[j][v] - offset;
                }

                byte alpha = (byte)(255 * alphaValue);
                for(int v=0; v<4; v++) colors[vertexIndex + v].a = alpha;
            }

            textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }
    }
}