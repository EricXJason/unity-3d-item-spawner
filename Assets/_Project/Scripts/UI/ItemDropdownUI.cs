using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     右側道具下拉選單：採代碼監聽、DOTween 縮放與透明度漸變控制
/// </summary>
public class ItemDropdownUI : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private TextMeshProUGUI headerText;

    [SerializeField] private Transform contentPanel;
    [SerializeField] private Button dropdownButton;


    [Header("Settings")] [SerializeField] private float duration = 0.3f;

    private CanvasGroup contentCanvasGroup;

    private bool isOpen;

    private void Awake()
    {
        if (dropdownButton != null) dropdownButton.onClick.AddListener(Toggle);


        if (contentPanel != null)
        {
            contentPanel.localScale = new Vector3(1f, 0f, 1f);
            contentCanvasGroup = contentPanel.GetComponent<CanvasGroup>();
            if (contentCanvasGroup == null) contentCanvasGroup = contentPanel.gameObject.AddComponent<CanvasGroup>();
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

        isOpen = !isOpen;

        contentPanel.DOKill();
        contentCanvasGroup.DOKill();

        if (isOpen)
        {
            contentCanvasGroup.blocksRaycasts = true;

            contentPanel.DOScaleY(1f, duration).SetEase(Ease.OutCubic);
            contentCanvasGroup.DOFade(1f, duration);

            if (headerText != null) headerText.text = "Items ▲";
        }
        else
        {
            contentCanvasGroup.blocksRaycasts = false;

            contentPanel.DOScaleY(0f, duration).SetEase(Ease.InCubic);
            contentCanvasGroup.DOFade(0f, duration);

            if (headerText != null) headerText.text = "Items ▼";
        }
    }
}