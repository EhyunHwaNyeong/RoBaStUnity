using UnityEngine;
using UnityEngine.EventSystems; // 필수

public class Unit_UI : MonoBehaviour, IPointerClickHandler
{
    public GameObject unitCanvas; // 미리 만들어둔 World Space Canvas

    void Start()
    {
        // 처음에는 UI를 꺼둡니다.
        if (unitCanvas != null) unitCanvas.SetActive(false);
    }

    // 유닛(Collider2D 필요)을 클릭했을 때 호출
    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleUI();
        Debug.Log($"{gameObject.name} 클릭 시도!");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.selectedObject = this.gameObject;
            Debug.Log("GameManager에 오브젝트 등록 성공!");
        }
        else
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다! 하이러키를 확인하세요.");
        }
    }

    public void ToggleUI()
    {
        bool isActive = unitCanvas.activeSelf;
        unitCanvas.SetActive(!isActive);
        
        if (!isActive) {
            // UI가 켜질 때 애니메이션이나 연출을 추가할 수 있습니다.
            Debug.Log("UI 표시됨");
        }
    }
}
