using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

/// <summary>
///     管理桌面端與延展實境模式切換，支援執行期自動偵測與手動切換空間畫布
/// </summary>
public class DeviceModeManager : MonoBehaviour
{
    public static DeviceModeManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Mode Settings")]
    [SerializeField] private bool isXRMode;
    [SerializeField] private Vector3 xrCanvasLocalPosition = new Vector3(0f, 0f, 2f);
    [SerializeField] private Vector3 xrCanvasLocalScale = new Vector3(0.001f, 0.001f, 0.001f);

    // Runtime References
    private Camera mainCamera;
    private RenderMode originalRenderMode;

    public bool IsXRMode => isXRMode;

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
            Debug.LogError("[DeviceModeManager] 場景中找不到標記為 MainCamera 的相機！", this);
        }

#if UNITY_EDITOR
        if (targetCanvas == null)
        {
            Debug.LogWarning("[DeviceModeManager] targetCanvas 尚未在 Inspector 指派！", this);
        }
#endif

        if (targetCanvas != null)
        {
            originalRenderMode = targetCanvas.renderMode;
        }

        if (XRSettings.isDeviceActive)
        {
            SetXRMode(true);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.xKey.wasPressedThisFrame || Keyboard.current.f1Key.wasPressedThisFrame)
        {
            ToggleMode();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && targetCanvas != null)
        {
            SetXRMode(isXRMode);
        }
    }
#endif

    [ContextMenu("Toggle XR Mode")]
    public void ToggleMode()
    {
        SetXRMode(!isXRMode);
    }

    public void SetXRMode(bool enableXR)
    {
        if (targetCanvas == null || mainCamera == null) return;

        isXRMode = enableXR;

        if (isXRMode)
        {
            targetCanvas.renderMode = RenderMode.WorldSpace;
            targetCanvas.worldCamera = mainCamera;
            targetCanvas.transform.SetParent(mainCamera.transform);
            targetCanvas.transform.localPosition = xrCanvasLocalPosition;
            targetCanvas.transform.localRotation = Quaternion.identity;
            targetCanvas.transform.localScale = xrCanvasLocalScale;
        }
        else
        {
            targetCanvas.transform.SetParent(null);
            targetCanvas.renderMode = originalRenderMode;
            targetCanvas.transform.localScale = Vector3.one;
        }
    }
}
