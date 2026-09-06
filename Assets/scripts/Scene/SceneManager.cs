using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수!

public class MainMenu : MonoBehaviour
{
    // [START] 버튼 클릭 시 호출
    public void PlayGame()
    {
        // "GameScene"이라는 이름의 씬으로 이동합니다.
        // (프로젝트의 실제 게임 플레이 씬 이름과 동일해야 합니다)
        SceneManager.LoadScene("GameScene");
    }

    // [END GAME] 버튼 클릭 시 호출
    public void QuitGame()
    {
        Debug.Log("게임 종료!"); // 에디터 테스트 확인용 로그

#if UNITY_EDITOR
        // 유니티 에디터에서 플레이 중일 때 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 실제 게임에서 종료
        Application.Quit();
#endif
    }
}