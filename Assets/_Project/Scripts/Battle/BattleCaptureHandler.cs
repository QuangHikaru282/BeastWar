using System.Collections;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Xử lý logic quăng bóng và hoạt ảnh thu phục chuẩn Pokémon:
/// 1. Quả bóng bay hình vòng cung từ phía người chơi tới vị trí quái địch.
/// 2. Quái địch nhấp nháy, thu nhỏ và biến mất vào trong bóng.
/// 3. Quả bóng rơi xuống đất và thực hiện hoạt ảnh lắc lư (Wiggle) từ 1 đến 3 lần.
/// 4. Nếu thành công: Bóng đứng im, phát hiệu ứng thu phục thành công và thêm quái vào túi.
/// 5. Nếu thất bại: Bóng rung dữ dội và vỡ ra, quái thoát ra ngoài và hồi phục 20% lượng máu đã mất!
/// </summary>
public class BattleCaptureHandler : MonoBehaviour
{
    public static BattleCaptureHandler Instance { get; private set; }

    [Header("Sprite Quả Bóng")]
    [SerializeField] private Sprite pokeballSprite;

    [Header("UI Text Lượt ném (Tùy chọn)")]
    [SerializeField] private UnityEngine.UI.Text pokeballCountText;
    [SerializeField] private TMPro.TextMeshProUGUI pokeballCountTextTMP;

    private int throwsLeft = 3;
    private bool captureSessionActive = false;
    private bool isThrowingInProgress = false;
    private BeastUnit targetEnemy;

    public System.Action<bool> OnCaptureSessionEnd; // true = đã bắt, false = trượt

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshPokeballUI();
    }

    public void RefreshPokeballUI()
    {
        string display = captureSessionActive ? $"{throwsLeft}/3" : "∞";
        if (pokeballCountText != null) pokeballCountText.text = display;
        if (pokeballCountTextTMP != null) pokeballCountTextTMP.text = display;
    }

    /// <summary>
    /// Được gọi khi người chơi bấm vào ô Pokéball trong Balo
    /// </summary>
    public void OnPokeballButtonPressed()
    {
        Debug.Log("<color=cyan>[Capture] OnPokeballButtonPressed() đã được bấm!</color>");
        if (isThrowingInProgress)
        {
            Debug.LogWarning("[Capture] Quá trình ném bóng đang diễn ra, vui lòng chờ!");
            return;
        }

        // Tìm BattleManager dù Instance chưa kịp gán
        BattleManager bm = BattleManager.Instance ?? FindFirstObjectByType<BattleManager>();
        if (bm == null)
        {
            Debug.LogError("[Capture] Không tìm thấy BattleManager trong Scene!");
            return;
        }

        var enemy = bm.GetActiveEnemyUnit();
        if (enemy == null || !enemy.IsAlive)
        {
            Debug.LogWarning("[Capture] Không có quái địch hợp lệ trên sân!");
            return;
        }

        // Chặn bắt thú của Trainer hoặc Gym Leader
        BattleTransferData bData = Resources.Load<BattleTransferData>("BattleTransferData");
        if (bData != null && (bData.isTrainerBattle || bData.isGymLeaderBattle))
        {
            Debug.LogWarning("[Capture] Không thể bắt thú của Trainer!");
            if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
            {
                BattleUIManager.Instance.ActionPanel.SetGuide("Không thể bắt trộm thú cưng của Trainer khác!");
            }
            return;
        }

        // Trừ 1 bóng trong kho đồ (nếu có)
        if (BattleItemHandler.Instance != null)
        {
            BattleItemHandler.Instance.UseItemByName("Pokeball");
        }

        StartCoroutine(CaptureSequenceRoutine(bm, enemy));
    }

    private Sprite GetPokeballSprite()
    {
        if (pokeballSprite != null) return pokeballSprite;

        var pokeballItem = Resources.Load<Kinnly.Item>("Item/Pokeball");
        if (pokeballItem != null && pokeballItem.image != null)
        {
            pokeballSprite = pokeballItem.image;
            return pokeballSprite;
        }

        var allItems = Resources.LoadAll<Kinnly.Item>("");
        foreach (var item in allItems)
        {
            if (item != null && item.name.ToLower().Contains("pokeball") && item.image != null)
            {
                pokeballSprite = item.image;
                return pokeballSprite;
            }
        }

        // Tạo Texture Pokéball nét nếu không tìm thấy file sprite
        Texture2D tex = new Texture2D(32, 32);
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
                if (dist > 15f) tex.SetPixel(x, y, Color.clear);
                else if (dist > 13f) tex.SetPixel(x, y, Color.black);
                else if (Mathf.Abs(y - 15.5f) < 2f) tex.SetPixel(x, y, (dist < 5f) ? (dist < 3f ? Color.white : Color.black) : Color.black);
                else if (y > 15.5f) tex.SetPixel(x, y, Color.red);
                else tex.SetPixel(x, y, Color.white);
            }
        }
        tex.Apply();
        pokeballSprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32);
        return pokeballSprite;
    }

    /// <summary>
    /// Chuỗi Hoạt ảnh Quăng bóng & Bắt thú chuẩn Pokémon
    /// </summary>
    private IEnumerator CaptureSequenceRoutine(BattleManager bm, BeastUnit enemy)
    {
        isThrowingInProgress = true;
        targetEnemy = enemy;

        if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
        {
            BattleUIManager.Instance.ActionPanel.SetGuide($"Đang ném Pokéball vào {enemy.Data.baseBeast.beastName}...");
        }

        // 1. Tạo GameObject quả bóng trên sân
        GameObject ballObj = new GameObject("ThrownPokeball");
        SpriteRenderer sr = ballObj.AddComponent<SpriteRenderer>();
        sr.sprite = GetPokeballSprite();
        sr.sortingOrder = 1000; // Luôn hiển thị trên cùng

        // Vị trí bắt đầu ném: Phía người chơi (bục trái)
        Vector3 startPos = new Vector3(-3.5f, -1.8f, -1f);
        if (bm != null)
        {
            var playerUnit = bm.GetActivePlayerUnit();
            if (playerUnit != null)
            {
                startPos = playerUnit.transform.position + Vector3.up * 0.4f + Vector3.back * 1f;
            }
        }
        ballObj.transform.position = startPos;
        ballObj.transform.localScale = Vector3.one * 1.5f;

        // Vị trí quái địch (bục phải)
        Vector3 enemyPos = enemy.transform.position;
        Vector3 targetHitPos = enemyPos + Vector3.up * 0.4f + Vector3.back * 1f;

        Debug.Log($"<color=green>[Capture] Ném bóng từ {startPos} tới quái tại {targetHitPos}...</color>");

        // 2. Hoạt ảnh ném bóng bay hình vòng cung (Parabolic Jump) + Xoay tròn
        float flightDuration = 0.7f;
        ballObj.transform.DOJump(targetHitPos, 2.5f, 1, flightDuration).SetEase(Ease.OutQuad);
        ballObj.transform.DORotate(new Vector3(0, 0, -720f), flightDuration, RotateMode.FastBeyond360);

        yield return new WaitForSeconds(flightDuration);

        // 3. Bóng chạm vào quái -> Quái nhấp nháy đỏ/trắng và thu nhỏ hút vào trong bóng
        SpriteRenderer enemySR = enemy.GetComponentInChildren<SpriteRenderer>();
        if (enemySR != null)
        {
            enemySR.DOColor(new Color(1f, 0.4f, 0.4f, 1f), 0.15f).SetLoops(2, LoopType.Yoyo);
        }

        enemy.transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack);
        
        // Quả bóng rơi xuống đất ngay tại vị trí bục của quái
        Vector3 groundPos = enemyPos + Vector3.down * 0.3f + Vector3.back * 1f;
        ballObj.transform.DOJump(groundPos, 0.5f, 1, 0.35f);

        yield return new WaitForSeconds(0.4f);
        enemy.gameObject.SetActive(false); // Ẩn quái tạm thời

        // 4. Tính toán tỉ lệ bắt và số lần lắc bóng
        float hpRatio = (float)enemy.CurrentHP / enemy.Data.MaxHP;
        float hpModifier = hpRatio < 0.20f ? 0.90f
                         : hpRatio < 0.40f ? 0.70f
                         : hpRatio < 0.70f ? 0.45f
                         : 0.25f;
        float baseRate = (enemy.Data != null && enemy.Data.baseBeast != null) ? enemy.Data.baseBeast.captureRate : 0.5f;
        float finalRate = Mathf.Clamp01(baseRate * hpModifier + 0.15f);

        float roll = Random.value;
        bool isSuccess = roll <= finalRate;

        // Xác định số lần lắc bóng (1 đến 3 lần)
        int wigglesCount = isSuccess ? 3 : (roll < finalRate + 0.35f ? 2 : 1);
        Debug.Log($"[Capture] Tỉ lệ bắt: {finalRate:P0} (Roll: {roll:F2}) -> Lắc {wigglesCount} lần. Kết quả: {(isSuccess ? "THÀNH CÔNG" : "THẤT BẠI")}");

        // 5. Hoạt ảnh lắc lư bóng (Wiggle Animation kiểu Pokémon)
        for (int i = 0; i < wigglesCount; i++)
        {
            yield return new WaitForSeconds(0.45f);

            // Lắc sang trái
            ballObj.transform.DORotate(new Vector3(0, 0, 25f), 0.12f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(0.12f);

            // Lắc sang phải
            ballObj.transform.DORotate(new Vector3(0, 0, -25f), 0.22f).SetEase(Ease.InOutQuad);
            yield return new WaitForSeconds(0.22f);

            // Về giữa
            ballObj.transform.DORotate(Vector3.zero, 0.12f).SetEase(Ease.InQuad);
            yield return new WaitForSeconds(0.12f);

            // Nhảy tưng nhẹ
            ballObj.transform.DOJump(groundPos, 0.12f, 1, 0.12f);
            yield return new WaitForSeconds(0.15f);
        }

        yield return new WaitForSeconds(0.5f);

        // 6. XỬ LÝ KẾT QUẢ
        if (isSuccess)
        {
            // === THU PHỤC THÀNH CÔNG ===
            // Bóng đứng im, chớp sáng xác nhận
            sr.DOColor(Color.gray, 0.15f).SetLoops(2, LoopType.Yoyo);

            if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
            {
                BattleUIManager.Instance.ActionPanel.SetGuide($"Tuyệt vời! Đã thu phục thành công {enemy.Data.baseBeast.beastName}!");
            }

            // Thêm vào kho thú của người chơi
            PlayerData pd = global::QuestManager.Instance != null && global::QuestManager.Instance.playerData != null
                ? global::QuestManager.Instance.playerData
                : Resources.Load<PlayerData>("PlayerData");

            if (pd != null)
            {
                pd.AddBeast(enemy.Data);
                // Nếu đội hình còn chỗ trống (< 3), thêm vào đội hình luôn
                if (pd.currentFormation != null && pd.currentFormation.Count < PlayerData.MaxFormationSize)
                {
                    pd.currentFormation.Add(enemy.Data);
                }
                pd.Save();
            }

            yield return new WaitForSeconds(1.5f);
            Destroy(ballObj);

            bm?.OnCaptureBeastSuccess();

            OnCaptureSessionEnd?.Invoke(true);
        }
        else
        {
            // === THOÁT KHỎI BÓNG (THẤT BẠI) ===
            // Bóng rung dữ dội
            ballObj.transform.DOShakePosition(0.35f, 0.25f, 25);
            ballObj.transform.DOShakeRotation(0.35f, 35f, 25);
            yield return new WaitForSeconds(0.35f);

            // Bóng biến mất
            Destroy(ballObj);

            // Quái xuất hiện trở lại
            enemy.gameObject.SetActive(true);
            enemy.transform.localScale = Vector3.zero;
            enemy.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            if (enemySR != null) enemySR.color = Color.white;

            // Hồi lại 20% lượng máu đã mất
            int missingHP = enemy.Data.MaxHP - enemy.CurrentHP;
            int healAmount = Mathf.Max(1, Mathf.RoundToInt(missingHP * 0.20f));
            int newHP = Mathf.Min(enemy.Data.MaxHP, enemy.CurrentHP + healAmount);
            enemy.Data.currentHP = newHP;

            // Cập nhật lại UI máu của quái
            var hpBar = enemy.GetComponentInChildren<HPBarUI>();
            if (hpBar != null) hpBar.UpdateHP(newHP);

            DamagePopup.CreateText(enemy.transform.position + Vector3.up * 0.8f, $"+{healAmount} HP", Color.green, 2.5f);

            if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
            {
                BattleUIManager.Instance.ActionPanel.SetGuide($"{enemy.Data.baseBeast.beastName} đã phá bóng thoát ra và hồi phục {healAmount} HP!");
            }

            yield return new WaitForSeconds(1.5f);

            // Trả lại lượt cho người chơi tiếp tục chọn chiêu
            if (BattleUIManager.Instance != null && BattleUIManager.Instance.ActionPanel != null)
            {
                BattleUIManager.Instance.ActionPanel.SetGuide($"Lượt của bạn! Hãy chọn kĩ năng hoặc vật phẩm.");
            }

            if (BattleKeyboardNavigationUI.Instance != null)
            {
                BattleKeyboardNavigationUI.Instance.FocusFirstSkill();
            }

            OnCaptureSessionEnd?.Invoke(false);
        }

        isThrowingInProgress = false;
        RefreshPokeballUI();
    }

    public void CancelSession()
    {
        captureSessionActive = false;
        isThrowingInProgress = false;
        RefreshPokeballUI();
    }

    public bool IsCaptureSessionActive => captureSessionActive;
    public int ThrowsLeft => throwsLeft;
}
