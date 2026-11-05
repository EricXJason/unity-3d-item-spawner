# Unity 3D Item Spawner

在起伏曲面上以拖曳方式放置三維道具的 Unity 專案。支援點選已放置的道具並刪除，以及桌面與 XR 畫布模式的切換。

- Unity 6000.3.22f1，Universal Render Pipeline 17.3.0
- 桌面建置目標：macOS（視窗可縮放）

## 展示影片

[YouTube：完整操作錄影](https://www.youtube.com/watch?v=ks2Eg1XgcBc)

| 時間 | 內容 |
| :---: | :--- |
| 00:00 | 調整視窗大小，UI 依螢幕比例縮放 |
| 00:04 | 點擊右側 `Items` 按鈕，展開道具清單 |
| 00:07 | 拖曳方塊到起伏地形，放開後依落點法線生成 |
| 00:10 | 拖曳球體與圓柱，顯示垂直指引線與預覽物 |
| 00:13 | 拖到地形範圍外放開，不產生物件 |
| 00:17 | 點選場景中的道具，開啟互動選單並刪除 |
| 00:24 | 按 `X` 鍵，UI 在 Screen Space 與 World Space 之間切換 |

## 功能對照

| 需求 | 實作 |
| :--- | :--- |
| 高低不同的有限範圍曲面 | Unity Terrain（`SurfaceTerrain`，資料 `TD_Surface`），含 Terrain Collider |
| 視窗可縮放、UI 適應比例 | Player Settings 啟用 Resizable Window，預設 1280x720 視窗化；Canvas Scaler 為 Scale With Screen Size，參考解析度 1920x1080，Match 0.5 |
| 右側道具下拉選單，點選後展開 | `ItemDropdownUI`，以 DOTween 做縮放與淡入淡出 |
| 數種三維道具 | 方塊、圓柱、球體三個 Prefab，各有獨立材質 |
| 拖曳複製道具並跟隨游標 | `ItemDragSlot` 轉發 UI 拖曳事件，`PlacementManager` 建立預覽物 |
| 放開後依落點生成 | 從游標發射射線至地面圖層，於命中點生成並對齊法線 |
| 落在 UI 或無場景位置則取消 | 游標在 UI 上或射線未命中時隱藏預覽物，放開時不生成 |
| 點選道具出現互動選單並可刪除 | `ItemSelectionManager` 偵測點擊，`ItemContextMenuUI` 顯示於物件旁，Delete 按鈕銷毀物件 |
| 加分：指引線 | `LineRenderer` 繪製垂直線標示落點位置 |
| 加分：XR 與桌面切換 | `DeviceModeManager` 切換 Canvas 的 Render Mode，見「XR 範圍」 |

## 操作

| 動作 | 方式 |
| :--- | :--- |
| 展開或收合道具清單 | 點擊右側 `Items` 按鈕 |
| 放置道具 | 從清單拖曳道具至曲面後放開滑鼠 |
| 取消放置 | 在 UI 上或地形範圍外放開滑鼠 |
| 開啟道具選單 | 點擊場景中已放置的道具 |
| 刪除道具 | 按選單中的 `Delete` |
| 關閉道具選單 | 點擊空白處或地面，或操作清單與開始拖曳 |
| 切換 UI 模式 | 鍵盤 `X` 或 `F1` |

## 實作說明

### 放置流程（`PlacementManager`）
1. 開始拖曳：複製道具作為預覽物並停用其 Collider，避免射線打到預覽物自己。
2. 拖曳中：每次更新從游標發射射線，只檢測 `groundLayer` 指定的圖層。
   - 游標在 UI 上，或射線未命中：隱藏預覽物與指引線，並標記為無效。
   - 命中：預覽物置於命中點沿法線方向，高度為道具半高加 `previewFloatHeight`，旋轉對齊法線，並更新指引線。
3. 放開：僅在最後一次檢測有效時，於命中點沿法線偏移半高處生成道具。預覽物一律銷毀。

生成的道具與預覽物都放在 `PlacementManager` 底下，Hierarchy 根層級不會累積 Clone 物件。

### 半高計算（`PlacedItem.HalfHeight`）
Prefab 尚未實例化時，Collider 的 `bounds` 不是有效值，所以改取 `MeshFilter.sharedMesh.bounds.extents.y` 乘上縮放。道具底部因此貼在曲面上，不會陷入地形。

### 指引線
線的起點在預覽物上方 `guideLineLength`，終點是預覽物中心。預覽物懸浮在落點之上，線沿法線方向延伸，不會被地形遮住。寬度與顏色在 `Awake` 設定一次。

### 互動選單（`ItemContextMenuUI`）
以 `WorldToScreenPoint` 將道具頂端轉為螢幕座標，再加上偏移量定位選單。以下情況會關閉選單：點擊空白處或地面、展開或收合清單、開始拖曳道具。

### XR 範圍（`DeviceModeManager`）
- 切換到 XR 模式時，Canvas 改為 World Space，掛在主相機底下，位置為前方 2 公尺、縮放 0.001。切回時還原原本的 Render Mode。
- `Awake` 會檢查 `XRSettings.isDeviceActive`，有 XR 裝置時直接進入 XR 模式。
- 專案未安裝 OpenXR 或 XR Interaction Toolkit，**沒有實作 XR 控制器輸入**，也沒有在實體頭戴裝置上測試。目前完成的是畫布模式的切換與裝置偵測入口。

## 專案結構

```text
Assets/_Project/
├── Materials/    M_Cube, M_Cylinder, M_Sphere
├── Prefabs/      PF_Cube, PF_Cylinder, PF_Sphere
├── Scenes/       SC_Main
├── Terrain/      TD_Surface
└── Scripts/
    ├── Items/      PlacedItem
    ├── Placement/  PlacementManager, ItemSelectionManager, DeviceModeManager
    └── UI/         ItemDropdownUI, ItemDragSlot, ItemContextMenuUI
```

資產前綴：`PF_` Prefab、`M_` 材質、`SC_` 場景、`TD_` TerrainData。

## 相依套件

- Input System 1.20.0（滑鼠與鍵盤輸入）
- Universal Render Pipeline 17.3.0
- uGUI 2.0.0 與 TextMeshPro
- DOTween（位於 `Assets/Plugins/Demigiant`，用於選單動畫）

## 執行與建置

1. 以 Unity 6000.3.22f1 開啟專案。
2. 開啟 `Assets/_Project/Scenes/SC_Main.unity` 後按 Play。
3. 建置：File > Build Profiles，選擇目標平台，場景清單已包含 `SC_Main`。

## 已知限制

- 道具只有方塊、圓柱、球體三種。
- 沒有復原功能與場景存檔。
- 未實作 XR 控制器輸入，見「XR 範圍」。
- 只在 macOS 建置與測試，Windows 與 Linux 未驗證。

## 開發紀錄

提交遵循 Conventional Commits（`feat`、`fix`、`refactor`、`chore`、`docs`、`style`），從專案初始化起逐步提交，未壓縮歷史。
