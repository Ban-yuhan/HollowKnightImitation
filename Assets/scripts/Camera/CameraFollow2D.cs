using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f; // Lerp 속도 대신 완충 시간(초)을 사용합니다.

    private bool isInBossRoom = false;
    private Vector3 velocity = Vector3.zero; // SmoothDamp 내부에서 사용할 속도 값

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition;

        if (!isInBossRoom)
        {
            // 플레이어 추적 위치
            targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        }
        else
        {
            // 보스룸 고정 위치
            targetPosition = new Vector3(36.97f, 3.12f, transform.position.z);
        }

        // Vector3.Lerp 대신 SmoothDamp를 사용해 물리 이동 떨림을 완전 해소
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    public bool GetisInBossRoom()
    {
        return isInBossRoom;
    }

    public void SetisInBossRoom(bool value)
    {
        isInBossRoom = value;
    }
}