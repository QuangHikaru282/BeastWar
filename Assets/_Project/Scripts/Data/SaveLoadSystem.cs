using System.IO;
using UnityEngine;

/// <summary>
/// Hệ thống lưu trữ dữ liệu chính thành file JSON (thay cho PlayerPrefs).
/// </summary>
public static class SaveLoadSystem
{
    // Tên file lưu mặc định
    private const string SAVE_FILE_NAME = "BeastBallSave.json";

    // Đường dẫn đầy đủ tới file (VD: C:/Users/.../AppData/LocalLow/CompanyName/ProductName/BeastBallSave.json)
    public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    /// <summary>
    /// Lưu object bất kỳ xuống file JSON.
    /// </summary>
    public static void SaveData<T>(T data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true); // true để format JSON đẹp, dễ debug
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"[SaveLoadSystem] Đã lưu dữ liệu thành công tại: {SaveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadSystem] Lỗi khi lưu file: {e.Message}");
        }
    }

    /// <summary>
    /// Đọc file JSON và trả về object. Trả về null nếu file không tồn tại.
    /// </summary>
    public static T LoadData<T>() where T : class
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.LogWarning("[SaveLoadSystem] Không tìm thấy file save, sẽ tạo mới.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            T data = JsonUtility.FromJson<T>(json);
            Debug.Log($"[SaveLoadSystem] Đã tải dữ liệu thành công từ: {SaveFilePath}");
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadSystem] Lỗi khi tải file: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Xóa file lưu hiện tại.
    /// </summary>
    public static void DeleteSave()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log("[SaveLoadSystem] Đã xóa file save thành công!");
        }
    }
}
