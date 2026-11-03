using UnityEngine;

/// <summary>
///     被放置在曲面場景上的 3D 道具實體
/// </summary>
public class PlacedItem : MonoBehaviour
{
    [Header("Item Config")] [SerializeField]
    private string itemName = "Default";

    public string ItemName => itemName;

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}