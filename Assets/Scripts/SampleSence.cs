using UnityEngine;
using UnityEngine.SceneManagement;

public class SampleScene : MonoBehaviour
{
    public void GoSecondMenu()
    {
        SceneManager.LoadSceneAsync("SecondMenu");
    }
}
