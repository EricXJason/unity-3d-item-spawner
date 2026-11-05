using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
///     管理場景道具互動選單 UI，包含螢幕座標追蹤、資訊顯示與刪除按鈕事件
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ItemContextMenuUI : MonoBehaviour
{
    public static ItemContextMenuUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameLabel;
    [SerializeField] private Button deleteButton;

    [Header("Menu Settings")]
    [SerializeField] private Vector2 screenOffset = new Vector2(25f, 20f);
    [FormerlySerializedAs("fadeDuration")]
    [SerializeField] private float animationDuration = 0.15f;

    // Runtime References
    private Camera mainCamera;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private PlacedItem currentTargetItem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        TryGetComponent(out rectTransform);
        TryGetComponent(out canvasGroup);

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("[ItemContextMenuUI] 場景中找不到標記為 MainCamera 的相機！", this);
        }

#if UNITY_EDITOR
        if (itemNameLabel == null)
        {
            Debug.LogWarning("[ItemContextMenuUI] itemNameLabel 尚未在 Inspector 指派！", this);
        }

        if (deleteButton == null)
        {
            Debug.LogWarning("[ItemContextMenuUI] deleteButton 尚未在 Inspector 指派！", this);
        }
#endif

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        SetVisibilityImmediate(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveListener(OnDeleteButtonClicked);
        }

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
        }
    }

    public void Show(PlacedItem targetItem)
    {
        if (targetItem == null) return;

        currentTargetItem = targetItem;

        if (itemNameLabel != null)
        {
            itemNameLabel.text = targetItem.ItemName;
        }

        UpdateMenuScreenPosition();

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            canvasGroup.DOFade(1f, animationDuration);
        }
    }

    public void Hide()
    {
        currentTargetItem = null;

        if (canvasGroup != null)
        {
            canvasGroup.DOKill();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            canvasGroup.DOFade(0f, animationDuration);
        }
    }

    private void UpdateMenuScreenPosition()
    {
        if (currentTargetItem == null || mainCamera == null) return;

        Vector3 worldAnchorPosition = currentTargetItem.transform.position + Vector3.up * currentTargetItem.HalfHeight;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldAnchorPosition);

        rectTransform.position = screenPosition + (Vector3)screenOffset;
    }

    private void SetVisibilityImmediate(bool isVisible)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = isVisible ? 1f : 0f;
        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }

    private void OnDeleteButtonClicked()
    {
        if (currentTargetItem != null)
        {
            currentTargetItem.DestroySelf();
        }

        Hide();
    }
}
