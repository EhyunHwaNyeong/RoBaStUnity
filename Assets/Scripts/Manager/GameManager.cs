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

        // 이동 전 유닛 태그 저장 (턴 체크용)
        string unitTag = selectedObject.tag;

        Vector3Int currentCell = targetTilemap.WorldToCell(selectedObject.transform.position);
        Vector3Int forwardDir = Vector3Int.RoundToInt(selectedObject.transform.up);

        switch (action)
        {
            case "Up": selectedObject.transform.position = targetTilemap.GetCellCenterWorld(currentCell + forwardDir); break;
            case "Down": selectedObject.transform.position = targetTilemap.GetCellCenterWorld(currentCell - forwardDir); break;
            case "Left": selectedObject.transform.Rotate(0, 0, 90f); break;
            case "Right": selectedObject.transform.Rotate(0, 0, -90f); break;
        }

        // 2. 물리 동기화 및 대기
        Physics2D.SyncTransforms();
        yield return new WaitForFixedUpdate();

        // 3. 사망 규칙 체크
        if (KillManager.Instance != null)
        {
            KillManager.Instance.CheckAndApplyDeathRules(selectedObject);
        }

        // 4. 연출 종료 후 처리
        // 유닛이 파괴되었을 수도 있으므로 null 체크
        if (selectedObject != null && selectedObject.activeInHierarchy)
        {
            // AP가 없으면 턴 넘김
            if (CheckIfAPEmpty(unitTag))
            {
                 Debug.Log($"{unitTag}팀 AP 소진. 턴 종료.");
                 TurnManager.Instance.SwitchTurn();
            }
            else 
            {
                // AP가 남았으면 UI 갱신 (이동 후 위치 기반 버튼 활성/비활성)
                UIManager.Instance.HandlePostActionUI(selectedObject);
            }
        }
        else
        {
            // 유닛이 죽어서 사라진 경우에도 AP 체크하여 턴 넘김
            if (CheckIfAPEmpty(unitTag))
            {
                TurnManager.Instance.SwitchTurn();
            }
        }
    }

    // AP Empty 체크 헬퍼 함수
    private bool CheckIfAPEmpty(string tag)
    {
        if (AP_Counter_Manager.Instance != null)
        {
            return AP_Counter_Manager.Instance.IsAPEmpty(tag);
        }
        return false;
    }

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