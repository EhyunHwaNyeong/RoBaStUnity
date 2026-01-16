using UnityEngine;
using TMPro;

public class OptionPanel : MonoBehaviour
{
    public TMP_InputField apInput;

    public void CloseWithApply()
    {
        if (int.TryParse(apInput.text, out int value))
        {
            GameSettings.MaxAP = Mathf.Max(1, value);
            Debug.Log("AP 적용됨: " + GameSettings.MaxAP);
        }
        else
        {
            apInput.text = GameSettings.MaxAP.ToString();
        }

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        apInput.text = GameSettings.MaxAP.ToString();
    }
}
