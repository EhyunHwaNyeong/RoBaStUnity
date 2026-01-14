using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private GameObject currentActiveUI; 

    [Header("현재 선택된 오브젝트")]
    public GameObject selectedObject;

    [System.Serializable]
    public struct LayerUI
    {
        public string layerName;
        public GameObject uiPanel; 
        public bool hideAfterAction; // Commander는 false, Knight는 true로 설정 권장
    }

    [Header("레이어별 UI 설정")]
    public List<LayerUI> layerUIList;

    [Header("UI 버튼 참조")]
    public GameObject forwardButton; 
    public GameObject backwardButton; 

    [Header("설정")]
    public float moveAmount = 1.0f;
    public Vector3 uiOffset = new Vector3(0, 1.2f, 0); 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CloseAllUI();
    }

    // 유닛 클릭 시 호출
    public void SelectNewObject(GameObject newObj)
    {
        // 1. 이전 UI 초기화
        if (currentActiveUI != null)
        {
            currentActiveUI.SetActive(false);
            currentActiveUI.transform.SetParent(null); 
        }

        selectedObject = newObj;

        // 2. 레이어에 맞는 UI 패널 찾기
        string layerName = LayerMask.LayerToName(newObj.layer);
        var target = layerUIList.Find(x => x.layerName == layerName);

        if (target.uiPanel != null)
        {
            currentActiveUI = target.uiPanel;
            
            // 3. UI를 유닛의 자식으로 설정 (유닛과 함께 이동/회전)
            currentActiveUI.transform.SetParent(newObj.transform);
            currentActiveUI.transform.localPosition = uiOffset;
            currentActiveUI.transform.localRotation = Quaternion.identity; 
            
            currentActiveUI.SetActive(true);
        }
        
        // 4. 이동 가능 버튼 업데이트
        UpdateNavigationButtons();
    }

    // 전진/후진 버튼 활성화 여부 결정
    public void UpdateNavigationButtons()
    {
        if (selectedObject == null) return;

        // 예상 이동 위치 계산
        Vector3 forwardPos = selectedObject.transform.position + (selectedObject.transform.up * moveAmount);
        Vector3 backwardPos = selectedObject.transform.position - (selectedObject.transform.up * moveAmount);

        int unitLayerMask = LayerMask.GetMask("Commander", "Knight"); 

        // [핵심] 자기 자신을 제외하고 장애물이 있는지 체크
        bool isForwardBlocked = CheckTargetPos(forwardPos, unitLayerMask);
        bool isBackwardBlocked = CheckTargetPos(backwardPos, unitLayerMask);

        if (forwardButton != null) forwardButton.SetActive(!isForwardBlocked);
        if (backwardButton != null) backwardButton.SetActive(!isBackwardBlocked);
    }

    // 특정 좌표에 '나를 제외한' 유닛이 있는지 체크
    private bool CheckTargetPos(Vector3 targetPos, int layerMask)
    {
        // 지점이 아닌 작은 원(0.1f) 영역을 검사하여 판정 정확도를 높임
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(targetPos, 0.1f, layerMask);

        foreach (var hit in hitColliders)
        {
            // 감지된 것이 있고, 그것이 현재 선택된 유닛이 아니라면 장애물임
            if (hit != null && hit.gameObject != selectedObject)
            {
                return true; 
            }
        }
        return false;
    }

    // UI 버튼에서 호출 (Up, Down, Left, Right)
    public void ControlSelected(string action)
    {
        if (selectedObject == null) return;

        // 이동 및 회전 로직
        switch (action)
        {
            case "Up": selectedObject.transform.position += selectedObject.transform.up * moveAmount; break;
            case "Down": selectedObject.transform.position -= selectedObject.transform.up * moveAmount; break;
            case "Left": selectedObject.transform.Rotate(0, 0, 90f); break;
            case "Right": selectedObject.transform.Rotate(0, 0, -90f); break;
        }

        // 이동 후 새로운 위치를 기준으로 버튼 상태 즉시 갱신
        UpdateNavigationButtons();

        // 레이어 설정(hideAfterAction)에 따라 UI를 닫을지 결정
        CheckAndHideUI(selectedObject.layer);
    }

    public void CloseAllUI() 
    { 
        foreach (var item in layerUIList) 
            if (item.uiPanel != null) item.uiPanel.SetActive(false); 
    }

    void CheckAndHideUI(int layer)
    {
        string layerName = LayerMask.LayerToName(layer);
        var target = layerUIList.Find(x => x.layerName == layerName);
        
        // hideAfterAction이 true인 경우(Knight 등)만 UI를 끄고 선택 해제
        if (target.hideAfterAction && target.uiPanel != null)
        {
            target.uiPanel.SetActive(false);
            target.uiPanel.transform.SetParent(null); // 부모 관계 해제
            selectedObject = null;
            currentActiveUI = null;
        }
    }
}