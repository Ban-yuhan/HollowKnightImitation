using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DistanceFadeSprite : MonoBehaviour
{
    [Header("타겟 및 거리 설정")]
    [SerializeField] private Transform player;
    [SerializeField] private float showDistance = 4f;
    [SerializeField] private float fullShowDistance = 2f;
    [SerializeField] private float fadeSpeed = 5f;

    [Header("기준점 위치 보정 (세로로 길 때 사용)")]
    [Tooltip("마이너스(-) 값으로 설정하면 감지 기준점이 아래로 내려갑니다.")]
    [SerializeField] private Vector2 centerOffset = new Vector2(0f, -1f);

    private SpriteRenderer spriteRenderer;
    private float targetAlpha = 0f;
    private float currentAlpha = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        SetAlpha(0f);
    }

    private void Update()
    {
        if (player == null) return;

        // 보정된 실제 감지 중심점 위치
        Vector2 checkPosition = (Vector2)transform.position + centerOffset;

        // 보정된 중심점 기준 거리 계산
        float distance = Vector2.Distance(checkPosition, player.position);

        if (distance <= fullShowDistance)
        {
            targetAlpha = 1f;
        }
        else if (distance <= showDistance)
        {
            targetAlpha = Mathf.InverseLerp(showDistance, fullShowDistance, distance);
        }
        else
        {
            targetAlpha = 0f;
        }

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        SetAlpha(currentAlpha);
    }

    private void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    // 씬 뷰에서 내려간 노란색/초록색 원 위치를 직접 확인하면서 조절 가능합니다.
    private void OnDrawGizmosSelected()
    {
        Vector3 checkPosition = transform.position + (Vector3)centerOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(checkPosition, showDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(checkPosition, fullShowDistance);
    }
}