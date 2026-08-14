using System;
using System.Collections.Generic;
using UnityEngine;

public enum FarmConversationSpeaker
{
    NPC,
    Player
}

[Serializable]
public class FarmConversationLine
{
    [Tooltip("Chọn nhân vật đang nói câu này.")]
    public FarmConversationSpeaker speaker = FarmConversationSpeaker.NPC;

    [TextArea(2, 5)]
    [Tooltip("Nội dung sẽ hiển thị trong khung hội thoại.")]
    public string text;
}

[RequireComponent(typeof(Collider2D))]
public class FarmNPCConversation : MonoBehaviour
{
    [Header("Thông tin NPC")]
    [SerializeField] private string npcName = "Tên NPC";
    [SerializeField] private Sprite npcPortrait;

    [Header("Nội dung hội thoại")]
    [SerializeField] private List<FarmConversationLine> dialogueLines = new List<FarmConversationLine>();

    public string NPCName => npcName;
    public Sprite NPCPortrait => npcPortrait;
    public IReadOnlyList<FarmConversationLine> DialogueLines => dialogueLines;

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (FarmConversationManager.Instance != null)
            FarmConversationManager.Instance.SetNearbyNPC(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (FarmConversationManager.Instance != null)
            FarmConversationManager.Instance.ClearNearbyNPC(this);
    }

    private void OnDisable()
    {
        if (FarmConversationManager.Instance != null)
            FarmConversationManager.Instance.ClearNearbyNPC(this);
    }
}