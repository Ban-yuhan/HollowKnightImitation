using UnityEngine;

public class DoorToBoss : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "BossMap";
    private bool isTransitioning = false; // 중복 이동 방지 플래그

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 태그 확인 및 중복 실행 방지
        if (collision.CompareTag("Player") && !isTransitioning)
        {
            isTransitioning = true;

            // FadeManager를 통해 페이드 연출을 거치며 BossMap으로 이동
            if (FadeManager.Instance != null)
            {
                FadeManager.Instance.LoadSceneWithFade(targetSceneName);
            }
            else
            {
                // FadeManager가 없을 경우 바로 씬 이동
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
        }
    }
}