using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("공통 유닛 UI 설정")]
    [Tooltip("모든 유닛이 공통으로 사용할 UI 패널 프리팹 또는 오브젝트")]
    public GameObject commonUnitUIPanel; 
    
    [Header("설정")]
    public Vector3 uiOffset = new Vector3(0, 1.2f, 0);

    private GameObject currentActiveUI;
    private UnitUIPanel currentPanelScript;
    private GameObject currentTargetUnit; 

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
        
        // 시작 시 공통 UI가 씬에 배치되어 있다면 비활성화
        if (commonUnitUIPanel != null) commonUnitUIPanel.SetActive(false);
        CloseAllUI();
    }

    public void ShowUnitUI(GameObject unit)
    {
        if (commonUnitUIPanel == null) return;

        CloseAllUI();
        currentTargetUnit = unit;
        currentActiveUI = commonUnitUIPanel;

        currentActiveUI.SetActive(true);
        currentActiveUI.transform.SetParent(unit.transform);
        
        // 1. 위치 설정
        currentActiveUI.transform.localPosition = uiOffset;

        // 2. [수정] 부모(유닛)의 회전값을 그대로 따르도록 함
        // World rotation을 0으로 만드는 대신, 부모에 대한 상대 회전을 0으로 만듭니다.
        currentActiveUI.transform.localRotation = Quaternion.identity;

        // 3. 스케일 보정
        Vector3 parentScale = unit.transform.localScale;
        currentActiveUI.transform.localScale = new Vector3(
            1f / Mathf.Abs(parentScale.x), 
            1f / Mathf.Abs(parentScale.y), 
            1f
        );

        currentPanelScript = currentActiveUI.GetComponent<UnitUIPanel>();
        RefreshNavigationButtons(unit);
    }

    public void CloseAllUI()
    {
        if (currentActiveUI != null)
        {
            currentActiveUI.SetActive(false);
            currentActiveUI.transform.SetParent(null); // 부모 관계 해제
            currentActiveUI.transform.localScale = Vector3.one; 
        }

        currentActiveUI = null;
        currentPanelScript = null;
        currentTargetUnit = null; 
    }

    public void RefreshNavigationButtons(GameObject unit)
    {
        if (unit == null || currentPanelScript == null) return;

        Vector3Int currentCell = GameManager.Instance.targetTilemap.WorldToCell(unit.transform.position);
        Vector3Int forwardDir = Vector3Int.RoundToInt(unit.transform.up);
        
        Vector3Int targetForwardCell = currentCell + forwardDir;
        Vector3Int targetBackwardCell = currentCell - forwardDir;

        int obstacleMask = LayerMask.GetMask("Commander", "Knight");

        bool hasForwardTile = GameManager.Instance.targetTilemap.HasTile(targetForwardCell);
        bool canForward = hasForwardTile && !GameManager.Instance.CheckTargetCellBlocked(targetForwardCell, obstacleMask);

        bool hasBackwardTile = GameManager.Instance.targetTilemap.HasTile(targetBackwardCell);
        bool canBackward = hasBackwardTile && !GameManager.Instance.CheckTargetCellBlocked(targetBackwardCell, obstacleMask);

        // 공통 UI의 버튼 활성/비활성 설정
        currentPanelScript.SetNavigationButtons(canForward, canBackward, true, true);
    }

    public void HandlePostActionUI(GameObject unit)
    {
        if (unit == null) return;
        UnitData data = unit.GetComponent<UnitData>();

        // 공통 UI를 사용하므로 특정 팀/레이어 구분 없이 UnitData의 상태에 따라 처리
        // 만약 모든 유닛이 한 번 움직이고 끝난다면 아래 로직 유지
        if (data != null)
        {
            // 이동 후 행동이 끝난 유닛이라면 UI 닫기
            // (이 로직은 게임 규칙에 따라 수정 가능합니다)
            data.SetMovementState(true); 
            CloseAllUI();
        }
    }

    // --- 버튼 이벤트 함수들 ---

    public void OnMoveButtonClick()
    {
        if (currentTargetUnit == null) return;
        
        if (AP_Counter_Manager.Instance.ConsumeAP(currentTargetUnit.tag, 1))
        {
            GameManager.Instance.selectedObject = currentTargetUnit; 
            GameManager.Instance.MoveUnitForward();
        }
    }

    public void OnBackwardButtonClick()
    {
        if (currentTargetUnit == null) return;
        
        if (AP_Counter_Manager.Instance.ConsumeAP(currentTargetUnit.tag, 2))
        {
            GameManager.Instance.selectedObject = currentTargetUnit;
            GameManager.Instance.MoveUnitBackward();
        }
    }
    public void OnLeftRotateClick()
    {
        if (currentTargetUnit == null) return;
        // 회전 비용을 1 AP라고 가정
        if (AP_Counter_Manager.Instance.ConsumeAP(currentTargetUnit.tag, 1))
        {
            GameManager.Instance.RotateUnit(90f); // 왼쪽 90도
        }
    }

    public void OnRightRotateClick()
    {
        if (currentTargetUnit == null) return;
        if (AP_Counter_Manager.Instance.ConsumeAP(currentTargetUnit.tag, 1))
        {
            GameManager.Instance.RotateUnit(-90f); // 오른쪽 90도
        }
    }
}