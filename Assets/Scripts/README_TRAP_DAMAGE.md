# Hướng Dẫn: Trap Gây Damage Cho Player

## Tổng Quan

Script `TrapController` (trong file `TrapPeakController.cs`) đã được implement để tự động gây damage cho player khi player đạp trap.

## Cách Sử Dụng

### 1. Setup Trap GameObject

1. Chọn trap GameObject trong scene
2. Thêm component `TrapController` (script `TrapPeakController.cs`)
3. Đảm bảo trap có **Collider2D**:
   - Nếu muốn trap trigger khi player đi qua: set **Is Trigger = true**
   - Nếu muốn trap là solid collision: set **Is Trigger = false**

### 2. Cấu Hình TrapController

Trong Inspector, bạn có thể thiết lập:

- **Damage** (mặc định: 100): Lượng damage trap gây ra cho player
- **Single Use** (mặc định: false): 
  - `true`: Trap chỉ hoạt động 1 lần, sau đó tự disable
  - `false`: Trap có thể trigger nhiều lần
- **Debug** (mặc định: false): Bật log để debug

### 3. Cách Hoạt Động

1. Khi player (có `PlayerHealth` component) đạp trap:
   - `OnTriggerEnter2D()` hoặc `OnCollisionEnter2D()` được gọi
   - Script tìm `PlayerHealth` component trên player (hoặc parent)
   - Gọi `playerHealth.TakeDamage(damage)`
   - Player mất máu

2. Nếu `singleUse = true`:
   - Sau khi trigger, trap sẽ disable collider
   - Trap không thể trigger lại

### 4. Yêu Cầu

- **Trap GameObject**: Phải có `Collider2D`
- **Player GameObject**: Phải có `PlayerHealth` component
- **Player Tag**: Không bắt buộc, script tự tìm `PlayerHealth` component

### 5. Ví Dụ Setup

```
Trap GameObject
├── SpriteRenderer (hiển thị trap)
├── Collider2D (Is Trigger = true)
└── TrapController
    ├── Damage: 100
    ├── Single Use: false
    └── Debug: false
```

### 6. Testing

1. Play game
2. Điều khiển player đạp trap
3. Kiểm tra:
   - Player mất máu (xem HealthBarUI)
   - Console log (nếu Debug = true)
   - Nếu health = 0, player sẽ chết và hiện death screen

## Lưu Ý

- Damage được tính bằng số thực (float), không phải integer
- Nếu player đã chết (`isDead = true`), trap sẽ không gây damage nữa
- Script tự động tìm `PlayerHealth` trên player hoặc parent, nên không cần tag đặc biệt
- Nếu trap không hoạt động, kiểm tra:
  - Trap có Collider2D không?
  - Player có `PlayerHealth` component không?
  - Collider2D có đúng layer để detect collision không?

