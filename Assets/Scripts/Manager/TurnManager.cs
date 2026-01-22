using UnityEngine;
using UnityEngine.UI; // Image 컴포넌트 사용을 위해 추가
using System.Collections;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public bool isSwitchingTurn = false; // 중복 방지 플래그

    [Header("현재 턴 정보")]
    public string currentTurnTag = "Black"; // 시작 팀 설정

    [Header("턴 종료 버튼 UI 설정")]
    public Image blackTurnButtonImage; // 블랙 팀 턴 종료 버튼의 Image 컴포넌트
    public Image whiteTurnButtonImage; // 화이트 팀 턴 종료 버튼의 Image 컴포넌트
    
    public Sprite activeSprite;   // 자신의 턴일 때 표시할 이미지
    public Sprite inactiveSprite; // 자신의 턴이 아닐 때 표시할 이미지

    private float lastClickTime = 0f;
    

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 게임 시작 시 첫 턴 데이터 적용 (AP 회복 등)
        // ApplyTurnStart(currentTurnTag);
        UpdateAllButtonVisuals();
    }

    // 버튼의 이미지를 현재 턴 상태에 따라 갱신하는 함수
    private void UpdateAllButtonVisuals()
    {
        if (blackTurnButtonImage != null)
        {
            blackTurnButtonImage.sprite = (currentTurnTag == "Black") ? activeSprite : inactiveSprite;
        }

        if (whiteTurnButtonImage != null)
        {
            whiteTurnButtonImage.sprite = (currentTurnTag == "White") ? activeSprite : inactiveSprite;
        }
    }

    // [핵심] 턴 종료 버튼에 이 함수를 연결하세요
    public void OnTurnEndButtonClick(string requesterTag)
    {
        // 클릭 간격을 0.5초로 제한 (연타 방지)
        if (Time.time - lastClickTime < 0.5f) return;
        lastClickTime = Time.time;
        // 현재 턴인 팀이 누른 것이 아니라면 무시
        if (currentTurnTag != requesterTag || isSwitchingTurn)
        {
            Debug.LogWarning($"<color=red>거부:</color> 현재는 {currentTurnTag}의 턴입니다. ({requesterTag} 버튼 눌림)");
            return;
        }

        Debug.Log($"<color=white>{currentTurnTag} 팀이 수동으로 턴을 종료했습니다.</color>");
        SwitchTurn();
    }

    public void SwitchTurn()
    {
        // 1. 이미 전환 중이면 즉시 차단
        if (isSwitchingTurn) return;
        
        StartCoroutine(SafeSwitchTurnRoutine());
    }

    private IEnumerator SafeSwitchTurnRoutine()
    {
        isSwitchingTurn = true; 

        KillManager.Instance.CheckTurnEndDeath(currentTurnTag);

        if (AP_Counter_Manager.Instance != null) 
            AP_Counter_Manager.Instance.ResetRestoreFlag();

        // 1. 현재(이전) 팀의 선택 해제 및 UI 닫기 (태그 바꾸기 전에 수행)
        if (GameManager.Instance != null) GameManager.Instance.DeselectObject();
        if (UIManager.Instance != null) UIManager.Instance.CloseAllUI();

        Debug.Log($"<color=orange>턴 전환 프로세스 시작 (현재: {currentTurnTag})</color>");

        // 2. 팀 태그 교체 (여기서 실제 턴이 넘어감)
        currentTurnTag = (currentTurnTag == "Black") ? "White" : "Black";
        
        // 2. [추가] 턴이 바뀌었으므로 버튼 이미지 즉시 갱신
        UpdateAllButtonVisuals();

        // 3. 새로운 팀에게만 회복 및 상태 리셋 적용
        ApplyTurnStart(currentTurnTag);
        
        // Debug.Log($"<color=cyan>턴 변경 완료! 현재 턴: [ {currentTurnTag} ]</color>");

        // 4. 동일 프레임 내 중복 호출을 완전히 무시하기 위한 대기 시간
        // 이 시간 동안은 SwitchTurn()이 호출되어도 상단 if(isSwitchingTurn)에 의해 차단됨
        Debug.Log($"<color=cyan>턴 전환 시스템 안정화 완료</color>");
        yield return new WaitForSecondsRealtime(0.3f); 
        isSwitchingTurn = false;
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