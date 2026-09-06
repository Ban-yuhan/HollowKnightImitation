using UnityEngine;
using System.Collections;

public class DoorLock : MonoBehaviour
{
    [SerializeField] private Transform gateTransform;
    [SerializeField] private float downYPosition; // 문이 닫혔을 때(내려갔을 때)의 Y 좌표
    [SerializeField] private float upYPosition;   // 문이 열렸을 때(올라갔을 때)의 Y 좌표
    [SerializeField] private float dropSpeed = 15f; // 움직이는 속도

    // 문을 내릴 때 호출
    public void GateDown()
    {
        StartCoroutine(MoveGateRoutine(downYPosition));
    }

    // 문을 올릴 때 호출
    public void GateUp()
    {
        StartCoroutine(MoveGateRoutine(upYPosition));
    }

    private IEnumerator MoveGateRoutine(float targetY)
    {
        Vector3 targetPos = new Vector3(gateTransform.position.x, targetY, gateTransform.position.z);

        // 목표 Y 좌표에 도달할 때까지 매 프레임 이동
        while (Mathf.Abs(gateTransform.position.y - targetY) > 0.01f)
        {
            gateTransform.position = Vector3.MoveTowards(gateTransform.position, targetPos, dropSpeed * Time.deltaTime);
            yield return null; // 다음 프레임까지 대기
        }

        // 최종 위치 보정
        gateTransform.position = targetPos;
    }
}