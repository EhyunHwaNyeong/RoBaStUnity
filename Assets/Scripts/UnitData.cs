using UnityEngine;

public class UnitData : MonoBehaviour
{
    public int deathCount = 0;
    public bool isDead = false;

    // 데스카운트 변경 시 호출될 메서드
    public void UpdateDeathCount(int amount)
    {
        if (isDead) return;
        
        deathCount += amount;
        deathCount = Mathf.Clamp(deathCount, 0, 4); // 0~4 사이 유지
        
        Debug.Log($"{gameObject.name}의 DeathCount: {deathCount}");
    }
}