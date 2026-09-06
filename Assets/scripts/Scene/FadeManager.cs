using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.0f; // 페이드에 걸리는 시간 (초)

    private void Awake()
    {
        // 싱글톤 패턴 설정 및 씬 전환 시 파괴 방지
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 지정한 씬으로 페이드아웃 후 전환 + 페이드인
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeAndLoadScene(sceneName));
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        // 1. 페이드 아웃 (화면이 어두워짐)
        yield return StartCoroutine(Fade(1f));

        // 2. 씬 비동기 로드 및 완료 대기
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 3. 페이드 인 (화면이 다시 밝아짐)
        yield return StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        fadeImage.raycastTarget = true; // 페이드 중 클릭 방지

        Color color = fadeImage.color;
        float startAlpha = color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        fadeImage.color = color;

        // 완전 밝아지면 터치/클릭 방지 해제
        if (targetAlpha == 0f)
        {
            fadeImage.raycastTarget = false;
        }
    }
}