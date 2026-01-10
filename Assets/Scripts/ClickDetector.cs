using UnityEngine;
using UnityEngine.EventSystems; // 필수

public class ClickDetector : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("이벤트 시스템을 통한 클릭 감지!");
    }
}
