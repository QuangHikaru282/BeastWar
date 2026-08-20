# Quy tắc dự án BeastWar

## ❌ TUYỆT ĐỐI KHÔNG làm những điều sau:

### 1. Không viết code tự động (Auto-detection)
- KHÔNG bao giờ dùng `FindObjectsByType`, `FindObjectsOfType`, `FindWithTag` để **tự động tìm kiếm và gán đối tượng** trong game.
- KHÔNG viết code **tự động quét scene**, tự động đăng ký, tự động phân loại đối tượng.
- **Lý do:** Cấu trúc scene của dự án phức tạp và người dùng cần kiểm soát trực tiếp. Code tự động luôn thất bại vì không biết cấu trúc thực tế bên trong.

### 2. Không tự suy đoán tên GameObject, Layer, Tag
- KHÔNG giả định tên GameObject là "Water", "Collision", "Ground" hay bất cứ tên gì mà không hỏi người dùng trước.
- KHÔNG dùng `.name.ToLower().Contains(...)` để nhận diện đối tượng.

### 3. Không thêm logic "tiện ích" không được yêu cầu
- KHÔNG tự ý thêm tính năng auto-register, auto-setup, auto-detect vào script mà không có sự đồng ý của người dùng.

---

## ✅ LUÔN làm theo cách sau:

### 1. Gán thủ công qua Inspector (Manual Reference)
- Mọi reference đều phải là **`[SerializeField]` kéo thả trong Inspector** do người dùng tự gán.
- Ví dụ đúng:
  ```csharp
  [SerializeField] private PolygonCollider2D waterCollider;
  [SerializeField] private Tilemap groundTilemap;
  ```

### 2. Hỏi trước khi làm
- Trước khi viết bất kỳ logic liên quan đến cấu trúc scene, **hỏi người dùng** về tên, tag, layer, cấu trúc Hierarchy thực tế.

### 3. Để người dùng kiểm soát hoàn toàn
- Code chỉ làm đúng chức năng được yêu cầu. Mọi dữ liệu đầu vào phải do người dùng cung cấp thủ công.
