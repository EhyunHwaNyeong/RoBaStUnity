using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class KillManager : MonoBehaviour
{
    public static KillManager Instance;

    [System.Serializable]
    public struct TeamSettings
    {
        public string teamTag;         // "White" 또는 "Black"
        public GameObject killButton;  // 해당 팀의 화면 하단 킬 버튼
        public Transform deathArea;    // 해당 팀 사망 유닛 이동 지역
    }

    [Header("팀별 설정 (버튼 및 사망지역)")]
    public List<TeamSettings> teamSettings;

    [Header("참조")]
    public Tilemap targetTilemap;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- 1. 유닛 조작 후 사망 규칙 체크 (GameManager에서 호출) ---
    public void CheckAndApplyDeathRules(GameObject movedUnit)
    {
        if (targetTilemap == null) return;

        // [수정] 타일맵의 월드 중심점과 크기를 정확하게 가져옵니다.
        Bounds bounds = targetTilemap.localBounds;
        Vector3 center = targetTilemap.transform.TransformPoint(bounds.center);
        Vector2 size = new Vector2(bounds.size.x, bounds.size.y);

        int unitLayer = LayerMask.GetMask("Commander", "Knight");
        
        // 타일맵 영역 내의 모든 유닛 탐색
        Collider2D[] allUnits = Physics2D.OverlapBoxAll(center, size, 0, unitLayer);

        foreach (var col in allUnits)
        {
            GameObject unit = col.gameObject;
            UnitData data = unit.GetComponent<UnitData>();
            if (data == null || data.isDead) continue;

            // 해당 유닛을 누군가 바라보고 있는지 체크
            bool isBeingWatched = CheckIfWatchedByEnemy(unit);

            if (isBeingWatched)
            {
                // 적이 나를 바라보고 있다면 +1 (최대치는 로직상 4까지)
                data.UpdateDeathCount(1);
            }
            else
            {
                // 아무도 나를 바라보고 있지 않다면 -1 (최소 0)
                if (data.deathCount > 0)
                {
                    data.UpdateDeathCount(-1);
                }
            }

            // [조건 판정] 
            // 4가 되면 즉시 사망
            if (data.deathCount >= 4)
            {
                ExcludeUnit(unit);
            }
        }
    }

    // --- 2. 특정 유닛을 적군이 바라보고 있는지 체크하는 로직 ---
    private bool CheckIfWatchedByEnemy(GameObject targetUnit)
    {
        Vector3Int targetCell = targetTilemap.WorldToCell(targetUnit.transform.position);
        int unitLayer = LayerMask.GetMask("Commander", "Knight");

        // 주변 4칸(상하좌우)을 검사하여 나를 바라보는 적이 있는지 확인
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (Vector3Int dir in directions)
        {
            Vector3 checkPos = targetTilemap.GetCellCenterWorld(targetCell + dir);
            Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.2f, unitLayer);

            if (hit != null && hit.gameObject != targetUnit && hit.tag != targetUnit.tag)
            {
                // 적의 정면(transform.up)이 targetUnit의 위치를 향하고 있는지 체크
                Vector3Int enemyForward = Vector3Int.RoundToInt(hit.transform.up);
                Vector3Int dirToTarget = -dir; // 적에서 나를 보는 방향

                if (enemyForward == dirToTarget)
                {
                    return true; // 한 명이라도 나를 보고 있다면 true
                }
            }
        }
        return false;
    }

    // --- 3. 킬 버튼 눌렀을 때 실행 (OnClick 연결) ---
    public void ExecuteKillProcess(string buttonTeamTag)
    {
        GameObject selected = GameManager.Instance.selectedObject;
        
        if (selected == null || selected.tag != buttonTeamTag) return;

        UnitData data = selected.GetComponent<UnitData>();
        // DeathCount가 3 이상일 때만 버튼으로 제거 가능
        if (data != null && data.deathCount >= 3)
        {
            ExcludeUnit(selected);
        }
        else
        {
            Debug.Log("제거 조건(DeathCount 3)이 충족되지 않았습니다.");
        }
    }

    // --- 4. 유닛 제거 및 사망 지역 이동 ---
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

        if (GameManager.Instance.selectedObject == unit)
        {
            UIManager.Instance.CloseAllUI();
        }
    }
}