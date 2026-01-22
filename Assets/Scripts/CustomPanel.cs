using UnityEngine;
using TMPro;

public class CustomPanel : MonoBehaviour
{
    public TMP_InputField apInput;

    private const int MIN_AP = 3;
    private const int MAX_AP = 5;

    private void Awake()
    {
        if(GameSettings.MaxAP<MIN_AP || GameSettings.MaxAP > MAX_AP)
            GameSettings.MaxAP=MIN_AP;
    }
    // X(=Cancel) 버튼에서 호출
    public void CloseWithApply()
    {
        // 1) 적용
        if (int.TryParse(apInput.text, out int value))
        {
            GameSettings.MaxAP = Mathf.Clamp(value, MIN_AP, MAX_AP);
            Debug.Log("AP 적용됨: " + GameSettings.MaxAP);
        }
        else
        {
            apInput.text = GameSettings.MaxAP.ToString();
        }

        gameObject.SetActive(false);
    }

    // 옵션 열 때 현재 값 보여주고 싶으면(선택)
    private void OnEnable()
    {
        apInput.text = GameSettings.MaxAP.ToString();
    }
}
