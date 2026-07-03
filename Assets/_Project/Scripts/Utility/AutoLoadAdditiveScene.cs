using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tự động load thêm một Scene (Additive) khi Scene chứa script này được bật lên.
/// Dùng để load các scene gộp (như Player + Map) sau khi chuyển từ Battle về.
/// </summary>
public class AutoLoadAdditiveScene : MonoBehaviour
{
    [Header("Tên Scene muốn tải thêm (ví dụ: HUNG)")]
    public string sceneToLoadAdditive;

    private void Start()
    {
        if (!string.IsNullOrEmpty(sceneToLoadAdditive))
        {
            // Kiểm tra xem scene này đã được load chưa để tránh load đúp
            bool isLoaded = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == sceneToLoadAdditive)
                {
                    isLoaded = true;
                    break;
                }
            }

            // Nếu chưa có thì load thêm vào
            if (!isLoaded)
            {
                SceneManager.LoadScene(sceneToLoadAdditive, LoadSceneMode.Additive);
            }
        }

        // KHÔI PHỤC VỊ TRÍ NGƯỜI CHƠI SAU TRẬN ĐÁNH (Dùng Coroutine để tránh bị ghi đè)
        StartCoroutine(RestorePositionDelay());
    }

    private System.Collections.IEnumerator RestorePositionDelay()
    {
        yield return new WaitForEndOfFrame(); // Đợi đến cuối frame để các script khác (như EntranceScene) chạy xong
        yield return new WaitForSeconds(0.1f); // Thêm 1 chút thời gian chắc chắn

        BattleTransferData battleData = Resources.Load<BattleTransferData>("BattleTransferData");
        if (battleData != null && battleData.returnToLastPosition)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = battleData.lastPlayerPosition;
                battleData.returnToLastPosition = false; // Đã dịch chuyển xong, reset cờ
                Debug.Log($"[AutoLoadAdditiveScene] Đã khôi phục vị trí người chơi về: {battleData.lastPlayerPosition}");
            }
        }
    }
}
