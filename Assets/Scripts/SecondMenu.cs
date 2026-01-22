using UnityEngine;
using UnityEngine.SceneManagement;

public class SecondMenu : MonoBehaviour
{
    public void NewGame()
        {
            SceneManager.LoadSceneAsync("SampleScene");
        }

    public void GoMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
