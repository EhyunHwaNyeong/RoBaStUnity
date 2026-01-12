using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 어디서든 접근 가능하게 설정
    private GameObject currentActiveUI; // 현재 화면에 떠 있는 UI 보관용

    [Header("현재 선택된 오브젝트")]
    public GameObject selectedObject;
    
    [System.Serializable]
    public struct LayerUI
    {
        public string layerName;
        public GameObject uiPanel;
        public bool hideAfterAction; // 조작 후 UI를 숨길지 여부
    }

    [Header("레이어별 UI 설정")]
    public List<LayerUI> layerUIList;

    [Header("유닛 이동 거리 설정")]
    public float moveAmount= 1.0f; // 유닛이 한 번에 이동하는 거리

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 유닛이 클릭될 때 호출되는 함수
    public void SelectNewObject(GameObject newObj, GameObject newUI)
    {
        // 1. 이전에 켜져 있던 UI가 있다면 끈다
        if (currentActiveUI != null)
        {
            currentActiveUI.SetActive(false);
        }

        // 2. 새로운 오브젝트와 UI 등록
        selectedObject = newObj;
        currentActiveUI = newUI;

        // 3. 새 UI 켜기
        if (currentActiveUI != null)
        {
            currentActiveUI.SetActive(true);
        }
    }

    // UI 버튼에서 호출할 함수 (MoveSelected 수정)
    public void ControlSelected(string action)
    {
        if (selectedObject == null) return;

        switch (action)
        {
            case "Up":
                selectedObject.transform.position += selectedObject.transform.up * moveAmount;
                break;
            case "Down":
                selectedObject.transform.position -= selectedObject.transform.up * moveAmount;
                break;
            case "Left":
                // 90도 좌회전 (Z축 기준 +90)
                selectedObject.transform.Rotate(0, 0, 90f);
                break;
            case "Right":
                // 90도 우회전 (Z축 기준 -90)
                selectedObject.transform.Rotate(0, 0, -90f);
                break;
        }

        Debug.Log($"{selectedObject.name} 조작 완료: {action}");

        // 조작 후 해당 오브젝트의 레이어에 맞춰 UI 끄기
        CheckAndHideUI(selectedObject.layer);
    }
    
    void CloseAllUI()
    {
        foreach (var item in layerUIList)
        {
            if (item.uiPanel != null) item.uiPanel.SetActive(false);
        }
    }
    void ShowUIForLayer(int layer)
    {
        string layerName = LayerMask.LayerToName(layer);
        var target = layerUIList.Find(x => x.layerName == layerName);
        if (target.uiPanel != null) target.uiPanel.SetActive(true);
    }

    void CheckAndHideUI(int layer)
    {
        string layerName = LayerMask.LayerToName(layer);
        var target = layerUIList.Find(x => x.layerName == layerName);
        
        // Hide After Action이 체크되어 있을 때만 UI를 끎
        if (target.hideAfterAction && target.uiPanel != null)
        {
            target.uiPanel.SetActive(false);
            selectedObject = null; // 선택 해제
        }
    }
}