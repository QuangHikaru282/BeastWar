using System.Collections;
using UnityEngine;

public class NPCEmoteRandom : MonoBehaviour
{
    [Header("Emote Settings")]
    public Transform emotePoint;
    public GameObject[] emotePrefabs;

    [Header("Time Settings")]
    public float interval = 5f;
    public float emoteLifeTime = 1.2f;

    private GameObject currentEmote;

    private void OnEnable()
    {
        StartCoroutine(EmoteLoop());
    }

    private IEnumerator EmoteLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            PlayRandomEmote();
        }
    }

    private void PlayRandomEmote()
    {
        if (emotePoint == null)
        {
            Debug.LogWarning("Chưa gán EmotePoint cho NPC.");
            return;
        }

        if (emotePrefabs == null || emotePrefabs.Length == 0)
        {
            Debug.LogWarning("Chưa gán danh sách Emote Prefabs.");
            return;
        }

        if (currentEmote != null)
        {
            Destroy(currentEmote);
        }

        int randomIndex = Random.Range(0, emotePrefabs.Length);

        currentEmote = Instantiate(
            emotePrefabs[randomIndex],
            emotePoint.position,
            Quaternion.identity,
            emotePoint
        );

        currentEmote.transform.localPosition = Vector3.zero;

        Destroy(currentEmote, emoteLifeTime);
    }
}