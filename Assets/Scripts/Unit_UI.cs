using UnityEngine;
using UnityEngine.EventSystems;

public class Unit_UI : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭된 대상이 UI인지 유닛인지 확인
        if (eventData.pointerCurrentRaycast.gameObject != this.gameObject)
        {
            Debug.Log($"클릭이 차단됨! 현재 클릭된 대상: {eventData.pointerCurrentRaycast.gameObject.name}");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectNewObject(this.gameObject);
            Debug.Log($"{gameObject.name} 선택됨");
        }
    }
}