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
        if (!TurnManager.Instance.IsMyTurn(newObj.tag))
        {
            Debug.Log($"지금은 {TurnManager.Instance.currentTurnTag}의 턴입니다!");
            return; 
        }
        selectedObject = newObj;

        // AP UI 및 유닛 UI 갱신
        if (AP_Counter_Manager.Instance != null)
            AP_Counter_Manager.Instance.ShowTeamPanel(newObj.tag);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowUnitUI(newObj);
    }

    // --- [수정] UI 버튼에서 호출하는 함수들 (AP 소모 체크 없음) ---
    // UIManager에서 이미 AP를 소모하고 들어오기 때문에 바로 이동 로직(Coroutine)을 실행합니다.
    public void MoveUnitForward()
    {
        StartCoroutine(ProcessMoveSequence("Up"));
    }

    public void MoveUnitBackward()
    {
        StartCoroutine(ProcessMoveSequence("Down"));
    }

    // --- 키보드나 다른 조작을 위한 함수 (AP 소모 체크 포함) ---
    public void ControlSelected(string action)
    {
        if (selectedObject == null || targetTilemap == null) return;

        int cost = (action == "Down") ? 2 : 1;
        
        // 여기서 AP를 소모 시도
        if (AP_Counter_Manager.Instance.ConsumeAP(selectedObject.tag, cost))
        {
            StartCoroutine(ProcessMoveSequence(action));
        }
    }

    private IEnumerator ProcessMoveSequence(string action)
    {
        if (selectedObject == null) yield break;

        // 1. [핵심] 유닛이 사라지기 전에 미리 필요한 정보를 변수에 저장해둡니다.
        string unitTag = selectedObject.tag; 
        GameObject unitRef = selectedObject; // 참조 저장

        Vector3Int currentCell = targetTilemap.WorldToCell(unitRef.transform.position);
        Vector3Int forwardDir = Vector3Int.RoundToInt(unitRef.transform.up);

        // 이동 처리
        switch (action)
        {
            case "Up": unitRef.transform.position = targetTilemap.GetCellCenterWorld(currentCell + forwardDir); break;
            case "Down": unitRef.transform.position = targetTilemap.GetCellCenterWorld(currentCell - forwardDir); break;
            case "Left": unitRef.transform.Rotate(0, 0, 90f); break;
            case "Right": unitRef.transform.Rotate(0, 0, -90f); break;
        }

        Physics2D.SyncTransforms();
        yield return new WaitForFixedUpdate();

        // 2. 사망 규칙 체크 (여기서 unitRef가 Destroy될 수 있음)
        if (KillManager.Instance != null && unitRef != null)
        {
            KillManager.Instance.CheckAndApplyDeathRules(unitRef);
        }

        // 3. [수정] 유닛 존재 여부를 먼저 꼼꼼히 체크
        bool isUnitAlive = (unitRef != null && unitRef.activeInHierarchy);
        bool isAPEmpty = AP_Counter_Manager.Instance.IsAPEmpty(unitTag);

        if (isUnitAlive)
        {
            if (isAPEmpty)
            {
                Debug.Log($"{unitTag}팀 AP 소진. 턴을 넘깁니다.");
                TurnManager.Instance.SwitchTurn();
            }
            else 
            {
                // 살아있고 AP가 남았다면 UI 다시 표시
                UIManager.Instance.HandlePostActionUI(unitRef);
            }
        }
        else
        {
            // 유닛이 죽어서 사라진 경우
            Debug.Log($"{unitTag}팀의 유닛이 파괴됨.");
            // 유닛이 죽었더라도 AP가 0이면 턴을 넘겨야 시스템이 멈추지 않습니다.
            if (isAPEmpty)
            {
                TurnManager.Instance.SwitchTurn();
            }
        }
    }

    // AP Empty 체크 헬퍼 함수
    // private bool CheckIfAPEmpty(string tag)
    // {
    //     if (AP_Counter_Manager.Instance != null)
    //     {
    //         return AP_Counter_Manager.Instance.IsAPEmpty(tag);
    //     }
    //     return false;
    // }

    public bool CheckTargetCellBlocked(Vector3Int cellPos, int layerMask)
    {
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