using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [Header("체력 칸 UI 이미지 (5개)")]
    [SerializeField] private Image[] maskImages;

    [Header("마스크 스프라이트 에셋")]
    [SerializeField] private Sprite fullMaskSprite;   // 찬 마스크
    [SerializeField] private Sprite brokenMaskSprite; // 부러진 마스크
    [SerializeField] private Sprite emptyMaskSprite;  // 빈 마스크

    private int previousHealth;

    public void InitHealthUI(int maxHealth)
    {
        previousHealth = maxHealth;
        UpdateAllMasks(maxHealth);
    }

    public void UpdateHealthUI(int currentHealth)
    {
        // 1. 데미지 입었을 때 (부러지는 연출 코루틴 실행)
        if (currentHealth < previousHealth)
        {
            for (int i = currentHealth; i < previousHealth; i++)
            {
                if (i >= 0 && i < maskImages.Length)
                {
                    StartCoroutine(PlayBreakAnimation(maskImages[i]));
                }
            }
        }
        // 2. 회복되었을 때
        else if (currentHealth > previousHealth)
        {
            UpdateAllMasks(currentHealth);
        }

        previousHealth = currentHealth;
    }

    private IEnumerator PlayBreakAnimation(Image targetMask)
    {
        targetMask.sprite = brokenMaskSprite;
        yield return new WaitForSeconds(0.2f);
        targetMask.sprite = emptyMaskSprite;
    }

    private void UpdateAllMasks(int currentHealth)
    {
        for (int i = 0; i < maskImages.Length; i++)
        {
            if (i < currentHealth)
            {
                maskImages[i].sprite = fullMaskSprite;
            }
            else
            {
                maskImages[i].sprite = emptyMaskSprite;
            }
        }
    }
}