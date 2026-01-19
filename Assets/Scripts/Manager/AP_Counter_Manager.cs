using UnityEngine;
using TMPro;

public class AP_Counter_Manager : MonoBehaviour
{
    public static AP_Counter_Manager Instance;

    [System.Serializable]
    public struct TeamAPUI
    {
        public string teamTag;         
        public GameObject apPanel;      
        public TextMeshProUGUI apText;  
    }

    [Header("팀별 UI 설정")]
    public TeamAPUI whiteTeamUI;
    public TeamAPUI blackTeamUI;

    [Header("설정")]
    public int maxAP = 3;          // 요구사항: 3으로 고정
    public int restoreAmount = 3;  // 요구사항: 턴 변경 시 회복

    [Header("실시간 AP")]
    public int blackCurrentAP;
    public int whiteCurrentAP;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        blackCurrentAP = maxAP;
        whiteCurrentAP = maxAP;
    }

    void Start()
    {
        // [핵심] 게임 시작 시 무조건 켭니다. 절대 끄지 않습니다.
        if (whiteTeamUI.apPanel != null) whiteTeamUI.apPanel.SetActive(true);
        if (blackTeamUI.apPanel != null) blackTeamUI.apPanel.SetActive(true);

        UpdateAllUI();
    }

    public void UpdateAllUI()
    {
        if (whiteTeamUI.apText != null) 
            whiteTeamUI.apText.text = $"( {whiteCurrentAP} / {maxAP} )";
        
        if (blackTeamUI.apText != null) 
            blackTeamUI.apText.text = $"( {blackCurrentAP} / {maxAP} )";
    }
    public bool IsAPEmpty(string tag)
    {
        if (tag == "Black") return blackCurrentAP <= 0;
        if (tag == "White") return whiteCurrentAP <= 0;
        return false;
    }

    // 외부에서 호출: tag와 소모량(amount)을 받아 처리
    public bool ConsumeAP(string tag, int amount)
    {
        bool hasEnough = false;

        if (tag == "Black")
        {
            if (blackCurrentAP >= amount) { blackCurrentAP -= amount; hasEnough = true; }
        }
        else if (tag == "White")
        {
            if (whiteCurrentAP >= amount) { whiteCurrentAP -= amount; hasEnough = true; }
        }
        
        if (hasEnough) UpdateAllUI();
        
        return hasEnough;
    }

    // 턴을 넘겨야 하는지 확인만 하는 함수 (자동 호출 X)
    public void CheckAndSwitchTurn(string tag)
    {
        if (IsAPEmpty(tag))
        {
            Debug.Log($"<color=yellow>{tag} 팀 AP 소진. 이동 완료 후 턴을 전환합니다.</color>");
            TurnManager.Instance.SwitchTurn();
        }
    }
    // TurnManager에서 호출
    public void RestoreTeamAP(string teamTag)
    {
        // 턴 시작 시 AP 회복 (최대치로 리셋 or 누적)
        // 요구사항: "회복되게 하고 싶어" -> 보통 최대치로 리셋 혹은 +restoreAmount
        if (teamTag == "Black") 
            blackCurrentAP = Mathf.Min(blackCurrentAP + restoreAmount, maxAP);
        else if (teamTag == "White") 
            whiteCurrentAP = Mathf.Min(whiteCurrentAP + restoreAmount, maxAP);
        
        UpdateAllUI();
        Debug.Log($"{teamTag} 팀 AP {restoreAmount}로 회복됨.");
    }

    // 호환성 유지용 (내용은 UpdateAllUI와 동일)
    public void ShowTeamPanel(string tag)
    {
        UpdateAllUI();
    }
}