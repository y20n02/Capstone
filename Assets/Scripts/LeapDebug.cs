using UnityEngine;
using Leap;   // Ultraleap 네임스페이스

public class LeapDebug : MonoBehaviour
{
    // Hierarchy에서 "Service Provider (Desktop)" 오브젝트를
    // 이 슬롯에 드래그해서 넣어줘야 함
    public LeapServiceProvider provider;

    void Update()
    {
        if (provider == null)
        {
            Debug.LogWarning("LeapDebug: provider가 비어있어요. Service Provider(Desktop)를 Inspector에 연결해줘!");
            return;
        }

        Frame frame = provider.CurrentFrame;
        if (frame == null || frame.Hands == null || frame.Hands.Count == 0)
        {
            // 손 안 보일 때
            // Debug.Log("손 없음");
            return;
        }

        // UnityEngine.Hand랑 헷갈리지 않게 Leap.Hand로 명시
        foreach (Leap.Hand hand in frame.Hands)
        {
            var palmPos  = hand.PalmPosition;
            var palmVel  = hand.PalmVelocity;
            var grab     = hand.GrabStrength;   // 쥐고 있는 정도 (0~1)
            var pinch    = hand.PinchStrength;  // 집는 정도 (0~1)
            
            Vector3 unityVel = new Vector3(
                hand.PalmVelocity.x,
                hand.PalmVelocity.y,
                hand.PalmVelocity.z
            );
            float speed = unityVel.magnitude;

            bool isMoving = speed > 150f;

            Debug.Log(
                $"손 종류: {(hand.IsLeft ? "왼손" : "오른손")}\n" +
                $"위치: {palmPos}" +
                $"속도:{speed:F1}mm/s  움직임 여부:{isMoving}" +
                $"쥠(GrabStrength): {grab:F2}" +
                $"집기(PinchStrength): {pinch:F2}"
            );
        }
    }
}
