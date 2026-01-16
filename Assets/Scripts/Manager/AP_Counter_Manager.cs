using UnityEngine;
using TMPro;

public class AP_Counter_Manager : MonoBehaviour
{
    public static AP_Counter_Manager Instance;

    [System.Serializable]
    public struct TeamAPUI
    {
        public string teamTag;          // "White" 또는 "Black"
        public GameObject apPanel;      // 해당 팀의 전체 AP 패널
        public TextMeshProUGUI apText;  // 해당 팀의 텍스트 (현재/최대)
    }

    [Header("팀별 UI 설정")]
    public TeamAPUI whiteTeamUI;
    public TeamAPUI blackTeamUI;

    [Header("공통 설정")]
    public int maxAP = 10;
    public int restoreAmount = 5;

    [Header("실시간 AP")]
    public int blackCurrentAP;
    public int whiteCurrentAP;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        blackCurrentAP = maxAP;
        whiteCurrentAP = maxAP;
        
        // 시작하자마자 UI 한번 갱신
        UpdateAllUI();
    }

    // [수정됨] 패널을 켤 때 텍스트도 확실하게 갱신하여 깜빡임/사라짐 방지
    public void ShowTeamPanel(string tag)
    {
        // 1. 필요한 패널 켜기
        if (tag == "White" && whiteTeamUI.apPanel != null)
            whiteTeamUI.apPanel.SetActive(true);
        else if (tag == "Black" && blackTeamUI.apPanel != null)
            blackTeamUI.apPanel.SetActive(true);

        // 2. [중요] 패널을 켠 직후 텍스트 값을 즉시 갱신 (빈 텍스트 방지)
        UpdateAllUI();
    }

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
        
        if (hasEnough)
        {
            UpdateAllUI();
            // AP 소모 후에도 패널이 꺼지지 않도록 확실하게 다시 호출
            ShowTeamPanel(tag); 
        }
        return hasEnough;
    }

    public void UpdateAllUI()
    {
        if (whiteTeamUI.apText != null) 
            whiteTeamUI.apText.text = $"({whiteCurrentAP} / {maxAP})";
        if (blackTeamUI.apText != null) 
            blackTeamUI.apText.text = $"({blackCurrentAP} / {maxAP})";
    }

    public void RestoreTeamAP(string teamTag)
    {
        if (teamTag == "Black") blackCurrentAP = Mathf.Min(blackCurrentAP + restoreAmount, maxAP);
        else if (teamTag == "White") whiteCurrentAP = Mathf.Min(whiteCurrentAP + restoreAmount, maxAP);
        
        UpdateAllUI();
        ShowTeamPanel(teamTag);
    }
}