using UnityEngine;
using TMPro;

public class OptionPanel : MonoBehaviour
{
    public TMP_InputField apInput;

    void OnEnable()
    {
        apInput.text = GameSettings.MaxAP.ToString();
    }

    public void ApplyAP()
    {
        if (int.TryParse(apInput.text, out int value))
        {
            GameSettings.MaxAP = Mathf.Max(1, value);
            Debug.Log("옵션에서 설정한 Max AP: " + GameSettings.MaxAP);
        }
    }
}
