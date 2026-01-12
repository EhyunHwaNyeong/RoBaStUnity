using UnityEngine;

public class SelectUI : MonoBehaviour
{
    private void OnMouseEnter()
    {
        Debug.Log($"{gameObject.name} 위에 마우스가 있음!");
    }

    // 2. 실제 클릭 로직
    private void OnMouseDown()
    {
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
}
