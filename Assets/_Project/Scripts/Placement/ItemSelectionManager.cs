using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
///     檢測滑鼠在場景中的點擊射線，控制選取 3D 道具時彈出互動選單，或點選空白與地面時關閉選單。
/// </summary>
public class ItemSelectionManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float maxRaycastDistance = 100f;

    // Runtime References
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[ItemSelectionManager] 場景中找不到標記為 MainCamera 的相機！", this);
        }
    }

    private void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleSelection();
        }
    }

    private void HandleSelection()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (mainCamera == null || Mouse.current == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, maxRaycastDistance))
        {
            if (raycastHit.collider.TryGetComponent<PlacedItem>(out var clickedPlacedItem))
            {
                if (ItemContextMenuUI.Instance != null)
                {
                    ItemContextMenuUI.Instance.Show(clickedPlacedItem);
                }
                return;
            }
        }

        if (ItemContextMenuUI.Instance != null)
        {
            ItemContextMenuUI.Instance.Hide();
        }
    }
}