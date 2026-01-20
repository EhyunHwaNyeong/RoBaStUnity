using UnityEngine;

public class UnitData : MonoBehaviour
{
    public bool hasMovedThisTurn = false;
    public bool canMoveMultipleTimes = false;
    public bool isDead = false;
    public int deathCount = 0;
    
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