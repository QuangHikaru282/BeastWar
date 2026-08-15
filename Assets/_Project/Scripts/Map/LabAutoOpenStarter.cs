using System.Collections;
using UnityEngine;

/// <summary>
/// Đặt script này vào một GameObject trong scene TruongLang.
/// Khi scene load xong, nếu người chơi chưa có Beast khởi đầu,
/// sẽ tự động cho Trưởng Làng thoại rồi bật bảng chọn Pet.
/// </summary>
public class LabAutoOpenStarter : MonoBehaviour
{
    [Header("Kéo thả vào đây")]
    [Tooltip("Kéo PlayerData ScriptableObject vào")]
    [SerializeField] private PlayerData playerData;

    [Tooltip("Kéo GameObject StarterSelectionPanel vào")]
    [SerializeField] private GameObject starterSelectionPanel;

    [Tooltip("Kéo Canvas chứa StarterSelectionPanel vào")]
    [SerializeField] private Canvas starterCanvas;

    [Header("Cấu hình thoại (tuỳ chọn)")]
    [TextArea(2, 4)]
    [SerializeField] private string introDialogue =
        "Làng của chúng ta đang bị quái vật quấy phá. " +
        "Cháu hãy nhận lấy một Pet khởi đầu và giúp ta giải quyết chúng nhé!";

    [Tooltip("Avatar Trưởng Làng (tuỳ chọn)")]
    [SerializeField] private Sprite elderAvatar;

    private void Start()
    {
        StartCoroutine(AutoOpenRoutine());
    }

    private IEnumerator AutoOpenRoutine()
    {
        // Chờ 2 frame để scene load ổn định
        yield return null;
        yield return null;

        // Kiểm tra điều kiện: chưa có Beast
        bool needsStarter = true;

        if (playerData != null &&
            playerData.ownedBeasts != null &&
            playerData.ownedBeasts.Count > 0)
        {
            needsStarter = false;
        }

        if (!needsStarter)
        {
            Debug.Log("[LabAutoOpenStarter] Người chơi đã có Beast, bỏ qua.");
            yield break;
        }

        // Đảm bảo StarterSelectionPanel đang ẩn trước khi thoại
        if (starterSelectionPanel != null)
            starterSelectionPanel.SetActive(false);

        // Chạy dialogue trước nếu có DialogueManager
        if (DialogueManager.Instance != null && !string.IsNullOrEmpty(introDialogue))
        {
            bool dialogueDone = false;
            DialogueManager.Instance.StartDialogue(
                "Trưởng Làng",
                introDialogue,
                () => { dialogueDone = true; },
                elderAvatar
            );

            // Đợi người chơi bấm xong hội thoại
            yield return new WaitUntil(() => dialogueDone);
        }

        // Chờ thêm 1 frame sau khi dialogue đóng
        yield return null;

        // Bật Canvas cha nếu đang bị ẩn
        if (starterCanvas != null && !starterCanvas.gameObject.activeSelf)
        {
            starterCanvas.gameObject.SetActive(true);
        }

        // Bật bảng chọn Pet
        if (starterSelectionPanel != null)
        {
            starterSelectionPanel.SetActive(true);
            starterSelectionPanel.transform.SetAsLastSibling();

            // Đẩy Sort Order lên cao nhất
            Canvas panelCanvas = starterSelectionPanel.GetComponentInParent<Canvas>();
            if (panelCanvas != null) panelCanvas.sortingOrder = 99999;

            Debug.Log("[LabAutoOpenStarter] Đã bật bảng chọn Pet khởi đầu.");
        }
        else
        {
            Debug.LogError("[LabAutoOpenStarter] Chưa gán StarterSelectionPanel!");
        }
    }
}
