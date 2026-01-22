using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class KillManager : MonoBehaviour
{
    public static KillManager Instance;
    public Tilemap targetTilemap;
    public Transform deathUnitArea;

    [System.Serializable]
    public struct TeamSettings {
        public string teamTag;
        public Transform deathArea;
    }
    public List<TeamSettings> teamSettings;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- 1. 유닛 조작 직후: 스택 4단계 즉시 사망 체크 ---
    public void CheckAndApplyDeathRules()
    {
        if (targetTilemap == null) return;

        int unitLayer = LayerMask.GetMask("Commander", "Knight");
        Bounds bounds = targetTilemap.localBounds;
        Vector3 center = targetTilemap.transform.TransformPoint(bounds.center);
        Vector2 size = new Vector2(bounds.size.x, bounds.size.y);
        Collider2D[] allUnits = Physics2D.OverlapBoxAll(center, size, 0, unitLayer);

        Dictionary<GameObject, int> exposedCountMap = new Dictionary<GameObject, int>();
        foreach (var col in allUnits) exposedCountMap[col.gameObject] = 0;

        // 필드 전체 스캔하여 노출 스택 계산
        foreach (var col in allUnits)
        {
            GameObject attacker = col.gameObject;
            UnitData attackerData = attacker.GetComponent<UnitData>();
            if (attackerData == null || attackerData.isDead) continue;

            Vector3Int attackerCell = targetTilemap.WorldToCell(attacker.transform.position);
            Vector3Int forwardDir = Vector3Int.RoundToInt(attacker.transform.up);
            Vector3 checkPos = targetTilemap.GetCellCenterWorld(attackerCell + forwardDir);

            Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.4f, unitLayer);
            if (hit != null && hit.gameObject != attacker && !hit.CompareTag(attacker.tag))
            {
                exposedCountMap[hit.gameObject]++;
            }
        }

        // 결과 적용 및 조건 2(4스택 즉시 사망) 체크
        foreach (var col in allUnits)
        {
            UnitData data = col.GetComponent<UnitData>();
            if (data == null || data.isDead) continue;

            data.deathCount = exposedCountMap[col.gameObject];

            // [조건 2] DeathCount가 4일 때 즉시 사망 처리
            if (data.deathCount >= 4)
            {
                Debug.Log($"<color=red>[즉시 사망]</color> {col.name}: 4스택 도달");
                ExcludeUnit(col.gameObject);
            }
        }
    }

    // --- 2. 턴 종료 시 호출: 조건 1(내 팀 턴 종료 시 3스택 사망) 체크 ---
    public void CheckTurnEndDeath(string endedTurnTag)
    {
        UnitData[] allUnits = FindObjectsByType<UnitData>(FindObjectsSortMode.None);
        foreach (var data in allUnits)
        {
            if (data.isDead) continue;

            // [조건 1] 스택이 3이고, 방금 종료된 턴의 태그가 유닛 자신의 태그와 같을 때 사망
            if (data.deathCount == 3 && data.CompareTag(endedTurnTag))
            {
                Debug.Log($"<color=orange>[턴 종료 사망]</color> {data.name}: 자신의 턴 종료 시 3스택 유지");
                ExcludeUnit(data.gameObject);
            }
        }
    }

    public void ExcludeUnit(GameObject unit)
    {
        UnitData data = unit.GetComponent<UnitData>();
        if (data == null || data.isDead) return;

        data.isDead = true;
        unit.GetComponent<Collider2D>().enabled = false;
        var setting = teamSettings.Find(x => x.teamTag == unit.tag);
        if (setting.deathArea != null) {
            unit.transform.SetParent(setting.deathArea);
            unit.transform.localPosition = Vector3.zero;
        }
        if (WinManager.Instance != null)
        {
            WinManager.Instance.RegisterDeath(unit);
        }
        unit.SetActive(false);
    }
}