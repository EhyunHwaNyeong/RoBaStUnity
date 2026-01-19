using UnityEngine;

public class UnitUIPanel : MonoBehaviour
{
    [Header("방향 및 회전 버튼")]
    public GameObject btnForward;   // 이동용
    public GameObject btnBackward;  // 이동용
    public GameObject btnLeft;      // 회전용 (항상 표시)
    public GameObject btnRight;     // 회전용 (항상 표시)

    /// <summary>
    /// f, b는 이동 가능 여부, l, r은 회전 가능 여부
    /// </summary>
    public void SetNavigationButtons(bool f, bool b, bool l, bool r)
    {
        // 전진/후진은 타일 상황에 따라 On/Off
        if (btnForward != null) btnForward.SetActive(f);
        if (btnBackward != null) btnBackward.SetActive(b);

        // [수정] 좌/우 회전 버튼은 항상 켜둠 (또는 l, r 값에 따름)
        if (btnLeft != null) btnLeft.SetActive(l);
        if (btnRight != null) btnRight.SetActive(r);
    }
}