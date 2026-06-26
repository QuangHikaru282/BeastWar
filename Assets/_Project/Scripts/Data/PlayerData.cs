using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject lưu trạng thái người chơi — dùng chung xuyên suốt các Scene.
/// Kéo asset này vào mọi Manager cần đọc/ghi thông tin người chơi.
/// </summary>
[CreateAssetMenu(fileName = "PlayerData", menuName = "BeastBall/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Bộ sưu tập Beast")]
    public List<BeastData> ownedBeasts = new List<BeastData>();

    [Header("Đội hình hiện tại (tối đa 3)")]
    public List<BeastData> currentFormation = new List<BeastData>();

    public const int MaxFormationSize = 3;

    [Header("Giới tính nhân vật")]
    [Tooltip("Male hoặc Female")]
    public string characterGender = "Male";

    [Header("Nhiệm vụ Tân Thủ")]
    // 0: Chưa nhận Pet
    // 1: Đã nhận Pet, cần đi thu phục
    // 2: Đã thu phục thành công (Hoàn thành)
    public int tutorialQuestStage = 0; 

    // ─── TIẾN TRÌNH ẢI ───────────────────────────────────────────────

    [Header("Tiến trình ải")]
    [Tooltip("Ải cao nhất đã mở khoá (mặc định là 1 = ải đầu tiên đã mở)")]
    public int highestUnlockedStage = 1;

    [Header("Tiến trình Arena")]
    [Tooltip("Ải Arena cao nhất đã mở khoá (1-10)")]
    public int arenaHighestUnlockedStage = 1;

    [Header("Map đã mở khóa")]
    public List<string> unlockedMaps = new List<string>();

    [Tooltip("Số vàng hiện có")]
    public int gold = 0;

    /// <summary>Mảng lưu số sao của từng ải (index 0 = ải 1, index 1 = ải 2 ...).
    /// Kích thước 20 (tương ứng 20 ải).</summary>
    [SerializeField] private int[] stageStarsArray = new int[21]; // index 0 bỏ trống, dùng 1-20

    [SerializeField] private int[] arenaStageStarsArray = new int[11]; // index 0 bỏ trống, dùng 1-10

    // ─── Beast Methods ───────────────────────────────────────────────

    /// <summary>Thêm Beast vào bộ sưu tập (sau khi bắt được).</summary>
    public void AddBeast(BeastData beast)
    {
        if (beast == null) return;
        if (!ownedBeasts.Contains(beast))
            ownedBeasts.Add(beast);
        Debug.Log($"[PlayerData] Đã thêm {beast.beastName} vào bộ sưu tập. Tổng: {ownedBeasts.Count}");
        
        // Nếu đang ở giai đoạn 1 (yêu cầu đi bắt Pet) mà bắt được con mới, thì đánh dấu hoàn thành!
        if (tutorialQuestStage == 1)
        {
            tutorialQuestStage = 2;
            Debug.Log("[PlayerData] Nhiệm vụ tân thủ đã hoàn thành!");
        }
    }

    /// <summary>Lưu đội hình hiện tại.</summary>
    public void SetFormation(List<BeastData> formation)
    {
        currentFormation = new List<BeastData>(formation);
    }

    // ─── Stage Methods ───────────────────────────────────────────────

    /// <summary>Lấy số sao của ải stageId (1-based). Trả về 0 nếu chưa qua.</summary>
    public int GetStageStars(int stageId)
    {
        if (stageId <= 0 || stageId >= stageStarsArray.Length) return 0;
        return stageStarsArray[stageId];
    }

    /// <summary>
    /// Ghi kết quả sau khi thắng một ải.
    /// Tự động mở khoá ải tiếp theo và cộng vàng thưởng.
    /// </summary>
    public void SetStageResult(int stageId, int stars, int rewardGold = 0)
    {
        if (stageId <= 0) return;

        // Cộng vàng (chỉ cộng lần đầu thắng ải - khi số sao cũ bằng 0)
        if (stageId < stageStarsArray.Length && stageStarsArray[stageId] == 0)
        {
            gold += rewardGold;
            Debug.Log($"[PlayerData] Nhận {rewardGold} vàng. Tổng: {gold}");
        }

        // Chỉ ghi nếu số sao mới tốt hơn
        if (stageId < stageStarsArray.Length && stars > stageStarsArray[stageId])
        {
            stageStarsArray[stageId] = Mathf.Clamp(stars, 0, 3);
        }

        // Mở ải tiếp theo
        if (stageId >= highestUnlockedStage)
        {
            highestUnlockedStage = stageId + 1;
            Debug.Log($"[PlayerData] Mở ải {highestUnlockedStage}!");
        }

        Save();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    // ─── Arena Methods ───────────────────────────────────────────────

    public int GetArenaStageStars(int stageId)
    {
        if (stageId <= 0 || stageId >= arenaStageStarsArray.Length) return 0;
        return arenaStageStarsArray[stageId];
    }

    public void SetArenaStageResult(int stageId, int stars, int rewardGold = 0)
    {
        if (stageId <= 0) return;

        if (stageId < arenaStageStarsArray.Length && arenaStageStarsArray[stageId] == 0)
        {
            gold += rewardGold;
            Debug.Log($"[PlayerData] Nhận {rewardGold} vàng từ Arena. Tổng: {gold}");
        }

        if (stageId < arenaStageStarsArray.Length && stars > arenaStageStarsArray[stageId])
        {
            arenaStageStarsArray[stageId] = Mathf.Clamp(stars, 0, 3);
        }

        if (stageId >= arenaHighestUnlockedStage)
        {
            arenaHighestUnlockedStage = stageId + 1;
            Debug.Log($"[PlayerData] Mở ải Arena {arenaHighestUnlockedStage}!");
        }

        Save();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public bool IsMapUnlocked(string mapName)
    {
        return unlockedMaps.Contains(mapName);
    }

    public void UnlockMap(string mapName)
    {
        if (!unlockedMaps.Contains(mapName))
        {
            unlockedMaps.Add(mapName);
            Save();
        }
    }

    // ─── Reset ───────────────────────────────────────────────────────

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        ownedBeasts.Clear();
        currentFormation.Clear();
        highestUnlockedStage = 1;
        arenaHighestUnlockedStage = 1;
        unlockedMaps.Clear();
        gold = 0;
        stageStarsArray = new int[21];
        arenaStageStarsArray = new int[11];
        characterGender = "Male";
        tutorialQuestStage = 0;
        Save();
    }

    // ─── Save/Load System ───────────────────────────────────────────────

    [System.Serializable]
    private class SaveData
    {
        public List<string> ownedBeastNames = new List<string>();
        public List<string> formationBeastNames = new List<string>();
        public string characterGender;
        public int tutorialQuestStage;
        public int highestUnlockedStage;
        public int arenaHighestUnlockedStage;
        public int gold;
        public int[] stageStarsArray;
        public int[] arenaStageStarsArray;
        public List<string> unlockedMaps;
    }

    public void Save()
    {
        SaveData data = new SaveData
        {
            characterGender = this.characterGender,
            tutorialQuestStage = this.tutorialQuestStage,
            highestUnlockedStage = this.highestUnlockedStage,
            arenaHighestUnlockedStage = this.arenaHighestUnlockedStage,
            gold = this.gold,
            stageStarsArray = this.stageStarsArray,
            arenaStageStarsArray = this.arenaStageStarsArray,
            unlockedMaps = this.unlockedMaps
        };

        foreach (var b in ownedBeasts) if (b != null) data.ownedBeastNames.Add(b.name);
        foreach (var b in currentFormation) if (b != null) data.formationBeastNames.Add(b.name);

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("PlayerDataSave", json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey("PlayerDataSave")) return;
        string json = PlayerPrefs.GetString("PlayerDataSave");
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null) return;

        this.characterGender = data.characterGender;
        this.tutorialQuestStage = data.tutorialQuestStage;
        this.highestUnlockedStage = data.highestUnlockedStage;
        this.arenaHighestUnlockedStage = data.arenaHighestUnlockedStage > 0 ? data.arenaHighestUnlockedStage : 1;
        this.gold = data.gold;
        if (data.stageStarsArray != null) this.stageStarsArray = data.stageStarsArray;
        if (data.arenaStageStarsArray != null) this.arenaStageStarsArray = data.arenaStageStarsArray;
        if (data.unlockedMaps != null) this.unlockedMaps = data.unlockedMaps;

        // Restore Beasts from Resources
        BeastData[] allBeasts = Resources.LoadAll<BeastData>("");
        Dictionary<string, BeastData> beastDict = new Dictionary<string, BeastData>();
        foreach (var b in allBeasts) beastDict[b.name] = b;

        ownedBeasts.Clear();
        foreach (var bName in data.ownedBeastNames)
        {
            if (beastDict.TryGetValue(bName, out BeastData b)) ownedBeasts.Add(b);
        }

        currentFormation.Clear();
        foreach (var bName in data.formationBeastNames)
        {
            if (beastDict.TryGetValue(bName, out BeastData b)) currentFormation.Add(b);
        }
    }
}
