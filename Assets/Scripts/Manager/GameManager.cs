using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Tilemap targetTilemap;
    public GameObject selectedObject;
    public float moveSpeed = 7f;
    private bool isMoving = false;
    private bool isRotating = false; // 회전 중 중복 실행 방지

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 1. 마우스 클릭 입력 확인
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 2. UI 위를 클릭 중이라면 게임 세상의 클릭은 무시
            if (EventSystem.current.IsPointerOverGameObject()) return;

            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        if (isMoving) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        // 레이어 마스크를 지정하여 유닛만 검출하도록 설정 (Unit 레이어가 있다고 가정)
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            /// 1. 유닛(Black/White)을 클릭한 경우
            if (hit.collider.CompareTag("White") || hit.collider.CompareTag("Black"))
            {
                if (selectedObject == hit.collider.gameObject) return;
                SelectNewObject(hit.collider.gameObject);
                return;
            }
        }

        // 유닛이 아닌 곳을 클릭했을 때만 해제
        DeselectObject();
    }

    public void SelectNewObject(GameObject newObj)
    {
        // [수정] 현재 턴인 팀만 선택 가능하도록 로직 강화
        if (!TurnManager.Instance.IsMyTurn(newObj.tag)) 
        {
            Debug.Log($"<color=red>조작 실패:</color> 현재는 {TurnManager.Instance.currentTurnTag}의 턴입니다. (클릭한 유닛: {newObj.tag})");
            return;
        }

        UnitData unitData = newObj.GetComponent<UnitData>();
        if (unitData != null && unitData.hasMovedThisTurn) 
        {
            Debug.Log("이 유닛은 이미 이번 턴에 행동을 마쳤습니다.");
            return;
        }

        // 기존 선택된 유닛이 있다면 시각 효과 제거
        if (selectedObject != null) 
        {
            selectedObject.GetComponent<UnitData>()?.SetSelectedVisual(false);
        }

        // 새로운 유닛 정보 할당
        selectedObject = newObj;
        selectedObject.GetComponent<UnitData>()?.SetSelectedVisual(true);
        
        Debug.Log($"<color=green>{newObj.name} 선택됨</color> (팀: {newObj.tag})");

        // UI 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowUnitUI(newObj);
        }
    }

    public void DeselectObject()
    {
        if (selectedObject != null)
        {
            selectedObject.GetComponent<UnitData>()?.SetSelectedVisual(false);
            selectedObject = null;
        }
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseAllUI();
        }
    }

    // --- 이하 이동 로직 (기존과 동일하되 안전장치 추가) ---
    public void MoveUnitForward() { if(!isMoving) StartCoroutine(ProcessMoveSequence("Up")); }
    public void MoveUnitBackward() { if(!isMoving) StartCoroutine(ProcessMoveSequence("Down")); }
    // 90도 회전 로직
    public void RotateUnit(float angle)
    {
        // 이동 중이거나 이미 회전 중이면 무시
        if (selectedObject == null || isMoving || isRotating) return;
        StartCoroutine(ProcessRotateSequence(angle));
    }

    private IEnumerator ProcessMoveSequence(string action)
    {
        if (selectedObject == null) yield break;
        
        isMoving = true;
        GameObject unitRef = selectedObject; 
        string unitTag = unitRef.tag; // 턴 체크를 위해 태그 저장 
        
        Vector3Int currentCell = targetTilemap.WorldToCell(unitRef.transform.position);
        Vector3Int forwardDir = Vector3Int.RoundToInt(unitRef.transform.up);
        Vector3Int targetCell = (action == "Up") ? currentCell + forwardDir : currentCell - forwardDir;

        if (CheckTargetCellBlocked(targetCell, LayerMask.GetMask("Commander", "Knight")))
        {
            isMoving = false;
            yield break;
        }

        Vector3 startPos = unitRef.transform.position;
        Vector3 endPos = targetTilemap.GetCellCenterWorld(targetCell);
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            if(unitRef == null) break;
            unitRef.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        
        if(unitRef != null) unitRef.transform.position = endPos;

        Physics2D.SyncTransforms();
        KillManager.Instance.CheckAndApplyDeathRules();
        // yield return new WaitForFixedUpdate();
        
        isMoving = false;

        if (UIManager.Instance != null && unitRef != null)
            UIManager.Instance.HandlePostActionUI(unitRef);

        if (AP_Counter_Manager.Instance != null && AP_Counter_Manager.Instance.IsAPEmpty(unitTag))
        {
            // 이미 TurnManager가 전환 중이 아닐 때만 전환 명령을 내림
            if (!TurnManager.Instance.isSwitchingTurn)
            {
                Debug.Log($"<color=yellow>{unitTag} 팀 AP 소진으로 인한 자동 턴 전환.</color>");
                TurnManager.Instance.SwitchTurn();
            }
        }
    }
    private IEnumerator ProcessRotateSequence(float angle)
    {
        isRotating = true;
        GameObject unitRef = selectedObject;
        string unitTag = unitRef.tag; // 턴 체크를 위해 태그 저장
        
        Quaternion startRotation = unitRef.transform.rotation;
        Quaternion endRotation = unitRef.transform.rotation * Quaternion.Euler(0, 0, angle);
        
        float t = 0;
        float rotationSpeed = 5f; 

        while (t < 1f)
        {
            t += Time.deltaTime * rotationSpeed;
            if (unitRef == null) break;
            
            unitRef.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        if (unitRef != null)
        {
            unitRef.transform.rotation = endRotation;
            
            // 회전 후 이동 버튼 상태 갱신
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HandlePostActionUI(unitRef);
                KillManager.Instance.CheckAndApplyDeathRules();
            }
                
            // [중요] 회전 애니메이션이 끝난 후 AP가 0인지 확인하여 턴을 넘깁니다.
            if (AP_Counter_Manager.Instance != null && AP_Counter_Manager.Instance.IsAPEmpty(unitTag))
            {
                // 이미 TurnManager가 전환 중이 아닐 때만 전환 명령을 내림
                if (!TurnManager.Instance.isSwitchingTurn)
                {
                    Debug.Log($"<color=yellow>{unitTag} 팀 AP 소진으로 인한 자동 턴 전환.</color>");
                    TurnManager.Instance.SwitchTurn();
                }
            }
        }

        isRotating = false;
    }
    public bool CheckTargetCellBlocked(Vector3Int cellPos, int layerMask)
    {
        if (!targetTilemap.HasTile(cellPos)) return true;
        Vector3 checkPos = targetTilemap.GetCellCenterWorld(cellPos);
        Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.1f, layerMask);
        return (hit != null && hit.gameObject != selectedObject);
    }
}