using UnityEngine;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    [Header("현재 턴 정보")]
    public string currentTurnTag = "Black"; // 시작 팀 설정

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 게임 시작 시 첫 턴 데이터 적용 (AP 회복 등)
        ApplyTurnStart(currentTurnTag);
    }

    // [핵심] 턴 종료 버튼에 이 함수를 연결하세요
    public void OnTurnEndButtonClick(string requesterTag)
    {
        // 현재 턴인 팀이 누른 것이 아니라면 무시
        if (currentTurnTag != requesterTag)
        {
            Debug.LogWarning($"<color=red>거부:</color> 현재는 {currentTurnTag}의 턴입니다. ({requesterTag} 버튼 눌림)");
            return;
        }

        Debug.Log($"<color=white>{currentTurnTag} 팀이 수동으로 턴을 종료했습니다.</color>");
        SwitchTurn();
    }

    public void SwitchTurn()
    {
        Debug.Log($"<color=orange>현재 턴 종료 시도: {currentTurnTag}</color>");

        // 1. 기존 유닛 선택 해제 및 UI 닫기
        if (GameManager.Instance != null) GameManager.Instance.DeselectObject();
        if (UIManager.Instance != null) UIManager.Instance.CloseAllUI();

        // 2. 사망 규칙 체크 (예외 처리 추가)
        try {
            if (KillManager.Instance != null)
                KillManager.Instance.CheckTurnEndDeath(currentTurnTag);
        } catch (System.Exception e) {
            Debug.LogError("KillManager 체크 중 에러 발생: " + e.Message);
        }

        // 3. 팀 태그 교체 (명확하게 삼항 연산자 사용)
        string nextTurnTag = (currentTurnTag == "Black") ? "White" : "Black";
        currentTurnTag = nextTurnTag;

        Debug.Log($"<color=cyan>턴 변경 완료! 이제부터 [ {currentTurnTag} ] 의 턴입니다.</color>");

        // 4. 새로운 팀 상태 초기화
        ApplyTurnStart(currentTurnTag);
    }

    private void ApplyTurnStart(string teamTag)
    {
        // 해당 팀의 AP 회복
        if (AP_Counter_Manager.Instance != null)
        {
            AP_Counter_Manager.Instance.RestoreTeamAP(teamTag);
        }

        // 전장의 모든 유닛 중 해당 팀 유닛들의 행동 가능 상태 리셋
        UnitData[] allUnits = FindObjectsByType<UnitData>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit.CompareTag(teamTag))
            {
                unit.ResetTurnState();
            }
        }
    }

    // 유닛 클릭 시 현재 턴 유닛인지 확인하는 함수
    public bool IsMyTurn(string unitTag)
    {
        return currentTurnTag == unitTag;
    }
}