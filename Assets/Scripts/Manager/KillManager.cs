using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class KillManager : MonoBehaviour
{
    public static KillManager Instance;

    [Header("설정")]
    public Tilemap targetTilemap;
    public Transform deathUnitArea;

    // 태그별 전 전 전 전 전 전용 버튼 설정을 위한 구조체
    [System.Serializable]
    public struct TaggedButton
    {
        public string tag;           // "White" 또는 "Black"
        public GameObject killButton; // 해당 태그 유닛을 제거할 때 사용할 버튼 오브젝트
    }

    [Header("태그별 제거 버튼 설정")]
    public List<TaggedButton> tagButtonList;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (deathUnitArea == null)
            deathUnitArea = new GameObject("DeathUnitArea").transform;
    }

    // --- 1. 유닛 조작 후 사망 규칙 체크 (GameManager에서 호출) ---
    public void CheckAndApplyDeathRules(GameObject movedUnit)
    {
        if (movedUnit == null || targetTilemap == null) return;

        Vector3Int currentCell = targetTilemap.WorldToCell(movedUnit.transform.position);
        Vector3Int forwardDir = Vector3Int.RoundToInt(movedUnit.transform.up);
        Vector3Int targetCell = currentCell + forwardDir;

        Vector3 checkWorldPos = targetTilemap.GetCellCenterWorld(targetCell);
        int unitLayer = LayerMask.GetMask("Commander", "Knight");
        Collider2D hit = Physics2D.OverlapCircle(checkWorldPos, 0.2f, unitLayer);

        // 내 앞칸에 다른 태그를 가진 적이 있다면
        if (hit != null && hit.gameObject != movedUnit && hit.tag != movedUnit.tag)
        {
            UnitData targetData = hit.GetComponent<UnitData>();
            if (targetData != null)
            {
                targetData.UpdateDeathCount(1);
                
                // DeathCount가 4가 되면 즉시 제거
                if (targetData.deathCount >= 4)
                {
                    ExcludeUnit(hit.gameObject);
                }
            }
        }
        
        // 이동 후 선택된 유닛의 태그에 맞춰 버튼 UI 갱신
        UpdateKillButtonUI(movedUnit);
        // 조작 후 해당 팀의 AP 패널이 계속 보이도록 보장
        AP_Counter_Manager.Instance.ShowTeamPanel(movedUnit.tag);
    }

    // --- 2. 태그별 버튼 UI 활성화 관리 ---
    public void UpdateKillButtonUI(GameObject selectedUnit)
    {
        // 일단 모든 태그별 버튼을 끔
        foreach (var item in tagButtonList)
        {
            if (item.killButton != null) item.killButton.SetActive(false);
        }

        if (selectedUnit == null) return;

        UnitData data = selectedUnit.GetComponent<UnitData>();
        if (data == null) return;

        // DeathCount가 3일 때만 해당 태그의 전용 버튼을 활성화
        if (data.deathCount == 3)
        {
            var target = tagButtonList.Find(x => x.tag == selectedUnit.tag);
            if (target.killButton != null)
            {
                target.killButton.SetActive(true);
            }
        }
    }

    // --- 3. 특정 버튼을 눌렀을 때 호출 (태그 체크 포함) ---
    // 버튼의 OnClick 이벤트에 연결하세요. 파라미터로 본인의 태그를 넘길 수 있습니다.
    public void OnExecuteButtonPressed(string buttonTag)
    {
        GameObject selected = GameManager.Instance.selectedObject;
        if (selected == null) return;

        // 선택된 유닛의 태그와 버튼의 태그가 일치하는지 확인
        if (selected.tag != buttonTag) return;

        UnitData data = selected.GetComponent<UnitData>();
        if (data != null && data.deathCount == 3)
        {
            ExcludeUnit(selected);
        }
    }

    // --- 4. 유닛 전장 제외 로직 ---
    public void ExcludeUnit(GameObject unit)
    {
        UnitData data = unit.GetComponent<UnitData>();
        if (data == null || data.isDead) return;

        data.isDead = true;
        unit.GetComponent<Collider2D>().enabled = false;
        
        // 유닛 비활성화 및 보관소 이동
        unit.transform.SetParent(deathUnitArea);
        unit.SetActive(false); 

        if (GameManager.Instance.selectedObject == unit)
        {
            GameManager.Instance.CloseAllUI();
            UpdateKillButtonUI(null); // 버튼도 함께 정리
        }
    }
}