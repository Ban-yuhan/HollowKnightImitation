using UnityEngine;

public class BossCameraPosition : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            CameraFollow2D cameraFollow = Camera.main.GetComponent<CameraFollow2D>();
            if (cameraFollow != null)
            {
                cameraFollow.SetisInBossRoom(true);
            }
        }
    }
}
