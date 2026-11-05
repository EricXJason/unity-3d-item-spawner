using UnityEngine;

/// <summary>
///     被放置在曲面場景上的 3D 道具實體
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlacedItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemName = "Default";

    public string ItemName => itemName;

    public float HalfHeight
    {
        get
        {
            if (TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh != null)
            {
                return meshFilter.sharedMesh.bounds.extents.y * transform.lossyScale.y;
            }

            return 0.5f;
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
