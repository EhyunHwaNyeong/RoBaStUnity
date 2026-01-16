using UnityEngine;

public class UnitData : MonoBehaviour
{
    [Header("유닛 상태")]
    public int deathCount = 0;
    public bool isDead = false;

    // [추가] 현재 적의 정면에 위치하여 카운트가 올라간 상태인지 체크하는 변수
    public bool isCurrentlyWatched = false; 

    public void UpdateDeathCount(int amount)
    {
        deathCount += amount;
        // 카운트가 0~4 범위를 벗어나지 않도록 제한
        deathCount = Mathf.Clamp(deathCount, 0, 4);
        Debug.Log($"{gameObject.name}의 현재 DeathCount: {deathCount}");
    }
}