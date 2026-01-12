using UnityEngine;

public class SelectableItem : MonoBehaviour
{
    private void OnMouseDown()
    {
        // 클릭 시 Game Manager에 자신을 등록
        GameManager.Instance.selectedObject = this.gameObject;
        Debug.Log($"{gameObject.name}이(가) 선택되었습니다.");
        
        // 여기서 UI를 활성화하는 로직을 추가할 수도 있습니다.
    }
}
