using System;
using System.Collections.Generic;
using UnityEngine;

public enum SinglePortraitDialogueSpeaker
{
    NPC,
    Player
}

[Serializable]
public class SinglePortraitDialogueLine
{
    [Tooltip("Chọn NPC hoặc Player là người nói câu này.")]
    public SinglePortraitDialogueSpeaker speaker = SinglePortraitDialogueSpeaker.NPC;

    [TextArea(2, 5)]
    [Tooltip("Nhập nội dung câu thoại.")]
    public string text;
}

[RequireComponent(typeof(Collider2D))]
public class SinglePortraitNPCConversation : MonoBehaviour
{
    [Header("THÔNG TIN NPC")]
    [SerializeField] private string npcName = "Tên NPC";
    [SerializeField] private Sprite npcPortrait;

    [Header("CÁC CÂU HỘI THOẠI")]
    [SerializeField]
    private List<SinglePortraitDialogueLine> lines =
        new List<SinglePortraitDialogueLine>();

    public string NPCName => npcName;
    public Sprite NPCPortrait => npcPortrait;
    public IReadOnlyList<SinglePortraitDialogueLine> Lines => lines;

    private void Reset()
    {
        Collider2D interactionArea = GetComponent<Collider2D>();
        interactionArea.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (SinglePortraitDialogueManager.Instance != null)
            SinglePortraitDialogueManager.Instance.RegisterNearbyNPC(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (SinglePortraitDialogueManager.Instance != null)
            SinglePortraitDialogueManager.Instance.UnregisterNearbyNPC(this);
    }

    private void OnDisable()
    {
        if (SinglePortraitDialogueManager.Instance != null)
            SinglePortraitDialogueManager.Instance.UnregisterNearbyNPC(this);
    }
}