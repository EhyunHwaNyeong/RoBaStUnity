using UnityEngine;

[CreateAssetMenu(fileName = "NewLayerData", menuName = "Custom/LayerData")]
public class LayerData : ScriptableObject
{
    public string targetTag; 
    public int maxCapacity;
    public int currentAmount;

    public void Refill(int amount)
    {
        currentAmount = Mathf.Min(currentAmount + amount, maxCapacity);
    }
}