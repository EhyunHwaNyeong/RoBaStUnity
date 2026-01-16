using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("현재 턴 정보")]
    public string currentTurnTag = "Black"; // 시작은 Black부터

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 게임 시작 시 첫 턴 적용
        ApplyTurnStart(currentTurnTag);
    }

    // --- 턴 교체 함수 ---
    public void SwitchTurn()
    {
        // 1. 턴 종료 전, 현재 팀의 사망 조건 체크
        if (KillManager.Instance != null)
        {
            KillManager.Instance.CheckTurnEndDeath(currentTurnTag);
        }

        // 2. [수정됨] 턴 교체 로직 (가장 중요한 부분)
        if (currentTurnTag == "Black") 
        {
            currentTurnTag = "White";
        }
        else 
        {
            currentTurnTag = "Black"; // [해결] 여기를 Black으로 확실하게 변경
        }

        Debug.Log($"턴이 변경되었습니다: {currentTurnTag}");

        // 3. 변경된 턴 적용 (AP 회복 등)
        ApplyTurnStart(currentTurnTag);
    }

    private void ApplyTurnStart(string teamTag)
    {
        // AP 매니저에게 해당 팀의 AP 회복 및 UI 갱신 요청
        if (AP_Counter_Manager.Instance != null)
        {
            AP_Counter_Manager.Instance.RestoreTeamAP(teamTag);
        }
    }
    
    // 유닛 선택 가능 여부 확인
    public bool IsMyTurn(string unitTag)
    {
        return currentTurnTag == unitTag;
    }
}