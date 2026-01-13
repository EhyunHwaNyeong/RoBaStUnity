using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    // 아까 만든 ScriptableObject 파일들을 여기에 드래그해서 넣을 겁니다.
    public List<LayerData> layerDataList = new List<LayerData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작할 때 모든 레이어의 현재량을 최대치로 초기화
        foreach (var data in layerDataList)
        {
            data.currentAmount = data.maxCapacity;
        }
    }

    public bool TryConsume(string tag, int cost)
    {
        // layerName 대신 targetTag와 비교합니다.
        LayerData target = layerDataList.Find(d => d.targetTag == tag);
        
        if (target != null && target.currentAmount >= cost)
        {
            target.currentAmount -= cost;
            return true;
        }
        return false;
    }   
}