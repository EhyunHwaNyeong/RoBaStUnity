using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class KillManager : MonoBehaviour
{
    public static KillManager Instance;

    [Header("참조 설정")]
    public Tilemap targetTilemap;
    public Transform deathUnitArea;

    [System.Serializable]
    public struct TeamSettings
    {
        public string teamTag;         // "White" 또는 "Black"
        public GameObject killButton;  // 해당 팀의 화면 하단 킬 버튼
        public Transform deathArea;    // 해당 팀 사망 유닛 이동 지역
    }

    [Header("팀별 설정")]
    public List<TeamSettings> teamSettings;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (deathUnitArea == null)
            deathUnitArea = new GameObject("DeathUnitArea_Global").transform;
    }

    // --- 1. 유닛 조작 후 스택 갱신 및 즉시 사망 체크 ---
    public void CheckAndApplyDeathRules(GameObject movedUnit)
    {
        if (targetTilemap == null) return;

        int unitLayer = LayerMask.GetMask("Commander", "Knight");
        Bounds bounds = targetTilemap.localBounds;
        Vector3 center = targetTilemap.transform.TransformPoint(bounds.center);
        Vector2 size = new Vector2(bounds.size.x, bounds.size.y);
        Collider2D[] allUnits = Physics2D.OverlapBoxAll(center, size, 0, unitLayer);

        Dictionary<GameObject, int> exposedCountMap = new Dictionary<GameObject, int>();
        foreach (var col in allUnits) exposedCountMap[col.gameObject] = 0;

        foreach (var col in allUnits)
        {
            GameObject attacker = col.gameObject;
            UnitData attackerData = attacker.GetComponent<UnitData>();
            if (attackerData == null || attackerData.isDead) continue;

            Vector3Int attackerCell = targetTilemap.WorldToCell(attacker.transform.position);
            Vector3Int forwardDir = Vector3Int.RoundToInt(attacker.transform.up);
            Vector3 checkPos = targetTilemap.GetCellCenterWorld(attackerCell + forwardDir);

            Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.2f, unitLayer);

            if (hit != null && hit.gameObject != attacker && hit.tag != attacker.tag)
            {
                exposedCountMap[hit.gameObject]++;
            }
        }

        foreach (var col in allUnits)
        {
            GameObject unit = col.gameObject;
            UnitData data = unit.GetComponent<UnitData>();
            if (data == null || data.isDead) continue;

            int currentExposedCount = exposedCountMap[unit];
            
            if (data.deathCount != currentExposedCount)
            {
                Debug.Log($"[DeathLog] {unit.name}: 스택 동기화 ({data.deathCount} -> {currentExposedCount})");
                data.deathCount = currentExposedCount;
            }

            // 조건 2: 스택이 4일 땐 조작 즉시 제거
            if (data.deathCount >= 4)
            {
                Debug.Log($"[DeathLog] {unit.name}: 스택 4 도달로 즉시 사망.");
                ExcludeUnit(unit);
            }
        }
    }

    // --- 2. 킬 버튼 눌렀을 때 실행 (조건 1: 스택 3 이상 제거) ---
    public void ExecuteKillProcess(string buttonTeamTag)
    {
        GameObject selected = GameManager.Instance.selectedObject;
        if (selected == null || selected.tag != buttonTeamTag) return;

        UnitData data = selected.GetComponent<UnitData>();
        if (data != null && data.deathCount >= 3)
        {
            Debug.Log($"[KillButton] {selected.name}: 스택 {data.deathCount}에서 유저 조작으로 제거.");
            ExcludeUnit(selected);
        }
    }

    // --- 3. 턴 종료 시 호출 (조건 1: 태그가 동일한 Turn이 지났을 때 제거) ---
    public void CheckTurnEndDeath(string endedTurnTag)
    {
        if (targetTilemap == null) return;

        int unitLayer = LayerMask.GetMask("Commander", "Knight");
        Bounds bounds = targetTilemap.localBounds;
        Vector3 center = targetTilemap.transform.TransformPoint(bounds.center);
        Vector2 size = new Vector2(bounds.size.x, bounds.size.y);
        Collider2D[] allUnits = Physics2D.OverlapBoxAll(center, size, 0, unitLayer);

        foreach (var col in allUnits)
        {
            UnitData data = col.GetComponent<UnitData>();
            if (data == null || data.isDead) continue;

            // [수정된 조건 1] 스택이 3이고, 방금 종료된 턴의 태그와 유닛의 태그가 같을 때만 제거
            if (data.deathCount == 3 && col.gameObject.tag == endedTurnTag)
            {
                Debug.Log($"[TurnEndLog] {col.name}: 자신의 팀({endedTurnTag}) 턴 종료 시 스택 3으로 사망.");
                ExcludeUnit(col.gameObject);
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
        if (setting.deathArea != null)
        {
            unit.transform.SetParent(setting.deathArea);
            unit.transform.localPosition = Vector3.zero;
        }

        unit.SetActive(false);
        if (GameManager.Instance.selectedObject == unit) UIManager.Instance.CloseAllUI();
    }
}