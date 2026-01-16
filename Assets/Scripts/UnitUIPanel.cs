using UnityEngine;

public class UnitUIPanel : MonoBehaviour
{
    [Header("이 패널이 제어할 버튼들")]
    public GameObject forwardButton;
    public GameObject backwardButton;

    public void SetButtons(bool canForward, bool canBackward)
    {
        if (forwardButton != null) forwardButton.SetActive(canForward);
        if (backwardButton != null) backwardButton.SetActive(canBackward);
    }
}