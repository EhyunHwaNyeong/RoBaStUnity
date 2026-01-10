using UnityEngine;

public class Unit_UI : MonoBehaviour
{
    public GameObject unitCanvas; // 미리 만들어둔 World Space Canvas

    void Start()
    {
        // 처음에는 UI를 꺼둡니다.
        if (unitCanvas != null) unitCanvas.SetActive(false);
    }

    // 유닛(Collider2D 필요)을 클릭했을 때 호출
    private void OnMouseDown()
    {
        ToggleUI();
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
