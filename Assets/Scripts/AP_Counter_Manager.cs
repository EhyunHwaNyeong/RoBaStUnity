using UnityEngine;
using TMPro; // TextMeshPro를 사용한다고 가정합니다.

public class AP_Counter_Manager : MonoBehaviour
{
    public static AP_Counter_Manager Instance;

    [Header("공유 설정")]
    public int maxAP = 10;      // 두 팀이 공유하는 최대 용량
    public int restoreAmount = 5; // 회복 버튼 클릭 시 공유하는 회복량

    [Header("실시간 AP (개별 작동)")]
    public int blackCurrentAP;
    public int whiteCurrentAP;

    [Header("UI 연결")]
    public TextMeshProUGUI blackAPText; // "Black"용 UI (현재/최대)
    public TextMeshProUGUI whiteAPText; // "White"용 UI (현재/최대)

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작 시 두 팀 모두 최대치로 초기화
        blackCurrentAP = maxAP;
        whiteCurrentAP = maxAP;
        UpdateAllUI();
    }

    // Tag에 따라 AP 소모
    public bool ConsumeAP(string tag, int amount)
    {
        if (tag == "Black")
        {
            if (blackCurrentAP < amount) return false;
            blackCurrentAP -= amount;
        }
        else if (tag == "White")
        {
            if (whiteCurrentAP < amount) return false;
            whiteCurrentAP -= amount;
        }
        
        UpdateAllUI();
        return true;
    }

    // 특정 팀 전용 회복 함수 (버튼에서 호출)
    public void RestoreTeamAP(string teamTag)
    {
        if (teamTag == "Black")
        {
            blackCurrentAP = Mathf.Min(blackCurrentAP + restoreAmount, maxAP);
        }
        else if (teamTag == "White")
        {
            whiteCurrentAP = Mathf.Min(whiteCurrentAP + restoreAmount, maxAP);
        }

        UpdateAllUI();
        Debug.Log($"{teamTag} 팀 AP {restoreAmount} 회복 완료");
    }

    public void UpdateAllUI()
    {
        if (blackAPText != null) blackAPText.text = $"({blackCurrentAP} / {maxAP})";
        if (whiteAPText != null) whiteAPText.text = $"({whiteCurrentAP} / {maxAP})";
    }
}