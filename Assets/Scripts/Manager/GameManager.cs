using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("타일맵 설정")]
    public Tilemap targetTilemap;

    [Header("현재 선택된 오브젝트")]
    public GameObject selectedObject;
    private GameObject currentActiveUI; 

    [System.Serializable]
    public struct LayerUI
    {
        public string layerName;
        public GameObject uiPanel;    // 레이어별 부모 UI 패널
        public bool hideAfterAction;  // 행동 후 UI를 닫을지 여부
    }

    [Header("레이어별 UI 설정")]
    public List<LayerUI> layerUIList;

    [Header("UI 버튼 참조 (패널 내 실제 버튼들)")]
    public GameObject forwardButton; 
    public GameObject backwardButton; 

    [Header("UI 위치 설정")]
    public Vector3 uiOffset = new Vector3(0, 1.2f, 0); 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CloseAllUI();
    }

    // --- 1. 유닛 선택 및 초기화 ---

    public void SelectNewObject(GameObject newObj)
    {
        // 이전 유닛 UI 정리
        if (currentActiveUI != null)
        {
            currentActiveUI.SetActive(false);
            currentActiveUI.transform.SetParent(null); 
        }

        selectedObject = newObj;
        AP_Counter_Manager.Instance.ShowTeamPanel(newObj.tag);

        // 해당 레이어의 UI 설정 찾기
        string layerName = LayerMask.LayerToName(newObj.layer);
        var target = layerUIList.Find(x => x.layerName == layerName);

        if (target.uiPanel != null)
        {
            currentActiveUI = target.uiPanel;
            
            // UI를 유닛 머리 위로 배치 및 활성화
            currentActiveUI.transform.SetParent(newObj.transform);
            currentActiveUI.transform.localPosition = uiOffset;
            currentActiveUI.transform.localRotation = Quaternion.identity; 
            currentActiveUI.SetActive(true); 

            // 이동 가능 여부 체크하여 버튼 활성화
            UpdateNavigationButtons();
        }
        KillManager.Instance.UpdateKillButtonUI(newObj);
    }

    // --- 2. 이동 및 장애물 판정 로직 ---

    public void UpdateNavigationButtons()
    {
        if (selectedObject == null || targetTilemap == null) return;

        // 1. 현재 칸과 방향 계산
        Vector3Int currentCell = targetTilemap.WorldToCell(selectedObject.transform.position);
        Vector3Int forwardDir = Vector3Int.RoundToInt(selectedObject.transform.up);

        // 2. 앞/뒤 칸 좌표 계산
        Vector3Int nextForwardCell = currentCell + forwardDir;
        Vector3Int nextBackwardCell = currentCell - forwardDir;

        // 3. 검사할 레이어 (Commander, Knight 등 유닛 레이어만!)
        int unitLayerMask = LayerMask.GetMask("Commander", "Knight"); 

        // 4. [수정] 장애물 체크 (나 자신은 제외하도록 설계됨)
        bool isForwardBlocked = CheckTargetCellBlocked(nextForwardCell, unitLayerMask);
        bool isBackwardBlocked = CheckTargetCellBlocked(nextBackwardCell, unitLayerMask);

        // 5. 버튼 출력 (막히지 않았을 때만 SetActive(true))
        if (forwardButton != null) 
        {
            forwardButton.SetActive(!isForwardBlocked);
            // 디버그용: 버튼이 왜 꺼졌는지 로그 확인
            if (isForwardBlocked) Debug.Log("전진 버튼 비활성화: 앞이 막혔거나 맵 밖임");
        }
        
        if (backwardButton != null) 
        {
            backwardButton.SetActive(!isBackwardBlocked);
        }
    }

    private bool CheckTargetCellBlocked(Vector3Int cellPos, int layerMask)
    {
        // [범위 체크] 5x5 기준 (중앙 0,0이면 -2~2 / 구석 0,0이면 0~4)
        // 현재 본인의 타일맵 설정에 맞게 숫자 범위를 꼭 확인하세요!
        if (cellPos.x < -2 || cellPos.x > 2 || cellPos.y < -2 || cellPos.y > 2) 
        {
            return true; // 맵 밖은 무조건 막힘 처리
        }

        Vector3 worldCheckPos = targetTilemap.GetCellCenterWorld(cellPos);
        
        // 해당 위치의 모든 Collider를 가져옴
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldCheckPos, 0.2f, layerMask);

        foreach (var hit in hits)
        {
            // [중요] 타일맵이 아니고 && 나 자신(selectedObject)이 아닐 때만 장애물로 인정
            if (hit.gameObject != selectedObject && hit.GetComponent<Tilemap>() == null)
            {
                // 다른 유닛의 Collider가 발견됨
                return true; 
            }
        }

        return false; // 아무것도 없거나 나 자신뿐이면 지나갈 수 있음
    }

    // --- 3. 유닛 제어 (UI 버튼 이벤트 연결용) ---

    public void ControlSelected(string action)
    {
        if (selectedObject == null || targetTilemap == null) return;

        int cost = (action == "Down") ? 2 : 1;
        if (!AP_Counter_Manager.Instance.ConsumeAP(selectedObject.tag, cost)) return;

        Vector3Int currentCell = targetTilemap.WorldToCell(selectedObject.transform.position);
        Vector3Int forwardDir = Vector3Int.RoundToInt(selectedObject.transform.up);

        // 1. 위치/회전 변경
        switch (action)
        {
            case "Up":
                selectedObject.transform.position = targetTilemap.GetCellCenterWorld(currentCell + forwardDir);
                break;
            case "Down":
                selectedObject.transform.position = targetTilemap.GetCellCenterWorld(currentCell - forwardDir);
                break;
            case "Left":
                selectedObject.transform.Rotate(0, 0, 90f);
                break;
            case "Right":
                selectedObject.transform.Rotate(0, 0, -90f);
                break;
        }

        // [핵심 해결법] 2. 물리 위치 강제 동기화 (이동 직후 판정 오류 방지)
        Physics2D.SyncTransforms();

        // 조작한 유닛 기준으로 전방에 적이 있는지 확인
        KillManager.Instance.CheckAndApplyDeathRules(selectedObject);

        // 3. UI 처리 실행
        CheckAndHideUI(selectedObject.layer);
    }

    private void CheckAndHideUI(int layer)
    {
        string layerName = LayerMask.LayerToName(layer);
        var target = layerUIList.Find(x => x.layerName == layerName);
        
        if (target.hideAfterAction)
        {
            CloseAllUI();
        }
        else
        {
            // [보정] UI 패널이 유닛 회전에 영향을 받지 않도록 각도 리셋
            if (currentActiveUI != null)
            {
                currentActiveUI.transform.localRotation = Quaternion.identity;
                currentActiveUI.SetActive(true); // 혹시 꺼졌을지 모르니 다시 활성화
            }

            // 새로운 위치에서 주변 장애물 재검사
            UpdateNavigationButtons();
        }
    }

    public void CloseAllUI() 
    { 
        foreach (var item in layerUIList) 
        {
            if (item.uiPanel != null) 
            {
                item.uiPanel.SetActive(false); 
                item.uiPanel.transform.SetParent(null); 
            }
        }
        selectedObject = null;
        currentActiveUI = null;
    }
}