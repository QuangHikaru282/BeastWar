# Báo cáo Phân tích PokemonUnity & Đề xuất Rework BeastBall

Tài liệu này phân tích chi tiết kiến trúc của dự án mã nguồn mở **PokemonUnity** và đề xuất các giải pháp kỹ thuật cụ thể mà chúng ta có thể áp dụng hoặc tham khảo để xây dựng lại hệ thống cốt lõi của **BeastBall** theo đúng GDD.

---

## 1. Cấu trúc và Thành phần chính của PokemonUnity

Sau khi clone dự án về thư mục `PokemonUnity_Ref`, chúng ta có cấu trúc như sau:
* **`veekun-pokedex.sqlite` (25.8 MB):** Cơ sở dữ liệu SQLite cực kỳ đầy đủ. Nó lưu trữ tất cả thông tin về Pokémon, chỉ số cơ bản, moveset, các chiêu thức (Moves), thuộc tính nguyên tố (Types), vật phẩm (Items), v.v.
* **`Project DLLs/`:** Chứa các thư viện lõi biên dịch sẵn (`PokemonUnity.Shared.dll` và `PokemonUnity.Application.dll`). Toàn bộ logic toán học, hệ thống dữ liệu và vòng lặp trận đấu nguyên bản của Pokémon được viết bằng C# thuần tại đây để đảm bảo tốc độ và tính tách biệt.
* **`Pokemon Unity/Assets/Scripts/PokemonEssentials/`:** Lớp cầu nối (Bridge Layer) trong Unity. Lớp này chứa các Component MonoBehavior, hệ thống UI và Coroutines để kéo dữ liệu từ DLLs và SQLite hiển thị lên màn hình game.

---

## 2. Các điểm sáng Kiến trúc có thể áp dụng cho BeastBall

### A. Tách biệt tuyệt đối giữa Logic (Core) và Hiển thị (Unity View)
* **Cách PokemonUnity làm:** 
  * Class `Battler.cs` và `Pokemon.cs` quản lý toàn bộ các biến số trạng thái, phép tính toán sát thương, và kiểm tra tính hợp lệ hoàn toàn độc lập với Unity (không kế thừa `MonoBehaviour`).
  * Khi vào trận, `BattleScene.cs` trong Unity sẽ gọi các `IEnumerator` (Coroutines) để chạy hiệu ứng đồ họa (như nháy đỏ khi mất máu, chạy slider HP bar, lắc Sprite) đồng bộ theo trạng thái logic trả về.
* **Áp dụng cho BeastBall:** 
  * **Rework:** Chúng ta phải tách lớp dữ liệu chiến đấu ra khỏi MonoBehavior. Hãy viết các lớp C# thuần như `BeastState` (quản lý HP hiện tại, MP hiện tại, các Status Effects) và `CombatEngine` (chạy phép tính).
  * Lớp `BattleManager.cs` hiện tại của BeastBall đang quá rác vì nó vừa tính sát thương, vừa chạy DOTween di chuyển Sprite, vừa điều khiển State Machine. Khi rework sang 3v3, cấu trúc cũ chắc chắn sẽ gây lỗi chồng chéo hoạt ảnh. Việc tách biệt Core và View là bắt buộc.

### B. Sử dụng SQLite để quản lý Dữ liệu thay vì lạm dụng ScriptableObject
* **Cách PokemonUnity làm:** 
  * Họ dùng `veekun-pokedex.sqlite`. Mọi chỉ số của hàng trăm Pokémon và hàng nghìn Moves được nạp động qua kết nối SQL (`Mono.Data.Sqlite.dll`).
* **Áp dụng cho BeastBall:**
  * Hiện tại Collaborators của bạn đang tạo từng file ScriptableObject thủ công cho từng Beast (`Bat.asset`, `Cua.asset`) và từng Move (`PhunLua.asset`, `KepCua.asset`) trong thư mục `Resources`. Cách này rất dễ gây lỗi nhập liệu và khó quản lý khi số lượng Beast tăng lên (thiết kế GDD yêu cầu ~22-50 loài Beast).
  * **Giải pháp đề xuất:** Chúng ta không cần chuyển hẳn game sang dùng SQLite nếu dự án BeastBall quy mô nhỏ. Thay vào đó, chúng ta có thể **thiết kế một Tool Editor** trong Unity đọc từ một bảng Excel hoặc SQLite (cấu trúc tương tự Veekun) và **tự động sinh ra (Auto-generate) các file ScriptableObject** chỉ bằng 1 cú click chuột. Việc này sẽ giữ lại lợi thế của ScriptableObject trong Unity nhưng loại bỏ công đoạn nhập tay dễ sai sót.

### C. Thiết lập Đội hình và Lượt đấu 3v3 (Turn Order)
* **Cách PokemonUnity làm:**
  * Trong `BattleScene.cs`, họ quản lý trận đấu thông qua mảng `battlers` trên sân (hỗ trợ cả Single, Double 2v2 và Triple 3v3).
  * Vòng lặp lượt đánh chia làm 2 pha rõ rệt:
    1. **Pha chọn lệnh (Command Phase):** Thu thập lệnh hành động (`Choice`) cho tất cả các Beast còn sống trên sân (từ vị trí 1 đến 3).
    2. **Pha thực thi (Execution Phase):** Sắp xếp danh sách các Beast theo tốc độ **Speed (SPD)** sau khi đã tính toán các yếu tố ảnh hưởng (như bị tê liệt Paralyze giảm 25% SPD). Sau đó chạy vòng lặp thực hiện từng chiêu thức theo thứ tự SPD từ cao xuống thấp.
* **Áp dụng cho BeastBall:**
  * Đây chính xác là những gì GDD BeastBall yêu cầu nhưng code hiện tại đang làm sai.
  * Chúng ta sẽ rework `BattleManager.cs` để quản lý mảng `BeastUnit[] activePlayerTeam = new BeastUnit[3]` và `BeastUnit[] activeEnemyTeam = new BeastUnit[3]`.
  * Thay vì người chơi click tới đâu quái lao lên đánh tới đó, chúng ta sẽ mở một UI chọn lệnh cho cả 3 con trước. Khi bấm "Xác nhận", `BattleManager` sẽ tổng hợp danh sách hành động, sắp xếp theo `BeastUnit.Data.speed` và dùng Coroutine chạy tuần tự các hành động của cả 6 con trên sân.

### D. Hệ thống Trạng thái (Status Effects)
* **Cách PokemonUnity làm:**
  * Mỗi `Battler` có thuộc tính `Status` (Faint, Burn, Poison, Paralysis, Sleep, Freeze) và biến đếm lượt `StatusCount`.
  * Các ảnh hưởng chỉ số được tính toán động tại Property:
    ```csharp
    public int SPD => (Status == Status.PARALYSIS) ? Mathf.RoundToInt(baseSpeed * 0.5f) : baseSpeed;
    ```
  * Cuối mỗi lượt đấu, vòng lặp trận đấu sẽ duyệt qua các quái trên sân và trừ máu trực tiếp nếu dính `Status.BURN` hoặc `Status.POISON`.
* **Áp dụng cho BeastBall:**
  * Chúng a sẽ định nghĩa một enum `StatusCondition { None, Burn, Poison, Paralyze, Slow }` và nhét nó vào trong dữ liệu runtime của thú.
  * Trong hàm tính toán chỉ số Tốc độ hoặc Tấn công của Beast, chúng ta sẽ nhân thêm hệ số phạt nếu dính trạng thái (ví dụ: Paralyze thì nhân 0.75 SPD, Slow thì nhân 0.5 SPD, Burn thì nhân 0.9 ATK).
  * Cuối lượt đấu của BattleManager, thực hiện trừ máu quái nếu dính Burn (5% HP max) hoặc Poison (8% HP max) trước khi bắt đầu lượt mới.

---

## 3. Lộ trình Rework BeastBall sử dụng tài nguyên tham khảo

1. **Bước 1: Rework Lớp Dữ liệu (Data Layer)**
   * Thêm thuộc tính `Element`, `maxMP` và `Level/EXP` vào `BeastData.cs`.
   * Thêm thuộc tính `Element`, `mpCost` và `StatusCondition` vào `MoveData.cs`.
   * Viết logic khắc hệ nguyên tố và công thức tính sát thương chuẩn theo GDD.
2. **Bước 2: Rework Lớp Chiến đấu 3v3 (Battle Loop)**
   * Thay đổi `BattleManager` để spawn đồng thời 3 con Player và 3 con Enemy lên sân.
   * Viết Pha chọn lệnh (Command Phase) hiển thị Menu 5 nút (Attack/Skill/Item/Switch/Escape) cho từng con thú.
   * Viết Pha thực thi (Execution Phase) sắp xếp quái theo Speed và chạy hoạt ảnh tuần tự.
3. **Bước 3: Tích hợp hệ thống Status Effects & Items**
   * Tích hợp cơ chế cắn thuốc/ném bóng từ túi đồ Kinnly trong combat.
   * Áp dụng hiệu ứng Burn, Poison, Paralyze, Slow lên thú và xử lý trừ máu cuối lượt.
