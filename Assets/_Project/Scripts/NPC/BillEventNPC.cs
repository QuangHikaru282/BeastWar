using UnityEngine;
using Kinnly;
using System.Collections;

/// <summary>
/// Quản lý sự kiện Nhà Bill:
/// 1. Ban đầu Bill bị kẹt trong hình hài Beast, nhờ người chơi bấm máy.
/// 2. Sau khi kích hoạt máy giải cứu, Bill biến lại thành Người.
/// 3. Bill cảm ơn và tặng người chơi Phiếu Giảm Giá Xe Đạp (Bike Voucher).
/// </summary>
public class BillEventNPC : MonoBehaviour, IInteractable
{
    [Header("Dữ liệu người chơi")]
    [Tooltip("Kéo PlayerData asset vào đây")]
    [SerializeField] private PlayerData playerData;

    [Header("Hình ảnh Bill")]
    [Tooltip("GameObject / SpriteRenderer đại diện cho Bill dạng Beast")]
    [SerializeField] private GameObject billBeastForm;

    [Tooltip("GameObject / SpriteRenderer đại diện cho Bill dạng Người")]
    [SerializeField] private GameObject billHumanForm;

    [Header("Hiệu ứng chuyển đổi (Tùy chọn)")]
    [Tooltip("Hiệu ứng hạt / khói khi biến hình")]
    [SerializeField] private ParticleSystem transformVFX;

    [Tooltip("Âm thanh khi kích hoạt máy biến hình")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip transformSFX;

    [Header("Câu thoại")]
    [TextArea(2, 4)]
    [SerializeField] private string[] trappedDialogue = new string[]
    {
        "Cứu tôi với! Đừng sợ, tôi là Bill - nhà nghiên cứu đây!",
        "Tôi vừa làm một thí nghiệm kết hợp và vô tình bị kẹt trong thân xác con Beast này!",
        "Xin cậu hãy đến cỗ máy bên cạnh và gạt cần điều khiển để giải cứu tôi!"
    };

    [TextArea(2, 4)]
    [SerializeField] private string[] rescuedDialogue = new string[]
    {
        "Ôi trời đất ơi! Cuối cùng tôi cũng trở lại làm người rồi! Cảm ơn cậu rất nhiều!",
        "Để đền đáp lòng tốt của cậu, xin hãy nhận lấy 'Phiếu Giảm Giá Xe Đạp' này!",
        "Cầm phiếu này đến Cửa Hàng Xe Đạp ở thị trấn, họ sẽ bán cho cậu một chiếc xe đạp với giá chỉ 1 Đồng!"
    };

    [TextArea(2, 4)]
    [SerializeField] private string[] completedDialogue = new string[]
    {
        "Chiếc xe đạp sẽ giúp cậu di chuyển nhanh hơn gấp nhiều lần trên mọi cung đường đấy.",
        "Chúc cậu luôn kiên định trên hành trình trở thành Nhà Vô Địch cùng người bạn Beast của mình!"
    };

    private bool isRescued = false;

    private void Start()
    {
        // Nếu người chơi đã có phiếu hoặc xe đạp, Bill đã được giải cứu từ trước
        if (playerData != null && (playerData.hasBikeVoucher || playerData.hasBicycle))
        {
            isRescued = true;
        }

        UpdateVisuals();
    }

    public void Interact(PlayerInventory playerInventory)
    {
        if (DialogueManager.Instance == null) return;

        if (!isRescued)
        {
            DialogueManager.Instance.StartDialogue("Bill (Dạng Beast)", trappedDialogue, null);
        }
        else
        {
            if (playerData != null && !playerData.hasBikeVoucher && !playerData.hasBicycle)
            {
                // Trao quà phiếu xe đạp
                DialogueManager.Instance.StartDialogue("Bill", rescuedDialogue, () =>
                {
                    playerData.hasBikeVoucher = true;
                    playerData.Save();
                    Debug.Log("<color=green>[BillEventNPC]</color> Đã nhận Phiếu Giảm Giá Xe Đạp!");
                });
            }
            else
            {
                DialogueManager.Instance.StartDialogue("Bill", completedDialogue, null);
            }
        }
    }

    /// <summary>
    /// Được gọi bởi Machine Switch / Cần gạt máy móc trong phòng Bill khi người chơi tương tác với máy.
    /// </summary>
    public void TriggerRescueMachine()
    {
        if (isRescued) return;
        StartCoroutine(RescueSequenceRoutine());
    }

    private IEnumerator RescueSequenceRoutine()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
        {
            yield return new WaitUntil(() => !DialogueManager.Instance.IsDialogueActive);
        }

        if (audioSource != null && transformSFX != null)
        {
            audioSource.PlayOneShot(transformSFX);
        }

        if (transformVFX != null)
        {
            transformVFX.Play();
        }

        yield return new WaitForSeconds(0.6f);

        isRescued = true;
        UpdateVisuals();

        yield return new WaitForSeconds(0.4f);

        // Tự động mở thoại cảm ơn và tặng phiếu
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue("Bill", rescuedDialogue, () =>
            {
                if (playerData != null)
                {
                    playerData.hasBikeVoucher = true;
                    playerData.Save();
                }
            });
        }
    }

    private void UpdateVisuals()
    {
        if (billBeastForm != null) billBeastForm.SetActive(!isRescued);
        if (billHumanForm != null) billHumanForm.SetActive(isRescued);
    }
}
