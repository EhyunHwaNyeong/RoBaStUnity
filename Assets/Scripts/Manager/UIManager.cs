using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [System.Serializable]
    public struct LayerUI
    {
        public string teamTag;       // "White" 또는 "Black"
        public string layerName;     // "Commander" 또는 "Knight"
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

    // --- 유닛 선택 시 UI 출력 ---
    public void ShowUnitUI(GameObject unit)
    {
        CloseAllUI(); // 기존 UI 정리

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
            
            // 버튼 상태 갱신
            RefreshNavigationButtons(unit);
        }
    }

    // --- 이동 가능 여부 체크 및 버튼 출력 결정 ---
    public void RefreshNavigationButtons(GameObject unit)
    {
        if (unit == null || currentPanelScript == null) return;

        // 1. 이동 버튼 갱신 (기존 로직)
        Vector3Int currentCell = GameManager.Instance.targetTilemap.WorldToCell(unit.transform.position);
        Vector3Int forwardDir = Vector3Int.RoundToInt(unit.transform.up);
        int unitLayerMask = LayerMask.GetMask("Commander", "Knight");

        bool canForward = !GameManager.Instance.CheckTargetCellBlocked(currentCell + forwardDir, unitLayerMask);
        bool canBackward = !GameManager.Instance.CheckTargetCellBlocked(currentCell - forwardDir, unitLayerMask);

        currentPanelScript.SetButtons(canForward, canBackward);
    }

    // --- UI 유지 여부 확인 (행동 후 호출) ---
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
            if (currentActiveUI != null)
            {
                currentActiveUI.transform.localRotation = Quaternion.identity;
                currentActiveUI.SetActive(true);
                RefreshNavigationButtons(unit);
            }
        }
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
}