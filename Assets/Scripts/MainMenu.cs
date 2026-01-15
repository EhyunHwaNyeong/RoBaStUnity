using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    //새 메서드 PlayGame함수로 게임씬 소환
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("SecondMenu");
    }

    //게임 종료 코드
    public void QuitGame()
    {
        Application.Quit();
    }
}
