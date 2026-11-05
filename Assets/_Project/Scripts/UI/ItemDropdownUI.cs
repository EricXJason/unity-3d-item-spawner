using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
///     右側道具下拉選單，負責展開與收合的縮放及透明度動畫
/// </summary>
public class ItemDropdownUI : MonoBehaviour
{
    private const string HeaderExpandedText = "Items ▲";
    private const string HeaderCollapsedText = "Items ▼";

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private Transform contentPanel;
    [SerializeField] private Button dropdownButton;

    [Header("Dropdown Settings")]
    [FormerlySerializedAs("duration")]
    [SerializeField] private float animationDuration = 0.3f;

    // Runtime References
    private CanvasGroup contentCanvasGroup;
    private bool isOpen;

    private void Awake()
    {
#if UNITY_EDITOR
        if (dropdownButton == null)
        {
            Debug.LogWarning("[ItemDropdownUI] dropdownButton 尚未在 Inspector 指派！", this);
        }

        if (contentPanel == null)
        {
            Debug.LogWarning("[ItemDropdownUI] contentPanel 尚未在 Inspector 指派！", this);
        }
#endif

        if (dropdownButton != null)
        {
            dropdownButton.onClick.AddListener(Toggle);
        }

        if (contentPanel != null)
        {
            contentPanel.localScale = new Vector3(1f, 0f, 1f);
            if (!contentPanel.TryGetComponent<CanvasGroup>(out contentCanvasGroup))
            {
                contentCanvasGroup = contentPanel.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (contentCanvasGroup != null)
        {
            contentCanvasGroup.alpha = 0f;
            contentCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        if (dropdownButton != null) dropdownButton.onClick.RemoveListener(Toggle);
        if (contentPanel != null) contentPanel.DOKill();
        if (contentCanvasGroup != null) contentCanvasGroup.DOKill();
    }

    private void Toggle()
    {
        if (contentPanel == null || contentCanvasGroup == null) return;

        if (ItemContextMenuUI.Instance != null)
        {
            ItemContextMenuUI.Instance.Hide();
        }

        isOpen = !isOpen;

        contentPanel.DOKill();
        contentCanvasGroup.DOKill();

        if (isOpen)
        {
            contentCanvasGroup.blocksRaycasts = true;

            contentPanel.DOScaleY(1f, animationDuration).SetEase(Ease.OutCubic);
            contentCanvasGroup.DOFade(1f, animationDuration);

            if (headerText != null) headerText.text = HeaderExpandedText;
        }
        else
        {
            contentCanvasGroup.blocksRaycasts = false;

            contentPanel.DOScaleY(0f, animationDuration).SetEase(Ease.InCubic);
            contentCanvasGroup.DOFade(0f, animationDuration);

            if (headerText != null) headerText.text = HeaderCollapsedText;
        }
    }
}
