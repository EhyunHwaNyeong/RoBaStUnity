using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 어디서든 접근 가능하게 설정

    [Header("현재 선택된 오브젝트")]
    public GameObject selectedObject; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // UI 버튼들이 호출할 함수
    public void MoveSelected(string direction)
    {
        if (selectedObject == null) return;

        float moveAmount = 1.0f;
        Vector3 nextPos = selectedObject.transform.position;

        switch (direction)
        {
            case "Up": nextPos += Vector3.up * moveAmount; break;
            case "Down": nextPos += Vector3.down * moveAmount; break;
            case "Left": nextPos += Vector3.left * moveAmount; break;
            case "Right": nextPos += Vector3.right * moveAmount; break;
        }

        selectedObject.transform.position = nextPos;
        Debug.Log($"{selectedObject.name}을(를) {direction}으로 이동시켰습니다.");
    }
}