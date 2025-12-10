using UnityEngine;

public class HotplateMission : MonoBehaviour
{
    [Header("Target Potion")]
    public PotionShakeController targetPotion;

    [Header("Snap Settings")]
    public Transform snapPoint;
    public bool makeKinematicOnClear = true;

    [Header("Mission")]
    public bool missionCleared = false;

    [Header("UI Sequence")]
    public IntroUISequence uiSequence;   // ⭐ Put / Great 텍스트 관리하는 스크립트

    [Header("Next Scene")]
    public float waitBeforeNextScene = 3f;   // Great 텍스트 보여줄 시간

    private void OnTriggerEnter(Collider other)
    {
        if (missionCleared) return;
        if (targetPotion == null) return;

        PotionShakeController potion =
            other.GetComponentInParent<PotionShakeController>();

        if (potion != null && potion == targetPotion)
        {
            if (potion.IsFullyShaken)
            {
                MissionClear();
            }
            else
            {
                Debug.Log("아직 충분히 안 흔들렸어요!");
            }
        }
    }

    private void MissionClear()
    {
        missionCleared = true;
        Debug.Log("🔥 미션 클리어! 포션이 활성화되었습니다.");

        var potionTransform = targetPotion.transform;
        var rb = targetPotion.GetComponent<Rigidbody>();
        var autoStick = targetPotion.GetComponent<AutoStickToHand>();

        // 1) 손과의 연결 완전 끊기
        if (autoStick != null)
        {
            autoStick.ForceDetach(true);
            autoStick.enabled = false;
        }

        // 2) 핫플레이트 위 스냅
        if (snapPoint != null)
        {
            potionTransform.position = snapPoint.position;
            potionTransform.rotation = snapPoint.rotation;
        }

        // 3) 그 자리에서 고정
        if (rb != null && makeKinematicOnClear)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // 4) ✅ Put 단계 UI 처리 (Put 텍스트들 → Great 텍스트로)
        if (uiSequence != null)
        {
            uiSequence.OnPutDone();   // 03Put + 03Direction 사라지고 04Great 뜨게
        }

        // 5) 잠깐 기다렸다가 다음 씬으로
        StartCoroutine(NextSceneRoutine());
    }

    private System.Collections.IEnumerator NextSceneRoutine()
    {
        yield return new WaitForSeconds(waitBeforeNextScene);
        SceneLoader.LoadAccumulate();
    }
}
