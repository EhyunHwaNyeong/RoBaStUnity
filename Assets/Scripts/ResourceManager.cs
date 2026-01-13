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

    public bool TryConsume(int cost, params string[] tags) // cost를 앞으로 보냈습니다.
    {
        foreach (string tag in tags)
        {
            LayerData target = layerDataList.Find(d => d.targetTag == tag);
            
            // 해당 태그의 자원이 부족하면 바로 거절
            if (target == null || target.currentAmount < cost)
            {
                Debug.Log($"{tag} 자원이 부족하거나 없습니다.");
                return false; 
            }
        }

        // 모든 태그의 자원이 충분할 때만 일괄 소모
        foreach (string tag in tags)
        {
            LayerData target = layerDataList.Find(d => d.targetTag == tag);
            target.currentAmount -= cost;
        }
        
        return true;
    }   
}