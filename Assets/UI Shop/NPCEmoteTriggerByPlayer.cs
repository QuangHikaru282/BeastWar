using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class NPCEmoteData
{
    public GameObject emotePrefab;

    [TextArea(2, 4)]
    public string text;
}

public class NPCEmoteTriggerByPlayer : MonoBehaviour
{
    [Header("Emote List")]
    public NPCEmoteData[] emotes;

    [Header("Emote Settings")]
    public Vector3 emoteOffset = new Vector3(0f, 1.3f, 0f);
    public float emoteLifeTime = 2f;
    public float cooldown = 5f;

    [Header("Text Settings")]
    public Vector3 textOffset = new Vector3(0f, 0.55f, 0f);
    public float textSize = 2.5f;
    public Color textColor = Color.white;

    [Header("Text Frame Settings")]
    public Sprite textFrameSprite;
    public Vector2 framePadding = new Vector2(0.6f, 0.35f);
    public Vector3 frameOffset = Vector3.zero;

    [Header("Render Settings")]
    public string sortingLayerName = "Default";
    public int orderInLayer = 100;

    private Dictionary<Transform, float> npcCooldowns = new Dictionary<Transform, float>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        Transform npc = GetNPCTransform(other);

        if (npc != null)
        {
            TrySpawnEmote(npc);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Transform npc = GetNPCTransform(other);

        if (npc != null)
        {
            TrySpawnEmote(npc);
        }
    }

    private Transform GetNPCTransform(Collider2D other)
    {
        if (other.CompareTag("NPC"))
        {
            return other.transform;
        }

        Transform parent = other.transform.parent;

        while (parent != null)
        {
            if (parent.CompareTag("NPC"))
            {
                return parent;
            }

            parent = parent.parent;
        }

        return null;
    }

    private void TrySpawnEmote(Transform npc)
    {
        if (emotes == null || emotes.Length == 0)
        {
            Debug.LogWarning("Chưa gán danh sách Emotes.");
            return;
        }

        if (npcCooldowns.ContainsKey(npc))
        {
            if (Time.time < npcCooldowns[npc])
            {
                return;
            }
        }

        npcCooldowns[npc] = Time.time + cooldown;

        SpawnRandomEmote(npc);
    }

    private void SpawnRandomEmote(Transform npc)
    {
        int randomIndex = Random.Range(0, emotes.Length);
        NPCEmoteData data = emotes[randomIndex];

        if (data == null || data.emotePrefab == null)
        {
            return;
        }

        GameObject root = new GameObject("NPC Emote");
        root.transform.SetParent(npc);
        root.transform.localPosition = emoteOffset;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        GameObject emoteObject = Instantiate(data.emotePrefab, root.transform);
        emoteObject.transform.localPosition = Vector3.zero;
        emoteObject.transform.localRotation = Quaternion.identity;
        emoteObject.transform.localScale = Vector3.one;

        SpriteRenderer[] spriteRenderers = emoteObject.GetComponentsInChildren<SpriteRenderer>();

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = orderInLayer;
        }

        CreateTextWithFrame(root.transform, data.text);

        Destroy(root, emoteLifeTime);
    }

    private void CreateTextWithFrame(Transform parent, string sentence)
    {
        if (string.IsNullOrEmpty(sentence))
        {
            return;
        }

        GameObject textRoot = new GameObject("Text Box");
        textRoot.transform.SetParent(parent);
        textRoot.transform.localPosition = textOffset;
        textRoot.transform.localRotation = Quaternion.identity;
        textRoot.transform.localScale = Vector3.one;

        GameObject textObject = new GameObject("Emote Text");
        textObject.transform.SetParent(textRoot.transform);
        textObject.transform.localPosition = Vector3.zero;
        textObject.transform.localRotation = Quaternion.identity;
        textObject.transform.localScale = Vector3.one;

        TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
        textMesh.text = sentence;
        textMesh.fontSize = textSize;
        textMesh.color = textColor;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.textWrappingMode = TextWrappingModes.NoWrap;

        MeshRenderer textRenderer = textObject.GetComponent<MeshRenderer>();
        textRenderer.sortingLayerName = sortingLayerName;
        textRenderer.sortingOrder = orderInLayer + 2;

        textMesh.ForceMeshUpdate();

        Vector3 textSizeWorld = textMesh.textBounds.size;

        if (textFrameSprite != null)
        {
            GameObject frameObject = new GameObject("Text Frame");
            frameObject.transform.SetParent(textRoot.transform);
            frameObject.transform.localPosition = frameOffset;
            frameObject.transform.localRotation = Quaternion.identity;
            frameObject.transform.localScale = Vector3.one;

            SpriteRenderer frameRenderer = frameObject.AddComponent<SpriteRenderer>();
            frameRenderer.sprite = textFrameSprite;
            frameRenderer.sortingLayerName = sortingLayerName;
            frameRenderer.sortingOrder = orderInLayer + 1;

            frameRenderer.drawMode = SpriteDrawMode.Sliced;
            frameRenderer.size = new Vector2(
                textSizeWorld.x + framePadding.x,
                textSizeWorld.y + framePadding.y
            );
        }
    }
}