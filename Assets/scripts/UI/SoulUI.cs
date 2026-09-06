using UnityEngine;

public class SoulUI : MonoBehaviour
{
    [Header("UI 애니메이터")]
    [SerializeField] private Animator soulAnimator;

    private int currentLevel = -1;

    private void Awake()
    {
        if (soulAnimator == null)
            soulAnimator = GetComponent<Animator>();
    }

    public void InitSoulUI(int currentSoul)
    {
        if (soulAnimator == null) return;

        int newLevel = GetSoulStateLevel(currentSoul);
        currentLevel = newLevel;

        // 1. 파라미터 값 설정
        soulAnimator.SetInteger("SoulLevel", currentLevel);

        // 2. 이미지에 맞춰 실제 State 이름으로 강제 재생 (시작 시 SoulAnimFull 재생)
        string stateName = GetStateName(currentLevel);
        soulAnimator.Play(stateName, 0, 0f);
    }

    public void UpdateSoulUI(int currentSoul)
    {
        if (soulAnimator == null) return;

        int newLevel = GetSoulStateLevel(currentSoul);

        if (newLevel != currentLevel)
        {
            currentLevel = newLevel;
            soulAnimator.SetInteger("SoulLevel", currentLevel);
        }
    }

    private int GetSoulStateLevel(int soulCount)
    {
        if (soulCount == 0) return 0;         // 0단계
        if (soulCount < 4) return 1;          // 1단계
        if (soulCount < 8) return 2;          // 2단계
        if (soulCount < 12) return 3;         // 3단계
        return 4;                             // FULL (12개)
    }

    // ★ 보내주신 유니티 애니메이터 이미지 속 State 이름과 100% 동일하게 매칭
    private string GetStateName(int level)
    {
        switch (level)
        {
            case 0: return "SoulAnim0";
            case 1: return "SoulAnimIdle1";
            case 2: return "SoulAnimIdle2";
            case 3: return "SoulAnimIdle3";
            case 4: return "SoulAnimFull";
            default: return "SoulAnimFull";
        }
    }
}