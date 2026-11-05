using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     管理 3D 空間射線檢測、預覽物追蹤、落點垂直指引線繪製以及道具實體生成。
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class PlacementManager : MonoBehaviour
{    
    public static PlacementManager Instance { get; private set; }

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxRaycastDistance = 100f;
    [SerializeField] private bool alignToNormal = true;

    [Header("Guide Line Settings")]
    [SerializeField] private float guideLineWidth = 0.2f;
    [SerializeField] private float guideLineLength = 6f;
    [SerializeField] private float previewFloatHeight = 0.3f;
    [SerializeField] private Color guideLineColor = Color.blue;
    
    // Runtime References
    private Camera mainCamera;
    private LineRenderer guideLine;
    private PlacedItem currentPlacedItem;
    private PlacedItem previewPlacedItem;
    private bool isValidPlacement;
    private Vector3 finalHitPoint;
    private Vector3 finalHitNormal;
    private float currentItemHalfHeight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[PlacementManager] 場景中找不到標記為 MainCamera 的相機！", this);
        }
        
#if UNITY_EDITOR
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("[PlacementManager] groundLayer 尚未在 Inspector 指派！", this);
        }
#endif

        if (TryGetComponent<LineRenderer>(out guideLine))
        {
            guideLine.positionCount = 2;
            guideLine.startWidth = guideLineWidth;
            guideLine.endWidth = guideLineWidth;
            guideLine.startColor = guideLineColor;
            guideLine.endColor = guideLineColor;
            guideLine.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    public void StartDragging(PlacedItem placedItem, PointerEventData eventData)
    {
        if (placedItem == null) return;
        
        if (previewPlacedItem != null)
        {
            ResetPlacement();   
        }

        currentPlacedItem = placedItem;
        currentItemHalfHeight = placedItem.HalfHeight;

        previewPlacedItem = Instantiate(placedItem, transform);
        if (previewPlacedItem.TryGetComponent<Collider>(out var previewCollider))
        {
            previewCollider.enabled = false;
        }
            
        UpdateDragging(eventData);
    }
    
    public void UpdateDragging(PointerEventData eventData)
    {
        if (previewPlacedItem == null || eventData == null || mainCamera == null) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            SetInvalidState();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, maxRaycastDistance, groundLayer))
        {
            isValidPlacement = true;
            finalHitPoint = raycastHit.point;
            finalHitNormal = raycastHit.normal;

            Quaternion targetRotation = alignToNormal 
                ? Quaternion.FromToRotation(Vector3.up, raycastHit.normal) 
                : Quaternion.identity;

            Vector3 upwardDirection = alignToNormal ? raycastHit.normal : Vector3.up;
            Vector3 previewPosition = raycastHit.point + upwardDirection * (currentItemHalfHeight + previewFloatHeight);

            previewPlacedItem.transform.position = previewPosition;
            previewPlacedItem.transform.rotation = targetRotation;
            previewPlacedItem.gameObject.SetActive(true);

            Vector3 lineTopPosition = previewPosition + upwardDirection * guideLineLength;
            guideLine.enabled = true;
            guideLine.SetPosition(0, lineTopPosition);
            guideLine.SetPosition(1, previewPosition);
        }
        else
        {
            SetInvalidState();
        }
    }

    public void EndDragging()
    {
        guideLine.enabled = false;

        if (currentPlacedItem != null && isValidPlacement)
        {
            Vector3 upwardDirection = alignToNormal ? finalHitNormal : Vector3.up;
            Vector3 spawnPosition = finalHitPoint + upwardDirection * currentItemHalfHeight;
            Quaternion spawnRotation = alignToNormal 
                ? Quaternion.FromToRotation(Vector3.up, finalHitNormal) 
                : Quaternion.identity;

            Instantiate(currentPlacedItem, spawnPosition, spawnRotation, transform);
        }

        ResetPlacement();
    }

    private void ResetPlacement()
    {
        if (previewPlacedItem != null)
        {
            Destroy(previewPlacedItem.gameObject);
            previewPlacedItem = null;
        }

        currentPlacedItem = null;
        isValidPlacement = false;
    }

    private void SetInvalidState()
    {
        isValidPlacement = false;

        if (previewPlacedItem != null)
        {
            previewPlacedItem.gameObject.SetActive(false);
        }

        guideLine.enabled = false;
    }
}
