using UnityEngine;
using DG.Tweening;

public class BeastEffectController : MonoBehaviour
{
    [Header("Idle Animation (Float)")]
    [SerializeField] private bool enableFloat = true;
    [SerializeField] private float floatDistance = 0.15f;
    [SerializeField] private float floatDuration = 1.2f;

    [Header("Spawn Animation")]
    [SerializeField] private bool enableSpawnScale = true;
    [SerializeField] private float spawnScaleDuration = 0.5f;

    private Tween floatTween;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (enableSpawnScale)
        {
            PlaySpawnEffect();
        }
        else if (enableFloat)
        {
            StartFloatAnimation();
        }
    }

    public void PlaySpawnEffect()
    {
        if (spriteRenderer == null) return;

        Vector3 originalScale = spriteRenderer.transform.localScale;
        spriteRenderer.transform.localScale = Vector3.zero;

        // Bật popup từ 0 lên 1, sau đó bắt đầu float
        spriteRenderer.transform.DOScale(originalScale, spawnScaleDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                if (enableFloat)
                {
                    StartFloatAnimation();
                }
            });
    }

    private void StartFloatAnimation()
    {
        if (spriteRenderer == null) return;

        // Lưu lại vị trí gốc
        float startY = spriteRenderer.transform.localPosition.y;

        // Loop animation di chuyển lên xuống
        floatTween = spriteRenderer.transform.DOLocalMoveY(startY + floatDistance, floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDestroy()
    {
        if (floatTween != null)
        {
            floatTween.Kill();
        }
    }
}
