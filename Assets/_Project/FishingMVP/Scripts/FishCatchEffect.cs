using UnityEngine;
using System.Collections;

public class FishCatchEffect : MonoBehaviour
{
    public Sprite storedFishSprite;
    public string storedFishName;

    // Hàm tĩnh này có thể được gọi từ bất kỳ đâu, không cần kéo thả script vào Unity
    public static void Show(Sprite fishSprite, Vector3 startPosition, string fishName)
    {
        // 1. Tạo một GameObject mới
        GameObject effectObj = new GameObject("FishCatchEffect");
        
        // 2. Định vị trí xuất hiện (nằm trên đầu nhân vật 1 chút)
        effectObj.transform.position = startPosition + Vector3.up * 1f;

        // 3. Gắn Component SpriteRenderer để hiển thị ảnh con cá
        SpriteRenderer sr = effectObj.AddComponent<SpriteRenderer>();
        sr.sprite = fishSprite;
        sr.sortingOrder = 500; // Đảm bảo luôn nằm trên cùng, không bị che khuất
        
        // Phóng to con cá lên một chút cho dễ nhìn
        effectObj.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

        // 4. Kích hoạt hiệu ứng bay nhảy
        FishCatchEffect effect = effectObj.AddComponent<FishCatchEffect>();
        effect.storedFishSprite = fishSprite;
        effect.storedFishName = fishName;
    }

    private IEnumerator Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 startPos = transform.position;
        
        float duration = 2.0f; // Thời gian sống: 2 giây
        float elapsed = 0f;
        float bounceHeight = 1.0f; // Độ cao nảy lên: 1.0 mét

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Dùng đường cong Sin để làm cá nảy nhẹ lên rồi rớt xuống
            float currentHeight = Mathf.Sin(t * Mathf.PI) * bounceHeight;
            transform.position = startPos + new Vector3(0, currentHeight, 0);


            // Nửa sau của thời gian (từ giây thứ 1 đến giây thứ 2): Bắt đầu Fade out mờ dần
            if (t > 0.5f)
            {
                float alpha = 1f - ((t - 0.5f) * 2f); // Chuyển đổi t thành alpha (1 -> 0)
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }

            yield return null;
        }

        // 5. Tích hợp Fake Inventory (Giả vờ nhét cá vào balo)
        Kinnly.PlayerInventory inventory = FindObjectOfType<Kinnly.PlayerInventory>();
        if (inventory != null)
        {
            // Tạo một ScriptableObject Item giả ngay trên RAM
            Kinnly.Item fakeFishItem = ScriptableObject.CreateInstance<Kinnly.Item>();
            fakeFishItem.name = storedFishName;
            fakeFishItem.image = storedFishSprite;
            fakeFishItem.isStackable = true; // Cho phép cộng dồn

            // Gọi hệ thống Kinnly để nhét vào UI
            inventory.AddItem(fakeFishItem, 1);
        }

        // Hiệu ứng hoàn tất -> Xóa bỏ khỏi Scene để giải phóng RAM
        Destroy(gameObject);
    }
}
