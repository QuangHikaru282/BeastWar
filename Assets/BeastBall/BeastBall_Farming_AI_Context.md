# 🤖 BeastBall Farming System - AI Context & Architecture

**MỤC ĐÍCH FILE NÀY**: Đây là bản ghi nhớ kiến trúc hệ thống (Context Memory) dành cho AI Coding Assistant. Khi AI mở project BeastBall, AI PHẢI đọc file này đầu tiên để hiểu cấu trúc Farming System.

## 1. Tổng Quan (Overview)
Đây là hệ thống 2D Farming được xây dựng chuẩn S.O.L.I.D, hoàn toàn độc lập (decoupled), được tối ưu hóa từ project Happy Harvest để dùng riêng cho BeastBall.
- **Namespace:** `BeastBall.Farming`
- **Thư mục gốc:** `Assets/BeastBall/Scripts/Farming/`

## 2. Cấu Trúc 4 Tầng (Architecture Layers)

### A. Tầng Dữ Liệu (Data Layer - `Scripts/Farming/Data/`)
Sử dụng pattern ScriptableObject Database để tra cứu O(1) bằng String ID.
- `IDatabaseEntry` / `BaseDatabase<T>`: Nền tảng database.
- `Item` (Abstract SO): Lớp cơ sở cho mọi vật dụng. Định nghĩa hàm `CanUse(target)` và `Use(target)`.
- `Crop` (SO): Định nghĩa thông số cây (Tile stages, thời gian lớn, sản phẩm thu hoạch).
- `ItemDatabase` / `CropDatabase`: Nơi lưu trữ SO, phải gọi `Init()` lúc runtime để build Dictionary lookup.
- `GroundData` / `CropData`: Class lưu trạng thái thời gian thực của đất (độ ẩm) và cây (giai đoạn lớn, tg chết khô).

### B. Tầng Quản Lý (Core Managers - `Scripts/Farming/Managers/`)
- **`FarmingTerrainManager` (Bộ não)**: Quản lý 3 Tilemap (Ground, Crop, Water). Dùng Sparse Storage `Dictionary<Vector3Int, Data>`. Vòng lặp `Update()` tự động mô phỏng việc bốc hơi nước, cây lớn lên và cây chết khô.
- **`FarmingInventoryManager`**: Quản lý túi đồ 9 ô độc lập. Hỗ trợ Stacking (`MaxStackSize`) và Equip. Đóng vai trò là cầu nối: gọi `UseEquippedItem(target)` sẽ kích hoạt hàm `Use()` của Item tương ứng.

### C. Tầng Công Cụ (Polymorphic Items - `Scripts/Farming/Items/`)
Mọi tool đều kế thừa `Item` và gọi ngược lại `FarmingTerrainManager`:
- `Hoe.cs`: Gọi `TillAt()` (Cuốc).
- `WaterCan.cs`: Gọi `WaterAt()` (Tưới).
- `SeedBag.cs`: Gọi `PlantAt()` (Gieo hạt - Consumable).
- `Basket.cs`: Gọi `HarvestAt()` và TỰ ĐỘNG nhét sản phẩm vào `FarmingInventoryManager`.
- `Product.cs`: Nông sản, có thể ăn/sử dụng không cần target ô đất.

### D. Tầng Giao Diện (UI Layer - `Scripts/Farming/UI/`)
- `FarmingInventoryUI.cs`: Kết nối Backend Logic và Frontend. Quét mảng `Entries` của InventoryManager mỗi frame/event để vẽ icon, cập nhật số lượng và khung sáng (Highlight) trang bị.

## 3. Hệ Thống Save / Load (Persistence)
- **Vượt rào JsonUtility**: Dùng Struct `TerrainDataSave` với cấu trúc **Parallel Lists** (`Positions[]` và `Datas[]`) vì JSON mặc định của Unity không hỗ trợ Dictionary.
- **String ID Serialization**: ScriptableObjects được save dưới dạng `String ID` (ví dụ "crop_tomato"). Khi load lên, AI phải dùng `CropDatabase.GetFromID()` để ánh xạ lại thành Object.

## 4. Tình Trạng Hiện Tại (Current State)
- Mã nguồn đã hoàn thiện 100% logic cốt lõi.
- AI khi code tính năng mới (ví dụ: Bón phân, Pet tự thu hoạch) PHẢI tuân thủ namespace `BeastBall.Farming` và kế thừa abstract class `Item`. Không sửa trực tiếp vào Manager trừ khi cần tính năng lõi mới.
