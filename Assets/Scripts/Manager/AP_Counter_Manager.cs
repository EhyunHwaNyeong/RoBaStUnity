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
        UpdateAllUI();
    }

    // [핵심] 태그에 맞는 AP 패널만 띄워주는 기능
    public void ShowTeamPanel(string tag)
    {
        // 일단 모든 AP 패널을 숨김 (필요한 경우에만)
        // whiteTeamUI.apPanel.SetActive(false);
        // blackTeamUI.apPanel.SetActive(false);

        if (tag == "White" && whiteTeamUI.apPanel != null)
            whiteTeamUI.apPanel.SetActive(true);
        else if (tag == "Black" && blackTeamUI.apPanel != null)
            blackTeamUI.apPanel.SetActive(true);
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
            ShowTeamPanel(tag); // 소모한 팀의 UI를 다시 확인해서 띄움
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

    // 회복 버튼 등에서 호출
    public void RestoreTeamAP(string teamTag)
    {
        if (teamTag == "Black") blackCurrentAP = Mathf.Min(blackCurrentAP + restoreAmount, maxAP);
        else if (teamTag == "White") whiteCurrentAP = Mathf.Min(whiteCurrentAP + restoreAmount, maxAP);
        UpdateAllUI();
        ShowTeamPanel(teamTag);
    }
}