using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("타일맵 설정")]
    public Tilemap targetTilemap;

    [Header("현재 선택된 오브젝트")]
    public GameObject selectedObject;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SelectNewObject(GameObject newObj)
    {
        selectedObject = newObj;

        // AP UI 갱신 (기존 매니저)
        AP_Counter_Manager.Instance.ShowTeamPanel(newObj.tag);

        // [UIManager 호출] 유닛 UI 출력 및 버튼 갱신
        UIManager.Instance.ShowUnitUI(newObj);
    }

    public void ControlSelected(string action)
    {
        if (selectedObject == null || targetTilemap == null) return;

        int cost = (action == "Down") ? 2 : 1;
        if (!AP_Counter_Manager.Instance.ConsumeAP(selectedObject.tag, cost)) return;

        StartCoroutine(ProcessMoveSequence(action));
    }

    private IEnumerator ProcessMoveSequence(string action)
    {
        Vector3Int currentCell = targetTilemap.WorldToCell(selectedObject.transform.position);
        Vector3Int forwardDir = Vector3Int.RoundToInt(selectedObject.transform.up);

        switch (action)
        {
            case "Up": selectedObject.transform.position = targetTilemap.GetCellCenterWorld(currentCell + forwardDir); break;
            case "Down": selectedObject.transform.position = targetTilemap.GetCellCenterWorld(currentCell - forwardDir); break;
            case "Left": selectedObject.transform.Rotate(0, 0, 90f); break;
            case "Right": selectedObject.transform.Rotate(0, 0, -90f); break;
        }

        Physics2D.SyncTransforms();
        yield return new WaitForFixedUpdate();

        KillManager.Instance.CheckAndApplyDeathRules(selectedObject);

        if (selectedObject != null && selectedObject.activeInHierarchy)
        {
            // [UIManager 호출] 이동 후 UI 상태 업데이트 (유지 혹은 닫기)
            UIManager.Instance.HandlePostActionUI(selectedObject);
        }
    }

    // UIManager에서 호출할 수 있도록 public으로 유지
    public bool CheckTargetCellBlocked(Vector3Int cellPos, int layerMask)
    {
        // [수정된 범위 체크] x: -4~2, y: -4~2 범위 밖이면 차단
        if (cellPos.x < -4 || cellPos.x > 2 || cellPos.y < -4 || cellPos.y > 2) return true;

        Vector3 worldCheckPos = targetTilemap.GetCellCenterWorld(cellPos);
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldCheckPos, 0.2f, layerMask);

        foreach (var hit in hits)
        {
            if (hit.gameObject != selectedObject && hit.GetComponent<Tilemap>() == null)
                return true;
        }
        return false;
    }
}