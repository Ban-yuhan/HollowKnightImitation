using UnityEngine;

public class BossTrigger : MonoBehaviour
{

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(isTriggered)
        {
            return;
        }

        if(collision.CompareTag("Player"))
        {
            CameraFollow2D cameraFollow = Camera.main.GetComponent<CameraFollow2D>();
            if (cameraFollow != null)
            {
                cameraFollow.SetisInBossRoom(true);
            }

            DoorLock doorLock = GameObject.Find("DoorLock").GetComponent<DoorLock>();

            if(doorLock != null)
            {
                doorLock.GateDown();
            }

            BossPattern bossPattern = GameObject.Find("Boss").GetComponent<BossPattern>();
            if (bossPattern != null)
            {
                bossPattern.ActivateBoss();
            }

            isTriggered = true;
        }

    }
}
