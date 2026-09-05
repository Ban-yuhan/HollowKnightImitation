using UnityEngine;

public class WaveObject : MonoBehaviour
{
    [Header("벽 감지 좌표 설정")]
    [SerializeField] private float minX = 30f;           // 맵 좌측 경계
    [SerializeField] private float maxX = 57f;           // 맵 우측 경계
    [SerializeField] private float wallThreshold = 3; // 경계선과 얼마나 가까워졌을 때 소멸할지

    [Header("컴포넌트")]
    private Animator anim;
    private Rigidbody2D rb;
    private bool isDestroying = false;

    [SerializeField] private string ParamNearWall = "NearWall";


    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDestroying) return;

        float currentX = transform.position.x;

        // 좌측 벽(minX) 또는 우측 벽(maxX) 근처에 다다랐는지 검사
        if (currentX <= minX + wallThreshold || currentX >= maxX - wallThreshold)
        {
            TriggerDestroy();
        }
    }

    // 외부(보스 스크립트 등)에서 맵 범위를 동적으로 전달해 줄 수 있는 함수
    public void SetBounds(float min, float max)
    {
        minX = min;
        maxX = max;
    }

    private void TriggerDestroy()
    {
        isDestroying = true;

        
        // NearWall 트리거 발동 (소멸 애니메이션 실행)
        if (anim != null)
        {
            anim.SetTrigger(ParamNearWall);
        }


    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}