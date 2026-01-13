using UnityEngine;
using TMPro; // TextMeshPro 사용 권장

public class CounterUI : MonoBehaviour
{
    public string targetTag; // 여기에 "Player", "Enemy" 등의 태그명을 적습니다.
    public TMP_Text counterText;

    void Update()
    {
        var data = ResourceManager.Instance.layerDataList.Find(d => d.targetTag == targetTag);
        if (data != null)
        {
            counterText.text = $"{data.currentAmount} / {data.maxCapacity}";
        }
    }
}
