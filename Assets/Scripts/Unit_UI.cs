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
        if (GameManager.Instance != null)
        {
            // [핵심] GameManager에게 나를 선택하라고 명령합니다.
            GameManager.Instance.SelectNewObject(this.gameObject, unitCanvas);
            Debug.Log($"{gameObject.name} 선택 및 GameManager 등록 요청");
        }
        else
        {
            Debug.LogError("GameManager를 찾을 수 없습니다!");
        }
    }
}
