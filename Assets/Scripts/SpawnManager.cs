using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// 1. 반드시 클래스 바깥에 이 구조체가 있어야 다른 곳에서 참조할 수 있습니다.
[System.Serializable]
public struct SpawnData
{
    public string name;          // 식법용 이름
    public GameObject prefab;    // 소환할 프리팹
    public Vector3Int tilePos;   // 타일맵 좌표 (x, y)
}

public class SpawnManager : MonoBehaviour
{
    [Header("Settings")]
    public Tilemap targetTilemap;      // 대상 타일맵
    public List<SpawnData> spawnList;  // 소환 리스트

    [Header("Rotation Settings")]
    public string targetTag = "RotatedObject"; // 회전시킬 특정 태그
    public Vector3 customRotation = new Vector3(0, 0, 90f); // 회전 각도

    void Start()
    {
        SpawnObjects();
    }

    void SpawnObjects()
    {
        if (targetTilemap == null)
        {
            Debug.LogWarning("타일맵이 연결되지 않았습니다!");
            return;
        }

        foreach (var data in spawnList)
        {
            if (data.prefab == null) continue;

            // 좌표 변환
            Vector3 worldPos = targetTilemap.GetCellCenterWorld(data.tilePos);
            
            // 기본 회전 (회전 없음)
            Quaternion spawnRotation = Quaternion.identity;

            // 특정 태그 확인 시 회전 적용
            if (data.prefab.CompareTag(targetTag))
            {
                spawnRotation = Quaternion.Euler(customRotation);
            }

            // 생성
            GameObject go = Instantiate(data.prefab, worldPos, spawnRotation);
            go.transform.SetParent(this.transform);
        }
    }
}