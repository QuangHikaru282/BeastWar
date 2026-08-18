using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Gắn script này vào Y tá / Bác sĩ thú cưng (Nurse Joy / Healer NPC) trong Bệnh viện / PokeCenter.
/// Khi tương tác (bấm F):
/// 1. Hiện câu thoại chào mừng và hỏi có muốn hồi phục cho thú cưng không.
/// 2. Hồi phục 100% máu (Max HP) cho toàn bộ thú trong Đội hình & Túi Pet.
/// 3. Lưu lại dữ liệu (PlayerData.Save).
/// 4. Hiện câu thoại hoàn tất và chúc lên đường may mắn!
/// </summary>
public class NurseJoyHealerNPC : MonoBehaviour, Kinnly.IInteractable
{
    [Header("Dữ liệu Người chơi")]
    [Tooltip("Kéo file PlayerData trong Resources/Data vào đây")]
    [SerializeField] private PlayerData playerData;

    [Header("Ảnh đại diện NPC (Avatar)")]
    [Tooltip("Kéo Sprite ảnh đại diện Y tá hiển thị trong khung thoại")]
    [SerializeField] private Sprite nurseAvatar;

    [Header("Cấu hình Hội thoại")]
    [Tooltip("Tên hiển thị của NPC trong khung thoại")]
    [SerializeField] private string npcName = "Y Tá Joy";

    [TextArea(2, 4)]
    [SerializeField] private string welcomeDialogue = "Chào mừng bạn đến với Trung Tâm Y Tế Thú Cưng! Hãy để tôi chăm sóc và hồi phục sức khỏe hoàn toàn cho các Pet của bạn nhé!";

    [TextArea(2, 4)]
    [SerializeField] private string healedDialogue = "Tuyệt vời! Tất cả thú cưng của bạn đã được điều trị đầy máu và tràn đầy năng lượng! Chúc bạn có một hành trình may mắn!";

    [Header("Âm thanh / Hiệu ứng (Tùy chọn)")]
    [Tooltip("AudioClip phát chuông hồi phục kiểu PokeCenter (nếu có)")]
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioSource audioSource;

    private bool isHealing = false;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void Interact(Kinnly.PlayerInventory playerInventory)
    {
        if (isHealing) return;

        StartCoroutine(HealRoutine(playerInventory?.gameObject));
    }

    private IEnumerator HealRoutine(GameObject playerObj)
    {
        isHealing = true;

        // 1. Tạm thời dừng di chuyển của người chơi
        PlayerMapController playerCtrl = playerObj != null ? playerObj.GetComponent<PlayerMapController>() : null;
        if (playerCtrl != null) playerCtrl.SetCanMove(false);

        // 2. Hội thoại chào mừng
        bool step1Done = false;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                npcName,
                welcomeDialogue,
                () => { step1Done = true; },
                nurseAvatar
            );
            yield return new WaitUntil(() => step1Done);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 3. Thực hiện hồi 100% máu cho tất cả Pet
        HealAllBeasts();

        // 4. Phát âm thanh hồi máu (nếu có gán)
        if (healSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(healSound);
            yield return new WaitForSeconds(healSound.length > 1f ? 1.5f : healSound.length);
        }
        else
        {
            yield return new WaitForSeconds(0.8f);
        }

        // 5. Hội thoại thông báo đã hồi phục xong
        bool step2Done = false;
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(
                npcName,
                healedDialogue,
                () => { step2Done = true; },
                nurseAvatar
            );
            yield return new WaitUntil(() => step2Done);
        }

        // 6. Mở lại di chuyển cho người chơi
        if (playerCtrl != null) playerCtrl.SetCanMove(true);

        isHealing = false;
    }

    /// <summary>
    /// Hồi phục Max HP cho tất cả Beast trong Formation và OwnedBeasts
    /// </summary>
    public void HealAllBeasts()
    {
        PlayerData pData = playerData != null ? playerData : (global::QuestManager.Instance != null ? global::QuestManager.Instance.playerData : Resources.Load<PlayerData>("PlayerData"));
        if (pData == null) return;

        int healedCount = 0;

        // 1. Hồi máu cho đội hình chiến đấu (Formation)
        if (pData.currentFormation != null)
        {
            foreach (var beast in pData.currentFormation)
            {
                if (beast != null)
                {
                    beast.currentHP = beast.MaxHP;
                    healedCount++;
                }
            }
        }

        // 2. Hồi máu cho toàn bộ kho Pet sở hữu (Owned Beasts)
        if (pData.ownedBeasts != null)
        {
            foreach (var beast in pData.ownedBeasts)
            {
                if (beast != null)
                {
                    beast.currentHP = beast.MaxHP;
                }
            }
        }

        // Lưu lại dữ liệu sau khi hồi máu
        pData.Save();

        Debug.Log($"<color=green>[Healer Center]</color> Đã hồi phục đầy 100% máu cho toàn bộ {healedCount} thú cưng trong đội hình!");
    }
}
