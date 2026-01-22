using UnityEngine;

public class UnitData : MonoBehaviour
{
    [Header("Unit State")]
    public bool hasMovedThisTurn = false;
    public bool canMoveMultipleTimes = false;

    // 내부 저장용 변수
    [SerializeField] private int _deathCount = 0;

    // [추가] 프로퍼티를 통해 값이 변경될 때 로그 출력
    public int deathCount 
    {
        get => _deathCount;
        set 
        {
            // 원인 파악을 위해 조건문 밖으로 로그를 뺍니다.
            Debug.Log($"[Set 호출됨] {gameObject.name}에게 들어온 값: {value} (기존 값: {_deathCount})");

            if (_deathCount != value)
            {
                _deathCount = value;
                Debug.Log($"<color=yellow>[Stack Update]</color> {gameObject.name}의 최종 스택: <b>{_deathCount}</b>");
            }
        }
    }
    private bool _isDead = false;

    public bool isDead 
    {
        get => _isDead;
        set {
            _isDead = value;
            if (_isDead) {
                // 사망 시 실행할 로직을 여기에 넣을 수 있습니다.
                Debug.Log($"{gameObject.name}이 사망 판정을 받았습니다.");
                // 예: SpriteRenderer를 비활성화하거나 회색으로 변경
                // GetComponent<SpriteRenderer>().color = Color.gray;
            }
        }
    }
    
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    // [해결] TurnManager.cs(67,22) 에러 해결
    public void ResetTurnState()
    {
        hasMovedThisTurn = false;
        // 필요하다면 유닛의 색상을 원래대로 돌리는 로직 추가
    }

    // [해결] UIManager.cs(112,40) 에러 해결
    public void SetMovementState(bool moved)
    {
        if (canMoveMultipleTimes)
        {
            hasMovedThisTurn = false;
        }
        else
        {
            hasMovedThisTurn = moved;
        }
    }

    // [해결] GameManager에서 유닛 선택 시 시각 효과용
    public void SetSelectedVisual(bool isSelected)
    {
        if (originalScale == Vector3.zero) originalScale = transform.localScale;

        if (isSelected)
        {
            transform.localScale = originalScale * 1.15f; 
            // 시각적으로 돋보이게 하기 위해 순서를 맨 앞으로 보낼 수도 있습니다. (SpriteRenderer.sortingOrder 변경 등)
        }
        else
        {
            transform.localScale = originalScale; 
        }
    }
}