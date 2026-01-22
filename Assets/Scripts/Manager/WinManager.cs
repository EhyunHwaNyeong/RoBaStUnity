using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필수
using System.Collections;

public class WinManager : MonoBehaviour
{
    public static WinManager Instance;

    [Header("승리 UI 설정")]
    public GameObject winUIPanel;      
    public TextMeshProUGUI winText;    
    public float delayBeforeShowUI = 1.5f; // 승리 판정 후 UI가 뜰 때까지의 대기 시간

    [Header("공용 폰트 색상 설정")]
    // 여기서 지정한 색상이 어떤 팀이 이기든 공통으로 적용됩니다.
    public Color commonWinColor = Color.yellow;

    private int blackKnightDeathCount = 0;
    private int whiteKnightDeathCount = 0;
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (winUIPanel != null) winUIPanel.SetActive(false);
    }

    public void RegisterDeath(GameObject unit)
    {
        if (isGameOver) return;

        string team = unit.tag;
        int layer = unit.layer;
        int commanderLayer = LayerMask.NameToLayer("Commander");
        int knightLayer = LayerMask.NameToLayer("Knight");

        if (layer == commanderLayer)
        {
            string winner = (team == "Black") ? "White" : "Black";
            StartCoroutine(GameOverSequence(winner, "Commander Die"));
        }
        else if (layer == knightLayer)
        {
            if (team == "Black") blackKnightDeathCount++;
            else whiteKnightDeathCount++;

            if (blackKnightDeathCount >= 3) StartCoroutine(GameOverSequence("White", "3rd Knight Die"));
            else if (whiteKnightDeathCount >= 3) StartCoroutine(GameOverSequence("Black", "3rd Knight Die"));
        }
    }

    // [핵심] 게임 종료 시퀀스
    private IEnumerator GameOverSequence(string winnerTag, string reason)
    {
        isGameOver = true;

        // 1. 모든 조작 차단 (TurnManager의 플래그 활용)
        if (TurnManager.Instance != null) TurnManager.Instance.isSwitchingTurn = true;
        
        Debug.Log($"<color=yellow>Game Over: {winnerTag} WIN!</color>");

        yield return new WaitForSecondsRealtime(delayBeforeShowUI);

        if (winUIPanel != null)
        {
            winUIPanel.SetActive(true);
            if (winText != null) 
            {
                winText.enableVertexGradient = false;
                // [개선] 팀 상관없이 설정한 공용 색상 적용
                winText.color = commonWinColor; 
                winText.canvasRenderer.SetColor(commonWinColor);
                winText.SetAllDirty();
                
                // [개선] 영문 위주로 작성하여 폰트 깨짐 방지
                winText.text = $"{winnerTag} WIN!\n<size=24>{reason}</size>";
            }
        }
    }

    // [버튼 연결용] 원하는 씬으로 이동하는 함수
    public void GoToScene(string sceneName)
    {
        // 시간 배율을 1로 초기화 (혹시 0으로 바꿨을 경우를 대비)
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}