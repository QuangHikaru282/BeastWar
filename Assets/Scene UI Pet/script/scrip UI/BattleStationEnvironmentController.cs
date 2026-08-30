using System;
using UnityEngine;

public class BattleStationEnvironmentController : MonoBehaviour
{
    [Serializable]
    public class BattleStationTheme
    {
        [Header("Loại môi trường")]
        public BattleEnvironmentType environmentType;

        [Header("Background")]
        public Sprite backgroundSprite;

        [Header("Battle Station")]
        public Sprite playerBattleStationSprite;
        public Sprite enemyBattleStationSprite;
    }

    [Header("Background")]
    [SerializeField]
    private SpriteRenderer backgroundRenderer;

    [Header("Battle Station Visual")]
    [SerializeField]
    private SpriteRenderer playerBattleStationRenderer;

    [SerializeField]
    private SpriteRenderer enemyBattleStationRenderer;

    [Header("Danh sách môi trường")]
    [SerializeField]
    private BattleStationTheme[] themes;

    [Header("Giữ kích thước Battle Station cũ")]
    [SerializeField]
    private bool keepOriginalStationSize = true;

    private Vector2 originalPlayerStationSize;
    private Vector2 originalEnemyStationSize;

    private void Awake()
    {
        SaveOriginalStationSizes();
    }

    private void Start()
    {
        ApplyCurrentEnvironment();
    }

    private void SaveOriginalStationSizes()
    {
        if (playerBattleStationRenderer != null)
        {
            originalPlayerStationSize =
                playerBattleStationRenderer.bounds.size;
        }

        if (enemyBattleStationRenderer != null)
        {
            originalEnemyStationSize =
                enemyBattleStationRenderer.bounds.size;
        }
    }

    public void ApplyCurrentEnvironment()
    {
        BattleEnvironmentType currentEnvironment =
            BattleEnvironmentState.CurrentEnvironment;

        if (themes == null || themes.Length == 0)
        {
            Debug.LogWarning(
                "Danh sách Themes đang trống!"
            );

            return;
        }

        foreach (BattleStationTheme theme in themes)
        {
            if (theme.environmentType != currentEnvironment)
                continue;

            ApplyTheme(theme);
            return;
        }

        Debug.LogWarning(
            "Chưa cài hình ảnh cho môi trường: " +
            currentEnvironment
        );
    }

    private void ApplyTheme(BattleStationTheme theme)
    {
        /*
         * Chỉ thay Sprite Background.
         * Không thay đổi Position, Scale hoặc kích thước.
         */
        if (backgroundRenderer != null &&
            theme.backgroundSprite != null)
        {
            backgroundRenderer.sprite =
                theme.backgroundSprite;
        }

        // Đổi bệ Player
        ChangeStationSprite(
            playerBattleStationRenderer,
            theme.playerBattleStationSprite,
            originalPlayerStationSize
        );

        // Đổi bệ Enemy
        ChangeStationSprite(
            enemyBattleStationRenderer,
            theme.enemyBattleStationSprite,
            originalEnemyStationSize
        );
    }

    private void ChangeStationSprite(
        SpriteRenderer spriteRenderer,
        Sprite newSprite,
        Vector2 targetSize)
    {
        if (spriteRenderer == null || newSprite == null)
            return;

        spriteRenderer.sprite = newSprite;

        if (!keepOriginalStationSize)
            return;

        Vector2 currentSize =
            spriteRenderer.bounds.size;

        if (currentSize.x <= 0.001f ||
            currentSize.y <= 0.001f)
        {
            return;
        }

        Vector3 currentScale =
            spriteRenderer.transform.localScale;

        float scaleX =
            targetSize.x / currentSize.x;

        float scaleY =
            targetSize.y / currentSize.y;

        spriteRenderer.transform.localScale =
            new Vector3(
                currentScale.x * scaleX,
                currentScale.y * scaleY,
                currentScale.z
            );
    }
}