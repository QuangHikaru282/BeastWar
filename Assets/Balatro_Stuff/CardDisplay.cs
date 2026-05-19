using UnityEngine;

public class CardDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer cardSpriteRenderer;

    public void SetupCard(Texture2D cardTexture)
    {
        if (cardSpriteRenderer == null) 
            cardSpriteRenderer = GetComponent<SpriteRenderer>();

        cardSpriteRenderer.material.SetTexture("_MainTex", cardTexture);
    }
}