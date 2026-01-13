using UnityEngine;

public class ButtonAction : MonoBehaviour
{
    [Header("설정")]
    public string[] targetTags; // 이 버튼이 소모할 태그들 (인스펙터에서 리스트로 관리)
    public int moveCost = 1; // 변수 선언
    // 버튼 클릭 시 호출할 함수
    public void OnButtonClick(int moveCost)
    {
        // 수정된 ResourceManager의 TryConsume 호출
        if (ResourceManager.Instance.TryConsume(moveCost, targetTags))
        {
            Debug.Log($"{moveCost}만큼 소모하여 행동을 실행합니다.");
            // 여기에 실제 이동이나 행동 로직 추가
        }
        else
        {
            Debug.Log("조건을 만족하지 못해 실행할 수 없습니다.");
        }
    }
}
