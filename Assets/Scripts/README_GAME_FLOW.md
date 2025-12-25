# Hướng Dẫn Game Flow: Menu > Pick Level > Play > Die > DeathUI > Menu

## Tổng Quan

Game flow được thiết kế như sau:
1. **Menu** → Main menu với button Play và Quit
2. **Pick Level** → Level selection screen để chọn level
3. **Play** → Load level scene và bắt đầu chơi
4. **Die** → Player chết (do trap, enemy, v.v.)
5. **DeathUI** → Hiển thị death screen với options
6. **Menu** → Quay về main menu từ death screen

## Các Script Quan Trọng

### 1. MenuManager.cs
- **Vị trí**: `Assets/Scripts/MenuManager.cs`
- **Chức năng**: Quản lý main menu scene
- **Cách sử dụng**:
  - Thêm script này vào một GameObject trong MainMenu scene
  - Gán các button vào inspector:
    - `playButton`: Button để vào level selection
    - `quitButton`: Button để quit game
  - Hoặc script sẽ tự động tìm button có tên chứa "Play"/"Start" và "Quit"/"Exit"
  - Thiết lập `levelSelectSceneName` (mặc định: "LevelSelect")

### 2. LevelSelectManager.cs
- **Vị trí**: `Assets/Scripts/LevelSelectManager.cs`
- **Chức năng**: Quản lý level selection screen
- **Cách sử dụng**:
  - Thêm script này vào một GameObject trong LevelSelect scene
  - Trong Inspector, thêm các level vào list `levels`:
    - `levelName`: Tên hiển thị (ví dụ: "Level 1")
    - `sceneName`: Tên scene của level (ví dụ: "Dungeon1")
    - `levelButton`: Button để chọn level (optional - có thể tự tìm)
    - `isUnlocked`: Level có được unlock không
  - Gán `backButton` hoặc script sẽ tự tìm button có tên chứa "Back"/"Menu"
  - Thiết lập `mainMenuSceneName` (mặc định: "MainMenu")

### 3. DeathManager.cs
- **Vị trí**: `Assets/Scripts/DeathManager.cs`
- **Chức năng**: Quản lý death screen (đã có sẵn)
- **Cách sử dụng**:
  - Thêm script này vào một GameObject trong game scene (nên dùng DontDestroyOnLoad)
  - Gán `deathScreenUI` GameObject (Canvas hoặc Panel chứa death screen UI)
  - Thiết lập `mainMenuSceneName` (mặc định: "MainMenu")
  - Trong death screen UI, thêm button và gọi `DeathManager.Instance.ReturnToMainMenu()` khi click

### 4. TrapController.cs
- **Vị trí**: `Assets/Scripts/TrapPeakController.cs` (tên class là `TrapController`)
- **Chức năng**: Gây damage cho player khi đạp trap
- **Cách sử dụng**:
  - Thêm script này vào trap GameObject
  - Đảm bảo trap có Collider2D (set Is Trigger = true nếu muốn trigger behavior)
  - Thiết lập `damage` (mặc định: 100)
  - Thiết lập `singleUse` = true nếu trap chỉ hoạt động 1 lần
  - Trap sẽ tự động tìm `PlayerHealth` component và gọi `TakeDamage()`

## Setup Scene

### 1. MainMenu Scene
1. Tạo scene mới tên "MainMenu"
2. Tạo Canvas cho UI
3. Thêm button "Play" và "Quit"
4. Tạo GameObject trống tên "MenuManager"
5. Thêm component `MenuManager` vào GameObject đó
6. Gán các button vào inspector (hoặc để script tự tìm)

### 2. LevelSelect Scene
1. Tạo scene mới tên "LevelSelect"
2. Tạo Canvas cho UI
3. Thêm các button cho từng level (ví dụ: "Level1Button", "Level2Button")
4. Thêm button "Back" để quay về menu
5. Tạo GameObject trống tên "LevelSelectManager"
6. Thêm component `LevelSelectManager` vào GameObject đó
7. Trong Inspector, thêm các level vào list:
   - Level 1: sceneName = "Dungeon1" (hoặc tên scene level của bạn)
   - Level 2: sceneName = "Dungeon2" (hoặc tên scene level của bạn)
   - v.v.
8. Gán các button vào inspector (hoặc để script tự tìm)

### 3. Game Scene (Level Scene)
1. Đảm bảo có `DeathManager` trong scene (hoặc tạo GameObject với `DeathManager` component)
2. Tạo death screen UI (Canvas hoặc Panel)
3. Thêm button "Return to Menu" trong death screen UI
4. Trong button, gán method: `DeathManager.Instance.ReturnToMainMenu()`
   - Hoặc tạo script đơn giản:
   ```csharp
   public void OnReturnToMenuClicked()
   {
       if (DeathManager.Instance != null)
       {
           DeathManager.Instance.ReturnToMainMenu();
       }
   }
   ```
5. Gán death screen UI GameObject vào `DeathManager.deathScreenUI`

## Flow Hoạt Động

1. **Menu → Level Select**:
   - Player click "Play" button
   - `MenuManager.OnPlayButtonClicked()` được gọi
   - Load scene "LevelSelect"

2. **Level Select → Play**:
   - Player click level button
   - `LevelSelectManager.OnLevelButtonClicked()` được gọi
   - Load scene level tương ứng

3. **Play → Die**:
   - Player đạp trap → `TrapController` gây damage
   - Player health = 0 → `PlayerHealth.Die()` được gọi
   - `PlayerDeath.HandleDeath()` được gọi
   - Death animation chạy

4. **Die → DeathUI**:
   - Sau khi death animation xong
   - `DeathManager.ShowDeathScreen()` được gọi
   - Death screen UI hiển thị, game pause

5. **DeathUI → Menu**:
   - Player click "Return to Menu" button
   - `DeathManager.ReturnToMainMenu()` được gọi
   - Load scene "MainMenu"
   - Flow quay lại bước 1

## Lưu Ý

- Đảm bảo tất cả scene names đúng với tên scene trong Build Settings
- `DeathManager` nên dùng DontDestroyOnLoad để tồn tại qua các scene
- Khi load scene mới, `DeathManager` sẽ tự động reset death screen state
- Trap cần có Collider2D và đảm bảo player có `PlayerHealth` component
- Player cần có `PlayerDeath` component để xử lý death sequence

## Testing

1. Test trap damage: Đảm bảo trap gây damage khi player đạp
2. Test death flow: Đảm bảo player chết → death screen hiện → có thể quay về menu
3. Test menu flow: Đảm bảo có thể navigate giữa menu, level select, và game scenes
4. Test scene transitions: Đảm bảo không có lỗi khi chuyển scene

