using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [System.Serializable]
    public struct LayerUI
    {
        public string teamTag;       
        public string layerName;     
        public GameObject uiPanel;    
        public bool hideAfterAction;  
    }

    [Header("레이어 및 팀별 UI 설정")]
    public List<LayerUI> layerUIList;

    [Header("설정")]
    public Vector3 uiOffset = new Vector3(0, 1.2f, 0);

    private GameObject currentActiveUI;
    private UnitUIPanel currentPanelScript;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        CloseAllUI();
    }

    public void ShowUnitUI(GameObject unit)
    {
        CloseAllUI();

        string layerName = LayerMask.LayerToName(unit.layer);
        var target = layerUIList.Find(x => x.layerName == layerName && x.teamTag == unit.tag);

        if (target.uiPanel != null)
        {
            currentActiveUI = target.uiPanel;
            currentActiveUI.transform.SetParent(unit.transform);
            currentActiveUI.transform.localPosition = uiOffset;
            currentActiveUI.transform.localRotation = Quaternion.identity;
            currentActiveUI.SetActive(true);

            currentPanelScript = currentActiveUI.GetComponent<UnitUIPanel>();
            
            RefreshNavigationButtons(unit);
        }
    }

    public void RefreshNavigationButtons(GameObject unit)
    {
        if (unit == null || currentPanelScript == null) return;

        Vector3Int currentCell = GameManager.Instance.targetTilemap.WorldToCell(unit.transform.position);
        Vector3Int forwardDir = Vector3Int.RoundToInt(unit.transform.up);
        int unitLayerMask = LayerMask.GetMask("Commander", "Knight");

        // GameManager의 CheckTargetCellBlocked 사용
        bool canForward = !GameManager.Instance.CheckTargetCellBlocked(currentCell + forwardDir, unitLayerMask);
        bool canBackward = !GameManager.Instance.CheckTargetCellBlocked(currentCell - forwardDir, unitLayerMask);

        currentPanelScript.SetButtons(canForward, canBackward);
    }

    public void CloseAllUI()
    {
        if (currentActiveUI != null)
        {
            currentActiveUI.SetActive(false);
            currentActiveUI.transform.SetParent(null);
        }
        currentActiveUI = null;
        currentPanelScript = null;
    }

    // --- 버튼 이벤트 ---

    // 1. 이동 버튼 (전진)
    public void OnMoveButtonClick()
    {
        GameObject selected = GameManager.Instance.selectedObject;
        if (selected == null) return;

        // [AP 소모] 이동 비용 1
        if (AP_Counter_Manager.Instance.ConsumeAP(selected.tag, 1))
        {
            // [해결] 이제 GameManager에 MoveUnitForward가 존재하므로 오류가 사라집니다.
            GameManager.Instance.MoveUnitForward(); 
            
            // 행동 후 UI 갱신 여부 처리 (GameManager가 코루틴 종료 후 호출하기도 하지만, 즉시 반응을 위해)
            // 주의: 실제 물리 이동은 코루틴에서 일어나므로 UI 갱신은 코루틴 끝난 후가 정확할 수 있습니다.
            // 여기서는 CloseAllUI 조건 체크를 위해 남겨둡니다.
            HandlePostActionUI(selected);
        }
    }

    // 2. 후퇴 버튼 (예시 추가)
    public void OnBackwardButtonClick()
    {
        GameObject selected = GameManager.Instance.selectedObject;
        if (selected == null) return;

        // [AP 소모] 후퇴 비용 2 (예시)
        if (AP_Counter_Manager.Instance.ConsumeAP(selected.tag, 2))
        {
            GameManager.Instance.MoveUnitBackward(); // GameManager에 추가된 함수
            HandlePostActionUI(selected);
        }
    }

    public void HandlePostActionUI(GameObject unit)
    {
        if (unit == null) return;
        string layerName = LayerMask.LayerToName(unit.layer);
        var target = layerUIList.Find(x => x.layerName == layerName && x.teamTag == unit.tag);

        if (target.hideAfterAction)
        {
            CloseAllUI();
        }
        else
        {
            // 이동 후 버튼 상태 갱신은 유닛이 실제로 이동을 마친 뒤(GameManager 코루틴 끝)에 하는 것이 가장 정확합니다.
            // GameManager에서 HandlePostActionUI를 호출해주므로 여기서는 비워두거나 안전장치로 둡니다.
        }
    }
}