using UnityEngine;
using UnityEngine.EventSystems;

public class Unit_UI : MonoBehaviour, IPointerClickHandler
{
    // 이제 개별 Canvas 변수는 필요 없습니다. (메모리 절약)

    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance != null)
        {
            // GameManager에게 "이 오브젝트(나)"가 선택되었음을 알립니다.
            // GameManager가 내 레이어를 보고 어떤 UI를 띄울지 결정합니다.
            GameManager.Instance.SelectNewObject(this.gameObject);
            Debug.Log($"{gameObject.name} (레이어: {LayerMask.LayerToName(gameObject.layer)}) 선택됨");
        }
        else
        {
            Debug.LogError("GameManager를 찾을 수 없습니다!");
        }
    }
}